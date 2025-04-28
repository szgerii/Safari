using Engine;
using Microsoft.Xna.Framework;
using Safari.Debug;
using Safari.Model;
using Safari.Model.Tiles;
using System;
using System.Collections.Generic;

namespace Safari.Components;

/// <summary>
/// Pre-generates tile instances for a specific slot in the palette
/// </summary>
public class PaletteItem {
	public Tile Instance { get; private set; }
	/// <summary>
	/// The number for variants this item has (for example: treetypes)
	/// </summary>
	public int VariantCount { get; private set; }
	private int variantChoice = 0;
	/// <summary>
	/// The variant of the tile currently used
	/// </summary>
	public int VariantChoice {
		get => variantChoice;
		set {
			int prevVariant = variantChoice;
			variantChoice = value;
			if (prevVariant != variantChoice) {
				PrepareNext();
			}
		}
	}
	private Func<int, Tile> prepareNextInstance;

	/// <summary>
	/// Creates a palette item which generates tiles using the given function
	/// </summary>
	public PaletteItem(Func<int, Tile> prepareNextInstance, int variantCount = 1) {
		this.prepareNextInstance = prepareNextInstance;
		VariantCount = variantCount;
		PrepareNext();
	}

	/// <summary>
	/// Generates a new tile for placing
	/// </summary>
	public void PrepareNext() {
		Instance = prepareNextInstance(variantChoice);
	}

	/// <summary>
	/// Select the next variant in order
	/// </summary>
	public void SelectNext() {
		int n = VariantChoice;
		n++;
		if (n >= VariantCount) {
			n = 0;
		}
		VariantChoice = n;
	}

	/// <summary>
	/// Select the previous variant in order
	/// </summary>
	public void SelectPrev() {
		int n = VariantChoice;
		n--;
		if (n < 0) {
			n = VariantCount - 1;
		}
		VariantChoice = n;
	}
}

public class ConstructionHelperCmp : Component, IUpdatable {
	private int width;
	private int height;
	private bool[,] mapStatus;

	private Level Level => Owner as Level;
	public const int ROAD = 0;
	public const int GRASS = 1;
	public const int WATER = 2;
	public const int BUSH = 3;
	public const int TREE = 4;
	/// <summary>
	/// The palette that this comp can build (see the constants for specific indexes)
	/// </summary>
	public PaletteItem[] Palette { get; init; } = [
		new PaletteItem(_ => new Road()),
		new PaletteItem(_ => new Grass()),
		new PaletteItem(_ => new Water()),
		new PaletteItem(i => i == 0 ? new Bush() : new WideBush(), 2),
		new PaletteItem(i => new Tree((TreeType)i), Enum.GetValues(typeof(TreeType)).Length)
	];
	/// <summary>
	/// The currently selected index in the palette (set to -1 for nothing)
	/// </summary>
	public int SelectedIndex { get; set; }
	private List<Point> unbreakable = new List<Point>();

	/// <summary>
	/// Fetch the currently selected palette item
	/// </summary>
	public PaletteItem SelectedItem {
		get {
			if (SelectedIndex >= 0) {
				return Palette[SelectedIndex];
			} else {
				return null;
			}
		}
	}

	/// <summary>
	/// Fetch the tile instance of the currently selected item in the palette
	/// </summary>
	public Tile SelectedInstance {
		get {
			if (SelectedIndex >= 0) {
				return Palette[SelectedIndex].Instance;
			} else {
				return null;
			}
		}
	}

	

	/// <summary>
	/// Initializes the component with the size of the level (in tiles)
	/// </summary>
	public ConstructionHelperCmp(int width, int height) {
		this.width = width;
		this.height = height;
		mapStatus = new bool[width, height];
	}

	public override void Load() {
		Occupy(Level.Network.Start);
		Occupy(Level.Network.End);
		unbreakable.Add(Level.Network.Start);
		unbreakable.Add(Level.Network.End);
		base.Load();
	}

	/// <summary>
	/// Prints the current brush to the debug info panel
	/// </summary>
	private int lastIndex = -1;
	private string currentBrushStr = "";
	public void Update(GameTime gameTime) {
		if (SelectedIndex != lastIndex) {
			foreach (var field in typeof(ConstructionHelperCmp).GetFields()) {
				if ((int)field.GetValue(null) == SelectedIndex) {
					currentBrushStr = field.Name;
				}
			}

			lastIndex = SelectedIndex;
		}

		DebugInfoManager.AddInfo("Current brush", currentBrushStr, DebugInfoPosition.BottomRight);
	}

