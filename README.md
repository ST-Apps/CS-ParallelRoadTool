# CS-ParallelRoadTool
A mod that allows players to easily draw parallel/stacked roads in Cities: Skylines.

**Tested on version 1.15-f5**

## Features
- Limitless parallel/stacked configurations: you can choose any network (not only roads!) and distances (both horizontal and vertical) to easily create every layout that you want
- Visual overlay guidelines to help you visualizing what you're going to build before actually building
- One-way networks support: you can reverse the direction of any one-way network, so that you can easily build highways without needing to manually upgrade direction later
- Ability to save/load presets
- Left-hand drive support

## Known limitations/issues

- **[Fine Road Anarchy](https://steamcommunity.com/workshop/filedetails/?id=802066100) is highly recommended!** (*mod works without it but you won't be able to connect some segments without an anarchy mod*)
- Sharp angles produce weird results, curves work better
- Roads will always follow terrain elevation

## Features that may come somewhere in the future

- Ability to toggle on/off terrain conforming
- Ability to center cursor on the middle point between all the selected roads

## Support my work

If you'd like to support my work you can donate using PayPal:

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=CZB2MWEN2JZAN)

## How to contribute

If you find an issue, please search [here](/../../issues/) to see if someone already reported it and, eventually, comment on the already reported one.
If you can't find any issue similar to yours, please click [here](/../../issues/new) and provide all the informations that you can (screenshots and logs are welcome).

If you're interested in helping with development, choose an issue and ask to be assigned. I'll create a branch for that issue and you'll be able to work there.
If there's no issue for the thing you'd like to work on, feel free to open a new one.

### Translations

- Fork the project
- Duplicate `en.xml` from [Localization Folder](ParallelRoadTool/Assets/Localizations) and rename it with the ISO code for your language
- Create a Pull Request

### Branching explanation

- master: current stable version on Steam Workshop.
- dev: current beta version on Steam Workshop, any feature branch will be merged here for testing.
- ISSUE-x: issue-related branches, any branch will focus on solving the reported issue. Those branches will be merged into *dev* for testing and then moved into *master* when they are working.
