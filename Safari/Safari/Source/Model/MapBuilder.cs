using Microsoft.Xna.Framework;
using Safari.Model.Tiles;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Model.Entities.Tourists;
using System.Collections.Generic;
using System.IO;

namespace Safari.Model;

/// <summary>
/// Static helper class that can generate the starting map
/// </summary>
public static class MapBuilder {
	public readonly static int ZEBRA_COUNT = 8;
	public readonly static int GIRAFFE_COUNT = 4;
	public readonly static int ELEPHANT_COUNT = 4;
	public readonly static int LION_COUNT = 4;
	public readonly static int TIGER_COUNT = 3;
	public readonly static int TIGER_WHITE_COUNT = 3;

	public readonly static IReadOnlyList<Point> LAKE_LOC = new List<Point>() {
		new Point(13, 42),
		new Point(13, 43),
		new Point(13, 44),
		new Point(14, 41),
		new Point(14, 42),
		new Point(14, 43),
		new Point(14, 44),
		new Point(15, 41),
		new Point(15, 42),
		new Point(15, 43),
		new Point(15, 44),
		new Point(16, 41),
		new Point(16, 42),
		new Point(16, 43),
		new Point(29, 19),
		new Point(29, 20),
		new Point(29, 21),
		new Point(29, 22),
		new Point(30, 18),
		new Point(30, 19),
		new Point(30, 20),
		new Point(30, 21),
		new Point(30, 22),
		new Point(30, 23),
		new Point(31, 18),
		new Point(31, 19),
		new Point(31, 20),
		new Point(31, 21),
		new Point(31, 22),
		new Point(31, 23),
		new Point(32, 18),
		new Point(32, 19),
		new Point(32, 20),
		new Point(32, 21),
		new Point(32, 22),
		new Point(32, 23),
		new Point(33, 18),
		new Point(33, 19),
		new Point(33, 20),
		new Point(33, 21),
		new Point(33, 22),
		new Point(33, 23),
		new Point(34, 19),
		new Point(34, 20),
		new Point(34, 21),
		new Point(34, 22),
		new Point(54, 10),
		new Point(54, 11),
		new Point(55, 10),
		new Point(55, 11),
		new Point(55, 12),
		new Point(55, 13),
		new Point(56, 10),
		new Point(56, 11),
		new Point(56, 12),
		new Point(56, 13),
		new Point(56, 14),
		new Point(56, 15),
		new Point(57, 12),
		new Point(57, 13),
		new Point(57, 14),
		new Point(57, 15),
		new Point(57, 16),
		new Point(58, 15),
		new Point(58, 16),
		new Point(67, 32),
		new Point(67, 33),
		new Point(68, 32),
		new Point(68, 33),
		new Point(69, 33),
		new Point(70, 33),
		new Point(71, 32),
		new Point(71, 33),
		new Point(72, 32),
		new Point(72, 33),
		new Point(73, 32),
		new Point(73, 33),
		new Point(74, 31),
		new Point(74, 32),
		new Point(74, 33),
		new Point(75, 31),
		new Point(75, 32),
		new Point(76, 31),
		new Point(76, 32),
		new Point(77, 31),
		new Point(77, 32),
		new Point(78, 31),
		new Point(78, 32),
		new Point(79, 31),
		new Point(79, 32),
		new Point(80, 31),
		new Point(80, 32),
		new Point(81, 31),
		new Point(81, 32),
		new Point(82, 31),
		new Point(82, 32),
		new Point(83, 31),
		new Point(83, 32),
		new Point(84, 29),
		new Point(84, 30),
		new Point(84, 31),
		new Point(84, 32),
		new Point(85, 28),
		new Point(85, 29),
		new Point(85, 30),
		new Point(85, 31),
		new Point(85, 32),
		new Point(85, 33),
		new Point(86, 28),
		new Point(86, 29),
		new Point(86, 30),
		new Point(86, 31),
		new Point(86, 32),
		new Point(86, 33),
		new Point(87, 28),
		new Point(87, 29),
		new Point(87, 30),
		new Point(87, 31),
		new Point(87, 32),
		new Point(87, 33),
		new Point(88, 28),
		new Point(88, 29),
		new Point(88, 30),
		new Point(88, 31),
		new Point(88, 32),
		new Point(88, 33),
		new Point(89, 32),
		new Point(89, 33),
		new Point(90, 32),
		new Point(90, 33),
	};
	public readonly static IReadOnlyList<Point> GRASS_LOC = new List<Point>() {
		new Point(12, 10),
		new Point(12, 11),
		new Point(12, 12),
		new Point(13, 10),
		new Point(13, 11),
		new Point(13, 12),
		new Point(13, 13),
		new Point(13, 14),
		new Point(14, 9),
		new Point(14, 10),
		new Point(14, 11),
		new Point(14, 12),
		new Point(14, 13),
		new Point(14, 14),
		new Point(14, 15),
		new Point(15, 9),
		new Point(15, 10),
		new Point(15, 11),
		new Point(15, 12),
		new Point(15, 13),
		new Point(15, 14),
		new Point(15, 15),
		new Point(16, 9),
		new Point(16, 10),
		new Point(16, 11),
		new Point(16, 12),
		new Point(16, 13),
		new Point(16, 14),
		new Point(17, 10),
		new Point(17, 11),
		new Point(17, 12),
		new Point(17, 13),
		new Point(17, 14),
		new Point(18, 11),
		new Point(18, 12),
		new Point(18, 13),
		new Point(27, 44),
		new Point(28, 43),
		new Point(28, 44),
		new Point(28, 45),
		new Point(29, 43),
		new Point(29, 44),
		new Point(29, 45),
		new Point(29, 46),
		new Point(30, 43),
		new Point(30, 44),
		new Point(30, 45),
		new Point(30, 46),
		new Point(31, 43),
		new Point(31, 44),
		new Point(31, 45),
		new Point(31, 46),
		new Point(32, 44),
		new Point(32, 45),
		new Point(33, 44),
		new Point(33, 45),
		new Point(39, 9),
		new Point(39, 10),
		new Point(39, 11),
		new Point(39, 12),
		new Point(40, 9),
		new Point(40, 10),
		new Point(40, 11),
		new Point(40, 12),
		new Point(41, 10),
		new Point(41, 11),
		new Point(41, 12),
		new Point(46, 36),
		new Point(46, 37),
		new Point(46, 38),
		new Point(46, 39),
		new Point(46, 40),
		new Point(47, 35),
		new Point(47, 36),
		new Point(47, 37),
		new Point(47, 38),
		new Point(47, 39),
		new Point(47, 40),
		new Point(48, 35),
		new Point(48, 36),
		new Point(48, 37),
		new Point(48, 38),
		new Point(48, 39),
		new Point(48, 40),
		new Point(49, 36),
		new Point(49, 37),
		new Point(49, 38),
		new Point(49, 39),
		new Point(49, 40),
		new Point(66, 19),
		new Point(66, 20),
		new Point(66, 21),
		new Point(66, 22),
		new Point(67, 17),
		new Point(67, 18),
		new Point(67, 19),
		new Point(67, 20),
		new Point(67, 21),
		new Point(67, 22),
		new Point(67, 23),
		new Point(68, 17),
		new Point(68, 18),
		new Point(68, 19),
		new Point(68, 20),
		new Point(68, 21),
		new Point(68, 22),
		new Point(68, 23),
		new Point(69, 17),
		new Point(69, 18),
		new Point(69, 19),
		new Point(69, 20),
		new Point(69, 21),
		new Point(69, 22),
		new Point(69, 23),
		new Point(70, 18),
		new Point(70, 19),
		new Point(70, 20),
		new Point(70, 21),
		new Point(80, 40),
		new Point(80, 41),
		new Point(80, 42),
		new Point(81, 38),
		new Point(81, 39),
		new Point(81, 40),
		new Point(81, 41),
		new Point(81, 42),
		new Point(81, 43),
		new Point(82, 38),
		new Point(82, 39),
		new Point(82, 40),
		new Point(82, 41),
		new Point(82, 42),
		new Point(82, 43),
		new Point(83, 37),
		new Point(83, 38),
		new Point(83, 39),
		new Point(83, 40),
		new Point(83, 41),
		new Point(83, 42),
		new Point(83, 43),
		new Point(83, 44),
		new Point(84, 37),
		new Point(84, 38),
		new Point(84, 39),
		new Point(84, 40),
		new Point(84, 41),
		new Point(84, 42),
		new Point(84, 43),
		new Point(84, 44),
		new Point(85, 38),
		new Point(85, 39),
		new Point(85, 40),
		new Point(85, 41),
		new Point(85, 42),
		new Point(85, 43),
		new Point(85, 44),
		new Point(86, 38),
		new Point(86, 39),
		new Point(86, 40),
		new Point(86, 41),
		new Point(86, 42),
		new Point(86, 43),
		new Point(86, 44),
		new Point(87, 38),
		new Point(87, 39),
		new Point(87, 40),
		new Point(87, 41),
		new Point(87, 42),
		new Point(88, 39),
		new Point(88, 40),
	};
	public readonly static IReadOnlyList<Point> BUSH_LOC = new List<Point>() {
		new Point(24, 25),
		new Point(35, 38),
	};
	public readonly static IReadOnlyList<Point> WBUSH_LOC = new List<Point>() {
		new Point(54, 23),
		new Point(89, 14),
		new Point(91, 14),
	};
	public readonly static IReadOnlyList<(Point, TreeType)> TREE_LOC = new List<(Point, TreeType)>() {
		(new Point(10, 15), TreeType.Za),
		(new Point(18, 30), TreeType.Grandideri),
		(new Point(55, 34), TreeType.Gregorii),
		(new Point(84, 50), TreeType.Grandideri),
	};

