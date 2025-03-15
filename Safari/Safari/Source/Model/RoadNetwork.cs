namespace Safari.Model;

public struct TilePos {
	public int Row { get; set; }
	public int Col { get; set; }

	public TilePos(int row, int col) {
		Row = row;
		Col = col;
	}
}

public class RoadNetwork {
	private int width;
	private int height;
	private int[,] network;
	private TilePos start;
	private TilePos end;

	public RoadNetwork(int width, int height, TilePos start, TilePos end) {
		this.width = width;
		this.height = height;
		network = new int[height, width];
		this.start = start;
		this.end = end;
	}

	private void UpdateNetwork() {

	}

	public void SetRoad(TilePos position, bool value) {
		network[position.Row, position.Col] = value ? 1 : 0;
		UpdateNetwork();
	}

	public bool GetRoad(TilePos position) {
		return network[position.Row, position.Col] >= 1;
	}
}
