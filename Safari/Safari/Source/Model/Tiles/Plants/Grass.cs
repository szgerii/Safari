using Microsoft.Xna.Framework.Graphics;

namespace Safari.Model.Tiles;

public class Grass : AutoTile {
	public Grass() : base(Game.ContentManager.Load<Texture2D>("Assets/Grass/Grass")) {
		IsFoodSource = true;

		Sprite.LayerDepth = 0.4f;
		HasDiagonalTiling = true;
		UseDefaultLayout();
	}
}
