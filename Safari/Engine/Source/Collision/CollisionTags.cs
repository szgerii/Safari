using System;

namespace Engine.Collision;

[Flags]
public enum CollisionTags {
	World = 1,
	Player = 2,
	Enemy = 4,
	Damageable = 8
}
