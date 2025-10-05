# ZenBot.NET
![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

Fork of [Manu098vm](https://github.com/Manu098vm)'s [ManuBot.NET](https://github.com/Manu098vm/ManuBot.NET) fork of [kwsch](https://github.com/kwsch)'s [SysBot.NET](https://github.com/kwsch/SysBot.NET). 

**A list of the base fork’s features:**
* Game Mode selector (LGPE / SwSh / BDSP / PLA / SV) available in Hub settings
* WinForm executable compatible with Dark Mode (choose between Light, Dark, or System)
* Egg text requests via the `Egg: Yes` parameter
* Handle Wondercards as Discord attachments with the `trade` command
* Support for past-generation file conversions
* Discord trade embed display with custom emoji support
* Configurable Discord / Twitch / Donation commands
* `Reboot and Stop` routine to reset in-game status
* `UseTradePartnerDetails` option to automatically apply partner OT details to traded Pokémon
* Let’s Go Pikachu and Eevee support (thanks to [santacrab2](https://github.com/santacrab2/) for the codebase!)
* Uses deps instead of nuget for faster legality and feature updates.

**A list of this fork’s features:**
* Quicker LGPE Code Entry
* Support for Discord App Teams
* Reworked Embed layout with Alternate layout toggle
* Reinitialize legality settings without restarting the program using a command.
* MysteryModule for Random Pokemon and Egg trades
* PermissionsModule for checking that all whitelisted channels have the correct permissions required for the bot to function correctly
* GiveawayModule based on [Koi's](https://github.com/Koi-3088) [ForkBot.NET](https://github.com/Koi-3088/ForkBot.NET)
* RoleModule for handling adding/removing of trade roles in settings

Please refer to the [Wiki](https://github.com/Omni-KingZeno/ZenBot.NET/wiki) for detailed configurations and troubleshooting guides.

Special thanks to [notzyro](https://github.com/zyro670), [santacrab2](https://github.com/santacrab2/), and [9Bitdo](https://github.com/9bitdo/) for their help with code, updates, and ongoing support.

## SysBot.Base:
- Base logic library to be built upon in game-specific projects.
- Contains a synchronous and asynchronous Bot connection class to interact with sys-botbase.

## SysBot.Tests:
- Unit Tests for ensuring logic behaves as intended :)

# Example Implementations

The driving force to develop this project is automated bots for Nintendo Switch Pokémon games. An example implementation is provided in this repo to demonstrate interesting tasks this framework is capable of performing. Refer to the [Wiki](https://github.com/kwsch/SysBot.NET/wiki) for more details on the supported Pokémon features.

## SysBot.Pokemon:
- Class library using SysBot.Base to contain logic related to creating & running Sword/Shield bots.

## SysBot.Pokemon.WinForms:
- Simple GUI Launcher for adding, starting, and stopping Pokémon bots (as described above).
- Configuration of program settings is performed in-app and is saved as a local json file.

## SysBot.Pokemon.Discord:
- Discord interface for remotely interacting with the WinForms GUI.
- Provide a discord login token and the Roles that are allowed to interact with your bots.
- Commands are provided to manage & join the distribution queue.

## SysBot.Pokemon.Twitch:
- Twitch.tv interface for remotely announcing when the distribution starts.
- Provide a Twitch login token, username, and channel for login.

## SysBot.Pokemon.YouTube:
- YouTube.com interface for remotely announcing when the distribution starts.
- Provide a YouTube login ClientID, ClientSecret, and ChannelID for login.

Uses [Discord.Net](https://github.com/discord-net/Discord.Net) , [TwitchLib](https://github.com/TwitchLib/TwitchLib) and [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary) as a dependency via Nuget.

## Other Dependencies
Pokémon API logic is provided by [PKHeX](https://github.com/kwsch/PKHeX/), and template generation is provided by [Auto-Legality Mod](https://github.com/architdate/PKHeX-Plugins/). Current template generation uses [@santacrab2](https://www.github.com/santacrab2)'s [Auto-Legality Mod fork](https://github.com/santacrab2/PKHeX-Plugins).

# License
Refer to the `License.md` for details regarding licensing.
