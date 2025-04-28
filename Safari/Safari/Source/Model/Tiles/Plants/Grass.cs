namespace Safari.Model.Tiles;

public class Grass : AutoTile {
	public Grass() : base(Game.LoadTexture("Assets/Grass/Grass")) {
		IsFoodSource = true;
		LightRange = 1;

		Sprite.LayerDepth = 0.5f;
		UseDefaultLayout();
	}
}