	/// <summary>
	/// Dumps the current map tiles into a text file (formated for copy pasting it here)
	/// </summary>
	public static void DumpMap(Level level) {
		List<Tile> tiles = level.GetTilesInArea(new Rectangle(0, 0, level.MapWidth, level.MapHeight));
		using (StreamWriter sw = new StreamWriter("map_dump.txt")) {
			sw.WriteLine("public readonly static IReadOnlyList<Point> LAKE_LOC = new List<Point>() {");
			foreach (Tile tile in tiles) {
				if (tile is Water) {
					sw.WriteLine($"\tnew Point({tile.TilemapPosition.X}, {tile.TilemapPosition.Y}),");
				}
			}
			sw.WriteLine("};");
			sw.WriteLine("public readonly static IReadOnlyList<Point> GRASS_LOC = new List<Point>() {");
			foreach (Tile tile in tiles) {
				if (tile is Grass) {
					sw.WriteLine($"\tnew Point({tile.TilemapPosition.X}, {tile.TilemapPosition.Y}),");
				}
			}
			sw.WriteLine("};");
			sw.WriteLine("public readonly static IReadOnlyList<Point> BUSH_LOC = new List<Point>() {");
			foreach (Tile tile in tiles) {
				if (tile is Bush) {
					sw.WriteLine($"\tnew Point({tile.TilemapPosition.X}, {tile.TilemapPosition.Y}),");
				}
			}
			sw.WriteLine("};");
			sw.WriteLine("public readonly static IReadOnlyList<Point> WBUSH_LOC = new List<Point>() {");
			foreach (Tile tile in tiles) {
				if (tile is WideBush) {
					sw.WriteLine($"\tnew Point({tile.TilemapPosition.X}, {tile.TilemapPosition.Y}),");
				}
			}
			sw.WriteLine("};");
			sw.WriteLine("public readonly static IReadOnlyList<(Point, TreeType)> TREE_LOC = new List<(Point, TreeType)>() {");
			foreach (Tile tile in tiles) {
				if (tile is Tree) {
					sw.WriteLine($"\t(new Point({tile.TilemapPosition.X}, {tile.TilemapPosition.Y}), TreeType.Digitata),");
				}
			}
			sw.WriteLine("};");
		}
	}

