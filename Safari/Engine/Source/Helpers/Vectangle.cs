using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;

namespace Engine.Helpers;

/// <summary>
/// Floating point rectangle, based as closely as possible on MonoGame's <see cref="Rectangle"/>
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public struct Vectangle : IEquatable<Vectangle>, IEquatable<Rectangle> {
	private static Vectangle emptyVectangle = new();

	[JsonProperty]
	public float X { get; set; }
	[JsonProperty]
	public float Y { get; set; }
	[JsonProperty]
	public float Width { get; set; }
	[JsonProperty]
	public float Height { get; set; }

	public static Vectangle Empty => emptyVectangle;

	public readonly float Left => X;
	public readonly float Right => X + Width;
	public readonly float Top => Y;
	public readonly float Bottom => Y + Height;

	public readonly Vector2 Center => new(X + (Width / 2), Y + (Height / 2));

	public readonly bool IsEmpty => (X == 0) && (Y == 0) && (Width == 0) && (Height == 0);

	public Vector2 Location {
		readonly get => new(X, Y);
		set {
			X = value.X;
			Y = value.Y;
		}
	}

	public Vector2 Size {
		readonly get => new(Width, Height);
		set {
			Width = value.X;
			Height = value.Y;
		}
	}

	public Vectangle(float x, float y, float width, float height) {
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public Vectangle(Vector2 location, Vector2 size) {
		X = location.X;
		Y = location.Y;
		Width = size.X;
		Height = size.Y;
	}

	public Vectangle(Rectangle value) {
		X = value.X;
		Y = value.Y;
		Width = value.Width;
		Height = value.Height;
	}

	public Vectangle(Vectangle value) {
		X = value.X;
		Y = value.Y;
		Width = value.Width;
		Height = value.Height;
	}

	public static bool operator ==(Vectangle a, Vectangle b) {
		return (a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height);
	}
	public static bool operator !=(Vectangle a, Vectangle b) => !(a == b);

	public static bool operator ==(Vectangle a, Rectangle b) {
		return (a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height);
	}
	public static bool operator !=(Vectangle a, Rectangle b) => !(a == b);

	public static bool operator ==(Rectangle a, Vectangle b) => b == a;
	public static bool operator !=(Rectangle a, Vectangle b) => !(b == a);

	public readonly override bool Equals(object? obj) {
		return (obj is Vectangle vect && this == vect) ||
			   (obj is Rectangle rect && this == rect);
	}

	public readonly bool Equals(Vectangle vect) {
		return this == vect;
	}

	public readonly bool Equals(Rectangle rect) {
		return this == rect;
	}

#pragma warning disable IDE0070
	public readonly override int GetHashCode() {
		unchecked {
			var hash = 17;
			hash = hash * 23 + X.GetHashCode();
			hash = hash * 23 + Y.GetHashCode();
			hash = hash * 23 + Width.GetHashCode();
			hash = hash * 23 + Height.GetHashCode();
			return hash;
		}
	}
#pragma warning restore IDE0070

	public void Inflate(float horizontalAmt, float verticalAmt) {
		X -= horizontalAmt;
		Y -= verticalAmt;
		Width += horizontalAmt * 2;
		Height += verticalAmt * 2;
	}

	public void Offset(float offsetX, float offsetY) {
		X += offsetX;
		Y += offsetY;
	}

	public void Offset(Vector2 amount) {
		X += amount.X;
		Y += amount.Y;
	}

	public void Offset(Point amount) {
		X += amount.X;
		Y += amount.Y;
	}

	public static Vectangle operator +(Vectangle @base, Vector2 offset) {
		return new(@base.X + offset.X, @base.Y + offset.Y, @base.Width, @base.Height);
	}

	public static Vectangle operator -(Vectangle @base, Vector2 offset) {
		return new(@base.X - offset.X, @base.Y - offset.Y, @base.Width, @base.Height);
	}

	public readonly bool Contains(float x, float y) {
		return (X <= x) && (x < X + Width) && (Y <= y) && (y < Y + Height);
	}

	public readonly bool Contains(Vector2 point) {
		return Contains(point.X, point.Y);
	}

	public readonly bool Contains(Point point) {
		return Contains(point.X, point.Y);
	}

	public readonly bool Contains(Vectangle value) {
		return (X <= value.X) && ((value.X + value.Width) <= (X + Width)) && (Y <= value.Y) && ((value.Y + value.Height) <= (Y + Height));
	}

	public readonly bool Contains(Rectangle value) {
		return (X <= value.X) && ((value.X + value.Width) <= (X + Width)) && (Y <= value.Y) && ((value.Y + value.Height) <= (Y + Height));
	}

	public readonly bool Intersects(Vectangle value) {
		return (value.X < (X + Width)) && (X < (value.X + value.Width)) &&
			   (value.Y < (Y + Height)) && (Y < (value.Y + value.Height));
	}

	public readonly bool Intersects(Rectangle value) {
		return (value.X < (X + Width)) && (X < (value.X + value.Width)) &&
			   (value.Y < (Y + Height)) && (Y < (value.Y + value.Height));
	}

	public readonly void Deconstruct(out float x, out float y, out float width, out float height) {
		x = X;
		y = Y;
		width = Width;
		height = Height;
	}

	public static explicit operator Rectangle(Vectangle value) {
		return new Rectangle((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
	}

	public readonly override string ToString() {
		return $"{{X:{X} Y:{Y} Width:{Width} Height:{Height}}}";
	}

	public readonly void CopyTo(out Vectangle copyTarget) {
		copyTarget = new();

		copyTarget.X = X;
		copyTarget.Y = Y;
		copyTarget.Width = Width;
		copyTarget.Height = Height;
	}

	public static Vectangle Union(Vectangle value1, Vectangle value2) {
		float x = Math.Min(value1.X, value2.X);
		float y = Math.Min(value1.Y, value2.Y);
		float width = Math.Max(value1.Right, value2.Right) - x;
		float height = Math.Max(value1.Bottom, value2.Bottom) - y;
		
		return new Vectangle(x, y, width, height);
	}

	public static Vectangle Intersect(Vectangle value1, Vectangle value2) {
		if (!value1.Intersects(value2)) {
			return Empty;
		}

		float x = Math.Max(value1.X, value2.X);
		float y = Math.Max(value1.Y, value2.Y);
		float width = Math.Min(value1.Right, value2.Right) - x;
		float height = Math.Min(value1.Bottom, value2.Bottom) - y;
		
		return new Vectangle(x, y, width, height);
	}

	/// <summary>
	/// Calculates the actual collider for a scaled sprite
	/// </summary>
	/// <param name="baseCollider">The collider that would be accurate for 1f scaling</param>
	/// <param name="scale">The scaling used by the sprite</param>
	/// <returns>The properly sized and positioned collider</returns>
	public Vectangle WithSpriteScale(float scale) {
		Vectangle result = new(
			X = Utils.Round(X * scale),
			Y = Utils.Round(Y * scale),
			Width = Utils.Round(Width * scale),
			Height = Utils.Round(Height * scale)
		);

		return result;
	}

	/// <summary>
	/// Clamps a position into the bounds defined by this Vectangle
	/// </summary>
	/// <param name="pos">The floating-point position to clamp</param>
	/// <returns>The clamped Vector2</returns>
	public readonly Vector2 Clamp(Vector2 pos) {
		pos.X = Math.Clamp(pos.X, Left, Right);
		pos.Y = Math.Clamp(pos.Y, Top, Bottom);

		return pos;
	}

	/// <summary>
	/// Clamps a position into the bounds defined by this Vectangle
	/// </summary>
	/// <param name="pos">The int-based position to clamp</param>
	/// <returns>The clamped Point</returns>
	public readonly Point Clamp(Point pos) => Clamp(pos.ToVector2()).ToPoint();
}

// Rectangle extensions for dealing with Vectangles
public static class RectangleVectangleExtensions {
	public static bool Intersects(this Rectangle rect, Vectangle vect) {
		return (vect.X < (rect.X + rect.Width)) && (rect.X < (vect.X + vect.Width)) &&
			   (vect.Y < (rect.Y + rect.Height)) && (rect.Y < (vect.Y + vect.Height));
	}

	public static bool Contains(this Rectangle rect, Vectangle vect) {
		return (rect.X <= vect.X) && ((vect.X + vect.Width) <= (rect.X + rect.Width))
			&& (rect.Y <= vect.Y) && ((vect.Y + vect.Height) <= (rect.Y + rect.Height));
	}

	public static bool Equals(this Rectangle rect, Vectangle vect) {
		return rect == vect;
	}

	public static Vectangle ToVectangle(this Rectangle value) {
		return new Vectangle(value.X, value.Y, value.Width, value.Height);
	}
}
