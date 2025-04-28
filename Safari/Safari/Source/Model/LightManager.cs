using Engine;
using Engine.Graphics.Stubs.Texture;
using Engine.Interfaces;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Model;

/// <summary>
/// The manager responsible for managing light sources in the level, drawing the lightmap/fog of war
/// to the screen at night and handling the day-night cycle post processing.
/// </summary>
public class LightManager : IPostProcessPass {
	private IRenderTarget2D _output = null;
	private IRenderTarget2D _lightTexture = null;
	private Effect dayNightPass = Game.CanDraw ? Game.ContentManager.Load<Effect>("Fx/dayNightPass") : null;
	private int width;
	private int height;
	private int tileSize;
	private int[,] lightMap;

	IRenderTarget2D IPostProcessPass.Output => _output;
	Effect IPostProcessPass.Shader => dayNightPass;
	ITexture2D red = Utils.GenerateTexture(1, 1, Color.Red);

	public LightManager(int width, int height, int tileSize) {
		this.width = width;
		this.height = height;
		this.tileSize = tileSize;
		this.lightMap = new int[width, height];
	}

	public void PreDraw(GameTime gameTime) {
		EnsureCorrectRT();
		GraphicsDevice device = Game.Graphics.GraphicsDevice;
		device.SetRenderTarget(_lightTexture.ToRenderTarget2D());
		device.Clear(Color.Black);
		Matrix trMatrix = Camera.Active.TransformMatrix;
		Game.SpriteBatch.Begin(
			sortMode: SpriteSortMode.BackToFront,
			samplerState: SamplerState.PointClamp,
			transformMatrix: trMatrix
		);

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				if (CheckLight(x, y)) {
					Game.SpriteBatch.Draw(red.ToTexture2D(), new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
				}
			}
		}

		Game.SpriteBatch.End();

		if (!Game.Instance.IsHeadless) {
			dayNightPass.Parameters["Time"].SetValue((float)GameScene.Active.Model.TimeOfDay);
			dayNightPass.Parameters["sunrise_start"].SetValue((float)GameModel.SUNRISE_START);
			dayNightPass.Parameters["sunrise_end"].SetValue((float)GameModel.SUNRISE_END);
			dayNightPass.Parameters["sunset_start"].SetValue((float)GameModel.SUNSET_START);
			dayNightPass.Parameters["sunset_end"].SetValue((float)GameModel.SUNSET_END);
			dayNightPass.Parameters["LightMap"].SetValue(_lightTexture.ToRenderTarget2D());
		}
	}

	private void EnsureCorrectRT() {
		CorrectSizeRT(ref _output);
		CorrectSizeRT(ref _lightTexture);
	}

	private void CorrectSizeRT(ref IRenderTarget2D rt) {
		if (rt == null || Game.RenderTarget.Width != rt.Width || Game.RenderTarget.Height != rt.Height) {
			if (Game.Instance.IsHeadless) {
				rt = new NoopRenderTarget2D(Game.Graphics.GraphicsDevice, Game.RenderTarget.Width, Game.RenderTarget.Height);
			} else {
				rt = new RenderTarget2DAdapter(new(Game.Graphics.GraphicsDevice, Game.RenderTarget.Width, Game.RenderTarget.Height));
			}
		}
	}
	/// <summary>
	/// Adds a light source at the given coordinates
	/// <param name="x">the column in the level</param>
	/// <param name="y">the row in the level</param>
	/// <param name="range">how far the light should reach</param>
	/// </summary>
	public void AddLightSource(int x, int y, int range) => ModifyLightmap(x, y, range, 1);
	/// <summary>
	/// Adds a light source at the given coordinates
	/// </summary>
	/// <param name="p">the point in the level</param>
	/// <param name="range">how far the light should reach</param>
	public void AddLightSource(Point p, int range) => AddLightSource(p.X, p.Y, range);

	/// <summary>
	/// Removes a light source at the given coordinates
	/// </summary>
	/// <param name="x">the column in the level</param>
	/// <param name="y">the row in the level</param>
	/// <param name="range">light range</param>
	public void RemoveLightSource(int x, int y, int range) => ModifyLightmap(x, y, range, -1);
	/// <summary>
	/// Removes a light source at the given coordinates
	/// </summary>
	/// <param name="p">the point in the level the light used to be</param>
	/// <param name="range">how far the light used to reach</param>
	public void RemoveLightsource(Point p, int range) => RemoveLightSource(p.X, p.Y, range);

	/// <summary>
	/// Check whether a given point in the level is lit
	/// </summary>
	/// <param name="x">the column in the level</param>
	/// <param name="y">the row in the level</param>
	public bool CheckLight(int x, int y) => lightMap[x, y] > 0;
	/// <summary>
	/// Check whether a given point in the level is lit
	/// </summary>
	/// <param name="p">the point in the level</param>
	public bool CheckLight(Point p) => CheckLight(p.X, p.Y);

	private void ModifyLightmap(int x, int y, int range, int amount) {
		if (!BoundsCheck(x, y)) {
			throw new ArgumentException("Given position is outside the bounds of the lightmap.");
		}
		Point start = new Point(x, y);
		HashSet<Point> points = new();
		Queue<(Point, int)> queue = new();
		points.Add(start);
		lightMap[x, y] += amount;
		queue.Enqueue((start, range));
		while (queue.Count > 0) {
			(Point current, int fuel) = queue.Dequeue();
			if (!points.Contains(current)) {
				points.Add(current);
				lightMap[current.X, current.Y] += amount;
			}
			if (fuel > 0) {
				Point left = new Point(current.X - 1, current.Y);
				Point right = new Point(current.X + 1, current.Y);
				Point up = new Point(current.X, current.Y - 1);
				Point down = new Point(current.X, current.Y + 1);
				if (BoundsCheck(left) && !points.Contains(left)) {
					queue.Enqueue((left, fuel - 1));
				}
				if (BoundsCheck(right) && !points.Contains(right)) {
					queue.Enqueue((right, fuel - 1));
				}
				if (BoundsCheck(up) && !points.Contains(up)) {
					queue.Enqueue((up, fuel - 1));
				}
				if (BoundsCheck(down) && !points.Contains(down)) {
					queue.Enqueue((down, fuel - 1));
				}
			}
		}
	}

	private bool BoundsCheck(Point p) => BoundsCheck(p.X, p.Y);
	private bool BoundsCheck(int x, int y) {
		return (
			x >= 0 &&
			y >= 0 &&
			x < width &&
			y < height
		);
	}
}
