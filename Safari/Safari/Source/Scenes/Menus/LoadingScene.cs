using Engine;
using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Helpers;
using Safari.Model;
using Safari.Persistence;
using Safari.Popups;

namespace Safari.Scenes.Menus;

public class LoadingScene : MenuScene, IResettableSingleton, IUpdatable {
	private static LoadingScene instance;
	private static GameScene gameToLoad = null;
	private static string parkNameToLoad = null;
	private static int parkSlotToLoad = -1;
	private static bool newGame;
	private static bool loadGame = false;
	private static int updates = 0;
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

	public void LoadNewGame(string parkName, GameDifficulty difficulty) {
		gameToLoad = new GameScene(parkName, difficulty);
		SceneManager.Load(instance);
        loadGame = true;
		newGame = true;
		updates = 0;
	}

	public void LoadSave(string parkName, int slotNumber) {
		parkNameToLoad = parkName;
        parkSlotToLoad = slotNumber;
		DebugConsole.Instance.Write(slotNumber.ToString());
        SceneManager.Load(instance);
        loadGame = true;
		newGame = false;
		updates = 0;
	}

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0, 0), PanelSkin.Default, Anchor.TopLeft);
        text = new Label("The game is loading. Please be patient!", Anchor.Center, new Vector2(-1));
        panel.AddChild(text);
    }

    protected override void DestroyUI() {
        panel = null;
        text = null;
    }

    public override void Update(GameTime gameTime) {
		if (updates == 0) { updates++; return; }
        if (loadGame) {
			if (newGame) {
				SceneManager.Load(gameToLoad);
				loadGame = false;
				gameToLoad = null;
			} else {
				try {
					new GameModelPersistence(parkNameToLoad).Load(parkSlotToLoad);
					loadGame = false;
				} catch {
					loadGame = false;

					AlertMenu am = new AlertMenu("Corrupt save file", "An unexpected error occured when trying to read a corrupt save file");
					am.Chosen += (object sender, bool choice) => {
						SceneManager.Load(MainMenu.Instance);
					};
					am.Show();
				}
			}
        }
    }
}
