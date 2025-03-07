using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Engine.Input;

public enum MouseButtons {
	LeftButton,
	RightButton,
	MiddleButton,
	XButton1,
	XButton2
}

public class MouseMovedEventArgs : EventArgs {
	public Vector2 OldPosition { get; set; }
	public Vector2 NewPosition { get; set; }
	public Vector2 Movement { get; set; }

	public MouseMovedEventArgs(Vector2 oldPosition, Vector2 newPosition, Vector2 movement) {
		OldPosition = oldPosition;
		NewPosition = newPosition;
		Movement = movement;
	}
}

public class MouseScrollWheelChangedEventArgs : EventArgs {
	public int OldValue { get; set; }
	public int NewValue { get; set; }
	public int Delta { get; set; }

	public MouseScrollWheelChangedEventArgs(int oldValue, int newValue, int delta) {
		OldValue = oldValue;
		NewValue = newValue;
		Delta = delta;
	}
}

/// <summary>
/// Manages all mouse checks and events, including location, movement, buttons and scroll wheels
/// </summary>
public class Mouse {
	public MouseState PrevMS { get; set; }
	public MouseState CurrentMS { get; set; }

	private Dictionary<MouseButtons, DateTime> downTimeouts = new Dictionary<MouseButtons, DateTime>();

	public delegate void PressedCallback();
	public delegate void ReleasedCallback();
	public delegate void MovedCallback(MouseMovedEventArgs e);
	public delegate void ScrollWheelChangedCallback(MouseScrollWheelChangedEventArgs e);
	public delegate void HScrollWheelChangedCallback(MouseScrollWheelChangedEventArgs e);

	private Dictionary<MouseButtons, PressedCallback> pressedCallbacks = new Dictionary<MouseButtons, PressedCallback>();
	private Dictionary<MouseButtons, ReleasedCallback> releasedCallbacks = new Dictionary<MouseButtons, ReleasedCallback>();
	private MovedCallback movedCallback = null;
	private ScrollWheelChangedCallback scrollWheelChangedCallback = null;
	private HScrollWheelChangedCallback hScrollWheelChangedCallback = null;

	internal void UpdateEvents() {
		foreach (MouseButtons mouseButton in pressedCallbacks.Keys) {
			if (JustPressed(mouseButton)) {
				pressedCallbacks[mouseButton]();
			}
		}

		foreach (MouseButtons mouseButton in releasedCallbacks.Keys) {
			if (JustReleased(mouseButton)) {
				releasedCallbacks[mouseButton]();
			}
		}

		if (JustMoved && movedCallback != null) {
			movedCallback(new MouseMovedEventArgs(PrevMS.Position.ToVector2(), CurrentMS.Position.ToVector2(), Movement));
		}

		if (ScrollChanged && scrollWheelChangedCallback != null) {
			scrollWheelChangedCallback(new MouseScrollWheelChangedEventArgs(PrevMS.ScrollWheelValue, CurrentMS.ScrollWheelValue, ScrollMovement));
		}

		if (HScrollChanged && hScrollWheelChangedCallback != null) {
			hScrollWheelChangedCallback(new MouseScrollWheelChangedEventArgs(PrevMS.HorizontalScrollWheelValue, CurrentMS.HorizontalScrollWheelValue, HScrollMovement));
		}
	}

	/// <summary>
	/// Checks whether the given mouse button is down in the current frame
	/// </summary>
	public bool IsDown(MouseButtons mouseButton) => IsDown(CurrentMS, mouseButton);

	/// <summary>
	/// Checks whether the given mouse button is up in the current frame
	/// </summary>
	public bool IsUp(MouseButtons mouseButton) => !IsDown(CurrentMS, mouseButton);

	/// <summary>
	/// Checks whether the given mouse button was just pressed in this frame,
	/// meaning it wasn't down in the previous frame, but now is
	/// </summary>
	public bool JustPressed(MouseButtons mouseButton)
		=> IsDown(CurrentMS, mouseButton) && !IsDown(PrevMS, mouseButton);

	/// <summary>
	/// Checks whether the given mouse button was just released in this frame,
	/// meaning it was down in the previous frame, but now isn't
	/// </summary>
	public bool JustReleased(MouseButtons mouseButton)
		=> !IsDown(CurrentMS, mouseButton) && IsDown(PrevMS, mouseButton);

