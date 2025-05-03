using Engine.Graphics;
using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using Safari.Helpers;
using System;
using System.Collections.Generic;

namespace Safari.Scenes.Menus;

public class SettingsMenu : MenuScene, IResettableSingleton {
	private static SettingsMenu instance;
	public static SettingsMenu Instance {
		get {
			instance ??= new();
			return instance;
		}
	}
	public static void ResetSingleton() {
        instance?.Unload();
        instance = null;
	}

	private Header title;
    private Panel settingsPanel;

    private Panel fpsPanel;
    private Label fpsText;
    private Slider fpsSlider;

    private Panel vsyncPanel;
    private Label vsyncText;
    private Button vsyncButton;

    private Panel screenTypePanel;
    private Label screenTypeText;
    private Panel screenTypeButtonPanel;
    private Button screenTypeWindowed;
    private Button screenTypeBorderless;
    private Button screenTypeFullscreen;

    private Panel cameraSpeedPanel;
    private Label cameraSpeedText;
    private Slider cameraSpeedSlider;
    private float cameraStoredValue;

    private Panel resolutionPanel;
    private Label resolutionText;
    private Panel resolutionChangePanel;
    private Label resolutionsDisplay;
    private Button prevResolution;
    private Button nextResolution;
    private List<(int, int)> resolutions;
    private (int, int) selectedResolution;
    private int currentResolution;

    private Panel buttonPanel;
    private Button saveChangesButton;
    private Button menuAndDiscardButton;

    private static float scale = ((float)DisplayManager.Height / 1080f) * 1.2f;
    public static event EventHandler ScaleChanged;

    /// <summary>
    /// Get the scaling for text that matches the resolution.
    /// </summary>
    public static float Scale {
        get {
            return scale == 0 ? 1f : scale;
        }
        private set {
            scale = value;
            ScaleChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0, 0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(title);

        settingsPanel = new Panel(new Vector2(0, 0.8f), PanelSkin.None, Anchor.TopCenter);

        #region FPS
        //FPS Settings
        fpsPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        fpsPanel.Padding = new Vector2(0);

        fpsText = new Label("Frame rate:", Anchor.CenterLeft, new Vector2(0.5f, -1));
        fpsText.Padding = new Vector2(10);

        fpsSlider = new Slider(30, 91, new Vector2(0.5f, 0.4f), SliderSkin.Default, Anchor.CenterRight);
        fpsSlider.Value = DisplayManager.TargetFPS == 0 ? 91 : DisplayManager.TargetFPS;

        fpsText.Text = "Frame rate: " + ((fpsSlider.Value == 91) ? "Unlimited" : fpsSlider.Value);

        fpsSlider.OnValueChange = (Entity entity) => {
            fpsText.Text = "Frame rate: " + ((fpsSlider.Value == 91) ? "Unlimited" : fpsSlider.Value);
            DisplayManager.SetTargetFPS(fpsSlider.Value == 91 ? 0 : fpsSlider.Value, false);
        };

        fpsPanel.AddChild(fpsText);
        fpsPanel.AddChild(fpsSlider);
        settingsPanel.AddChild(fpsPanel);
        #endregion

        #region VSYNC
        //VSYNC Settings
        vsyncPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);

        vsyncPanel.Padding = new Vector2(0);

        vsyncText = new Label("VSync:", Anchor.CenterLeft, new Vector2(0.5f, -1));
        vsyncText.Padding = new Vector2(10);

        vsyncButton = new Button("", ButtonSkin.Default, Anchor.CenterRight, new Vector2(150, 50));
        vsyncButton.ToggleMode = true;
        vsyncButton.Checked = DisplayManager.VSync;
        vsyncButton.ButtonParagraph.Text = vsyncButton.Checked ? "VSync ON" : "VSync OFF";
        vsyncButton.Padding = new Vector2(0);
        vsyncButton.OnValueChange = (Entity entity) => {
            vsyncButton.ButtonParagraph.Text = vsyncButton.Checked ? "VSync ON" : "VSync OFF";
            DisplayManager.SetVSync(vsyncButton.Checked, false);
        };

        vsyncPanel.AddChild(vsyncText);
        vsyncPanel.AddChild(vsyncButton);
        settingsPanel.AddChild(vsyncPanel);
        #endregion

        #region SCREEN_TYPE
        //SCREEN TYPE Settings
        screenTypePanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        screenTypePanel.Padding = new Vector2(0);

        screenTypeText = new Label("Window mode: ", Anchor.CenterLeft, new Vector2(0.5f, -1));
        screenTypeText.Padding = new Vector2(10);

        screenTypeButtonPanel = new Panel(new Vector2(0.75f, 0), PanelSkin.None, Anchor.CenterRight);
        screenTypeButtonPanel.Padding = new Vector2(0, 0.25f);

        screenTypeWindowed = new Button("Windowed", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.3f, 0.5f));
        screenTypeWindowed.Padding = new Vector2(0);
        screenTypeWindowed.ToggleMode = true;
        screenTypeWindowed.OnClick = (Entity entity) => {
            screenTypeWindowed.Checked = true;
            screenTypeBorderless.Checked = false;
            screenTypeFullscreen.Checked = false;
            DisplayManager.SetWindowType(WindowType.WINDOWED, false);
        };
        screenTypeButtonPanel.AddChild(screenTypeWindowed);

