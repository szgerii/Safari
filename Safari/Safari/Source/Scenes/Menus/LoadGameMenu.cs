using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Scenes.Menus;

class LoadGameMenu : MenuScene {
    private readonly static LoadGameMenu instance = new LoadGameMenu();
    private Header title;
    private Paragraph message;
    private Button menuButton;

    public static LoadGameMenu Instance => instance;

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(title);

        message = new Paragraph("This feature is WIP, come back another time.", Anchor.Center);
        panel.AddChild(message);

        menuButton = new Button("Back to Menu", ButtonSkin.Default, Anchor.BottomRight, new Vector2(250, 60));
        menuButton.OnClick = MenuButtonClicked;
        panel.AddChild(menuButton);
    }

    private void MenuButtonClicked(Entity entity) {
        SceneManager.Load(MainMenu.Instance);
    }

    protected override void DestroyUI() {
        panel = null;
        title = null;
        message = null;
        menuButton = null;
    }
}