	/// <summary>
	/// Checks whether a tile can be built at the given coordinates
	/// </summary>
	public bool CanBuild(Point p, Tile tile) {
		if (!Check(p)) {
			return false;
		}
		foreach (Point offset in tile.ConstructionBlockOffsets) {
			if (!Check(p + offset)) {
				return false;
			}
		}
		return true;
	}
	/// <summary>
	/// Checks whether a tile can be built at the given coordinates
	/// </summary>
	public bool CanBuild(int x, int y, Tile tile) => CanBuild(new(x, y), tile);

	/// <summary>
	/// Checks whether the selected tile can be built at the given coordinates
	/// </summary>
	public bool CanBuildCurrent(int x, int y) => SelectedInstance != null ? CanBuild(x, y, SelectedInstance) : false;

	/// <summary>
	/// Checks whether the selected tile can be built at the given coordinates
	/// </summary>
	public bool CanBuildCurrent(Point p) => CanBuildCurrent(p.X, p.Y);

	/// <summary>
	/// Select the next option in the palette in order
	/// </summary>
	public void SelectNext() {
		if (SelectedIndex != -1) {
			SelectedIndex++;
			if (SelectedIndex >= Palette.Length) {
				SelectedIndex = 0;
			}
		}
	}

	/// <summary>
	/// Select the previous option in the palette in order
	/// </summary>
	public void SelectPrev() {
		if (SelectedIndex != -1) {
			SelectedIndex--;
			if (SelectedIndex < 0) {
				SelectedIndex = Palette.Length - 1;
			}
		}
	}

	/// <summary>
	/// Places a tile at the given coordinates, ignoring the palette selection
	/// </summary>
	public void InstaBuild(Point p, Tile tile) {
		if (!CanBuild(p, tile)) {
			return;
		}
		Level.SetTile(p, tile);
		Occupy(p);
		foreach (Point offset in tile.ConstructionBlockOffsets) {
			Occupy(p + offset);
		}
	}
	/// <summary>
	/// Places a tile at the given coordinates, ignoring the palette selection
	/// </summary>
	public void InstaBuild(int x, int y, Tile tile) => InstaBuild(new(x, y), tile);

	/// <summary>
	/// Attempts placing the currently selected tile at the given coordinates
	/// </summary>
	public void BuildCurrent(Point p) {
		Tile tile = SelectedInstance;
		if (tile == null || !CanBuild(p, tile)) {
			return;
		}
		Level.SetTile(p, tile);
		Occupy(p);
		foreach (Point offset in tile.ConstructionBlockOffsets) {
			Occupy(p + offset);
		}
		Palette[SelectedIndex].PrepareNext();
	}
	/// <summary>
	/// Attempts placing the currently selected tile at the given coordinates
	/// </summary>
	public void BuildCurrent(int x, int y) => BuildCurrent(new(x, y));

	/// <summary>
	/// Attempts demolishing the the tile at the given coordinates
	/// </summary>
	public void Demolish(Point p) {
		if (Level.IsOutOfPlayArea(p.X, p.Y)) {
			return;
		}
		Tile tile = Level.GetTile(p);
		if (tile == null || unbreakable.Contains(p)) {
			return;
		}
		Level.ClearTile(p);
		Free(p);
		foreach (Point offset in tile.ConstructionBlockOffsets) {
			if (Level.IsOutOfPlayArea((p + offset).X, (p + offset).Y)) {
				continue;
			}
			Free(p + offset);
		}
	}
	/// <summary>
	/// Attempts demolishing the the tile at the given coordinates
	/// </summary>
	public void Demolish(int x, int y) => Demolish(new(x, y));

	private void Occupy(int x, int y) {
		mapStatus[x, y] = true;
	}

	private void Occupy(Point p) => Occupy(p.X, p.Y);

	private void Free(int x, int y) {
		mapStatus[x, y] = false;
	}

	private void Free(Point p) => Free(p.X, p.Y);

	private bool Check(int x, int y) {
		return !Level.IsOutOfPlayArea(x, y) && !mapStatus[x, y];
	}

	private bool Check(Point p) => Check(p.X, p.Y);
}