	/// <summary>
	/// Builds the starting map (based on the const lists)
	/// </summary>
	/// <param name="strippedInit">Whether to skip placing down animals/plants/jeeps/etc</param>
	public static void BuildStartingMap(Level level, bool strippedInit = false, bool loadInit = false) {
		// entrance / exit placement
		level.SetTile(level.Network.Start, new Road());
		level.SetTile(level.Network.End, new Road());
		Point current = new Point(-1, level.Network.Start.Y);
		Jeep.GarageSpot = new Point(-2, level.Network.Start.Y);
		Jeep.PickUpSpot = level.Network.Start - new Point(2, 0);
		Jeep.DropOffSpot = level.Network.End - new Point(0, 2);
		Tourist.PickupSpot = Jeep.PickUpSpot - new Point(0, 1);
		while (current.X < level.Network.Start.X - 1) {
			current.X++;
			Road r = new Road();
			level.SetTile(current, r);
		}
		current = new Point(level.Network.End.X, -1);
		while (current.Y < level.Network.End.Y - 1) {
			current.Y++;
			Road r = new Road();
			level.SetTile(current, r);
		}

		// fence placement
		int x = Level.PLAY_AREA_CUTOFF_X - 1;
		int y = Level.PLAY_AREA_CUTOFF_Y - 1;
		while (x < level.MapWidth - Level.PLAY_AREA_CUTOFF_X) {
			if (level.GetTile(x, y) == null) {
				level.SetTile(x, y, new Fence());
			}
			x++;
		}
		while (y < level.MapHeight - Level.PLAY_AREA_CUTOFF_Y) {
			if (level.GetTile(x, y) == null) {
				level.SetTile(x, y, new Fence());
			}
			y++;
		}
		while (x > Level.PLAY_AREA_CUTOFF_X - 1) {
			if (level.GetTile(x, y) == null) {
				level.SetTile(x, y, new Fence());
			}
			x--;
		}
		while (y > Level.PLAY_AREA_CUTOFF_Y - 1) {
			if (level.GetTile(x, y) == null) {
				level.SetTile(x, y, new Fence());
			}
			y--;
		}

		if (!loadInit) {
			// starting road
			current = level.Network.Start;
			while (current.X < level.Network.End.X - 1) {
				current.X++;
				Road r = new Road();
				level.ConstructionHelperCmp.InstaBuild(current, r);
			}
			current = level.Network.End;
			while (current.Y < level.Network.Start.Y) {
				current.Y++;
				Road r = new Road();
				level.ConstructionHelperCmp.InstaBuild(current, r);
			}
		}

		if (!strippedInit && !loadInit) {
			AddStartingObjects(level);
		}
	}

