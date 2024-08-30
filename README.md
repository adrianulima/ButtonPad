# Button Pad

Button Pad is a mod for [Space Engineers](https://www.spaceengineersgame.com/) that adds an app to LCD screens, allowing them to function like regular button pads. You can assign terminal actions to buttons directly on LCD screens, creating a versatile and customizable interface for controlling your grid. The number of available buttons varies based on the screen size but can be adjusted using **Ctrl + Shift + Mouse Scroll Wheel**.

![image](https://github.com/user-attachments/assets/16583b80-d7e2-412c-8a94-dfb3a3bc2c1e)

> [!NOTE]
> This mod was the top 1 most popular mod on the game's Steam Workshop for the entire year of 2023.

## How to Install

1. Visit the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2933676834) for Button Pad and subscribe to the mod and its dependency, the [TouchScreenAPI](https://steamcommunity.com/sharedfiles/filedetails/?id=2668820525).
2. Launch Space Engineers and navigate to the save game settings.
3. Activate both the Button Pad mod and the TouchScreenAPI mod in your active mods list.
4. Open any LCD Block's Control Panel.
5. Change the Content property to Script.
6. Select "Button Pad" from the list.

## How to Use

1. Click on an empty button on the LCD screen to add an action.
2. Select the block and action from the available options, or click outside the button area to cancel.
   - If the action is a programmable block run (PB Run), enter the argument in the text field.
3. Choose the text that will be displayed on the button. This can be changed later by holding **SHIFT** and clicking the button.
4. To clear a button, hold **Ctrl** to see the option, then click the button.
5. To change the displayed text, hold **SHIFT** and click the button to cycle through text options.

Fully supports BLUEPRINTS!

## Screens and Touch

The Button Pad app is compatible with nearly all LCDs in the vanilla game and DLCs, including cockpits. For modded LCDs, use the Screen Calibration app to ensure optimal functionality.

The touch screen feature is powered by the [TouchScreenAPI mod](https://github.com/adrianulima/TouchScreenAPI), which provides both the cursor functionality and UI elements accessible to any modder. For further assistance, feel free to reach out via GitHub, Steam, or Discord (@adrianolima).

## Multiplayer and Servers

Button Pad works seamlessly in single-player, multiplayer, and server environments. As a TSS (LCD script), it operates primarily on the client side. Clients handle drawing and checking available actions on the grids, with the server's role limited to data persistence across sessions. Access is restricted to players with permission to interact with the block; sharing with a faction allows faction members to use the app as well.

## Changing the Scale

Adjust the app's scale using the following shortcuts:

- **Ctrl + Plus:** Increase scale.
- **Ctrl + Minus:** Decrease scale.
- **Ctrl + 0:** Reset to default scale.

This feature is particularly helpful for enhancing readability on smaller screens.

## Performance and Limitations

Currently, the app runs at 6 FPS due to game-imposed limitations on LCD screen texture updates. While a potential workaround exists to increase the refresh rate to 30 FPS, the initial release prioritizes performance stability. Future versions may include enhancements to improve refresh rates.

## Credits

- Developed by [Adriano Lima](https://github.com/adrianulima)
- Special thanks to the Space Engineers modding community for their continuous support and feedback.

> [!IMPORTANT]
> This mod is not affiliated with or endorsed by Keen Software House. It is a fan-made project developed independently for Space Engineers.

---

For any issues or suggestions, please contact me on [GitHub](https://github.com/adrianulima), [Steam](https://steamcommunity.com/id/adrianulima), or Discord (@adrianolima).
