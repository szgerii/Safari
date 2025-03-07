using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Graphics;

public enum WindowType {
	WINDOWED,
	BORDERLESS,
	FULL_SCREEN
}

public static class DisplayManager {
	public static int Width { get; private set; }
	public static int Height { get; private set; }
	public static float AspectRatio { get; private set; }
	public static bool VSync { get; private set; } = true;
	public static int TargetFPS { get; private set; }
	public static WindowType WindowType { get; private set; } = WindowType.WINDOWED;

	private static Game Game => Game.Instance;
	private static GraphicsDeviceManager Graphics => Game.Graphics;
	private static GameWindow Window => Game.Window;
	private static List<DisplayMode> supportedResolutions;

	public static void Init(WindowType windowType = WindowType.WINDOWED) {
		supportedResolutions = Graphics.GraphicsDevice.Adapter.SupportedDisplayModes.ToList();
		SetResolution(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode, false);

		VSync = true;
		TargetFPS = 0;
		WindowType = windowType;
		ApplyChanges();
	}

	public static void ApplyChanges() {
		Graphics.PreferredBackBufferWidth = Width;
		Graphics.PreferredBackBufferHeight = Height;
		Graphics.SynchronizeWithVerticalRetrace = VSync;

		switch (WindowType) {
			case WindowType.WINDOWED:
				Graphics.HardwareModeSwitch = true;
				Graphics.IsFullScreen = false;
				Window.IsBorderless = false;
				break;

			case WindowType.BORDERLESS:
				Graphics.HardwareModeSwitch = false;
				Graphics.IsFullScreen = false;
				Window.IsBorderless = true;
				break;

			case WindowType.FULL_SCREEN:
				Graphics.HardwareModeSwitch = true;
				Graphics.IsFullScreen = true;
				Window.IsBorderless = false;
				break;
		}

		if (TargetFPS <= 0) {
			Game.IsFixedTimeStep = false;
		} else {
			Game.IsFixedTimeStep = true;
			Game.TargetElapsedTime = TimeSpan.FromSeconds(1f / TargetFPS);
		}

		Graphics.ApplyChanges();
	}

	public static void DiscardChanges() {
		Width = Graphics.PreferredBackBufferWidth;
		Height = Graphics.PreferredBackBufferHeight;
		AspectRatio = Width / Height;
		VSync = Graphics.SynchronizeWithVerticalRetrace;

		if (Graphics.IsFullScreen) {
			WindowType = WindowType.FULL_SCREEN;
		} else if (Window.IsBorderless) {
			WindowType = WindowType.BORDERLESS;
		} else {
			WindowType = WindowType.WINDOWED;
		}

		TargetFPS = Game.IsFixedTimeStep ? Utils.Round(1 / Game.TargetElapsedTime.TotalSeconds) : 0;
	}

	public static bool IsSupported(int width, int height) {
		foreach (DisplayMode res in supportedResolutions) {
			if (res.Width == width && res.Height == height) {
				return true;
			}
		}

		return false;
	}
	
	private static void SetResolution(DisplayMode mode, bool apply = true) {
		Width = mode.Width;
		Height = mode.Height;
		AspectRatio = mode.AspectRatio;

		if (apply) {
			ApplyChanges();
		}
	}

	public static void SetResolution(int width, int height, bool apply = true) {
		if (WindowType == WindowType.WINDOWED) {
			Width = width;
			Height = height;
			AspectRatio = width / height;
			
			if (apply) {
				ApplyChanges();
			}

			return;
		}
		
		foreach (DisplayMode mode in supportedResolutions) {
			if (mode.Width == width && mode.Height == height) {
				SetResolution(mode, apply);
				return;
			}
		}

		throw new InvalidOperationException($"Trying to set display resolution to an unsupported value ({width}x{height})");
	}

	public static void IncreaseResolution() {
		for (int i = 0; i < supportedResolutions.Count - 1; i++) {
			if (supportedResolutions[i].Width == Width && supportedResolutions[i].Height == Height) {
				SetResolution(supportedResolutions[i + 1]);
				break;
			}
		}
	}

	public static void DecreaseResolution() {
		for (int i = supportedResolutions.Count; i >= 1; i--) {
			if (supportedResolutions[i].Width == Width && supportedResolutions[i].Height == Height) {
				SetResolution(supportedResolutions[i - 1]);
				break;
			}
		}
	}

	public static void SetWindowType(WindowType type, bool apply = true) {
		WindowType = type;
        if (apply) {
			ApplyChanges();
        }
    }
}