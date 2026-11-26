
# [![](https://raw.githubusercontent.com/FFXIV-CombatReborn/RebornAssets/main/IconAssets/NSR_Icon.png)](https://github.com/FFXIV-CombatReborn/NoSillyReborn)

**No Silly Reborn**

![Github Latest Releases](https://img.shields.io/github/downloads/FFXIV-CombatReborn/NoSillyReborn/latest/total.svg?style=for-the-badge)
![Github License](https://img.shields.io/github/license/FFXIV-CombatReborn/NoSillyReborn.svg?label=License&style=for-the-badge)
[![](https://dcbadge.limes.pink/api/server/p54TZMPnC9)](https://discord.gg/p54TZMPnC9)

No Silly Reborn intercepts your action inputs during combat and replaces incorrect actions with the correct ones based on your current job state.

## Features

- **Misclick interception**: Detects missimputs and corrections action selection based on rotation and player state.

## Commands

`/nosilly` and `/nsr` are the same command and take the same options.

- `/nsr` opens or closes the main window, where you turn rules and jobs on or off, read the activity log, and choose when No Silly Reborn steps in and how it tells you.
- `/nsr on`, `/nsr off` and `/nsr toggle` turn No Silly Reborn on or off without opening the window, which makes them easy to put in a macro. A chat line confirms the change, and the setting is kept after a restart.
- `/nsr rules` opens the role page that has your current job.
- `/nsr activity` opens the activity log.

## Installing
- Enter `/xlsettings` in the chat window and go to the Experimental tab in the opening window.
- **Skip below the DevPlugins section to the Custom Plugin Repositories section.**
- Copy and paste the repo.json link into the first free text input field.
```
https://raw.githubusercontent.com/FFXIV-CombatReborn/CombatRebornRepo/main/pluginmaster.json
```
- Click on the + button and make sure the checkmark beside the new field is set afterwards.
- **Click on the Save-icon in the bottom right.**

Following these steps, you should be able to see all contained plugins in the Available Plugins tab in the Dalamud Plugin Installer.
No Plugins will be installed, you have just made them available. You can now select which of these plugins you actually want to install.

## Want to contribute?

- Create a fork
- Make your changes
- Test the changes
- Create a PR and point it to main