	private static void AddStartingObjects(Level level) {
		// food / water sources
		foreach (Point p in LAKE_LOC) {
			level.ConstructionHelperCmp.InstaBuild(p, new Water());
		}

		foreach (Point p in GRASS_LOC) {
			level.ConstructionHelperCmp.InstaBuild(p, new Grass());
		}

		foreach (Point p in BUSH_LOC) {
			level.ConstructionHelperCmp.InstaBuild(p, new Bush());
		}

		foreach (Point p in WBUSH_LOC) {
			level.ConstructionHelperCmp.InstaBuild(p, new WideBush());
		}

		foreach ((Point p, TreeType type) in TREE_LOC) {
			level.ConstructionHelperCmp.InstaBuild(p, new Tree(type));
		}

		// animals (at random positions)
		for (int i = 0; i < ZEBRA_COUNT; i++) {
			Game.AddObject(new Zebra(GetRandomSpawn(level, AnimalSpecies.Lion.GetSize()), IntegerToGender(i)));
		}
		for (int i = 0; i < GIRAFFE_COUNT; i++) {
			Game.AddObject(new Giraffe(GetRandomSpawn(level, AnimalSpecies.Giraffe.GetSize()), IntegerToGender(i)));
		}
		for (int i = 0; i < ELEPHANT_COUNT; i++) {
			Game.AddObject(new Elephant(GetRandomSpawn(level, AnimalSpecies.Elephant.GetSize()), IntegerToGender(i)));
		}
		for (int i = 0; i < LION_COUNT; i++) {
			Game.AddObject(new Lion(GetRandomSpawn(level, AnimalSpecies.Lion.GetSize()), IntegerToGender(i)));
		}
		for (int i = 0; i < TIGER_COUNT; i++) {
			Game.AddObject(new Tiger(GetRandomSpawn(level, AnimalSpecies.Tiger.GetSize()), IntegerToGender(i)));
		}
		for (int i = 0; i < TIGER_WHITE_COUNT; i++) {
			Game.AddObject(new TigerWhite(GetRandomSpawn(level, AnimalSpecies.TigerWhite.GetSize()), IntegerToGender(i)));
		}

		for (int i = 0; i < Jeep.STARTING_JEEPS; i++) {
			Jeep.SpawnJeep();
		}
	}

	private static Gender IntegerToGender(int i) => i % 2 == 0 ? Gender.Female : Gender.Male;

	public static Vector2 GetRandomSpawn(Level level, Point? size = null) {
		size ??= new Point(1);

		int minTX = Level.PLAY_AREA_CUTOFF_X + 3;
		int maxTX = level.MapWidth - Level.PLAY_AREA_CUTOFF_X - 3;
		int minTY = Level.PLAY_AREA_CUTOFF_Y + 3;
		int maxTY = level.MapHeight - Level.PLAY_AREA_CUTOFF_Y - 3;

		Point[] offsets = new Point[size.Value.X * size.Value.Y - 1];
		for (int x = 0; x < size.Value.X; x++) {
			for (int y = 0; y < size.Value.Y; y++) {
				if (x == 0 && y == 0) continue;

				offsets[x * size.Value.Y + y - 1] = new Point(x, y);
			}
		}

		Point p = new Point();
		do {
			p = new Point(Game.Random.Next(minTX, maxTX), Game.Random.Next(minTY, maxTY));
		} while (!level.ConstructionHelperCmp.CanBuild(p, offsets));

		return new Vector2(p.X * level.TileSize, p.Y * level.TileSize);
	}
}
