using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.Marshalling;

namespace Safari.Model;

public enum RoadState {
	Empty,
	Road, // when calculating routes, this means "unused"
	
	// only used during route calculations
	UsedUp, 
	UsedRight,
	UsedDown,
	UsedLeft
}

public class RoadNetwork {
	private int width;
	private int height;
	private RoadState[,] network;
	private Point start;
	private Point end;
	private List<List<Point>> cachedRoutes;

	public RoadNetwork(int width, int height, Point start, Point end) {
		this.width = width;
		this.height = height;
		network = new RoadState[width, height];
		SetAt(start, RoadState.Road);
		SetAt(end, RoadState.Road);
		this.start = start;
		this.end = end;
		cachedRoutes = new();
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
			//UpdateNetwork();
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
			UpdateNetwork();
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
			y >=0 &&
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
	public void UpdateNetwork() {
		cachedRoutes = new();
		CalculateAllRoutes();
		NetworkCleanup();
	}

	private void SaveRoute(List<Point> route) {
		cachedRoutes.Add(new List<Point>(route));
	}

	private RoadState StateAt(Point p) => BoundsCheck(p.X, p.Y) ? network[p.X, p.Y] : RoadState.Empty;

	private void SetAt(Point p, RoadState state) => network[p.X, p.Y] = state;

	// Iterate through all possible valid, acyclic routes from the start point to the end point, 
	// and save all completed routes into cachedRoutes
	private void CalculateAllRoutes() {
		if (StateAt(start) != RoadState.Road || StateAt(end) != RoadState.Road) {
			return;
		}
		Point current = start; // The point the algorithm is currently on
		List<Point> route = new List<Point>() { current };
		do {
			if (current == end) {
				// save current route and step back (maybe maxroute, maybe primary and secondary direction)
				SaveRoute(route);

				route.RemoveAt(route.Count - 1);
				current = route.Last();
				continue;
			}

			// try stepping up
			if (StateAt(current) == RoadState.Road) {
				Point next = new Point(current.X, current.Y - 1);
				SetAt(current, RoadState.UsedUp);
				if (StateAt(next) == RoadState.Road) {
					current = next;
					route.Add(current);
					continue;
				}
			}
			// try stepping right
			if (StateAt(current) == RoadState.UsedUp) {
				Point next = new Point(current.X + 1, current.Y);
				SetAt(current, RoadState.UsedRight);
				if (StateAt(next) == RoadState.Road) {
					current = next;
					route.Add(current);
					continue;
				}
			}
			// try stepping down
			if (StateAt(current) == RoadState.UsedRight) {
				Point next = new Point(current.X, current.Y + 1);
				SetAt(current, RoadState.UsedDown);
				if (StateAt(next) == RoadState.Road) {
					current = next;
					route.Add(current);
					continue;
				}
			}
			// try stepping left
			if (StateAt(current) == RoadState.UsedDown) {
				Point next = new Point(current.X - 1, current.Y);
				SetAt(current, RoadState.UsedLeft);
				if (StateAt(next) == RoadState.Road) {
					current = next;
					route.Add(current);
					continue;
				}
			}

			// step back, all routes from the current route have been discovered
			if (StateAt(current) == RoadState.UsedLeft && current != start) {
				SetAt(current, RoadState.Road);
				route.RemoveAt(route.Count - 1);
				current = route.Last();
			}
		} while (current != start || StateAt(current) != RoadState.UsedLeft);
	}
}
