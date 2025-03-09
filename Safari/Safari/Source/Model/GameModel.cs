namespace Safari.Model;

public enum GameDifficulty {
	Easy,
	Normal,
	Hard
}

public class GameModel {
	private string parkName;
	private int funds;
	private GameDifficulty difficulty;
	private int tourPrice;

	public string ParkName => parkName;
	public int Funds {
		get => funds;
		set {
			funds = value;
		}
	}
	public GameDifficulty Difficulty => difficulty;
	public int TourPrice {
		get => tourPrice;
		set {
			tourPrice = value;
		}
	}

	public GameModel(string parkName, int funds, GameDifficulty difficulty) {
		this.parkName = parkName;
		this.funds = funds;
		this.difficulty = difficulty;
		this.tourPrice = 250;
	}
}
