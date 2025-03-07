using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Engine.Input;

/// <summary>
/// Manages all keyboard checks and events
/// </summary>
public class Keyboard {
	public KeyboardState PrevKS { get; set; }
	public KeyboardState CurrentKS { get; set; }

	private Dictionary<Keys, DateTime> downTimeouts = new Dictionary<Keys, DateTime>();

	public delegate void PressedCallback();
	public delegate void ReleasedCallback();

	private Dictionary<Keys, PressedCallback> pressedCallbacks = new Dictionary<Keys, PressedCallback>();
	private Dictionary<Keys, ReleasedCallback> releasedCallbacks = new Dictionary<Keys, ReleasedCallback>();

	internal void UpdateEvents() {
		foreach (Keys key in pressedCallbacks.Keys) {
			if (JustPressed(key)) {
				pressedCallbacks[key]();
			}
		}

		foreach (Keys key in releasedCallbacks.Keys) {
			if (JustReleased(key)) {
				releasedCallbacks[key]();
			}
		}
	}

	/// <summary>
	/// Checks whether the given key is down in the current frame
	/// </summary>
	public bool IsDown(Keys key) => CurrentKS.IsKeyDown(key);

	/// <summary>
	/// Checks whether the given key is up in the current frame
	/// </summary>
	public bool IsUp(Keys key) => !IsDown(key);

	/// <summary>
	/// Checks whether the given key was just pressed in this frame,
	/// meaning it wasn't down in the previous frame, but now is
	/// </summary>
	public bool JustPressed(Keys key) => CurrentKS.IsKeyDown(key) && !PrevKS.IsKeyDown(key);

	/// <summary>
	/// Checks whether the given key was just released in this frame,
	/// meaning it was down in the previous frame, but now isn't
	/// </summary>
	public bool JustReleased(Keys key) => CurrentKS.IsKeyUp(key) && !PrevKS.IsKeyUp(key);

	/// <summary>
	/// Checks whether a given key is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given timeout has passed.
	/// </summary>
	/// <param name="timeout">The minimum amount of time that has to pass before a true value for this key can be returned again.</param>
	public bool TimedIsDown(Keys key, TimeSpan timeout) {
		DateTime now = DateTime.Now;
		if ((!downTimeouts.ContainsKey(key) || now >= downTimeouts[key]) && IsDown(key)) {
			downTimeouts[key] = now + timeout;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks whether a given key is down, and ensures that, using this function, 
	/// a true value can't be returned again, until the given number of milliseconds has passed.
	/// </summary>
	/// <param name="timeout">The minimum number of milliseconds that has to pass before a true value for this key can be returned again.</param>
	public bool TimedIsDown(Keys key, int milliseconds = 200)
		=> TimedIsDown(key, TimeSpan.FromMilliseconds(milliseconds));

	/// <summary>
	/// Registers a callback for the pressed event of a given key
	/// </summary>
	public void OnPressed(Keys key, PressedCallback callback) {
		if (pressedCallbacks.ContainsKey(key)) {
			pressedCallbacks[key] += callback;
		} else {
			pressedCallbacks.Add(key, callback);
		}
	}

	/// <summary>
	/// Registers a callback for the released event of a given key
	/// </summary>
	public void OnReleased(Keys key, ReleasedCallback callback) {
		if (releasedCallbacks.ContainsKey(key)) {
			releasedCallbacks[key] += callback;
		} else {
			releasedCallbacks.Add(key, callback);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the pressed event of a given key
	/// </summary>
	public void ClearPressedCallbacks(Keys key) {
		if (pressedCallbacks.ContainsKey(key)) {
			pressedCallbacks.Remove(key);
		}
	}

	/// <summary>
	/// Removes all callbacks associated with the released event of a given key
	/// </summary>
	public void ClearReleasedCallbacks(Keys key) {
		if (releasedCallbacks.ContainsKey(key)) {
			releasedCallbacks.Remove(key);
		}
	}
}
