# VPVC
**Proximity voice chat for Valorant**

This project is still in early beta so expect bugs. It is built with the [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/).

Distributed builds are wrapped in a custom program that extracts the required files on launch so only one executable needs to be downloaded by the user.

## How it works
When you use VPVC, the app repeatedly takes a screenshot of your game and analyzes it to find out whether you're in the lobby, agent select or in-game, and your position on the map. This method of detecting your location isn't perfect but it ensures VPVC is safe to use and will not lead to any game bans.

While in the lobby, everyone in your party can hear each other.
In agent select, you can only hear members of your own team.
During gameplay, the volume of other players is determined by their distance on the map to you.

VPVC has voice chat built in so you don't need to install or use any additional software.

When you create a party in the app, you get a code that other players need to enter join your party.

More info on [the website](https://vpvc.app).