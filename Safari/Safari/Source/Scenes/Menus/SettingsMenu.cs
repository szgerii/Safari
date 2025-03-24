using Engine.Graphics;
using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using System.Collections.Generic;

namespace Safari.Scenes.Menus;

class SettingsMenu : MenuScene {
    private readonly static SettingsMenu instance = new SettingsMenu();
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
    private int currentResolution;

    private Panel buttonPanel;
    private Button menuButton;
    private Button saveChangesButton;
    private Button discardChangesButton;

    public static SettingsMenu Instance => instance;

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0, 0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(title);

        settingsPanel = new Panel(new Vector2(0, 0.8f), PanelSkin.None, Anchor.TopCenter);

        //fps settings setup
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

        //vsync settings setup
        vsyncPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        vsyncPanel.Padding = new Vector2(0);
        vsyncText = new Label("VSync:", Anchor.CenterLeft, new Vector2(0.5f, -1));
        vsyncText.Padding = new Vector2(10);
        vsyncButton = new Button("", ButtonSkin.Default, Anchor.CenterRight, new Vector2(150, 50));
        vsyncButton.ToggleMode = true;
        vsyncButton.Checked = DisplayManager.VSync;
        vsyncButton.ButtonParagraph.Text = vsyncButton.Checked ? "VSync ON" : "VSync OFF";
        vsyncButton.OnValueChange = (Entity entity) => {
            vsyncButton.ButtonParagraph.Text = vsyncButton.Checked ? "VSync ON" : "VSync OFF";
            DisplayManager.SetVSync(vsyncButton.Checked, false);
        };
        vsyncButton.Padding = new Vector2(0);
        vsyncPanel.AddChild(vsyncText);
        vsyncPanel.AddChild(vsyncButton);
        settingsPanel.AddChild(vsyncPanel);

        //screen type settings setup
        screenTypePanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        screenTypePanel.Padding = new Vector2(0);
        screenTypeText = new Label("Window mode: ", Anchor.CenterLeft, new Vector2(0.5f, -1));
        screenTypeText.Padding = new Vector2(10);
        screenTypeButtonPanel = new Panel(new Vector2(0.75f, 0), PanelSkin.None, Anchor.CenterRight);
        screenTypeButtonPanel.Padding = new Vector2(0, 0.25f);

        screenTypeWindowed = new Button("Windowed", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.33f, 0.5f));
        screenTypeWindowed.Padding = new Vector2(0);
        screenTypeWindowed.ToggleMode = true;
        screenTypeWindowed.OnClick = (Entity entity) => {
            screenTypeWindowed.Checked = true;
            screenTypeBorderless.Checked = false;
            screenTypeFullscreen.Checked = false;
            DisplayManager.SetWindowType(WindowType.WINDOWED, false);
        };
        screenTypeButtonPanel.AddChild(screenTypeWindowed);
        screenTypeBorderless = new Button("Borderless", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.33f, 0.5f));
        screenTypeBorderless.Padding = new Vector2(0);
        screenTypeBorderless.ToggleMode = true;
        screenTypeBorderless.OnClick = (Entity entity) => {
            screenTypeWindowed.Checked = false;
            screenTypeBorderless.Checked = true;
            screenTypeFullscreen.Checked = false;
            DisplayManager.SetWindowType(WindowType.BORDERLESS, false);
        };
        screenTypeButtonPanel.AddChild(screenTypeBorderless);
        screenTypeFullscreen = new Button("Fullscreen", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.33f, 0.5f));
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

        //camera speed setting setup
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

        //resolution setting setup
        resolutionPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        resolutionPanel.Padding = new Vector2(0);
        resolutionText = new Label("Resolution: ", Anchor.CenterLeft, new Vector2(0.5f, -1));
        resolutionText.Padding = new Vector2(10);
        resolutions = new List<(int, int)>();
        foreach (DisplayMode item in DisplayManager.supportedResolutions) {
            resolutions.Add((item.Width, item.Height));
        }
        resolutionChangePanel = new Panel(new Vector2(0.5f, 0), PanelSkin.None, Anchor.CenterRight);
        currentResolution = resolutions.FindIndex(x => x == (DisplayManager.Width, DisplayManager.Height));
        resolutionsDisplay = new Label(resolutions[currentResolution].Item1 + "x" + resolutions[currentResolution].Item2, Anchor.Center, new Vector2(0.5f, 0));

        prevResolution = new Button("-", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.25f, 0));
        prevResolution.Padding = new Vector2(0);
        prevResolution.OnClick = (Entity entity) => {
            if (currentResolution != 0) {
                --currentResolution;
            } 
                resolutionsDisplay.Text = resolutions[currentResolution].Item1 + "x" + resolutions[currentResolution].Item2;
            DisplayManager.SetResolution(resolutions[currentResolution].Item1, resolutions[currentResolution].Item2, false);
        };
        nextResolution = new Button("+", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.25f, 0));
        nextResolution.Padding = new Vector2(0);
        nextResolution.OnClick = (Entity entity) => {
            if (currentResolution != resolutions.Count - 1) {
                ++currentResolution;
            }
            resolutionsDisplay.Text = resolutions[currentResolution].Item1 + "x" + resolutions[currentResolution].Item2;
            DisplayManager.SetResolution(resolutions[currentResolution].Item1, resolutions[currentResolution].Item2, false);

        };

        resolutionChangePanel.AddChild(prevResolution);
        resolutionChangePanel.AddChild(resolutionsDisplay);
        resolutionChangePanel.AddChild(nextResolution);

        resolutionPanel.AddChild(resolutionText);
        resolutionPanel.AddChild(resolutionChangePanel);
        settingsPanel.AddChild(resolutionPanel);

        //button setup
        buttonPanel = new Panel(new Vector2(0.75f, 0.1f), PanelSkin.None, Anchor.BottomRight);

        saveChangesButton = new Button("Save", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.3f, -1));
        saveChangesButton.Padding = new Vector2(10);
        saveChangesButton.OnClick = saveChangesButtonClicked;

        discardChangesButton = new Button("Discard", ButtonSkin.Default, Anchor.Center, new Vector2(0.3f, -1));
        discardChangesButton.OnClick = discardChangesButtonClicked;
        discardChangesButton.Padding = new Vector2(10);

        menuButton = new Button("Back to Menu", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, -1));
        menuButton.OnClick = menuButtonClicked;
        menuButton.Padding = new Vector2(10);

        buttonPanel.AddChild(menuButton);
        buttonPanel.AddChild(saveChangesButton);
        buttonPanel.AddChild(discardChangesButton);

        panel.AddChild(settingsPanel);
        panel.AddChild(buttonPanel);
    }

    private void saveChangesButtonClicked(Entity entity) {
        CameraControllerCmp.DefaultScrollSpeed = cameraStoredValue;
        DisplayManager.ApplyChanges();
    }

    private void discardChangesButtonClicked(Entity entity) {
        cameraStoredValue = CameraControllerCmp.DefaultScrollSpeed;
        DisplayManager.DiscardChanges();
    }

    private void menuButtonClicked(Entity entity) {
        discardChangesButtonClicked(entity);
        SceneManager.Load(MainMenu.Instance);
    }

    protected override void DestroyUI() {
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
        menuButton = null;
        saveChangesButton = null;
        discardChangesButton = null;
    }
}
