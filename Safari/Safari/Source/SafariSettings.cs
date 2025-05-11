using Engine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Safari.Components;
using System;
using System.Collections.Generic;
using System.IO;

namespace Safari;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSettings
{
    private static string path = Path.Join(Game.SafariPath, "settings.json");
	#region RESOLUTION
	public static List<(int, int)> ResolutionOptions { get; private set; } = new();
    public static (int, int) DefaultResolution { get; set; } = (1280, 720);

    private (int, int) resolution;
    [JsonProperty]
    public (int, int) Resolution
    {
        get => resolution;
        set
        {
            if (ResolutionOptions.Contains(value)) {
                resolution = value;
            } else {
                resolution = DefaultResolution;
                WindowType = DefaultWindowType;
            }
        }
    }
    #endregion

    #region FPS
    public const int FPS_DEFAULT = 0;
    public const int FPS_MIN = 0;
    public const int FPS_MAX = 90;

    private int fps;
    /// <summary>
    /// Target fps, here 0 means unlimited
    /// </summary>
    [JsonProperty]
    public int Fps
    {
        get => fps;
        set
        {
            if (value < FPS_MIN || value > FPS_MAX)
            {
                fps = FPS_DEFAULT;
            }
            else
            {
                fps = value;
            }
        }
    }
    #endregion

    #region VSYNC
    public const bool VSYNC_DEFAULT = true;
    /// <summary>
    /// Whether vsync is turned on / off
    /// (vsync: auomatically match framerate to the screens refresh rate)
    /// </summary>
    [JsonProperty]
    public bool VSync { get; set; }
    #endregion

    #region WINDOW_TYPE
    public static WindowType DefaultWindowType { get; set; }

    /// <summary>
    /// The type of the window (borderless, fullscreen or windowed)
    /// </summary>
    [JsonProperty]
    public WindowType WindowType { get; set; }
    #endregion

    #region CAMERA_SPEED
    public const float CAMERA_SPEED_DEFAULT = 200f;
    public const float CAMERA_SPEED_MIN = 50f;
    public const float CAMERA_SPEED_MAX = 400f;

    private float cameraSpeed;
    /// <summary>
    /// The speed of the main camera
    /// </summary>
    [JsonProperty]
    public float CameraSpeed {
        get => cameraSpeed;
        set {
            if (value < CAMERA_SPEED_MIN || value > CAMERA_SPEED_MAX) {
                cameraSpeed = CAMERA_SPEED_DEFAULT;
            } else {
                cameraSpeed = value;
            }
        }
    }

	public const float CAMERA_ACCEL_DEFAULT = 23f;
	public const float CAMERA_ACCEL_MIN = 0f;
	public const float CAMERA_ACCEL_MAX = 100f;

	private float cameraAcceleration;
    /// <summary>
    ///  The acceleration of the main camera
    /// </summary>
    [JsonProperty]
    public float CameraAcceleration {
        get => cameraAcceleration;
        set {
            if (value < CAMERA_ACCEL_MIN || value > CAMERA_ACCEL_MAX) {
                cameraAcceleration = CAMERA_ACCEL_DEFAULT;
            } else {
                cameraAcceleration = value;
            }
        }
    }
    #endregion

    public static void Init() {
		foreach (DisplayMode dm in DisplayManager.SupportedResolutions) {
            ResolutionOptions.Add((dm.Width, dm.Height));
		}
#if DEBUG
        DefaultResolution = (1280, 720);
        DefaultWindowType = WindowType.WINDOWED;
#else
        DisplayMode native = Game.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode;
        DefaultResolution = DisplayManager.IsSupported(native.Width, native.Height) ? (native.Width, native.Height) : (1920, 1080);
        DefaultWindowType = WindowType.FULL_SCREEN;
#endif
        try {
            using (StreamReader sr = new StreamReader(path)) {
                Instance = JsonConvert.DeserializeObject<SafariSettings>(sr.ReadToEnd());
            }
        } catch {
            Instance = new SafariSettings();
        }
        Instance.Apply();
	}


    public static SafariSettings? Instance { get; private set; }

	[JsonConstructor]
    private SafariSettings() {
        Resolution = DefaultResolution;
        WindowType = DefaultWindowType;
        Fps = FPS_DEFAULT;
        VSync = VSYNC_DEFAULT;
        CameraSpeed = CAMERA_SPEED_DEFAULT;
        CameraAcceleration = CAMERA_ACCEL_DEFAULT;
    }

    public void Apply() {
        DisplayManager.SetResolution(resolution.Item1, resolution.Item2, false);
        DisplayManager.SetTargetFPS(fps, false);
        DisplayManager.SetVSync(VSync, false);
        DisplayManager.SetWindowType(WindowType, false);
		CameraControllerCmp.DefaultScrollSpeed = cameraSpeed;
		CameraControllerCmp.DefaultAcceleration = cameraAcceleration;

        DisplayManager.ApplyChanges();

        using (StreamWriter sw = new StreamWriter(path)) {
            sw.WriteLine(JsonConvert.SerializeObject(this));
        }
	}
}
