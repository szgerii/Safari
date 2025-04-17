using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Safari.Scenes.Menus;

class LoadingScene : MenuScene {
    private readonly static LoadingScene instance = new LoadingScene();
    private Label text = null;

    public static LoadingScene Instance => instance;
    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0, 0), PanelSkin.Default, Anchor.TopLeft);
        text = new Label("The game is loading. Please be patient!", Anchor.Center, new Vector2(-1));
        panel.AddChild(text);
    }

    protected override void DestroyUI() {
        panel = null;
        text = null;
    }
}
