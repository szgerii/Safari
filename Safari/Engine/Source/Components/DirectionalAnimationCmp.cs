using Microsoft.Xna.Framework;
using System;

namespace Engine.Components;

public enum Direction : byte {
	Up,
	Down,
	Left,
	Right
}

/// <summary>
/// Component for managing animation states for an object that has animations for all 4 different directions.
/// Requires a separate animated sprite component to bind to.
/// </summary>
public class DirectionalAnimationCmp : Component {
	private AnimatedSpriteCmp targetSprite;
	/// <summary>
	/// The animated sprite the component manages
	/// </summary>
	public AnimatedSpriteCmp TargetSprite {
		get {
			// in case the animated sprite cmp wasn't available when the ctor ran
			targetSprite ??= Owner.GetComponent<AnimatedSpriteCmp>();
			return targetSprite;
		}
		set {
			targetSprite = value;
		}
	}

	protected string animationName;
	/// <summary>
	/// The name of the currently playing animation
	/// </summary>
	public string AnimationName {
		get => animationName;
		set {
			animationName = value;
			UpdateAnimation();
		}
	}

	protected Direction direction;
	/// <summary>
	/// The direction the object is currently facing
	/// </summary>
	public Direction Direction {
		get => direction;
		set {
			if (direction != value) {
				direction = value;
				UpdateAnimation();
			}
		}
	}

	/// <param name="animatedSprite">The animated sprite component to operate on (if omitted, the owner's animated sprite cmp will be set later, once it's accessible)</param>
	/// <param name="initialAnimation">The name of the animation to play initially (NOTE: this will only be played instantly if the sprite cmp is provided)</param>
	/// <param name="initialDirection">The initial direction of the animations</param>
	public DirectionalAnimationCmp(AnimatedSpriteCmp animatedSprite = null, string initialAnimation = "Idle", Direction initialDirection = Direction.Right) {
		targetSprite = animatedSprite;
		animationName = initialAnimation;
		direction = initialDirection;

		if (targetSprite != null) {
			UpdateAnimation();
		}
	}

	/// <summary>
	/// Sets the object's direction based on a direction vector
	/// </summary>
	public void SetDirectionFromVector(Vector2 directionVector) {
		if (Math.Abs(directionVector.X) >= Math.Abs(directionVector.Y)) {
			if (directionVector.X < 0) {
				Direction = Direction.Left;
			} else {
				Direction = Direction.Right;
			}
		} else {
			if (directionVector.Y < 0) {
				Direction = Direction.Up;
			} else {
				Direction = Direction.Down;
			}
		}
	}

	protected void UpdateAnimation() {
		TargetSprite.CurrentAnimation = AnimationName + direction.ToString();
	}
}
