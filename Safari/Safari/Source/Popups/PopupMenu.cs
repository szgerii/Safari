using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Safari.Popups;

public abstract class PopupMenu {
    protected Panel panel;

    /// <summary>
    /// Shows the popup.
    /// </summary>
    public virtual void Show() {
        UserInterface.Active.AddEntity(this.panel);
        panel.BringToFront();
    }

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public virtual void Hide() {
        UserInterface.Active.RemoveEntity(this.panel);
    }
}
