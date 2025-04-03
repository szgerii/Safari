using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Scenes;

namespace Safari.Components;

/// <summary>
/// Component that marks a given GameObject (usually an entity) as a light source
/// The reason this is different from the way tiles calculate their light is because
/// entities can move between tiles.
/// </summary>
public class LightEntityCmp : Component, IUpdatable {

	private Level level;
	private int range;
	private Point? old_map_pos = null;
	private SpriteCmp sprite;

	public LightEntityCmp(Level level, int range) {
		this.level = level;
		this.range = range;
		
	}

	public override void Load() {
		sprite = Owner.GetComponent<SpriteCmp>();
	}

	public void Update(GameTime gameTime) {
		int tileSize = GameScene.Active.Model.Level.TileSize;
		Point offset = new Point(0, 0);
		if (sprite != null) {
			offset = (sprite.SourceRectangle?.Size ?? sprite.Texture.Bounds.Size) / new Point(2);
		}
		Point centerPoint = Owner.Position.ToPoint() + offset;
		int map_x = (int)(centerPoint.X / (float)level.TileSize);
		int map_y = (int)(centerPoint.Y / (float)level.TileSize);
		Point map_pos = new Point(map_x, map_y);
		if (old_map_pos == null) {
			level.LightManager.AddLightSource(map_pos, range);
			old_map_pos = map_pos;
		} else {
			if (old_map_pos != map_pos) {
				level.LightManager.RemoveLightsource((Point)old_map_pos, range);
				level.LightManager.AddLightSource(map_pos, range);
				old_map_pos = map_pos;
			}
		}
	}
}
