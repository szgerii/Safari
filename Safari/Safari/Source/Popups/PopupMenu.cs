using Engine;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Scenes;

namespace Safari.Popups;

public abstract class PopupMenu : IUpdatable {
    protected Panel background = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.TopLeft);
    protected Panel? panel;

    private Rectangle maskArea;
    /// <summary>
    /// Shows the popup and handles background logic.
    /// </summary>
    public virtual void Show() {
        if (panel.Parent != null) {
            return;
        }
        if (background != null) {
            UserInterface.Active.AddEntity(background);
            maskArea = this.background.CalcDestRect();
            GameScene.Active?.MaskedAreas.Add(maskArea);
        } else {
            maskArea = this.panel.CalcDestRect();
            GameScene.Active?.MaskedAreas.Add(maskArea);
        }
        UserInterface.Active.AddEntity(panel);

        panel.BringToFront();
    }

    /// <summary>
    /// Hides the popup and handles background logic.
    /// </summary>
    public virtual void Hide() {
        if(background == null && panel.Parent != null) {
            GameScene.Active?.MaskedAreas.Remove(maskArea);
        } else if (background != null && background.Parent != null) {
            GameScene.Active?.MaskedAreas.Remove(maskArea);
        }

        if (background != null && panel?.Parent != null) {
            UserInterface.Active.RemoveEntity(background);
        }
        if (panel != null && panel?.Parent != null) {
            UserInterface.Active.RemoveEntity(panel);
        }
    }

    public virtual void Update(GameTime gameTime) { }
}
