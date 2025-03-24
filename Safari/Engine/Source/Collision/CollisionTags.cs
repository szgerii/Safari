using System;

namespace Engine.Collision;

[Flags]
public enum CollisionTags {
	World = 1,
	Animal = 2,
	Poacher = 4,
	Ranger = 8,
	Jeep = 16,
}
