namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing a single water cell
/// </summary>
public class Water : AutoTile {
	public Water() : base(Game.LoadTexture("Assets/Water/Water")) {
		IsWaterSource = true;
		LightRange = 1;

		UseDefaultLayout();
	}
}
