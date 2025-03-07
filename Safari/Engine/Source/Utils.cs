using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine;

public static class Utils {
	/// <summary>
	/// Generates a single color Texture2D
	/// </summary>
	/// <param name="width">The width of the texture</param>
	/// <param name="height">The height of the texture</param>
	/// <param name="color">The color of the texture (purple by default)</param>
	/// <param name="outlineOnly">Whether the rectangle should have no filling</param>
	/// <returns>The generated texture</returns>
	public static Texture2D GenerateTexture(int width, int height, Color? color = null, bool outlineOnly = false) {
		Texture2D texture = new Texture2D(Game.Graphics.GraphicsDevice, width, height);

		Color[] data = new Color[width * height];
		for (int pixel = 0; pixel < data.Length; pixel++) {
			if (!outlineOnly || pixel < width || pixel >= data.Length - width || pixel % width == 0 || pixel % width == width - 1) {
				data[pixel] = color ?? Color.Purple;
			}
		}

		texture.SetData(data);
		return texture;
	}

	/// <summary>
	/// Returns the rounded value of the double as an integer
	/// </summary>
	/// <param name="value">The number to round</param>
	/// <returns>The rounded value as an int</returns>
	public static int Round(double value) {
		return (int)Math.Round(value);
	}

	/// <summary>
	/// Returns a Point clamped to the inclusive range of <paramref name="min"/> and <paramref name="max"/>
	/// </summary>
	/// <param name="value">The Point to clamp</param>
	/// <param name="min">The lowest possible Point (inclusive)</param>
	/// <param name="max">The highest possible Point (inclusive)</param>
	public static Point Clamp(this Point value, Point min, Point max) {
		return new Point(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
	}

	/// <summary>
	/// Rotates a vector around the origin by <paramref name="rad"/> radians
	/// </summary>
	/// <param name="vec">The vector to transform</param>
	/// <param name="rad">The amount to rotate the vector by (in radians)</param>
	public static Vector2 Rotate(this Vector2 vec, double rad) {
		double x = vec.X * Math.Cos(rad) - vec.Y * Math.Sin(rad);
		double y = vec.X * Math.Sin(rad) + vec.Y * Math.Cos(rad);
		return new Vector2((float)x, (float)y);
	}
}