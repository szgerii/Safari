using Engine.Scenes;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Helpers;
using Safari.Persistence;

namespace Safari.Scenes.Menus;

class SaveSelectMenu : MenuScene, IResettableSingleton{
    private static SaveSelectMenu? instance;
    private string park = null;
    public string Park { get => park; set => park = value; }
    public static SaveSelectMenu Instance {
        get {
            instance ??= new();
            return instance;
        }
    }
    public static void ResetSingleton() {
        instance?.Unload();
        instance = null;
    }

    private Header? title;
    private Panel? savesPanel;
    private Button? saveSelectButton;

    protected override void ConstructUI() {
        if(park == null) {
            SceneManager.Load(LoadGameMenu.Instance);
        }
        panel = new Panel(new Vector2(0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(title);

        savesPanel = new Panel(new Vector2(0.8f, 0.6f), PanelSkin.None, Anchor.Center);
        savesPanel.Padding = new Vector2(0);

        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);
        var saveObj = new GameModelPersistence(park);
        int counter = saveObj.Saves.Count;
        int slotNumber = 0;
        foreach (var save in saveObj.Saves) {
            int currentSlotNumber = slotNumber;
            Panel tempPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.Simple, Anchor.Auto);
            tempPanel.Padding = new Vector2(10,0);
            tempPanel.OnClick = (Entity entity) => {
                LoadingScene.Instance.LoadSave(park, currentSlotNumber);
            };

            var tempSave = save.MetaData;

            Label tempLabelName = new Label($"{counter}# {tempSave.ParkName}{(tempSave.GameAlreadyWon ? "(won)" : "")}", Anchor.CenterLeft, new Vector2(0.3f, 0));
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

            --counter;
            ++slotNumber;
        }

        saveSelectButton = new Button("Back to Save Selection", ButtonSkin.Default, Anchor.BottomRight, new Vector2(250, 60));
        saveSelectButton.OnClick = SaveSelectButtonButtonClicked;
        panel.AddChild(saveSelectButton);
        panel.AddChild(savesPanel);
    }

    private void SaveSelectButtonButtonClicked(Entity entity) {
        SceneManager.Load(LoadGameMenu.Instance);
    }

    protected override void DestroyUI() {
        panel = null;
        title = null;
        saveSelectButton = null;
        park = null;
        savesPanel = null;
}
}

