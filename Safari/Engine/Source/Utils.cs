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
	/// Merges an array of textures into a single atlas texture
	/// The size of a cell inside the atlas is determined by the max width and height of the textures
	/// Smaller textures are padded with transparent pixels on the right and bottom
	/// </summary>
	/// <param name="textures">The textures to place in the atlas</param>
	/// <param name="rowLength">The number of cells inside a single atlas row</param>
	/// <returns>The texture of the atlas</returns>
	public static Texture2D CreateAtlas(Texture2D[] textures, int rowLength) {
		if (textures.Length == 0) {
			throw new ArgumentException("textures cannot be an empty array", nameof(textures));
		}

		if (rowLength <= 0) {
			throw new ArgumentException("rowLength has to be an integer greater than zero", nameof(rowLength));
		}

		if (rowLength > textures.Length) {
			rowLength = textures.Length;
		}

		int unitWidth = -1, unitHeight = -1;
		foreach (Texture2D tex in textures) {
			if (tex.Width > unitWidth) {
				unitWidth = tex.Width;
			}

			if (tex.Height > unitHeight) {
				unitHeight = tex.Height;
			}
		}

		int colCount = rowLength;
		int rowCount = (int)Math.Ceiling(textures.Length / (float)colCount);

		Texture2D atlasTex = new Texture2D(Game.Graphics.GraphicsDevice, unitWidth * colCount, unitHeight * rowCount);
		Color[] atlasData = new Color[atlasTex.Width * atlasTex.Height];

		for (int row = 0; row < rowCount; row++) {
			for (int col = 0; col < colCount; col++) {
				int px = col * unitWidth;
				int py = row * unitHeight;
				int texIndex = row * colCount + col;

				if (texIndex >= textures.Length) {
					break;
				}

				Texture2D tex = textures[texIndex];
				Color[] texData = new Color[tex.Width * tex.Height];
				tex.GetData(texData);

				for (int i = 0; i < texData.Length; i++) {
					atlasData[(py + i / tex.Width) * atlasTex.Width + px + (i % tex.Width)] = texData[i];
				}
			}
		}

		atlasTex.SetData(atlasData);
		return atlasTex;
	}

	/// <summary>
	/// Shorthand for creating a single row texture atlas with <see cref="CreateAtlas(Texture2D[], int)"/>
	/// </summary>
	/// <param name="textures">The textures to place in the atlas</param>
	/// <returns>The texture of the created atlas</returns>
	public static Texture2D CreateAtlas(Texture2D[] textures) {
		return CreateAtlas(textures, textures.Length);
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
	/// Converts a Vector2 to a Point using rounding
	/// </summary>
	/// <param name="vec">The vector to round</param>
	/// <returns>The rounded data</returns>
	public static Point Round(Vector2 vec) {
		vec.Round();
		return vec.ToPoint();
	}

	/// <summary>
	/// Returns a Point clamped to the inclusive range of <paramref name="min"/> and <paramref name="max"/>
	/// </summary>
	/// <param name="value">The Point to clamp</param>
	/// <param name="min">The lowest possible Point (inclusive)</param>
	/// <param name="max">The highest possible Point (inclusive)</param>
	/// <returns>The clamped point object</returns>
	public static Point Clamp(this Point value, Point min, Point max) {
		return new Point(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
	}

	/// <summary>
	/// Rotates a vector around the origin by <paramref name="rad"/> radians
	/// </summary>
	/// <param name="vec">The vector to transform</param>
	/// <param name="rad">The amount to rotate the vector by (in radians)</param>
	/// <returns>The rotated vector</returns>
	public static Vector2 Rotate(this Vector2 vec, double rad) {
		double x = vec.X * Math.Cos(rad) - vec.Y * Math.Sin(rad);
		double y = vec.X * Math.Sin(rad) + vec.Y * Math.Cos(rad);
		return new Vector2((float)x, (float)y);
	}

	/// <summary>
	/// Clamps a Vector2 value between the bounds of a Rectangle
	/// </summary>
	/// <param name="bounds">The rectangle that defines the bounds</param>
	/// <param name="vec">The vector to clamp</param>
	/// <returns>The clamped vector</returns>
	public static Vector2 Clamp(this Rectangle bounds, Vector2 vec) {
		return Vector2.Clamp(vec, bounds.Location.ToVector2(), (bounds.Location + bounds.Size).ToVector2());
	}

	/// <summary>
	/// Clamps a Point value between the bounds of a Rectangle
	/// </summary>
	/// <param name="bounds">The rectangle that defines the bounds</param>
	/// <param name="point">The point to clamp</param>
	/// <returns>The clamped point object</returns>
	public static Point Clamp(this Rectangle bounds, Point point) => point.Clamp(bounds.Location, bounds.Location + bounds.Size);

	/// <summary>
	/// Formats a Vector2 for displaying on screen
	/// </summary>
	/// <param name="vec">The vector to format</param>
	/// <param name="includeLength">Include length of the vector</param>
	/// <returns>The formatted text</returns>
	public static string Format(this Vector2 vec, bool includeLength = true, bool includeDecimal = true) {
		if (includeDecimal) {
			return $"({vec.X:0.00}, {vec.Y:0.00})" + (includeLength ? $": {vec.Length():0.00}" : "");
		} else {
			return $"({vec.X:0}, {vec.Y:0})" + (includeLength ? $": {vec.Length():0}" : "");
		}
	}
}