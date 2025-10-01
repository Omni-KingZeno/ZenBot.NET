using PKHeX.Core;
using PKHeX.Core.Searching;
using SysBot.Base;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;
using System.Globalization;

namespace SysBot.Pokemon;

public class PokeTradeBotLGPE(PokeTradeHub<PB7> hub, PokeBotState cfg) : PokeRoutineExecutor7LGPE(cfg), ICountBot
{
    private readonly TradeSettings TradeSettings = hub.Config.Trade;
    private readonly TradeAbuseSettings AbuseSettings = hub.Config.TradeAbuse;
    public ICountSettings Counts => TradeSettings;

    /// <summary>
    /// Folder to dump received trade data to.
    /// </summary>
    /// <remarks>If null, will skip dumping.</remarks>
    private readonly IDumper DumpSetting = hub.Config.Folder;

    /// <summary>
    /// Synchronized start for multiple bots.
    /// </summary>
    public bool ShouldWaitAtBarrier { get; private set; }

    /// <summary>
    /// Tracks failed synchronized starts to attempt to re-sync.
    /// </summary>
    public int FailedBarrier { get; private set; }

    public override async Task MainLoop(CancellationToken token)
    {
        try
        {
            await InitializeHardware(hub.Config.Trade, token).ConfigureAwait(false);

            Log("Identifying trainer data of the host console.");
            var sav = await IdentifyTrainer(token).ConfigureAwait(false);
            RecentTrainerCache.SetRecentTrainer(sav);
           
            Log($"Starting main {nameof(PokeTradeBotLGPE)} loop.");
            await InnerLoop(sav, token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log(e.Message);
        }

        Log($"Ending {nameof(PokeTradeBotLGPE)} loop.");
        await HardStop().ConfigureAwait(false);
    }

    public override async Task HardStop()
    {
        UpdateBarrier(false);
        await CleanExit(CancellationToken.None).ConfigureAwait(false);
    }

    private async Task InnerLoop(SAV7b sav, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Config.IterateNextRoutine();
            var task = Config.CurrentRoutineType switch
            {
                PokeRoutineType.Idle => DoNothing(token),
                _ => DoTrades(sav, token),
            };
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (SocketException e)
            {
                if (e.StackTrace != null)
                    Connection.LogError(e.StackTrace);
                var attempts = hub.Config.Timings.ReconnectAttempts;
                var delay = hub.Config.Timings.ExtraReconnectDelay;
                var protocol = Config.Connection.Protocol;
                if (!await TryReconnect(attempts, delay, protocol, token).ConfigureAwait(false))
                    return;
            }
        }
    }

    private async Task DoNothing(CancellationToken token)
    {
        int waitCounter = 0;
        while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.Idle)
        {
            if (waitCounter == 0)
                Log("No task assigned. Waiting for new task assignment.");
            waitCounter++;
            if (waitCounter % 10 == 0 && hub.Config.AntiIdle)
                await Click(B, 1_000, token).ConfigureAwait(false);
            else
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }

    private async Task DoTrades(SAV7b sav, CancellationToken token)
    {
        var type = Config.CurrentRoutineType;
        int waitCounter = 0;
        while (!token.IsCancellationRequested && Config.NextRoutineType == type)
        {
            var (detail, priority) = GetTradeData(type);
            if (detail is null)
            {
                await WaitForQueueStep(waitCounter++, token).ConfigureAwait(false);
                continue;
            }
            waitCounter = 0;

            detail.IsProcessing = true;
            string tradetype = $" ({detail.Type})";
            Log($"Starting next {type}{tradetype} Bot Trade. Getting data...");
            hub.Config.Stream.StartTrade(this, detail, hub);
            hub.Queues.StartTrade(this, detail);

            await PerformTrade(sav, detail, type, priority, token).ConfigureAwait(false);
        }
    }

    private async Task WaitForQueueStep(int waitCounter, CancellationToken token)
    {
        if (waitCounter == 0)
        {
            // Updates the assets.
            hub.Config.Stream.IdleAssets(this);
            Log("Nothing to check, waiting for new users...");
        }

        const int interval = 10;
        if (waitCounter % interval == interval - 1 && hub.Config.AntiIdle)
            await Click(B, 1_000, token).ConfigureAwait(false);
        await Task.Delay(1_000, token).ConfigureAwait(false);
    }

