using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Scenes;

namespace Safari.Components;

/// <summary>
/// Component that marks a given GameObject (usually an entity) as a light source
/// The reason this is different from the way tiles calculate their light is because
/// entities can move between tiles.
/// </summary>
[LimitCmpOwnerType(typeof(Entity))]
public class LightEntityCmp : Component, IUpdatable {
	private Level level;
	private int range;
	private Point? old_map_pos = null;
	private Entity ownerEntity;

	public LightEntityCmp(Level level, int range) {
		this.level = level;
		this.range = range;
	}

	public override void Load() {
		ownerEntity = Owner as Entity;

		base.Load();
	}

	public void Update(GameTime gameTime) {
		Point centerPoint = ownerEntity.CenterPosition.ToPoint();
		int map_x = (int)(centerPoint.X / (float)level.TileSize);
		int map_y = (int)(centerPoint.Y / (float)level.TileSize);
		Point map_pos = new Point(map_x, map_y);
		if (old_map_pos == null) {
			if (!level.IsOutOfBounds(map_pos.X, map_pos.Y)) {
				level.LightManager.AddLightSource(map_pos, range);
			}
			old_map_pos = map_pos;
		} else {
			if (old_map_pos != map_pos) {
				if (!level.IsOutOfBounds(old_map_pos.Value.X, old_map_pos.Value.Y)) {
					level.LightManager.RemoveLightsource((Point)old_map_pos, range);
				}
				if (!level.IsOutOfBounds(map_pos.X, map_pos.Y)) {
					level.LightManager.AddLightSource(map_pos, range);
				}
				old_map_pos = map_pos;
			}
		}
	}
}
