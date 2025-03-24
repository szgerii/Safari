using Engine;
using Engine.Debug;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Safari.Model;

public enum RoadState {
	Empty,
	Road, // when calculating routes, this means "unused"

	// For BFS & floodfill algos
	FromTop,
	FromBottom,
	FromLeft,
	FromRight,
	FromNone
}

public class RoadNetwork {
	private int width;
	private int height;
	private RoadState[,] network;
	private Point start;
	private Point end;
	private List<List<Point>> cachedRoutes = new();
	private bool upToDate = false;
	private Random rand = new Random();

	/// <summary>
	/// Retrieve all routes in the network <br />
	/// A route includes both the start point and the end point <br />
	/// (if the network has changed since the last time a route has been
	/// requested, all routes are recalculated) <br />
	/// Note that this could be an empty list
	/// </summary>
	public List<List<Point>> Routes {
		get {
			if (!upToDate) {
				UpdateNetwork();
			}
			return cachedRoutes;
		}
	}

	/// <summary>
	/// Retrieve a random route from the network <br />
	/// A route includes both the start point and the end points <br />
	/// (if the network has changed since the last time a route has been
	/// requested, all routes are recalculated) <br />
	/// Note that his could be an empty list, which means that there are no valid routes
	/// </summary>
	public List<Point> RandomRoute {
		get {
			if (!upToDate) {
				UpdateNetwork();
			}
			return cachedRoutes.Count > 0 ? cachedRoutes[rand.Next(cachedRoutes.Count)] : new List<Point>();
		}
	}

	/// <summary>
	/// The example route used for debugging / presenting
	/// </summary>
	public List<Point> DebugRoute { get; set; } = new List<Point>();
	private static Texture2D debugTexture = null;

	/// <summary>
	/// Use this event any time an object store a route from this network.
	/// This event gets invoked when the extisting, saved routes are invalidated.
	/// </summary>
	public event EventHandler RoadChanged;

	static RoadNetwork() {
		DebugMode.AddFeature(new ExecutedDebugFeature("request-route", () => {
			if (SceneManager.Active is GameScene) {
				RoadNetwork network = GameScene.Active.Model.Level.Network;
				network.DebugRoute = network.RandomRoute;
			}
		}));

		DebugMode.AddFeature(new LoopedDebugFeature("draw-route", (object sender, GameTime gameTime) => {
			if (debugTexture == null) {
				debugTexture = Utils.GenerateTexture(1, 1, Color.DarkCyan);
			}
			if (SceneManager.Active is GameScene) {
				Level level = GameScene.Active.Model.Level;
				List<Point> dr = level.Network.DebugRoute;
				if (dr.Count > 0) {
					for (int i = 1; i < dr.Count; i++) {
						Point a = dr[i - 1];
						Point b = dr[i];
						DrawSegment(a, b, level);
					}
				}
			}
		}, GameLoopStage.POST_DRAW));
	}

	private static void DrawSegment(Point a, Point b, Level level) {
		int width2 = 6;
		Point middleA = new Point(a.X * level.TileSize, a.Y * level.TileSize);
		middleA += new Point(level.TileSize / 2, level.TileSize / 2);
		
		Point middleB = new Point(b.X * level.TileSize, b.Y * level.TileSize);
		middleB += new Point(level.TileSize / 2, level.TileSize / 2);
		Point loc = new Point(0, 0);
		Point size = new Point(0, 0);
		if (a.X == b.X) {
			// vertical
			if (middleA.Y < middleB.Y) {
				loc = middleA - new Point(width2, 0);
				size = new Point(width2 * 2, middleB.Y - middleA.Y);
			} else {
				loc = middleB - new Point(width2, 0);
				size = new Point(width2 * 2, middleA.Y - middleB.Y);
			}
		} else {
			// horizontal
			if (middleA.X < middleB.X) {
				loc = middleA - new Point(0, width2);
				size = new Point(middleB.X - middleA.X, width2 * 2);
			} else {
				loc = middleB - new Point(0, width2);
				size = new Point(middleA.X - middleB.X, width2 * 2);
			}
		}
		Game.SpriteBatch.Draw(debugTexture, new Rectangle(loc, size), Color.White);
	}

	public RoadNetwork(int width, int height, Point start, Point end) {
		this.width = width;
		this.height = height;
		network = new RoadState[width, height];
		SetAt(start, RoadState.Road);
		SetAt(end, RoadState.Road);
		this.start = start;
		this.end = end;
		RoadChanged += (object sender, EventArgs e) => DebugRoute = new List<Point>();
	}