    protected virtual (PokeTradeDetail<PB7>? detail, uint priority) GetTradeData(PokeRoutineType type)
    {
        if (hub.Queues.TryDequeue(type, out var detail, out var priority))
            return (detail, priority);
        if (hub.Queues.TryDequeueLedy(out detail))
            return (detail, PokeTradePriorities.TierFree);
        return (null, PokeTradePriorities.TierFree);
    }

    private async Task PerformTrade(SAV7b sav, PokeTradeDetail<PB7> detail, PokeRoutineType type, uint priority, CancellationToken token)
    {
        PokeTradeResult result;
        try
        {
            result = await PerformLinkCodeTrade(sav, detail, token).ConfigureAwait(false);
            if (result is PokeTradeResult.Success)
                return;
        }
        catch (SocketException socket)
        {
            Log(socket.Message);
            result = PokeTradeResult.ExceptionConnection;
            HandleAbortedTrade(detail, type, priority, result);
            throw; // let this interrupt the trade loop. re-entering the trade loop will recheck the connection.
        }
        catch (Exception e)
        {
            Log(e.Message);
            result = PokeTradeResult.ExceptionInternal;
        }

        HandleAbortedTrade(detail, type, priority, result);
    }

    private void HandleAbortedTrade(PokeTradeDetail<PB7> detail, PokeRoutineType type, uint priority, PokeTradeResult result)
    {
        detail.IsProcessing = false;
        if (result.ShouldAttemptRetry() && detail.Type != PokeTradeType.Random && !detail.IsRetry)
        {
            detail.IsRetry = true;
            hub.Queues.Enqueue(type, detail, Math.Min(priority, PokeTradePriorities.Tier2));
            detail.SendNotification(this, "Oops! Something happened. I'll requeue you for another attempt.");
        }
        else
        {
            detail.SendNotification(this, $"Oops! Something happened. Canceling the trade: {result}.");
            detail.TradeCanceled(this, result);
        }
    }

