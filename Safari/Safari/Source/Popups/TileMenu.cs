using Safari.Scenes;

namespace Safari.Popups;

public class TileMenu : CategoryMenu {
    public TileMenu() : base("Tiles and plants"){
        //grass
        //water
        //road
        //bush
        //tree
    }

    public override void Show() {
        GameScene.Active.MouseMode = MouseMode.Build;
        base.Show();
    }

    public override void Hide() {
        base.Hide();
    }
}
