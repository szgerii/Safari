using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Engine.Input;

/// <summary>
/// Represents an action with a set of keys, mouse buttons and gamepad buttons.
/// An action is considered pressed, when any of its keys, mouse buttons or gamepad buttons is pressed.
/// </summary>
public class ActionSegment {
	internal List<Keys> keys;
	internal List<MouseButtons> mouseButtons;
	internal List<Buttons> buttons;

	public ActionSegment(Keys[] keys = null, MouseButtons[] mouseButtons = null, Buttons[] buttons = null) {
		this.keys = new List<Keys>();
		this.mouseButtons = new List<MouseButtons>();
		this.buttons = new List<Buttons>();
		if (keys != null) {
			this.keys.AddRange(keys);
		}
		if (mouseButtons != null) {
			this.mouseButtons.AddRange(mouseButtons);
		}
		if (buttons != null) {
			this.buttons.AddRange(buttons);
		}
	}
}

/// <summary>
/// Represents a complex action with a set of actions.
/// A complex action is considered pressed if ALL of its actions are pressed.
/// </summary>
public class InputAction {
	internal List<ActionSegment> actions;

	public InputAction(ActionSegment[] actions = null) {
		this.actions = new List<ActionSegment>();
		if (actions != null) {
			this.actions.AddRange(actions);
		}
	}

	public InputAction(Keys[] keys = null, MouseButtons[] mouseButtons = null, Buttons[] buttons = null) {
		actions = new List<ActionSegment> {
			new ActionSegment(keys, mouseButtons, buttons)
		};
	}
}

/// <summary>
/// Manages all action checks and events. Actions are composed of action segments, which are bundles of Keyboard keys, Mouse buttons and gamepad buttons
/// </summary>
public class Actions {
	public KeyboardState PrevKS { get; set; }
	public KeyboardState CurrentKS { get; set; }
	public MouseState PrevMS { get; set; }
	public MouseState CurrentMS { get; set; }
	public GamePadState PrevGPS { get; set; }
	public GamePadState CurrentGPS { get; set; }
	public bool LockToActiveDevice { get; set; } = true;

	private Dictionary<string, DateTime> downTimeouts = new Dictionary<string, DateTime>();
	private Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();

	public delegate void PressedCallback();
	public delegate void ReleasedCallback();

	private Dictionary<string, PressedCallback> pressedCallbacks = new Dictionary<string, PressedCallback>();
	private Dictionary<string, ReleasedCallback> releasedCallbacks = new Dictionary<string, ReleasedCallback>();

	internal void UpdateEvents() {
		foreach (string name in pressedCallbacks.Keys) {
			if (JustPressed(name)) {
				pressedCallbacks[name]();
			}
		}

		foreach (string name in releasedCallbacks.Keys) {
			if (JustReleased(name)) {
				releasedCallbacks[name]();
			}
		}
	}

	/// <summary>
	/// Registers an action with a given name. An action can contain a number of action segments
	/// An Action is considered pressed, if ALL of its segments are pressed
	/// </summary>
	public void Register(string name, InputAction cAction) {
		if (!actions.ContainsKey(name)) {
			actions.Add(name, cAction);
		}
	}

	/// <summary>
	/// Deletes an action with a given name
	/// </summary>
	public void Remove(string name) {
		if (actions.ContainsKey(name)) {
			actions.Remove(name);
		}
	}

	/// <summary>
	/// Checks whether an action with a given name is down in the current frame
	/// </summary>
	public bool IsDown(string name)
		=> IsDown(CurrentKS, CurrentMS, CurrentGPS, actions[name]);

	/// <summary>
	/// Checks whether an action with a given name is up in the current frame
	/// </summary>
	public bool IsUp(string name)
		=> !IsDown(CurrentKS, CurrentMS, CurrentGPS, actions[name]);


	/// <summary>
	/// Checks whether an action with a given name was just pressed in this frame,
	/// meaning it wasn't down in the previous frame, but now is
	/// </summary>
	public bool JustPressed(string name)
		=> IsDown(CurrentKS, CurrentMS, CurrentGPS, actions[name]) && !IsDown(PrevKS, PrevMS, PrevGPS, actions[name]);

	/// <summary>
	/// Checks whether an action with a given name was just released in this frame,
	/// meaning it was down in the previous frame, but now isn't
	/// </summary>
	public bool JustReleased(string name)
		=> !IsDown(CurrentKS, CurrentMS, CurrentGPS, actions[name]) && IsDown(PrevKS, PrevMS, PrevGPS, actions[name]);

	/// <summary>
	/// Checks whether an action with a given name is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given timeout has passed.
	/// </summary>
	/// <param name="timeout">The minimum amount of time that has to pass before a true value for this action can be returned again.</param>
	public bool TimedIsDown(string name, TimeSpan timeout) {
		DateTime now = DateTime.Now;
		if ((!downTimeouts.ContainsKey(name) || now >= downTimeouts[name]) && IsDown(name)) {
			downTimeouts[name] = now + timeout;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks whether an action with a given name is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given number of milliseconds has passed.
	/// </summary>
	/// <param name="timeout">The minimum number of milliseconds that has to pass before a true value for this action can be returned again.</param>
	public bool TimedIsDown(string name, int milliseconds = 200)
		=> TimedIsDown(name, TimeSpan.FromMilliseconds(milliseconds));

	/// <summary>
	/// Registers a callback for the pressed event of a complex action with a given name
	/// </summary>
	public void OnPressed(string name, PressedCallback callback) {
		if (pressedCallbacks.ContainsKey(name)) {
			pressedCallbacks[name] += callback;
		} else {
			pressedCallbacks.Add(name, callback);
		}
	}

	/// <summary>
	/// Registers a callback for the released event of a complex action with a given name
	/// </summary>
	public void OnReleased(string name, ReleasedCallback callback) {
		if (releasedCallbacks.ContainsKey(name)) {
			releasedCallbacks[name] += callback;
		} else {
			releasedCallbacks.Add(name, callback);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the pressed event of a complex action with a given name
	/// </summary>
	public void ClearPressedCallbacks(string name) {
		if (pressedCallbacks.ContainsKey(name)) {
			pressedCallbacks.Remove(name);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the released event of a complex action with a given name
	/// </summary>
	public void ClearReleasedCallbacks(string name) {
		if (releasedCallbacks.ContainsKey(name)) {
			releasedCallbacks.Remove(name);
		}
	}

	internal bool IsDown(KeyboardState ks, MouseState ms, GamePadState gps, ActionSegment action) {
		if (InputManager.ActiveDevice == ActiveDevice.KeyboardMouse || !LockToActiveDevice) {
			foreach (Keys key in action.keys) {
				if (ks.IsKeyDown(key)) {
					return true;
				}
			}
		}
		if (InputManager.ActiveDevice == ActiveDevice.KeyboardMouse || !LockToActiveDevice) {
			foreach (MouseButtons mouseButton in action.mouseButtons) {
				if (InputManager.Mouse.IsDown(ms, mouseButton)) {
					return true;
				}
			}
		}
		if (InputManager.ActiveDevice == ActiveDevice.Gamepad || !LockToActiveDevice) {
			foreach (Buttons button in action.buttons) {
				if (gps.IsButtonDown(button)) {
					return true;
				}
			}
		}
		return false;
	}

	internal bool IsDown(KeyboardState ks, MouseState ms, GamePadState gps, InputAction cAction) {
		foreach (ActionSegment action in cAction.actions) {
			if (!IsDown(ks, ms, gps, action)) return false;
		}
		return true;
	}
}
