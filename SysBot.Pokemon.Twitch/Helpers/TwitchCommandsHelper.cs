using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;

namespace SysBot.Pokemon.Twitch;

public static class TwitchCommandsHelper<T> where T : PKM, new()
{
    /// <summary>
    /// Adds a user to the waiting list for a trade request.
    /// </summary>
    /// <param name="showdownSet">The Showdown set string representing the Pokémon.</param>
    /// <param name="display">The display name of the user.</param>
    /// <param name="username">The username of the user.</param>
    /// <param name="mUserId">The user ID of the user.</param>
    /// <param name="sub">Indicates if the user is a subscriber.</param>
    /// <param name="message">The message to be returned to the user.</param>
    /// <returns>True if the request was added to the queue.</returns>
    public static bool AddToWaitingList(string showdownSet, string display, string username, ulong mUserId, bool sub, out string message, bool eggTrade = false, bool mysteryEgg = false)
    {
        if (!TwitchBot<T>.Info.GetCanQueue())
        {
            message = "Sorry, I am not currently accepting queue requests!";
            return false;
        }

        var mode = TwitchBot<T>.Info.Hub.Config.Mode;
        var hasEggs = TradeExtensions<T>.HasEggs(mode);
        if (!mysteryEgg)
        {
            if (!ShowdownUtil.TryConvertSingleLine(showdownSet, out var set))
        {
            message = $"Skipping trade, @{username}: Invalid/Empty nickname provided for the species.";
            return false;
        }
        var template = AutoLegalityWrapper.GetTemplate(set);
        if (template.Species == 0)
        {
            message = $"Skipping trade, @{username}: Please read what you are supposed to type as the command argument.";
            return false;
        }

        if (set.InvalidLines.Count != 0)
        {
            message = $"Skipping trade, @{username}: Unable to parse Showdown Set:\n{string.Join('\n', set.InvalidLines)}";
            return false;
        }

        try
        {
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = eggTrade && hasEggs ? sav.GenerateEgg((RegenTemplate)template, out LegalizationResult result) : sav.GetLegal(set, out result);

            var la = new LegalityAnalysis(pkm);
            var enc = la.EncounterOriginal;
                var (canBeTraded, errorMessage) = pkm.CanBeTraded(enc);
                if (!canBeTraded)
            {
                message = $"Skipping trade, @{username}: {errorMessage}";
                return false;
            }

            if (pkm is T pk)
            {
                if (la.Valid)
                {
                    var tq = new TwitchQueue<T>(pk, new PokeTradeTrainerInfo(display, mUserId), username, sub);
                        TwitchBot<T>.QueuePool.RemoveAll(z => z.DisplayName == username); // remove old requests if any
                        TwitchBot<T>.QueuePool.Add(tq);
                        message = $"@{username} - added to the waiting list. Please whisper your trade code to me! Your request from the waiting list will be removed if you are too slow!";
                        return true;
                    }
                }

                var reason = result == LegalizationResult.Timeout ? "Set took too long to generate." : "Unable to legalize the Pokémon.";
                message = $"Skipping trade, @{username}: {reason}";
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TwitchCommandsHelper<T>));
                message = $"Skipping trade, @{username}: An unexpected problem occurred.";
            }
            return false;
        }
        else
        {
            try
            {
                if (hasEggs)
                {
                    var pkm = TradeExtensions<T>.GenerateMysteryEgg(TwitchBot<T>.Info.Hub.Config.Distribution.MysteryShinyOdds);

                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid)
                    {
                        var tq = new TwitchQueue<T>(pkm, new PokeTradeTrainerInfo(display, mUserId), username, sub);
                        TwitchBot<T>.QueuePool.RemoveAll(z => z.DisplayName == username); // remove old requests if any
                        TwitchBot<T>.QueuePool.Add(tq);
                        message = $"@{username} - added to the waiting list. Please whisper your trade code to me! Your request from the waiting list will be removed if you are too slow!";
                    return true;
                }

                    var reason = "Failed to generate a mystery egg.";
                    message = $"Skipping trade, @{username}: {reason}";
                }
                message = $"Skipping trade, @{username}: {mode} does not have eggs!";
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex);
            message = $"Skipping trade, @{username}: An unexpected problem occurred.";
        }
        return false;
    }
    }

    public static string ClearTrade(string user)
    {
        var result = TwitchBot<T>.Info.ClearTrade(user);
        return GetClearTradeMessage(result);
    }

    public static string ClearTrade(ulong userId)
    {
        var result = TwitchBot<T>.Info.ClearTrade(userId);
        return GetClearTradeMessage(result);
    }

    private static string GetClearTradeMessage(QueueResultRemove result)
    {
        return result switch
        {
            QueueResultRemove.CurrentlyProcessing => "Looks like you're currently being processed! Did not remove from queue.",
            QueueResultRemove.CurrentlyProcessingRemoved => "Looks like you're currently being processed! Removed from queue.",
            QueueResultRemove.Removed => "Removed you from the queue.",
            _ => "Sorry, you are not currently in the queue.",
        };
    }

    public static string GetCode(ulong parse)
    {
        var detail = TwitchBot<T>.Info.GetDetail(parse);
        return detail == null
            ? "Sorry, you are not currently in the queue."
            : $"Your trade code is {(typeof(T) == typeof(PB7) ? string.Join(", ", detail.Trade.PictoCodes) : $"{detail.Trade.Code:0000 0000}")}";
    }
}
