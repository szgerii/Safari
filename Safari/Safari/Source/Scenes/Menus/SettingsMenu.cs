using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Scenes.Menus;

class SettingsMenu : MenuScene {
    private readonly static SettingsMenu active = new SettingsMenu();
    private Header title;
    private Paragraph message;
    private Button menuButton;

    public static SettingsMenu Active => active;

    protected override void ConstructUI() {
        this.panel = new Panel(new Vector2(0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        this.panel.AddChild(title);

        message = new Paragraph("This feature is WIP, come back another time.", Anchor.Center);
        this.panel.AddChild(message);

        menuButton = new Button("Back to Menu", ButtonSkin.Default, Anchor.BottomRight, new Vector2(250, 60));
        menuButton.OnClick = MenuButtonClicked;
        this.panel.AddChild(menuButton);
    }

    private void MenuButtonClicked(Entity entity) {
        SceneManager.Load(MainMenu.Active);
    }

    protected override void DestroyUI() {
        title = null;
        message = null;
        menuButton = null;
    }
}