	/// <summary>
	/// Places a road at the given coordinates
	/// </summary>
	/// <param name="x">column in the network</param>
	/// <param name="y">row in the network</param>
	public void AddRoad(int x, int y) {
		if (!BoundsCheck(x, y)) {
			throw new ArgumentException("Given position is outside the bounds of the roadnetwork.");
		}
		if (!GetRoad(x, y)) {
			network[x, y] = RoadState.Road;
			if (upToDate) {
				upToDate = false;
				RoadChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}
	/// <summary>
	/// Places a road at the given coordinates
	/// </summary>
	/// <param name="position">The point in the network</param>
	public void AddRoad(Point position) => AddRoad(position.X, position.Y);

	/// <summary>
	/// Removes a road at the given coordinates
	/// </summary>
	/// <param name="x">column in the network</param>
	/// <param name="y">row in the network</param>
	public void ClearRoad(int x, int y) {
		if (!BoundsCheck(x, y)) {
			throw new ArgumentException("Given position is outside the bounds of the roadnetwork.");
		}
		if (GetRoad(x, y)) {
			network[x, y] = RoadState.Empty;
			if (upToDate) {
				upToDate = false;
				RoadChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}
	/// <summary>
	/// Removes a road at the given
	/// </summary>
	/// <param name="position">The point in the network</param>
	public void ClearRoad(Point position) => ClearRoad(position.X, position.Y);

	/// <summary>
	///	Checks whether a road is present at the given coordinates
	/// </summary>
	/// <param name="x">column in the network</param>
	/// <param name="y">row in the network</param>
	/// <returns>True if a road is present at the coordinates, false otherwise. </returns>
	public bool GetRoad(int x, int y) {
		if (!BoundsCheck(x, y)) {
			throw new ArgumentException("Given position is outside the bounds of the roadnetwork.");
		}
		return network[x, y] != RoadState.Empty;
	}
	/// <summary>
	/// Checks whether a road is present at the given coordinates
	/// </summary>
	/// <param name="position">The point in the network</param>
	/// <returns>True if a road is present at the coordinates, false otherwise.</returns>
	public bool GetRoad(Point position) => GetRoad(position.X, position.Y);

	// check whether the given coordinates are valid
	private bool BoundsCheck(int x, int y) {
		return (
			x >= 0 &&
			y >= 0 &&
			x < width &&
			y < height
		);
	}

	// Reset all used roads to regular Road
	private void NetworkCleanup() {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				if (network[i, j] != RoadState.Empty) {
					network[i, j] = RoadState.Road;
				}
			}
		}
	}

	// Must be called every time a road tile changes
	private void UpdateNetwork() {
		cachedRoutes = new();
		CalculateAllRoutes();
		upToDate = true;
	}

	// Saves the copy of a route to the cached routes
	private void SaveRoute(List<Point> route) {
		cachedRoutes.Add(new List<Point>(route));
	}

	// A collection of helper functions for manipulating the network
	private RoadState StateAt(Point p) => BoundsCheck(p.X, p.Y) ? network[p.X, p.Y] : RoadState.Empty;
	private void SetAt(Point p, RoadState state) => network[p.X, p.Y] = state;
	private bool FreeAt(Point p) => StateAt(p) == RoadState.Road;

	// Calculates a number of different, relatively interesting routes using the middle point method
	private void CalculateAllRoutes() {
		if (!FreeAt(start) || !FreeAt(end)) {
			return;
		}
		HashSet<Point> set = GatherPoints();
		if (!set.Contains(end)) {
			return;
		}
		set.Remove(start);
		set.Remove(end);
		while (set.Count > 0) {
			Point p = PickRandom(set);
			RemoveRange(set, p, 2);
			List<Point> route1 = GetPath(start, p);
			List<Point> route2 = GetPath(p, end);
			List<Point> route = new List<Point>(route1.Count + route2.Count - 1);
			for (int i = 0; i < route1.Count; i++) {
				route.Add(route1[i]);
				RemoveRange(set, route[i], 1);
			}
			for (int i = 1; i < route2.Count; i++) {
				route.Add(route2[i]);
				RemoveRange(set, route2[i], 1);
			}
			SaveRoute(route);
		}
	}

	// Calculates the shortest path between two points
	private List<Point> GetPath(Point from, Point to) {
		Queue<Point> points = new();
		HashSet<Point> used = new();
		points.Enqueue(from);
		SetAt(from, RoadState.FromNone);
		used.Add(from);
		bool finished = false;
		while (points.Count > 0) {
			Point current = points.Dequeue();
			if (current == to) {
				finished = true;
				break;
			}
			Point left = new Point(current.X - 1, current.Y);
			Point right = new Point(current.X + 1, current.Y);
			Point up = new Point(current.X, current.Y - 1);
			Point down = new Point(current.X, current.Y + 1);
			if (FreeAt(left)) {
				SetAt(left, RoadState.FromRight);
				used.Add(left);
				points.Enqueue(left);
			}
			if (FreeAt(right)) {
				SetAt(right, RoadState.FromLeft);
				used.Add(right);
				points.Enqueue(right);
			}
			if (FreeAt(up)) {
				SetAt(up, RoadState.FromBottom);
				used.Add(up);
				points.Enqueue(up);
			}
			if (FreeAt(down)) {
				SetAt(down, RoadState.FromTop);
				used.Add(down);
				points.Enqueue(down);
			}
		}
		Point back = to;
		List<Point> route = new();
		if (!finished) {
			return route;
		}
		while (back != from) {
			route.Add(back);
			switch (StateAt(back)) {
				case RoadState.FromTop:
					back = new Point(back.X, back.Y - 1);
					break;
				case RoadState.FromBottom:
					back = new Point(back.X, back.Y + 1);
					break;
				case RoadState.FromLeft:
					back = new Point(back.X - 1, back.Y);
					break;
				case RoadState.FromRight:
					back = new Point(back.X + 1, back.Y);
					break;
				default:
					throw new Exception("Impossible state reached");
			}
		}
		route.Add(from);
		route.Reverse();
		foreach (Point p1 in used) {
			SetAt(p1, RoadState.Road);
		}
		return route;
	}

	// Pick a random point from a set
	private Point PickRandom(HashSet<Point> set) {
		Point[] points = set.ToArray();
		return points[rand.Next(points.Length)];
	}

	// Remove a point, and its neighbours in a given range, from a set
	private void RemoveRange(HashSet<Point> set, Point p, int range) {
		Queue<(Point, int)> points = new();
		HashSet<Point> used = new();
		points.Enqueue((p, range));
		SetAt(p, RoadState.FromNone);
		while (points.Count > 0) {
			(Point current, int fuel) = points.Dequeue();
			if (set.Contains(current)) {
				set.Remove(current);
			}
			if (!used.Contains(current)) {
				used.Add(current);
			}
			if (fuel > 0) {
				Point left = new Point(current.X - 1, current.Y);
				Point right = new Point(current.X + 1, current.Y);
				Point up = new Point(current.X, current.Y - 1);
				Point down = new Point(current.X, current.Y + 1);
				if (FreeAt(left)) {
					SetAt(left, RoadState.FromRight);
					points.Enqueue((left, fuel - 1));
				}
				if (FreeAt(right)) {
					SetAt(right, RoadState.FromLeft);
					points.Enqueue((right, fuel - 1));
				}
				if (FreeAt(up)) {
					SetAt(up, RoadState.FromBottom);
					points.Enqueue((up, fuel - 1));
				}
				if (FreeAt(down)) {
					SetAt(down, RoadState.FromTop);
					points.Enqueue((down, fuel - 1));
				}
			}
		}
		foreach (Point p1 in used) {
			SetAt(p1, RoadState.Road);
		}
	}

	// Using flood fill, tries to grab all accessible points from 'start'
	private HashSet<Point> GatherPoints() {
		HashSet<Point> result = new();
		Queue<Point> points = new();
		points.Enqueue(start);
		SetAt(start, RoadState.FromNone);
		while (points.Count > 0) {
			Point current = points.Dequeue();
			if (!result.Contains(current)) {
				result.Add(current);
			}
			Point left = new Point(current.X - 1, current.Y);
			Point right = new Point(current.X + 1, current.Y);
			Point up = new Point(current.X, current.Y - 1);
			Point down = new Point(current.X, current.Y + 1);
			if (FreeAt(left)) {
				SetAt(left, RoadState.FromRight);
				points.Enqueue(left);
			}
			if (FreeAt(right)) {
				SetAt(right, RoadState.FromLeft);
				points.Enqueue(right);
			}
			if (FreeAt(up)) {
				SetAt(up, RoadState.FromBottom);
				points.Enqueue(up);
			}
			if (FreeAt(down)) {
				SetAt(down, RoadState.FromTop);
				points.Enqueue(down);
			}
		}
		foreach (Point p in result) {
			SetAt(p, RoadState.Road);
		}
		return result;
	}
}
