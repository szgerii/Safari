using Engine;
using Engine.Collision;

namespace Safari;

/// <summary>
/// Helper methods used across the game project
/// </summary>
public static class GameUtils {
	/// <summary>
	/// Calculates the actual collider for a scaled sprite
	/// </summary>
	/// <param name="baseCollider">The collider that would be accurate for 1f scaling</param>
	/// <param name="scale">The scaling used by the sprite</param>
	/// <returns>The properly sized and positioned collider</returns>
	public static Collider WithSpriteScale(this Collider baseCollider, float scale) {
		Collider result = new() {
			X = Utils.Round(baseCollider.X * scale),
			Y = Utils.Round(baseCollider.Y * scale),
			Width = Utils.Round(baseCollider.Width * scale),
			Height = Utils.Round(baseCollider.Height * scale)
		};

		return result;
	}
}
