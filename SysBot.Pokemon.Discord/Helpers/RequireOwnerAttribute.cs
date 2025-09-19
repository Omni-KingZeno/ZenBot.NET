using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SysBot.Pokemon.Discord;
public sealed class RequireOwnerAttribute : PreconditionAttribute
{
    // Override the CheckPermissions method
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        IApplication application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(continueOnCapturedContext: false);
        if (context.User.Id != application.Owner.Id && !SysCordSettings.Admins.Contains(context.User.Id))
        {
            return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by the owner of the bot.");
        }

        return PreconditionResult.FromSuccess();
    }
}