        screenTypeBorderless = new Button("Borderless", ButtonSkin.Default, Anchor.Center, new Vector2(0.3f, 0.5f));
        screenTypeBorderless.Padding = new Vector2(0);
        screenTypeBorderless.ToggleMode = true;
        screenTypeBorderless.OnClick = (Entity entity) => {
            screenTypeWindowed.Checked = false;
            screenTypeBorderless.Checked = true;
            screenTypeFullscreen.Checked = false;
            DisplayManager.SetWindowType(WindowType.BORDERLESS, false);
        };
        screenTypeButtonPanel.AddChild(screenTypeBorderless);

        screenTypeFullscreen = new Button("Fullscreen", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0.5f));
        screenTypeFullscreen.Padding = new Vector2(0);
        screenTypeFullscreen.ToggleMode = true;
        screenTypeFullscreen.OnClick = (Entity entity) => {
            screenTypeWindowed.Checked = false;
            screenTypeBorderless.Checked = false;
            screenTypeFullscreen.Checked = true;
            DisplayManager.SetWindowType(WindowType.FULL_SCREEN, false);
        };

        switch (DisplayManager.WindowType) {
            case WindowType.WINDOWED: screenTypeWindowed.Checked = true; break;
            case WindowType.BORDERLESS: screenTypeBorderless.Checked = true; break;
            case WindowType.FULL_SCREEN: screenTypeFullscreen.Checked = true; break;
            default: screenTypeWindowed.Checked = true; break;

        }
        screenTypeButtonPanel.AddChild(screenTypeFullscreen);
        screenTypePanel.AddChild(screenTypeText);
        screenTypePanel.AddChild(screenTypeButtonPanel);
        settingsPanel.AddChild(screenTypePanel);
        #endregion

        #region CAMERA_SPEED
        //CAMERA SPEED Settings
        cameraSpeedPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        cameraSpeedPanel.Padding = new Vector2(0);

        cameraSpeedText = new Label("Camera speed: ", Anchor.CenterLeft, new Vector2(0.5f, -1));
        cameraSpeedText.Padding = new Vector2(10);

        cameraStoredValue = CameraControllerCmp.DefaultScrollSpeed;

        cameraSpeedSlider = new Slider(50, 300, new Vector2(0.5f, 0.4f), SliderSkin.Default, Anchor.CenterRight);
        cameraSpeedSlider.Value = (int)CameraControllerCmp.DefaultScrollSpeed;
        cameraSpeedText.Text = "Camera speed: " + (float)cameraSpeedSlider.Value / 100f;
        cameraSpeedSlider.OnValueChange = (Entity entity) => {
            cameraSpeedText.Text = "Camera speed: " + (float)cameraSpeedSlider.Value / 100f;
            cameraStoredValue = (float)cameraSpeedSlider.Value;
        };

        cameraSpeedPanel.AddChild(cameraSpeedText);
        cameraSpeedPanel.AddChild(cameraSpeedSlider);
        settingsPanel.AddChild(cameraSpeedPanel);
        #endregion

        #region RESOLUTION
        //RESOLUTION Settings
        resolutionPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        resolutionPanel.Padding = new Vector2(0);

        resolutionText = new Label("Resolution: ", Anchor.CenterLeft, new Vector2(0.5f, -1));
        resolutionText.Padding = new Vector2(10);

        resolutions = new List<(int, int)>();
        bool addRest = false;
        foreach (DisplayMode item in DisplayManager.SupportedResolutions) {
            if (item.Width == 1280 && item.Height == 720) {
                addRest = true;
            }
            if (item.Width == 1920 && item.Height == 1080) {
                addRest = false;
                resolutions.Add((item.Width, item.Height));
            }
            if (addRest) {
                resolutions.Add((item.Width, item.Height));
            }
        }

        resolutionChangePanel = new Panel(new Vector2(0.5f, 0), PanelSkin.None, Anchor.CenterRight);
        currentResolution = resolutions.FindIndex(x => x == (DisplayManager.Width, DisplayManager.Height));
        selectedResolution = resolutions[currentResolution];
        resolutionsDisplay = new Label(resolutions[currentResolution].Item1 + "x" + resolutions[currentResolution].Item2, Anchor.Center, new Vector2(0.5f, 0));

        prevResolution = new Button("-", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.25f, 0));
        prevResolution.Padding = new Vector2(0);
        prevResolution.OnClick = (Entity entity) => {
            if (currentResolution != 0) {
                --currentResolution;
            }
            resolutionsDisplay.Text = resolutions[currentResolution].Item1 + "x" + resolutions[currentResolution].Item2;
            selectedResolution = resolutions[currentResolution];
            DisplayManager.SetResolution(resolutions[currentResolution].Item1, resolutions[currentResolution].Item2, false);
        };

        nextResolution = new Button("+", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.25f, 0));
        nextResolution.Padding = new Vector2(0);
        nextResolution.OnClick = (Entity entity) => {
            if (currentResolution != resolutions.Count - 1) {
                ++currentResolution;
            }
            resolutionsDisplay.Text = resolutions[currentResolution].Item1 + "x" + resolutions[currentResolution].Item2;
            selectedResolution = resolutions[currentResolution];
            DisplayManager.SetResolution(resolutions[currentResolution].Item1, resolutions[currentResolution].Item2, false);
        };

        resolutionChangePanel.AddChild(prevResolution);
        resolutionChangePanel.AddChild(resolutionsDisplay);
        resolutionChangePanel.AddChild(nextResolution);

        resolutionPanel.AddChild(resolutionText);
        resolutionPanel.AddChild(resolutionChangePanel);
        settingsPanel.AddChild(resolutionPanel);
        #endregion

        //button setup
        buttonPanel = new Panel(new Vector2(0.5f, 0.1f), PanelSkin.None, Anchor.BottomRight);

        menuAndDiscardButton = new Button("Exit & Discard", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.55f, -1));
        menuAndDiscardButton.Padding = new Vector2(10);
        menuAndDiscardButton.OnClick = MenuAndDiscardButtonClicked;

        saveChangesButton = new Button("Save", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, -1));
        saveChangesButton.Padding = new Vector2(10);
        saveChangesButton.OnClick = SaveChangesButtonClicked;

        buttonPanel.AddChild(menuAndDiscardButton);
        buttonPanel.AddChild(saveChangesButton);

        panel.AddChild(settingsPanel);
        panel.AddChild(buttonPanel);

        ScaleText();
    }

    private void SaveChangesButtonClicked(Entity entity) {
        CameraControllerCmp.DefaultScrollSpeed = cameraStoredValue;
        ScaleText();
        DisplayManager.ApplyChanges();
    }

    private void MenuAndDiscardButtonClicked(Entity entity) {
        cameraStoredValue = CameraControllerCmp.DefaultScrollSpeed;
        DisplayManager.DiscardChanges();
        SceneManager.Load(MainMenu.Instance);
    }

    private void ScaleText() {
        Scale = ((float)selectedResolution.Item2 / 1080f) * 1.2f;
        fpsText.Scale = Scale;
        vsyncText.Scale = Scale;
        screenTypeText.Scale = Scale;
        cameraSpeedText.Scale = Scale;
        resolutionText.Scale = Scale;
        resolutionsDisplay.Scale = Scale;

        prevResolution.ButtonParagraph.Scale = Scale;
        nextResolution.ButtonParagraph.Scale = Scale;
    }

    protected override void DestroyUI() {
        panel = null;
        title = null;
        settingsPanel = null;
        fpsPanel = null;
        fpsText = null;
        fpsSlider = null;
        vsyncPanel = null;
        vsyncText = null;
        vsyncButton = null;
        screenTypePanel = null;
        screenTypeText = null;
        screenTypeButtonPanel = null;
        screenTypeWindowed = null;
        screenTypeBorderless = null;
        screenTypeFullscreen = null;
        cameraSpeedPanel = null;
        cameraSpeedText = null;
        cameraSpeedSlider = null;
        cameraStoredValue = CameraControllerCmp.DefaultScrollSpeed;
        resolutionPanel = null;
        resolutionText = null;
        resolutionChangePanel = null;
        resolutionsDisplay = null;
        prevResolution = null;
        nextResolution = null;
        resolutions = null;
        buttonPanel = null;
        menuAndDiscardButton = null;
        saveChangesButton = null;
    }
}
