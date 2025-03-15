using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Safari.Source.Popups;

abstract class PopupMenu {
    protected Panel panel;

    /// <summary>
    /// Shows the popup.
    /// </summary>
    public virtual void Show() {
        UserInterface.Active.AddEntity(this.panel);
    }

    //Hides the popup
    protected void Hide() {
        UserInterface.Active.RemoveEntity(this.panel);
    }
}
