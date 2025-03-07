using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Engine.Input;

public enum GamePadAxes {
	LeftStickX,
	LeftStickY,
	RightStickX,
	RightStickY,
	LeftTrigger,
	RightTrigger
}

public class GamePadAxisChangedEventArgs : EventArgs {
	public float OldValue { get; set; }
	public float NewValue { get; set; }
	public float Delta { get; set; }

	public GamePadAxisChangedEventArgs(float oldValue, float newValue, float delta) {
		OldValue = oldValue;
		NewValue = newValue;
		Delta = delta;
	}
}

/// <summary>
/// Manages all gamepad checks and events (currently gamepad#1 only), including connection status, buttons and axis
/// </summary>
public class GamePad {
	public GamePadState PrevGPS;
	public GamePadState CurrentGPS;

	private Dictionary<Buttons, DateTime> downTimeouts = new Dictionary<Buttons, DateTime>();

	public delegate void PressedCallback();
	public delegate void ReleasedCallback();
	public delegate void AxisChangedCallback(GamePadAxisChangedEventArgs e);
	public delegate void ConnectedCallback();
	public delegate void DisconnectedCallback();

	private Dictionary<Buttons, PressedCallback> pressedCallbacks = new Dictionary<Buttons, PressedCallback>();
	private Dictionary<Buttons, ReleasedCallback> releasedCallbacks = new Dictionary<Buttons, ReleasedCallback>();
	private Dictionary<GamePadAxes, AxisChangedCallback> axisChangedCallbacks = new Dictionary<GamePadAxes, AxisChangedCallback>();
	private ConnectedCallback connectedCallback = null;
	private DisconnectedCallback disconnectedCallback = null;

	internal void UpdateEvents() {
		foreach (Buttons button in pressedCallbacks.Keys) {
			if (JustPressed(button)) {
				pressedCallbacks[button]();
			}
		}

		foreach (Buttons button in releasedCallbacks.Keys) {
			if (JustReleased(button)) {
				releasedCallbacks[button]();
			}
		}

		foreach (GamePadAxes axis in axisChangedCallbacks.Keys) {
			if (AxisChanged(axis)) {
				axisChangedCallbacks[axis](new GamePadAxisChangedEventArgs(AxisValue(PrevGPS, axis), AxisValue(CurrentGPS, axis), AxisMovement(axis)));
			}
		}

		if (JustConnected && connectedCallback != null) {
			connectedCallback();
		}

		if (JustDisconnected && disconnectedCallback != null) {
			disconnectedCallback();
		}
	}

	/// <summary>
	/// Checks whether the given gamepad button is down in the current frame
	/// </summary>
	public bool IsDown(Buttons button) => CurrentGPS.IsButtonDown(button);

	/// <summary>
	/// Checks whether the given gamepad button is up in the current frame
	/// </summary>
	public bool IsUp(Buttons button) => CurrentGPS.IsButtonUp(button);

	/// <summary>
	/// Checks whether the given gamepad button was just pressed in this frame,
	/// meaning it wasn't down in the previous frame, but now is
	/// </summary>
	public bool JustPressed(Buttons button)
		=> CurrentGPS.IsButtonDown(button) && !PrevGPS.IsButtonDown(button);


	/// <summary>
	/// Checks whether the given gamepad button was just released in this frame,
	/// meaning it was down in the previous frame, but now isn't
	/// </summary>
	public bool JustReleased(Buttons button)
		=> CurrentGPS.IsButtonUp(button) && !PrevGPS.IsButtonUp(button);

