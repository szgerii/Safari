using Engine;
using Microsoft.Xna.Framework;
using Safari.Model;

namespace Safari.Components;

public class LightEntityCmp : Component, IUpdatable {

	private Level level;
	private int range;
	private Point? old_map_pos = null;

	public LightEntityCmp(Level level, int range) {
		this.level = level;
		this.range = range;
	}

	public void Update(GameTime gameTime) {
		int map_x = (int)(Owner.Position.X / (float)level.TileSize);
		int map_y = (int)(Owner.Position.Y / (float)level.TileSize);
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
