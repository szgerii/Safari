using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;
using System;

namespace Engine;

public static class Utils {
	/// <summary>
	/// Generates a single color ITexture2D
	/// </summary>
	/// <param name="width">The width of the texture</param>
	/// <param name="height">The height of the texture</param>
	/// <param name="color">The color of the texture (purple by default)</param>
	/// <param name="outlineOnly">Whether the rectangle should have no filling</param>
	/// <returns>The generated texture</returns>
	public static ITexture2D GenerateTexture(int width, int height, Color? color = null, bool outlineOnly = false) {
		ITexture2D texture;
		
		if (Game.CanDraw) {
			texture = new Texture2DAdapter(new(Game.Graphics!.GraphicsDevice, width, height));
		} else {
			texture = new NoopTexture2D(null, width, height);
		}

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
	/// Merges an array of textures into a single atlas texture <br/>
	/// The size of a cell inside the atlas is determined by the max width and height of the textures <br/>
	/// Smaller textures are padded with transparent pixels on the right and bottom
	/// </summary>
	/// <param name="textures">The textures to place in the atlas</param>
	/// <param name="rowLength">The number of cells inside a single atlas row</param>
	/// <returns>The texture of the atlas</returns>
	public static ITexture2D CreateAtlas(ITexture2D[] textures, int rowLength) {
		if (textures.Length == 0) {
			return Game.Instance!.IsHeadless ?
				NoopTexture2D.Empty :
				throw new ArgumentException("textures cannot be an empty array", nameof(textures));
		}

		if (rowLength <= 0) {
			throw new ArgumentException("rowLength has to be an integer greater than zero", nameof(rowLength));
		}

		if (rowLength > textures.Length) {
			rowLength = textures.Length;
		}

		int unitWidth = -1, unitHeight = -1;
		foreach (ITexture2D tex in textures) {
			if (tex.Width > unitWidth) {
				unitWidth = tex.Width;
			}

			if (tex.Height > unitHeight) {
				unitHeight = tex.Height;
			}
		}

		int colCount = rowLength;
		int rowCount = (int)Math.Ceiling(textures.Length / (float)colCount);

		ITexture2D atlasTex;
		if (Game.CanDraw) {
			atlasTex = new Texture2DAdapter(new(Game.Graphics!.GraphicsDevice, unitWidth * colCount, unitHeight * rowCount));
		} else {
			atlasTex = new NoopTexture2D(null, unitWidth * colCount, unitHeight * rowCount);
		}
		Color[] atlasData = new Color[atlasTex.Width * atlasTex.Height];

		for (int row = 0; row < rowCount; row++) {
			for (int col = 0; col < colCount; col++) {
				int px = col * unitWidth;
				int py = row * unitHeight;
				int texIndex = row * colCount + col;

				if (texIndex >= textures.Length) {
					break;
				}

				ITexture2D tex = textures[texIndex];
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
	/// Shorthand for creating a single row texture atlas with <see cref="CreateAtlas(ITexture2D[], int)"/>
	/// </summary>
	/// <param name="textures">The textures to place in the atlas</param>
	/// <returns>The texture of the created atlas</returns>
	public static ITexture2D CreateAtlas(ITexture2D[] textures) {
		return CreateAtlas(textures, textures.Length);
	}

	/// <summary>
	/// Places a texture on top of another
	/// </summary>
	/// <param name="tex1">The base texture</param>
	/// <param name="tex2">The texture to overlay onto the base texture</param>
	/// <returns>The merged texture</returns>
	/// <exception cref="ArgumentException"></exception>
	public static ITexture2D MergeTextures(ITexture2D tex1, ITexture2D tex2) {
		if (tex1.Bounds != tex2.Bounds) {
			throw new ArgumentException("Cannot merge textures of different sizes");
		}

		Color[] tex1Data = new Color[tex1.Width * tex1.Height];
		tex1.GetData(tex1Data);
		Color[] tex2Data = new Color[tex2.Width * tex2.Height];
		tex2.GetData(tex2Data);

		ITexture2D result;
		if (Game.CanDraw) {
			result = new Texture2DAdapter(new(Game.Graphics!.GraphicsDevice, tex1.Width, tex1.Height));
		} else {
			result = new NoopTexture2D(null, tex1.Width, tex1.Height);
		}

		Color[] resultData = new Color[result.Width * result.Height];

		for (int i = 0; i < tex1Data.Length; i++) {
			resultData[i] = tex2Data[i].A > 0 ? tex2Data[i] : tex1Data[i];
		}

		result.SetData(resultData);
		return result;
	}

	/// <summary>
	/// Calculates the ideal Y-Sort offset for a (sub)texture by measuring the empty space at its bottom
	/// </summary>
	/// <param name="texture">The texture to analyze</param>
	/// <param name="sourceRect">The source rectangle of the displayed area</param>
	/// <returns>The ideal offset from the bottom of the texture area</returns>
	public static int GetYSortOffset(ITexture2D texture, Rectangle? sourceRect = null) {
		if (texture is NoopTexture2D) return 0;
		
		Rectangle src = sourceRect ?? texture.Bounds;

		Color[] texData = new Color[texture.Width * texture.Height];
		texture.GetData(texData);

		Point lastPos = src.Location + src.Size - new Point(1);
		int lastIndex = lastPos.Y * texture.Width + lastPos.X;
		Point firstPos = src.Location;
		int firstIndex = firstPos.Y * texture.Width + firstPos.X;

		int i = lastIndex;
		while (i >= firstIndex) {
			int x = i % texture.Width;

			// OPTIMIZE: this could be optimized, i'm just too tired to figure out the exact jump amount
			if (x < firstPos.X || x > lastPos.X) {
				i--;
				continue;
			}

			if (texData[i].A > 0f) {
				int y = i / texture.Width;
				return src.Height - (lastPos.Y - y);
			}

			i--;
		}

		return 0;
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
	/// Divides a point's components by the given amount
	/// </summary>
	/// <param name="point">The point to divide</param>
	/// <param name="value">The number to divide with</param>
	/// <returns>The divided Point</returns>
	/// <exception cref="DivideByZeroException"></exception>
	public static Point Divide(this Point point, float value) {
		if (value == 0) throw new DivideByZeroException("Cannot divide Point by zero");

		return new Point((int)(point.X / value), (int)(point.Y / value));
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

	/// <summary>
	/// Formats a Rectangle for displaying on screen
	/// </summary>
	/// <param name="rect">The rectangle to format</param>
	/// <returns>The formatted text</returns>
	public static string Format(this Rectangle rect) {
		return $"({rect.X}, {rect.Y}) - ({rect.X + rect.Width}, {rect.Y + rect.Height}): {rect.Size.X}, {rect.Size.Y}";
	}

	/// <summary>
	/// Returns a random value from a given enum
	/// </summary>
	/// <typeparam name="EnumType">The type of the enum</typeparam>
	/// <returns>A random value from the enum's possible values</returns>
	public static EnumType GetRandomEnumValue<EnumType>() where EnumType : Enum {
		Array values = Enum.GetValues(typeof(EnumType));
		return (EnumType)values.GetValue(Game.Random!.Next(values.Length))!;
	}

	/// <summary>
	/// Returns a random position from a bounds Rectangle (edges included)
	/// </summary>
	/// <param name="bounds">The bounds to pick from (inclusive)</param>
	/// <returns>The random position</returns>
	public static Vector2 GetRandomPosition(Rectangle bounds) {
		int x = Game.Random!.Next(bounds.X, bounds.Right + 1);
		int y = Game.Random!.Next(bounds.Y, bounds.Bottom + 1);

		return new Vector2(x, y);
	}
}