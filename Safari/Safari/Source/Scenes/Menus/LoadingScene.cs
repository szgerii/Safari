using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Helpers;

namespace Safari.Scenes.Menus;

public class LoadingScene : MenuScene, IResettableSingleton {
	private static LoadingScene instance;
	public static LoadingScene Instance {
		get {
			instance ??= new();
			return instance;
		}
	}
	public static void ResetSingleton() {
		instance?.Unload();
		instance = null;
	}

	private Label text = null;

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
