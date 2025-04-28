using Engine;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Popups;

public abstract class PopupMenu : IUpdatable {
    protected Panel background = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.TopLeft);
    protected Panel panel;

    /// <summary>
    /// Shows the popup.
    /// </summary>
    public virtual void Show() {
        if (background != null) {
            UserInterface.Active.AddEntity(background);
        }
        UserInterface.Active.AddEntity(panel);
        panel.BringToFront();
    }

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public virtual void Hide() {
        if (background != null && panel?.Parent != null) {
            UserInterface.Active.RemoveEntity(background);
        }
        if (panel != null && panel?.Parent != null) {
            UserInterface.Active.RemoveEntity(panel);
        }
    }

    public virtual void Update(GameTime gameTime) { }
}
