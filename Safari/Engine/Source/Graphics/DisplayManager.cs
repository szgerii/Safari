using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

	private static Game? Game => Game.Instance;
	private static GraphicsDeviceManager? Graphics => Game.Graphics;
	private static GameWindow? Window => Game?.Window;
	public static List<DisplayMode>? SupportedResolutions { get; private set; }


	// By default: include resolutions between 480p 4:3 and 4k 21:9
	// Change these BEFORE calling DM Init to limit the available resolutions
	public static int MIN_HEIGHT { get; set; } = 480;
	public static int MIN_WIDTH { get; set; } = 640;
	public static int MAX_WIDTH { get; set; } = 5120;
	public static int MAX_HEIGHT { get; set; } = 2160;

	public static void Init(WindowType windowType = WindowType.WINDOWED) {
		SupportedResolutions = Graphics!.GraphicsDevice.Adapter.SupportedDisplayModes
			.Where((dm) => dm.Width >= MIN_WIDTH && dm.Height >= MIN_HEIGHT && dm.Width <= MAX_WIDTH && dm.Height <= MAX_HEIGHT)
			.ToList();
		SetResolution(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode, false);

		VSync = true;
		TargetFPS = 0;
		WindowType = windowType;
		ApplyChanges();
	}

	[MemberNotNull(nameof(Graphics), nameof(Window), nameof(Game), nameof(SupportedResolutions))]
	private static void AssertInit() {
		if (Graphics == null || Window == null || Game == null || SupportedResolutions == null) {
			throw new InvalidOperationException("Cannot use this method of the display manager before it (and the game instance) have been initialized");
		}
	}

	public static void ApplyChanges() {
		AssertInit();

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
		AssertInit();

		Width = Graphics.PreferredBackBufferWidth;
		Height = Graphics.PreferredBackBufferHeight;
		AspectRatio = (float)Width / Height;
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
		AssertInit();

		foreach (DisplayMode res in SupportedResolutions) {
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
		AssertInit();

		if (WindowType == WindowType.WINDOWED) {
			Width = width;
			Height = height;
			AspectRatio = (float)width / height;
			
			if (apply) {
				ApplyChanges();
			}

			return;
		}
		
		foreach (DisplayMode mode in SupportedResolutions) {
			if (mode.Width == width && mode.Height == height) {
				SetResolution(mode, apply);
				return;
			}
		}

		throw new InvalidOperationException($"Trying to set display resolution to an unsupported value ({width}x{height})");
	}

	public static void IncreaseResolution() {
		AssertInit();

		for (int i = 0; i < SupportedResolutions.Count - 1; i++) {
			if (SupportedResolutions[i].Width == Width && SupportedResolutions[i].Height == Height) {
				SetResolution(SupportedResolutions[i + 1]);
				break;
			}
		}
	}

	public static void DecreaseResolution() {
		AssertInit();

		for (int i = SupportedResolutions.Count; i >= 1; i--) {
			if (SupportedResolutions[i].Width == Width && SupportedResolutions[i].Height == Height) {
				SetResolution(SupportedResolutions[i - 1]);
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

	public static void SetTargetFPS(int fps, bool apply = true) {
		TargetFPS = fps;
		if (apply) {
			ApplyChanges();
		}
	}

	public static void SetVSync(bool useVSync, bool apply = true) {
		VSync = useVSync;
		if (apply) {
			ApplyChanges();
		}
	}
}