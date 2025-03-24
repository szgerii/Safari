using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Collision;

public struct Collider {
	public readonly static Collider Empty = new(0, 0, 0, 0);

	public float X { get; set; }
	public float Y { get; set; }
	public float Width { get; set; }
	public float Height { get; set; }

	/// <summary>
	/// Position of the top left corner of the collider
	/// </summary>
	public readonly Vector2 Position => new Vector2(X, Y);
	/// <summary>
	/// Position of the bottom right corner of the collider
	/// </summary>
	public readonly Vector2 BottomRight => new Vector2(X + Width - 1, Y + Height - 1);

	public Collider(int x, int y, int width, int height) {
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public Collider(Rectangle rect) {
		X = rect.X;
		Y = rect.Y;
		Width = rect.Width;
		Height = rect.Height;
	}

	// AABB
	public readonly bool Intersects(Collider other) {
		return X < other.X + other.Width &&
				X + Width > other.X &&
				Y < other.Y + other.Height &&
				Y + Height > other.Y;
	}

	public void Offset(Vector2 offset) {
		X += offset.X;
		Y += offset.Y;
	}

	public readonly Rectangle ToRectangle() => new Rectangle((int)X, (int)Y, (int)Width, (int)Height);

	public static Collider operator +(Collider coll, Vector2 offset) {
		coll.Offset(offset);
		return coll;
	}

	public static Collider operator -(Collider coll, Vector2 offset) {
		coll.Offset(-offset);
		return coll;
	}

	public readonly override bool Equals([NotNullWhen(true)] object obj) {
		return base.Equals(obj);
	}

	public readonly override int GetHashCode() {
		return base.GetHashCode(); // TODO
	}

	public static bool operator ==(Collider a, Collider b) {
		return a.Equals(b);
	}

	public static bool operator !=(Collider a, Collider b) {
		return !a.Equals(b);
	}
}