	/// <summary>
	/// Checks whether a given gamepad button is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given timeout has passed.
	/// </summary>
	/// <param name="timeout">The minimum amount of time that has to pass before a true value for this gamepad button can be returned again.</param>
	public bool TimedIsDown(Buttons button, TimeSpan timeout) {
		DateTime now = DateTime.Now;
		if ((!downTimeouts.ContainsKey(button) || now >= downTimeouts[button]) && IsDown(button)) {
			downTimeouts[button] = now + timeout;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks whether a given gamepad button is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given number of milliseconds has passed.
	/// </summary>
	/// <param name="timeout">The minimum number of milliseconds that has to pass before a true value for this gamepad button can be returned again.</param>
	public bool TimedIsDown(Buttons button, int milliseconds = 200)
		=> TimedIsDown(button, TimeSpan.FromMilliseconds(milliseconds));

	/// <summary>
	/// Registers a callback for the pressed event of a given gamepad button
	/// </summary>
	public void OnPressed(Buttons button, PressedCallback callback) {
		if (pressedCallbacks.ContainsKey(button)) {
			pressedCallbacks[button] += callback;
		} else {
			pressedCallbacks.Add(button, callback);
		}
	}

	/// <summary>
	/// Registers a callback for the released event of a given gamepad button
	/// </summary>
	public void OnReleased(Buttons button, ReleasedCallback callback) {
		if (releasedCallbacks.ContainsKey(button)) {
			releasedCallbacks[button] += callback;
		} else {
			releasedCallbacks.Add(button, callback);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the pressed event of a given gamepad button
	/// </summary>
	public void ClearPressedCallbacks(Buttons button) {
		if (pressedCallbacks.ContainsKey(button)) {
			pressedCallbacks.Remove(button);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the released event of a given gamepad button
	/// </summary>s
	public void ClearReleasedCallbacks(Buttons button) {
		if (releasedCallbacks.ContainsKey(button)) {
			releasedCallbacks.Remove(button);
		}
	}

	/// <summary>
	/// Returns the current value of a given gamepad axis
	/// </summary>
	public float AxisValue(GamePadAxes axis) => AxisValue(CurrentGPS, axis);

	/// <summary>
	/// Checks whether a given gamepad axis has just changed, 
	/// meaning its value in this frame is different from it in the previous frame
	/// </summary>
	public bool AxisChanged(GamePadAxes axis)
		=> AxisValue(CurrentGPS, axis) != AxisValue(PrevGPS, axis);

	/// <summary>
	/// Returns how much a given gamepad axis has changed since the previous frame
	/// </summary>
	public float AxisMovement(GamePadAxes axis)
		=> AxisValue(CurrentGPS, axis) - AxisValue(PrevGPS, axis);

	/// <summary>
	/// Checks whether the gamepad is connected
	/// </summary>
	public bool Connected => CurrentGPS.IsConnected;

	/// <summary>
	/// Checks whether the gamepad is disconnected
	/// </summary>
	public bool Disconnected => !CurrentGPS.IsConnected;

	/// <summary>
	/// Checks whether the gamepad has just been connected, 
	/// meaning it wasn't connected in the previous frame, but now it is
	/// </summary>
	public bool JustConnected => CurrentGPS.IsConnected && !PrevGPS.IsConnected;

	/// <summary>
	/// Checks whether the gamepad has just been disconnected, 
	/// meaning it was connected in the previous frame, but now it isn't
	/// </summary>
	public bool JustDisconnected => !CurrentGPS.IsConnected && PrevGPS.IsConnected;

	/// <summary>
	/// Registers a callback for the value changed event of a given gamepad axis
	/// </summary>
	public void OnAxisChanged(GamePadAxes axis, AxisChangedCallback callback) {
		if (axisChangedCallbacks.ContainsKey(axis)) {
			axisChangedCallbacks[axis] += callback;
		} else {
			axisChangedCallbacks.Add(axis, callback);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the value changed event of a given gamepad axis
	/// </summary>
	public void ClearAxisChangedCallbacks(GamePadAxes axis) {
		if (axisChangedCallbacks.ContainsKey(axis)) {
			axisChangedCallbacks.Remove(axis);
		}
	}

	/// <summary>
	/// Registers a callback for the gamepad connected event
	/// </summary>
	public void OnConnected(ConnectedCallback callback) {
		if (connectedCallback == null) {
			connectedCallback = callback;
		} else {
			connectedCallback += callback;
		}
	}

	/// <summary>
	/// Registers a callback for the gamepad disconnected event
	/// </summary>
	public void OnDisconnected(DisconnectedCallback callback) {
		if (disconnectedCallback == null) {
			disconnectedCallback = callback;
		} else {
			disconnectedCallback += callback;
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the gamepad connected event
	/// </summary>
	public void ClearConnectedCallbacks() {
		connectedCallback = null;
	}

	/// <summary>
	/// Removes all callbacks associated with the gamepad disconnected event
	/// </summary>
	public void ClearDisconnectedCallbacks() {
		disconnectedCallback = null;
	}

	internal float AxisValue(GamePadState state, GamePadAxes axis) {
		switch (axis) {
			case GamePadAxes.LeftStickX: return state.ThumbSticks.Left.X;
			case GamePadAxes.LeftStickY: return state.ThumbSticks.Left.Y;
			case GamePadAxes.RightStickX: return state.ThumbSticks.Right.X;
			case GamePadAxes.RightStickY: return state.ThumbSticks.Right.Y;
			case GamePadAxes.LeftTrigger: return state.Triggers.Left;
			case GamePadAxes.RightTrigger: return state.Triggers.Right;
		}
		return 0;
	}
}
