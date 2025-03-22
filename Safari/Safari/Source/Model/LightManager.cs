using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Model;

public class LightManager : IPostProcessPass {
	private RenderTarget2D _output = null;
	private Effect dayNightPass = Game.ContentManager.Load<Effect>("Fx/dayNightPass");
	private int width;
	private int height;
	private int[,] lightMap;

	RenderTarget2D IPostProcessPass.Output => _output;
	Effect IPostProcessPass.Shader => dayNightPass;

	public LightManager(int width, int height) {
		this.width = width;
		this.height = height;
		this.lightMap = new int[width, height];
	}

	public void PreDraw(GameTime gameTime) {
		EnsureCorrectRT();
		dayNightPass.Parameters["Time"].SetValue((float)GameScene.Active.Model.TimeOfDay);
	}

	private void EnsureCorrectRT() {
		if (_output == null || Game.RenderTarget.Width != _output.Width || Game.RenderTarget.Height != _output.Height) {
			_output = new RenderTarget2D(Game.Graphics.GraphicsDevice, Game.RenderTarget.Width, Game.RenderTarget.Height);
		}
	}

	public void AddLightSource(int x, int y, int range) => ModifyLightmap(x, y, range, 1);
	public void AddLightSource(Point p, int range) => AddLightSource(p.X, p.Y, range);

	public void RemoveLightSource(int x, int y, int range) => ModifyLightmap(x, y, range, -1);
	public void RemoveLightsource(Point p, int range) => RemoveLightSource(p.X, p.Y, range);

	public bool CheckLight(int x, int y) => lightMap[x, y] > 0;
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