	/// <summary>
	/// Checks whether a given mouse button is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given timeout has passed.
	/// </summary>
	/// <param name="timeout">The minimum amount of time that has to pass before a true value for this mouse button can be returned again.</param>
	public bool TimedIsDown(MouseButtons mouseButton, TimeSpan timeout) {
		DateTime now = DateTime.Now;
		if ((!downTimeouts.ContainsKey(mouseButton) || now >= downTimeouts[mouseButton]) && IsDown(mouseButton)) {
			downTimeouts[mouseButton] = now + timeout;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks whether a given mouse button is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given number of milliseconds has passed.
	/// </summary>
	/// <param name="timeout">The minimum number of milliseconds that has to pass before a true value for this mouse button can be returned again.</param>
	public bool TimedIsDown(MouseButtons mouseButton, int milliseconds = 200)
		=> TimedIsDown(mouseButton, TimeSpan.FromMilliseconds(milliseconds));

	/// <summary>
	/// Registers a callback for the pressed event of a given mouse button
	/// </summary>
	public void OnPressed(MouseButtons mouseButton, PressedCallback callback) {
		if (pressedCallbacks.ContainsKey(mouseButton)) {
			pressedCallbacks[mouseButton] += callback;
		} else {
			pressedCallbacks.Add(mouseButton, callback);
		}
	}

	/// <summary>
	/// Registers a callback for the released event of a given mouse button
	/// </summary>
	public void OnReleased(MouseButtons mouseButton, ReleasedCallback callback) {
		if (releasedCallbacks.ContainsKey(mouseButton)) {
			releasedCallbacks[mouseButton] += callback;
		} else {
			releasedCallbacks.Add(mouseButton, callback);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the pressed event of a given mouse button
	/// </summary>
	public void ClearPressedCallbacks(MouseButtons mouseButton) {
		if (pressedCallbacks.ContainsKey(mouseButton)) {
			pressedCallbacks.Remove(mouseButton);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the released event of a given mouse button
	/// </summary>
	public void ClearReleasedCallbacks(MouseButtons mouseButton) {
		if (releasedCallbacks.ContainsKey(mouseButton)) {
			releasedCallbacks.Remove(mouseButton);
		}
	}

	/// <summary>
	/// The current location of the mouse
	/// </summary>
	public Point Location => CurrentMS.Position;

	/// <summary>
	/// Stores whether the mouse has just moved, 
	/// meaning its position in this frame is different from it in the previous frame
	/// </summary>
	public bool JustMoved => CurrentMS.Position != PrevMS.Position;

	/// <summary>
	/// The vector representing the mouse's movement since the previous frame
	/// </summary>
	public Vector2 Movement
		=> new Vector2(CurrentMS.Position.X - PrevMS.Position.X, CurrentMS.Position.Y - PrevMS.Position.Y);

	/// <summary>
	/// The current value of the mouse scroll wheel
	/// </summary>
	public int ScrollValue => CurrentMS.ScrollWheelValue;

	/// <summary>
	/// Stores whether the value of the mouse scroll wheel has changed since the last frame
	/// </summary>
	public bool ScrollChanged => CurrentMS.ScrollWheelValue != PrevMS.ScrollWheelValue;

	/// <summary>
	/// Returns how much the scroll wheel has moved since the previous frame
	/// </summary>
	public int ScrollMovement => CurrentMS.ScrollWheelValue - PrevMS.ScrollWheelValue;

	/// <summary>
	/// Returns the current value of the horizontal mouse scroll wheel
	/// </summary>
	public int HScrollValue => CurrentMS.HorizontalScrollWheelValue;

	/// <summary>
	/// Checks whether the value of the horizontal mouse scroll wheel has changed since the last frame
	/// </summary>
	public bool HScrollChanged => CurrentMS.HorizontalScrollWheelValue != PrevMS.HorizontalScrollWheelValue;

	/// <summary>
	/// Returns how much the horizonal scroll wheel has moved since the previous frame
	/// </summary>
	public int HScrollMovement => CurrentMS.HorizontalScrollWheelValue - PrevMS.HorizontalScrollWheelValue;

	/// <summary>
	/// Registers a callback for the mouse moved event
	/// </summary>
	public void OnMoved(MovedCallback callback) {
		if (movedCallback == null) {
			movedCallback = callback;
		} else {
			movedCallback += callback;
		}
	}

	/// <summary>
	/// Registers a callback for the scroll wheel changed event
	/// </summary>
	public void OnScrollWheelChanged(ScrollWheelChangedCallback callback) {
		if (scrollWheelChangedCallback == null) {
			scrollWheelChangedCallback = callback;
		} else {
			scrollWheelChangedCallback += callback;
		}
	}

	/// <summary>
	/// Registers a callback for the horizontal scroll wheel changed event
	/// </summary>
	public void OnHScrollWheelChanged(HScrollWheelChangedCallback callback) {
		if (hScrollWheelChangedCallback == null) {
			hScrollWheelChangedCallback = callback;
		} else {
			hScrollWheelChangedCallback += callback;
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the mouse moved event
	/// </summary>
	public void ClearMovedCallbacks() {
		movedCallback = null;
	}

	/// <summary>
	/// Removes all callbacks associated with the scroll wheel changed event
	/// </summary>
	public void ClearScrollWheelCallbacks() {
		scrollWheelChangedCallback = null;
	}

	/// <summary>
	/// Removes all callbacks associated with the horizontal scroll wheel changed event
	/// </summary>
	public void ClearHScrollWheelCallbacks() {
		hScrollWheelChangedCallback = null;
	}

	internal bool IsDown(MouseState state, MouseButtons mouseButton) {
		if (
			Location.X < 0 ||
			Location.Y < 0 ||
			Location.X >= Game.Graphics.PreferredBackBufferWidth ||
			Location.Y >= Game.Graphics.PreferredBackBufferHeight) {
			return false;
		}
		if (!Game.Instance.IsActive) {
			return false;
		}
		switch (mouseButton) {
			case MouseButtons.LeftButton: return state.LeftButton == ButtonState.Pressed;
			case MouseButtons.RightButton: return state.RightButton == ButtonState.Pressed;
			case MouseButtons.MiddleButton: return state.MiddleButton == ButtonState.Pressed;
			case MouseButtons.XButton1: return state.XButton1 == ButtonState.Pressed;
			case MouseButtons.XButton2: return state.XButton2 == ButtonState.Pressed;
		}
		return false;
	}
}
