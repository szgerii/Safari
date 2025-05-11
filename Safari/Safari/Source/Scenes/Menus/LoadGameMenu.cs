using Engine.Objects;
using Engine.Scenes;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Helpers;
using Safari.Persistence;
using Safari.Popups;

namespace Safari.Scenes.Menus;

public class LoadGameMenu : MenuScene, IResettableSingleton {
    private static LoadGameMenu instance;
    public static LoadGameMenu Instance {
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
    private Panel savesPanel;
    private Button menuButton;

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(title);

        savesPanel = new Panel(new Vector2(0.8f, 0.7f), PanelSkin.Simple, Anchor.Center);
        savesPanel.Padding = new Vector2(10);

        if (GameModelPersistence.ListExistingParkNames().Count == 0) {
            Label noSaveLabel = new Label("You don't have a saved game to load.", Anchor.Center, new Vector2(0));
            noSaveLabel.Scale = SettingsMenu.Scale;
            savesPanel.AddChild(noSaveLabel);
        } else {
            savesPanel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
            StyleProperty hover = new StyleProperty(Color.LightGray);
            StyleProperty click = new StyleProperty(Color.LightSlateGray);
            foreach (string park in GameModelPersistence.ListExistingParkNames()) {
                Panel tempPanel = new Panel(new Vector2(0, 0.25f), PanelSkin.Simple, Anchor.Auto);

                tempPanel.Padding = new Vector2(10);
                tempPanel.OnClick = (Entity entity) => {
                    SaveSelectMenu.Instance.Park = park;
                    SceneManager.Load(SaveSelectMenu.Instance);
                };

                var tempSave = new GameModelPersistence(park).Saves[0].MetaData;

                Label tempLabelName = new Label($"{tempSave.ParkName}{(tempSave.GameAlreadyWon ? "(won)" : "")}", Anchor.CenterLeft, new Vector2(0.3f, 0));
                tempLabelName.ClickThrough = true;
                tempLabelName.Scale = SettingsMenu.Scale;

                Label tempLabelSaveDate = new Label($"{tempSave.CreationDate.ToString("yyyy. MM. dd. HH:mm")}", Anchor.Center, new Vector2(0.3f, 0));
                tempLabelSaveDate.ClickThrough = true;
                tempLabelSaveDate.Scale = SettingsMenu.Scale;

                Label tempLabelIngameDate = new Label($"Playtime: {tempSave.PlayTime.ToString("%d")} days {tempSave.PlayTime.ToString(@"h\:mm")}", Anchor.CenterRight, new Vector2(0.4f, 0));
                tempLabelIngameDate.ClickThrough = true;
                tempLabelIngameDate.Scale = SettingsMenu.Scale;

                tempPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
                tempPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);

                tempPanel.AddChild(tempLabelName);
                tempPanel.AddChild(tempLabelSaveDate);
                tempPanel.AddChild(tempLabelIngameDate);
                savesPanel.AddChild(tempPanel);
            }
        }

        menuButton = new Button("Back to Menu", ButtonSkin.Default, Anchor.BottomRight, new Vector2(250, 60));
        menuButton.OnClick = MenuButtonClicked;
        panel.AddChild(savesPanel);
        panel.AddChild(menuButton);
    }

    private void MenuButtonClicked(Entity entity) {
        SceneManager.Load(MainMenu.Instance);
    }

    protected override void DestroyUI() {
        panel = null;
        title = null;
        menuButton = null;
    }
}
