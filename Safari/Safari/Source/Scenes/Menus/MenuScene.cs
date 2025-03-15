using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Safari.Source.Scenes.Menus;
abstract class MenuScene : Scene {
    private Panel container;

    public override void Load() {
        this.ConstructUI();
        UserInterface.Active.AddEntity(container);
        base.Load();
    }

    public override void Unload() {
        this.DestroyUI();
        UserInterface.Active.RemoveEntity(container);
        base.Unload();
    }

    protected abstract void ConstructUI();

    protected abstract void DestroyUI();
}
