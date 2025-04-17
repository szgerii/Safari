using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Components;

/// <summary>
/// An animation inside the spritesheet
/// </summary>
public class Animation {
	/// <summary>
	/// The index of the row the animation is in
	/// </summary>
	public int Row { get; set; }
	/// <summary>
	/// The length of the animation
	/// </summary>
	public int Length { get; set; }
	/// <summary>
	/// Whether the animation should restart after finishing
	/// </summary>
	public bool Loop { get; set; }
	/// <summary>
	/// The index of the first frame's column
	/// </summary>
	public int Offset { get; set; }
	/// <summary>
	/// Overwrites the spritesheet of the AnimatedSpriteCmp this animation is registered to
	/// The AnimatedSpriteCmp will use this instead for drawing
	/// </summary>
	public Texture2D Texture { get; set; }

	public Animation(int row, int length, bool loop = false, int offset = 0, Texture2D texture = null) {
		Row = row;
		Length = length;
		Loop = loop;
		Offset = offset;
		Texture	= texture;
	}
}

/// <summary>
/// Component for drawing animated sprites on the screen
/// </summary>
public class AnimatedSpriteCmp : SpriteCmp, IUpdatable {
	/// <summary>
	/// Runs when the component has finished playing an animation
	/// Callback gets a string param that is the animation's name
	/// </summary>
	public event EventHandler<string> AnimationFinished;

	/// <summary>
	/// The collection of animations registered to this AnimatedSpriteCmp
	/// This maps names to animations
	/// </summary>
	public Dictionary<string, Animation> Animations { get; set; } = new Dictionary<string, Animation>();

	protected Animation currentAnim;
	protected string currentAnimationName;
	/// <summary>
	/// The name of the current animation
	/// This can also be used for playing a new animation
	/// TODO: func for q-ing new anim
	/// </summary>
	public string CurrentAnimation {
		get => currentAnimationName;
		set {
			currentAnimationName = value;
			currentAnim = Animations[value];
			CurrentFrame = 0;
			frameTime = 0;
		}
	}

	protected int columnCount;
	/// <summary>
	/// The number of columns inside the spritesheet
	/// If there is a varying amount of columns per row, set this to the maximum
	/// </summary>
	public int ColumnCount {
		get => columnCount;
		set {
			FrameWidth = (currentAnim?.Texture?.Width ?? Texture?.Width ?? 0) / value;
			columnCount = value;
		}
	}

	protected int rowCount;
	/// <summary>
	/// The number of rows inside the spritesheet
	/// </summary>
	public int RowCount {
		get => rowCount;
		set {
			FrameHeight = (currentAnim?.Texture?.Height ?? Texture?.Height ?? 0) / value;
			rowCount = value;
		}
	}

	/// <summary>
	/// The width of a frame in pixels
	/// </summary>
	public int FrameWidth { get; protected set; }
	/// <summary>
	/// The height of a frame in pixels
	/// </summary>
	public int FrameHeight { get; protected set; } // TODO: calc these properly TODO: figure out what's not "proper" about it

	/// <summary>
	/// The index of the frame currently being displayed
	/// This is relative to the animation object, it does not account for offsets for example
	/// </summary>
	public int CurrentFrame { get; protected set; } = 0;

	protected int fps;
	protected int frameGapMs; // the time between each frame of the animation in ms
	/// <summary>
	/// The speed at which the animations are played
	/// </summary>
	public int FPS {
		get => fps;
		set {
			frameGapMs = 1000 / value;
			fps = value;
		}
	}

	public bool IsPlaying { get; protected set; }

	protected float frameTime;

	public AnimatedSpriteCmp(Texture2D texture, int columnCount, int rowCount, int fps) : base(texture) {
		ColumnCount = columnCount;
		RowCount = rowCount;
		FPS = fps;
		Origin = Vector2.Zero;
	}

	private Texture2D prevTex;
	public void Update(GameTime gameTime) {
		// TODO: handle this more efficiently
		Texture2D inUseTex = currentAnim?.Texture ?? Texture;
		
		if (inUseTex != prevTex) {
			ColumnCount = ColumnCount;
			RowCount = RowCount;
		}
		prevTex = inUseTex;

		if (currentAnim == null) return;

		IsPlaying = true;

		// if it's time, try to play the next frame
		frameTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (frameTime > frameGapMs) {
			frameTime = 0;

			CurrentFrame++;
			if (CurrentFrame >= currentAnim.Length) {
				if (currentAnim.Loop) {
					CurrentFrame = 0;
				} else {
					CurrentFrame = currentAnim.Length - 1;
					AnimationFinished?.Invoke(this, CurrentAnimation);
					IsPlaying = false;
				}
			}
		}
	}

	public Rectangle CalculateSrcRec() {
		return new Rectangle(currentAnim.Offset * FrameWidth + FrameWidth * CurrentFrame, FrameHeight * currentAnim.Row, FrameWidth, FrameHeight);
	}

	public override void Draw(GameTime gameTime) {
		if (currentAnim == null || !Visible) {
			return;
		}

		// draw the current frame
		Game.SpriteBatch.Draw(currentAnim.Texture ?? Texture, Owner.Position, CalculateSrcRec(), Tint, Rotation, Origin, Scale, Flip, RealLayerDepth);
	}
}