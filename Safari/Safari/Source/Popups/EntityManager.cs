using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Safari.Popups;



class EntityManager : PopupMenu {
    private readonly static EntityManager instance = new EntityManager();
    private bool visible;
    private Panel currentPanel = null;
    private Header title;
    private Panel buttonPanel;

    private Panel rangerPanel;
    private Button rangerTabBtn;

    private Panel animalPanel;
    private Button animalTabBtn;

    private Panel otherPanel;
    private Button otherTabBtn;

    public static EntityManager Instance => instance;

    private EntityManager() {
        background = null;
        visible = false;

        panel = new Panel(new Vector2(0.4f, 0.75f), PanelSkin.Default, Anchor.TopRight);
        panel.Tag = "PassiveFocus";
        panel.Padding = new Vector2(0);

        title = new Header("Entity Manager", Anchor.TopCenter, new Vector2(0, 15));
        title.Size = new Vector2(0, 0.1f);

        buttonPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        buttonPanel.Padding = new Vector2(15);

        Vector2 maxSize = new Vector2(0.3f, 80);

        rangerTabBtn = new Button("Rangers", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.3f, 0));
        rangerTabBtn.OnClick = switchToRangerTab;
        rangerTabBtn.Padding = new Vector2(0);
        rangerTabBtn.MaxSize = maxSize;

        animalTabBtn = new Button("Animals", ButtonSkin.Default, Anchor.Center, new Vector2(0.3f, 0));
        animalTabBtn.OnClick = switchToAnimalTab;
        animalTabBtn.Padding = new Vector2(0);
        animalTabBtn.MaxSize = maxSize;

        otherTabBtn = new Button("Others", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0));
        otherTabBtn.OnClick = switchToOtherTab;
        otherTabBtn.Padding = new Vector2(0);
        otherTabBtn.MaxSize = maxSize;

        buttonPanel.AddChild(rangerTabBtn);
        buttonPanel.AddChild(animalTabBtn);
        buttonPanel.AddChild(otherTabBtn);

        Vector2 tabPanelSize = new Vector2(0, 0.7f);

        #region RANGER_TAB
        rangerPanel = new Panel(tabPanelSize, PanelSkin.Default, Anchor.BottomLeft, new Vector2(0));
        #endregion

        #region ANIMAL_TAB
        #endregion

        #region OTHER_TAB
        #endregion

        panel.AddChild(title);
        panel.AddChild(buttonPanel);
        panel.AddChild(rangerPanel);
        switchToRangerTab(null);
    }

    public void Toggle() {
        if (visible) {
            Hide();
            visible = !visible;
        } else {
            Show();
            visible = !visible;
        }
    }

    private void switchToRangerTab(Entity entity) {
        if(currentPanel != rangerPanel) {
            panel.RemoveChild(currentPanel);
            currentPanel = rangerPanel;
            panel.AddChild(currentPanel);
        }
        UpdateRangerList();
    }

    private void switchToAnimalTab(Entity entity) {
        if (currentPanel != rangerPanel) {
            panel.RemoveChild(currentPanel);
            currentPanel = rangerPanel;
            panel.AddChild(currentPanel);
        }
        UpdateAnimalList();
    }

    private void switchToOtherTab(Entity entity) {
        if (currentPanel != rangerPanel) {
            panel.RemoveChild(currentPanel);
            currentPanel = rangerPanel;
            panel.AddChild(currentPanel);
        }
    }

    public override void Show() {
        base.Show();
    }

    public override void Hide() {
        base.Hide();
    }

    private void UpdateAnimalList() {
        //throw new NotImplementedException();
    }

    private void UpdateRangerList() {
        //throw new NotImplementedException();
    }
}