    private async Task<PokeTradeResult> PerformLinkCodeTrade(SAV7b sav, PokeTradeDetail<PB7> poke, CancellationToken token)
    {
        // Update Barrier Settings
        UpdateBarrier(poke.IsSynchronized);
        poke.TradeInitialize(this);
        hub.Config.Stream.EndEnterCode(this);

        var toSend = poke.TradeData;
        if (toSend.Species != 0)
            await SetBoxPokemon(toSend, 0, 0, token).ConfigureAwait(false);

        if (!await IsOnOverworld(token).ConfigureAwait(false))
        {
            await ExitTrade(true, token).ConfigureAwait(false);
            return PokeTradeResult.RecoverStart;
        }

        Log("Opening menu...");
        await Click(X, 2000, token).ConfigureAwait(false);
        while (await GetCurrentScreen(4, token).ConfigureAwait(false) is not ScreenScenario.Menu)
        {
            await Click(B, 2000, token).ConfigureAwait(false);
            await Click(X, 2000, token).ConfigureAwait(false);
        }

        Log("Selecting communicate...");
        await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token).ConfigureAwait(false);
        await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);

        while (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Menu ||
               await GetCurrentScreen(4, token).ConfigureAwait(false) is ScreenScenario.WaitingToTrade)
        {
            await Click(A, 1000, token).ConfigureAwait(false);
            if (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Save or ScreenScenario.Save2)
            {
                while (!await IsOnOverworld(token).ConfigureAwait(false))
                    await Click(B, 1000, token).ConfigureAwait(false);

                await Click(X, 2000, token).ConfigureAwait(false);

                Log("Opening menu");
                while (await GetCurrentScreen(4, token).ConfigureAwait(false) is not ScreenScenario.Menu)
                {
                    await Click(B, 2000, token).ConfigureAwait(false);
                    await Click(X, 2000, token).ConfigureAwait(false);
                }

                Log("Selecting communicate");
                await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token).ConfigureAwait(false);
                await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
            }
        }

        await Task.Delay(2000, token);
        Log("Selecting faraway connection");
        await SetStick(SwitchStick.RIGHT, 0, -30000, 0, token).ConfigureAwait(false);
        await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
        await Click(A, 10000, token).ConfigureAwait(false);

        await Click(A, 1000, token).ConfigureAwait(false);

        var codes = poke.PictoCodes;
        Log($"Entering Link Trade Code: {string.Join("-", codes.Select(c => c.ToString()))}");
        await EnterLinkCode(codes, token).ConfigureAwait(false);

        // Wait for Barrier to trigger all bots simultaneously.
        WaitAtBarrierIfApplicable(token);
        poke.TradeSearching(this);

        Log($"Searching for user {poke.Trainer.TrainerName}");
        await Task.Delay(3000, token);
        var btimeout = new Stopwatch();
        var tradeMaxWaitTime = hub.Config.Trade.TradeWaitTime * 1_000;
        btimeout.Restart();

        while (await IsInWaitingScreen(token).ConfigureAwait(false))
        {
            await Task.Delay(100, token);
            if (btimeout.ElapsedMilliseconds >= tradeMaxWaitTime)
            {
                poke.TradeCanceled(this, PokeTradeResult.NoTrainerFound);
                var msg = $"{poke.Trainer.TrainerName} not found";
                Log(msg);
                poke.SendNotification(this, msg);
                await ExitTrade(false, token).ConfigureAwait(false);
                hub.Config.Stream.EndEnterCode(this);
                return PokeTradeResult.NoTrainerFound;
            }
        }

        await Task.Delay(10000, token);
        var tradePartner = await GetTradePartnerInfo(sav, token).ConfigureAwait(false);
        if (!IsValidTradePartner(sav, tradePartner.OT, tradePartner.SID7))
        {
            poke.TradeCanceled(this, PokeTradeResult.NoTrainerFound);
            Log($"{poke.Trainer.TrainerName} not found");

            await ExitTrade(false, token).ConfigureAwait(false);
            hub.Config.Stream.EndEnterCode(this);
            return PokeTradeResult.NoTrainerFound;
        }

        if (hub.Config.Trade.UseTradePartnerDetails && TradeExtensions<PB7>.TrySetPartnerDetails(this, tradePartner, poke, hub.Config, out var toSendEdited))
        {
            toSend = toSendEdited;
            await SetBoxPokemon(toSendEdited, 0, 0, token).ConfigureAwait(false);
        }

        var trainerNID = ulong.Parse(tradePartner.SyncID, NumberStyles.HexNumber);
        RecordUtil<PokeTradeBotLGPE>.Record($"Initiating\t{trainerNID:X16}\t{tradePartner.OT}\t{poke.Trainer.TrainerName}\t{poke.Trainer.ID}\t{poke.ID}\t{toSend.EncryptionConstant:X8}");
        var message = $"Found Link Trade partner: {tradePartner.OT}-{tradePartner.TID7:000000} (ID: {trainerNID})";
        Log(message);

        var partnerCheck = await CheckPartnerReputation(this, poke, trainerNID, tradePartner.OT, AbuseSettings, token);
        if (partnerCheck != PokeTradeResult.Success)
        {
            await ExitTrade(false, token).ConfigureAwait(false);
            return partnerCheck;
        }

        poke.SendNotification(this, message);

        if (poke.Type == PokeTradeType.Dump)
        {
            var result = await ProcessDumpTradeAsync(poke, token).ConfigureAwait(false);
            await ExitTrade(false, token).ConfigureAwait(false);
            return result;
        }
        if(poke.Type == PokeTradeType.Clone)
        {
            var result = await ProcessCloneTradeAsync(poke, sav, token);
            await ExitTrade(false, token);
            return result;
        }

        while (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Box)
            await Click(A, 1000, token);

        poke.SendNotification(this,"You have 15 seconds to select your trade Pokemon.");
        Log("Waiting on trade screen...");

        await Task.Delay(15_000, token).ConfigureAwait(false);
        var tradeResult = await ConfirmAndStartTrading(0, token);
        if (tradeResult != PokeTradeResult.Success)
        {
            if (tradeResult == PokeTradeResult.TrainerLeft)
                Log("Trade canceled because trainer left the trade.");
            await ExitTrade(false, token).ConfigureAwait(false);
            return tradeResult;
        }

        if (token.IsCancellationRequested)
        {
            await ExitTrade(false, token).ConfigureAwait(false);
            return PokeTradeResult.ExceptionInternal;
        }
        //trade was successful
        var received = await ReadBoxPokemon(0, 0, token).ConfigureAwait(false);
        // Pokémon in b1s1 is same as the one they were supposed to receive (was never sent).
        if (SearchUtil.HashByDetails(received) == SearchUtil.HashByDetails(toSend) && received.Checksum == toSend.Checksum)
        {
            Log("User did not complete the trade.");
            await ExitTrade(false, token).ConfigureAwait(false);
            return PokeTradeResult.TrainerTooSlow;
        }

        // As long as we got rid of our inject in b1s1, assume the trade went through.
        Log("User completed the trade.");
        poke.TradeFinished(this, received);

        // Only log if we completed the trade.
        UpdateCountsAndExport(poke, received, toSend);

        // Log for Trade Abuse tracking.
        LogSuccessfulTrades(poke, trainerNID, tradePartner.OT);

        // Still need to wait out the trade animation.
        for (var i = 0; i < 30; i++)
            await Click(B, 0_500, token).ConfigureAwait(false);

        await ExitTrade(false, token).ConfigureAwait(false);
        return PokeTradeResult.Success;
    }

    private void UpdateCountsAndExport(PokeTradeDetail<PB7> poke, PB7 received, PB7 toSend)
    {
        var counts = TradeSettings;
        if (poke.Type == PokeTradeType.Random)
            counts.AddCompletedDistribution();
        else if (poke.Type == PokeTradeType.Clone)
            counts.AddCompletedClones();
        else
            counts.AddCompletedTrade();

        if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
        {
            var subfolder = poke.Type.ToString().ToLower();
            DumpPokemon(DumpSetting.DumpFolder, subfolder, received); // received by bot
            if (poke.Type is PokeTradeType.Specific or PokeTradeType.Clone)
                DumpPokemon(DumpSetting.DumpFolder, "traded", toSend); // sent to partner
        }
    }

    private async Task<PokeTradeResult> ConfirmAndStartTrading(int slot, CancellationToken token)
    {
        // We'll keep watching B1S1 for a change to indicate a trade started -> should try quitting at that point.
        var oldEC = await Connection.ReadBytesAsync(GetSlotOffset(0,slot), 8, token).ConfigureAwait(false);
        Log("Confirming and initiating trade.");
        await Click(A, 3_000, token).ConfigureAwait(false);
        for (int i = 0; i < 10; i++)
        {
            if (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Box or ScreenScenario.Menu)
                return PokeTradeResult.TrainerLeft;

            await Click(A, 1_500, token).ConfigureAwait(false);
        }

        var tradeCounter = 0;
        Log("Checking for received pokemon in slot 1...");
        while (true)
        {
            
            var newEC = await Connection.ReadBytesAsync(GetSlotOffset(0, slot), 8, token).ConfigureAwait(false);
            if (!newEC.SequenceEqual(oldEC))
            {
                Log("Change detected in slot 1.");
                await Task.Delay(15_000, token).ConfigureAwait(false);
                return PokeTradeResult.Success;
            }

            tradeCounter++;

            if (tradeCounter >= hub.Config.Trade.MaxTradeConfirmTime)
            {
                // If we don't detect a B1S1 change, the trade didn't go through in that time.
                Log("did not detect a change in slot 1.");
                return PokeTradeResult.TrainerTooSlow;
            }

            if (await IsOnOverworld(token).ConfigureAwait(false))
                return PokeTradeResult.TrainerLeft;
            await Task.Delay(1000, token);
        }
    }

    private async Task<PokeTradeResult> ProcessCloneTradeAsync(PokeTradeDetail<PB7> detail,SAV7b sav, CancellationToken token)
    {
        detail.SendNotification(this,"Highlight the Pokemon in your box You would like Cloned up to 6 at a time! " +
            "You have 5 seconds between highlights to move to the next pokemon. (The first 5 starts now!). " +
            "If you would like to less than 6 remain on the same pokemon until the trade begins.");
        await Task.Delay(10_000, token);

        var offered = await ReadPokemon(TradePartnerPokemonOffset, token).ConfigureAwait(false);
        var clones = new List<PB7>() { offered };
        detail.SendNotification(this,$"You added {(Species)offered.Species} to the clone list.");

        if (hub.Config.Discord.ReturnPKMs)
            detail.SendNotification(this, offered, "Here's what you showed me!");

        for (int i = 0; i < 6; i++)
        {
            await Task.Delay(5_000, token);

            var newOffered = await ReadPokemon(TradePartnerPokemonOffset, token).ConfigureAwait(false);
            if (clones.Any(z => SearchUtil.HashByDetails(z) == SearchUtil.HashByDetails(newOffered)))
            {
                continue;
            }
            else
            {
                clones.Add(newOffered);
                offered = newOffered;
                detail.SendNotification(this, $"You added {(Species)offered.Species} to the clone list.");

                if (hub.Config.Discord.ReturnPKMs)
                    detail.SendNotification(this, offered, "Here's what you showed me!");
            }

        }
       
        var clonestring = new StringBuilder();
        foreach (var str in clones)
            clonestring.AppendLine($"{(Species)str.Species}");
        detail.SendNotification(this,"Pokemon to be Cloned", clonestring.ToString());
     
        detail.SendNotification(this,"Exiting Trade to inject clones, please reconnect using the same link code.");
        await ExitTrade(false,token);

        foreach (var (i, clone) in clones.Select((clone, i) => (i, clone)))
        {
            await SetBoxPokemon(clone, 0, i, token).ConfigureAwait(false);
            await Task.Delay(1000, token).ConfigureAwait(false);
        }

        await Click(X, 2000, token).ConfigureAwait(false);
        Log("Opening menu...");

        while (await GetCurrentScreen(4, token).ConfigureAwait(false) is not ScreenScenario.Menu)
        {
            await Click(B, 2000, token);
            await Click(X, 2000, token);
        }

        Log("Selecting communicate...");
        await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token).ConfigureAwait(false);
        await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
        while (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Menu ||
              await GetCurrentScreen(4, token).ConfigureAwait(false) is ScreenScenario.WaitingToTrade)
        {
            await Click(A, 1000, token);
            if (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Save or ScreenScenario.Save2)
            {
                while (!await IsOnOverworld(token))
                    await Click(B, 1000, token);

                await Click(X, 2000, token).ConfigureAwait(false);

                Log("Opening menu...");
                while (await GetCurrentScreen(4, token).ConfigureAwait(false) is not ScreenScenario.Menu)
                {
                    await Click(B, 2000, token);
                    await Click(X, 2000, token);
                }
                Log("Selecting communicate...");
                await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token).ConfigureAwait(false);
                await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
            }
        }

        await Task.Delay(2000, token);
        Log("Selecting faraway connection...");

        await SetStick(SwitchStick.RIGHT, 0, -30000, 0, token).ConfigureAwait(false);
        await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
        await Click(A, 10000, token).ConfigureAwait(false);

        await Click(A, 1000, token).ConfigureAwait(false);
        await EnterLinkCode(detail.PictoCodes, token).ConfigureAwait(false);
        detail.TradeSearching(this);
        Log($"Searching for user {detail.Trainer.TrainerName}...");
        var waitingTimer = new Stopwatch();
        var tradeMaxWaitTime = hub.Config.Trade.TradeWaitTime * 1_000;
        waitingTimer.Start();

        while (await IsInWaitingScreen(token).ConfigureAwait(false))
        {
            await Task.Delay(100, token);
            if (waitingTimer.ElapsedMilliseconds >= tradeMaxWaitTime)
            {
                detail.TradeCanceled(this, PokeTradeResult.NoTrainerFound);
                Log($"{detail.Trainer.TrainerName} not found");

                await ExitTrade(false, token);
                hub.Config.Stream.EndEnterCode(this);
                return PokeTradeResult.NoTrainerFound;
            }
        }

        await Task.Delay(10000, token);
        var tradePartner = await GetTradePartnerInfo(sav, token).ConfigureAwait(false);
        if (!IsValidTradePartner(sav, tradePartner.OT, tradePartner.SID7))
        {
            detail.TradeCanceled(this, PokeTradeResult.NoTrainerFound);
            Log($"{detail.Trainer.TrainerName} not found");

            await ExitTrade(false, token).ConfigureAwait(false);
            hub.Config.Stream.EndEnterCode(this);
            return PokeTradeResult.NoTrainerFound;
        }

        var trainerNID = ulong.Parse(tradePartner.SyncID, NumberStyles.HexNumber);
        var message = $"Found Link Trade partner: {tradePartner.OT}-{tradePartner.TID7:000000} (ID: {trainerNID})";

        foreach (var toSend in clones)
        {
            for (int q = 0; q < clones.IndexOf(toSend); q++)
            {
                await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token);
                await SetStick(SwitchStick.RIGHT, 0, 0, 1000, token).ConfigureAwait(false);
            }

            while (await GetCurrentScreen(2, token).ConfigureAwait(false) is ScreenScenario.Box)
                await Click(A, 1000, token);

            detail.SendNotification(this, $"Sending {(Species)toSend.Species}. You have 15 seconds to select your trade pokemon");
            Log("Waiting on trade screen...");

            await Task.Delay(10_000, token).ConfigureAwait(false);
            detail.SendNotification(this, "You have 5 seconds left to get to the trade screen to not break the trade");
            await Task.Delay(5_000, token);

            var tradeResult = await ConfirmAndStartTrading(clones.IndexOf(toSend), token);
            if (tradeResult != PokeTradeResult.Success)
            {
                if (tradeResult == PokeTradeResult.TrainerLeft)
                    Log("Trade canceled because trainer left the trade.");
                await ExitTrade(false, token).ConfigureAwait(false);
                return tradeResult;
            }

            if (token.IsCancellationRequested)
            {
                await ExitTrade(false, token).ConfigureAwait(false);
                return PokeTradeResult.RoutineCancel;
            }
            await Task.Delay(30_000, token);
        }
        await ExitTrade(false, token);
        return PokeTradeResult.Success;
    }

    private async Task<PokeTradeResult> ProcessDumpTradeAsync(PokeTradeDetail<PB7> detail, CancellationToken token)
    {
        detail.SendNotification(this,"Highlight the Pokemon in your box, you have 30 seconds");
        var offered = await ReadPokemon(TradePartnerPokemonOffset, token).ConfigureAwait(false);
        detail.SendNotification(this, offered, "Here's what you showed me!");

        if (DumpSetting.Dump)
            DumpPokemon(DumpSetting.DumpFolder, detail.Type.ToString().ToLower(), offered);

        var quicktime = new Stopwatch();
        quicktime.Restart();
        while (quicktime.ElapsedMilliseconds <= 30_000)
        {
            var newOffered = await ReadPokemon(TradePartnerPokemonOffset, token).ConfigureAwait(false);
            if (SearchUtil.HashByDetails(offered) != SearchUtil.HashByDetails(newOffered))
            {
                detail.SendNotification(this, newOffered, "Here's the pokemon you showed me");
                offered = newOffered;

                if (DumpSetting.Dump)
                    DumpPokemon(DumpSetting.DumpFolder, detail.Type.ToString().ToLower(), offered);
            }
        }
        detail.SendNotification(this,"Time is up!");
        return PokeTradeResult.Success;
    }

    private async Task EnterLinkCode(PictoCode[] codes, CancellationToken token)
    {
        hub.Config.Stream.StartEnterCode(this);
        foreach (var code in codes)
        {
            if ((int)code > 4)
            {
                await SetStick(SwitchStick.RIGHT, 0, -30000, 0, token).ConfigureAwait(false);
                await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
            }
            if ((int)code <= 4)
            {
                for (int i = (int)code; i > 0; i--)
                {
                    await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token).ConfigureAwait(false);
                    await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
            else
            {
                for (int i = (int)code - 5; i > 0; i--)
                {
                    await SetStick(SwitchStick.RIGHT, 30000, 0, 0, token).ConfigureAwait(false);
                    await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
            await Click(A, 200, token).ConfigureAwait(false);
            await Task.Delay(500, token).ConfigureAwait(false);
            if ((int)code <= 4)
            {
                for (int i = (int)code; i > 0; i--)
                {
                    await SetStick(SwitchStick.RIGHT, -30000, 0, 0, token).ConfigureAwait(false);
                    await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
            else
            {
                for (int i = (int)code - 5; i > 0; i--)
                {
                    await SetStick(SwitchStick.RIGHT, -30000, 0, 0, token).ConfigureAwait(false);
                    await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
            if ((int)code > 4)
            {
                await SetStick(SwitchStick.RIGHT, 0, 30000, 0, token).ConfigureAwait(false);
                await SetStick(SwitchStick.RIGHT, 0, 0, 0, token).ConfigureAwait(false);
            }
        }
    }

    private async Task<TradePartnerLGPE> GetTradePartnerInfo(SAV7b host, CancellationToken token)
    {
        // We're able to see both users' MyStatus, but one of them will be ourselves.
        var partner = await GetTradePartnerMyStatus(Trader1MyStatusOffset, token).ConfigureAwait(false);
        if (!IsValidTradePartner(host, partner.OT, partner.DisplaySID))
            partner = await GetTradePartnerMyStatus(Trader2MyStatusOffset, token).ConfigureAwait(false);
        return new TradePartnerLGPE(partner);
    }

    private static bool IsValidTradePartner(SAV7b host, string partnerOT, uint partnerSID) =>
        !((partnerOT == host.OT && partnerSID == host.DisplaySID) || (partnerOT == string.Empty && partnerSID == 0));

    private void WaitAtBarrierIfApplicable(CancellationToken token)
    {
        if (!ShouldWaitAtBarrier)
            return;
        var opt = hub.Config.Distribution.SynchronizeBots;
        if (opt == BotSyncOption.NoSync)
            return;

        var timeoutAfter = hub.Config.Distribution.SynchronizeTimeout;
        if (FailedBarrier == 1) // failed last iteration
            timeoutAfter *= 2; // try to re-sync in the event things are too slow.

        var result = hub.BotSync.Barrier.SignalAndWait(TimeSpan.FromSeconds(timeoutAfter), token);

        if (result)
        {
            FailedBarrier = 0;
            return;
        }

        FailedBarrier++;
        Log($"Barrier sync timed out after {timeoutAfter} seconds. Continuing.");
    }

    private void UpdateBarrier(bool shouldWait)
    {
        if (ShouldWaitAtBarrier == shouldWait)
            return; // no change required

        ShouldWaitAtBarrier = shouldWait;
        if (shouldWait)
        {
            hub.BotSync.Barrier.AddParticipant();
            Log($"Joined the Barrier. Count: {hub.BotSync.Barrier.ParticipantCount}");
        }
        else
        {
            hub.BotSync.Barrier.RemoveParticipant();
            Log($"Left the Barrier. Count: {hub.BotSync.Barrier.ParticipantCount}");
        }
    }

    private async Task ExitTrade(bool unexpected, CancellationToken token)
    {
        if (unexpected)
            Log("Unexpected behavior, recovering position.");

        int ctr = 120_000;
        while (!await IsOnOverworld(token).ConfigureAwait(false))
        {
            if (ctr < 0)
            {
                await RestartGameLGPE(hub.Config,token).ConfigureAwait(false);
                return;
            }
            
            await Click(B, 1_000, token).ConfigureAwait(false);
            if (await IsOnOverworld(token).ConfigureAwait(false))
                return;
           
            await Click(await GetCurrentScreen(2, token).ConfigureAwait(false)
                is ScreenScenario.Box or ScreenScenario.YesNoSelector ? A : B, 1_000, token).ConfigureAwait(false);

            if (await IsOnOverworld(token).ConfigureAwait(false))
                return;

            await Click(B, 1_000, token).ConfigureAwait(false);
            if (await IsOnOverworld(token).ConfigureAwait(false))
                return;

            ctr -= 3_000;
        }
    }
}
