using Engine.Input;
using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Safari.Scenes;
using Safari.Scenes.Menus;

namespace Safari.Popups;

class PauseMenu : PopupMenu {
    private static readonly PauseMenu instance = new PauseMenu();
    private Header Title;
    private Panel ButtonPanel;
    private Button ResumeButton;
    private Button SaveButton;
    private Button SaveAndExitButton;
    private Button ExitButton;
    private bool visible;

    public static PauseMenu Instance => instance;

    public static bool Visible => Instance.visible;

    public PauseMenu() {
        visible = false;
        panel = new Panel(new Vector2(0.4f, 0.7f), PanelSkin.Default, Anchor.Center);
        panel.MaxSize = new Vector2(400, 600);
        panel.Padding = new Vector2(10);
        Panel bg = new Panel(new Vector2(0, 0));
        bg.Opacity = (byte)0.5f;
        panel.Background = bg;

        Title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(Title);

        ButtonPanel = new Panel(new Vector2(0, 0.8f), PanelSkin.None, Anchor.Center);

        ResumeButton = new Button("Resume", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(0.8f, -1));
        ResumeButton.ButtonParagraph.AlignToCenter = true;
        ResumeButton.OnClick = ResumeButtonClicked;
        ResumeButton.Padding = new Vector2(10);
        ButtonPanel.AddChild(ResumeButton);

        SaveButton = new Button("Save", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(0.8f, -1));
        SaveButton.ButtonParagraph.AlignToCenter = true;
        SaveButton.OnClick = SaveButtonClicked;
        SaveButton.Padding = new Vector2(10);
        ButtonPanel.AddChild(SaveButton);

        SaveAndExitButton = new Button("Save & Exit", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(0.8f, -1));
        SaveAndExitButton.ButtonParagraph.AlignToCenter = true;
        SaveAndExitButton.OnClick = SaveAndExitButtonClicked;
        SaveAndExitButton.Padding = new Vector2(10);
        ButtonPanel.AddChild(SaveAndExitButton);

        ExitButton = new Button("Exit", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(0.8f, -1));
        ExitButton.ButtonParagraph.AlignToCenter = true;
        ExitButton.OnClick = ExitButtonClicked;
        ExitButton.Padding = new Vector2(10);
        ButtonPanel.AddChild(ExitButton);

        panel.AddChild(ButtonPanel);
    }

    private void ResumeButtonClicked(Entity entity) {
        TogglePauseMenu();
        UserInterface.Active.MouseInputProvider.DoClick();
    }

    private void SaveButtonClicked(Entity entity) {
        new AlertMenu("Feature", "This feature is currently WIP, check back another time").Show();
    }

    private void SaveAndExitButtonClicked(Entity entity) {
        TogglePauseMenu();
        SceneManager.Load(MainMenu.Instance);
    }

    private void ExitButtonClicked(Entity entity) {
        TogglePauseMenu();
        SceneManager.Load(MainMenu.Instance);
    }

    public void TogglePauseMenu() {
        if (SceneManager.Active is not GameScene) {
            return;
        }
        if (visible) {
            base.Hide();
            visible = false;
            GameScene.Active.Model.Resume();
        } else {
            GameScene.Active.Model.Pause();
            base.Show();
            visible = true;
        }
    }

    public override void Update(GameTime gameTime) {
        if (InputManager.IsGameFocused) return;
        if (JustPressed(Keys.Escape)) {
            TogglePauseMenu();
        }
    }

    private bool JustPressed(Keys key) {
        bool wasUp = InputManager.Keyboard.PrevKS.IsKeyUp(key);
        bool isDown = InputManager.Keyboard.CurrentKS.IsKeyDown(key);

        return wasUp && isDown;
    }
}
