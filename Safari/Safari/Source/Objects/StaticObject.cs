using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects;

public class StaticObject : GameObject {
	public StaticObject(Vector2 pos, Texture2D tex, float ySortOffset = 0, float layerDepth = 0.5f) : base(pos) {
		SpriteCmp sprite = new(tex) {
			YSortEnabled = true,
			YSortOffset = ySortOffset,
			LayerDepth = layerDepth
		};

		Attach(sprite);
	}
}
