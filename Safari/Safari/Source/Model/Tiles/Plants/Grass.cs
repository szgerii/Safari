using Microsoft.Xna.Framework.Graphics;

namespace Safari.Model.Tiles;

public class Grass : AutoTile {
	public Grass() : base(Game.ContentManager.Load<Texture2D>("Assets/Grass/Grass")) {
		IsFoodSource = true;
		LightRange = 1;

		HasDiagonalTiling = true;
		UseDefaultLayout();
	}
}
