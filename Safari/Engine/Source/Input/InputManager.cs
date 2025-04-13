using GeonBit.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Engine.Input;

public enum ActiveDevice {
	KeyboardMouse,
	Gamepad
}

/// <summary>
/// Manages all input checks and input events that can happen in the glorious world of Engine
/// </summary>
public static class InputManager {
	private static KeyboardState prevKS;
	private static KeyboardState currentKS;
	private static MouseState prevMS;
	private static MouseState currentMS;
	private static GamePadState[] prevGPS = new GamePadState[4];
	private static GamePadState[] currentGPS = new GamePadState[4];

	public static Mouse Mouse { get; private set; } = new Mouse();
	public static Keyboard Keyboard { get; private set; } = new Keyboard();
	public static GamePad GamePad { get; private set; } = new GamePad();
	public static Actions Actions { get; private set; } = new Actions();
	public static ActiveDevice ActiveDevice { get; private set; } = ActiveDevice.KeyboardMouse;
	public static event EventHandler<ActiveDevice> ActiveDeviceChanged;
	public static int ActiveGamepad { get; private set; } = 1;
	public static bool ShowCursor { get; set; } = true;

	/// <summary>
	/// Whether the game had focus in the previous frame
	/// </summary>
	public static bool WasGameFocused { get; private set; } = true;
	public static bool IsGameFocused { get; private set; } = true;
	public static bool JustLostFocus => WasGameFocused && !IsGameFocused;

	public static void Initialize() {
		currentKS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
		prevKS = currentKS;
		currentMS = Microsoft.Xna.Framework.Input.Mouse.GetState();
		prevMS = currentMS;
		currentGPS[0] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One);
		prevGPS[0] = currentGPS[0];
		currentGPS[1] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.Two);
		prevGPS[1] = currentGPS[1];
		currentGPS[2] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.Three);
		prevGPS[2] = currentGPS[2];
		currentGPS[3] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.Four);
		prevGPS[3] = currentGPS[3];
	}

	public static void Update(GameTime gameTime) {
		// update focus
		int rootChildren = UserInterface.Active.Root.Children.Count;
		bool allPassiveFocus = true;
        for (int i = 0; i < rootChildren && allPassiveFocus; i++) {
			if (UserInterface.Active.Root.Children[i].Tag != "PassiveFocus") {
                allPassiveFocus = false;
			}
        }
        WasGameFocused = IsGameFocused;
		IsGameFocused = allPassiveFocus; 

        // Update states
        prevKS = currentKS;
		prevMS = currentMS;
		for (int i = 0; i < 4; i++) {
			prevGPS[i] = currentGPS[i];
		}

		currentKS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
		currentMS = Microsoft.Xna.Framework.Input.Mouse.GetState();
		currentGPS[0] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One);
		currentGPS[1] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.Two);
		currentGPS[2] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.Three);
		currentGPS[3] = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.Four);

		ActiveDevice prevAD = ActiveDevice;
		if (currentKS != prevKS) {
			//this works
			ActiveDevice = ActiveDevice.KeyboardMouse;
		}
		if (currentMS != prevMS) {
			//this works
			ActiveDevice = ActiveDevice.KeyboardMouse;
		}
		for (int i = 0; i < 4; i++) {
			if (!GamePadEquals(currentGPS[i], prevGPS[i])) {
				if (currentGPS[i].IsConnected) {
					ActiveDevice = ActiveDevice.Gamepad;
				}
				ActiveGamepad = i + 1;
				break;
			}
		}

		if (ShowCursor) {
			if (ActiveDevice != ActiveDevice.KeyboardMouse) {
				Game.Instance.IsMouseVisible = false;
			} else {
				Game.Instance.IsMouseVisible = true;
			}
		}

		if (ActiveDevice != prevAD) {
			ActiveDeviceChanged?.Invoke(null, ActiveDevice);
		}

		// Update states for keyboard
		Keyboard.CurrentKS = currentKS;
		Keyboard.PrevKS = prevKS;

		// Update states for mouse
		Mouse.PrevMS = prevMS;
		Mouse.CurrentMS = currentMS;

		// Update states for gamepad
		GamePad.CurrentGPS = currentGPS[ActiveGamepad - 1];
		GamePad.PrevGPS = prevGPS[ActiveGamepad - 1];

		// Update states for actions
		Actions.CurrentKS = currentKS;
		Actions.PrevKS = prevKS;
		Actions.CurrentMS = currentMS;
		Actions.PrevMS = prevMS;
		Actions.CurrentGPS = currentGPS[ActiveGamepad - 1];
		Actions.PrevGPS = prevGPS[ActiveGamepad - 1];

		// Update Keyboard events
		Keyboard.UpdateEvents();

		// Update Mouse events
		Mouse.UpdateEvents();

		// Update GamePad events
		GamePad.UpdateEvents();

		// Update Action events
		Actions.UpdateEvents();
	}

	public static bool GamePadEquals(GamePadState left, GamePadState right) {
		return left.Buttons == right.Buttons && left.DPad == right.DPad && left.ThumbSticks == right.ThumbSticks && left.Triggers == right.Triggers;
	}
}
