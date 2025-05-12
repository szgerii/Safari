using Microsoft.Xna.Framework;
using Safari.Model;
using SafariTest.Utils;

namespace SafariTest.Tests.Model;

[TestClass]
public class GameModelTests {

	[TestMethod]
	public void ModelInit() {
		DateTime start = new DateTime(1990, 1, 1);
		start.AddHours(6);
		
		GameModel model = new GameModel("park", 3000, GameDifficulty.Normal, start);
		Assert.AreEqual(GameDifficulty.Normal, model.Difficulty);
		Assert.AreEqual(start, model.IngameDate);
		Assert.AreEqual(3000, model.Funds);

		Assert.AreEqual(GameModel.WIN_CARN_NORMAL, model.WinCriteriaCarn);
		Assert.AreEqual(GameModel.WIN_FUNDS_NORMAL, model.WinCriteriaFunds);
		Assert.AreEqual(GameModel.WIN_HERB_NORMAL, model.WinCriteriaHerb);
		Assert.AreEqual(GameModel.WIN_DAYS_NORMAL, model.WinCriteriaDays);

		GameModel model2 = new GameModel("easy", 20000, GameDifficulty.Easy, start);
		Assert.AreEqual(GameModel.WIN_CARN_EASY, model2.WinCriteriaCarn);
		Assert.AreEqual(GameModel.WIN_FUNDS_EASY, model2.WinCriteriaFunds);
		Assert.AreEqual(GameModel.WIN_HERB_EASY, model2.WinCriteriaHerb);
		Assert.AreEqual(GameModel.WIN_DAYS_EASY, model2.WinCriteriaDays);

		GameModel model3 = new GameModel("hard", 20000, GameDifficulty.Hard, start);
		Assert.AreEqual(GameModel.WIN_CARN_HARD, model3.WinCriteriaCarn);
		Assert.AreEqual(GameModel.WIN_FUNDS_HARD, model3.WinCriteriaFunds);
		Assert.AreEqual(GameModel.WIN_HERB_HARD, model3.WinCriteriaHerb);
		Assert.AreEqual(GameModel.WIN_DAYS_HARD, model3.WinCriteriaDays);
	}

	[TestMethod]
	public void ModelTime() {
		DateTime start = new DateTime(1990, 1, 1);
		start.AddHours(6);
		double d = 0.01;

		GameModel model = new GameModel("park", 3000, GameDifficulty.Normal, start);
		Assert.AreEqual(0, model.FakeFrameMul, 0.01);
		Assert.AreEqual(0, model.RealExtraFrames, d);
		Assert.AreEqual(GameSpeed.Slow, model.GameSpeed);

		model.Pause();
		Assert.AreEqual(0, model.FakeFrameMul, d);
		Assert.AreEqual(0, model.RealExtraFrames, d);
		Assert.AreEqual(GameSpeed.Paused, model.GameSpeed);

		model.Resume();
		Assert.AreEqual(0, model.FakeFrameMul, d);
		Assert.AreEqual(0, model.RealExtraFrames, d);
		Assert.AreEqual(GameSpeed.Slow, model.GameSpeed);

		model.GameSpeed = GameSpeed.Medium;
		Assert.AreEqual(GameModel.MEDIUM_FAKE, model.FakeFrameMul, d);
		Assert.AreEqual((double)GameModel.MEDIUM_FRAMES / (double)GameModel.MEDIUM_FAKE, model.RealExtraFrames, d);

		model.GameSpeed = GameSpeed.Fast;
		Assert.AreEqual(GameModel.FAST_FAKE, model.FakeFrameMul, d);
		Assert.AreEqual((double)GameModel.FAST_FRAMES / (double)GameModel.FAST_FAKE, model.RealExtraFrames, d);

		model.Pause();
		Assert.AreEqual(GameSpeed.Paused, model.GameSpeed);
		model.Resume();
		Assert.AreEqual(GameSpeed.Fast, model.GameSpeed);

		Assert.IsTrue(model.IsDaytime);
		TimeSpan nightJump = TimeSpan.FromSeconds(GameModel.DAY_LENGTH * .75);
		GameTime gt = new GameTime(nightJump, nightJump);
		model.Advance(gt);
		DateTime target = start + TimeSpan.FromHours(18);
		Assert.AreEqual(0, (model.IngameDate - target).TotalHours, d);
		Assert.IsFalse(model.IsDaytime);
	}

	[TestMethod]
	public void ModelWinLose() {
		DateTime start = new DateTime(1990, 1, 1);
		start.AddHours(6);
		double d = 0.01;

		GameModel model = new GameModel("won", 3000, GameDifficulty.Normal, start);
		Assert.IsFalse(model.WinTimerRunning);
		
		model.Funds += model.WinCriteriaFunds;
		Assert.IsFalse(model.WinTimerRunning);
		model.HerbivoreCount += GameModel.WIN_HERB_NORMAL;
		model.CarnivoreCount += GameModel.WIN_CARN_NORMAL;
		Assert.IsTrue(model.WinTimerRunning);
		Assert.AreEqual(model.WinTimerDays, model.WinCriteriaDays, d);
		bool won = false;
		model.GameWon += (object? sender, EventArgs e) => won = true;
		TimeSpan jump = TimeSpan.FromSeconds(GameModel.DAY_LENGTH * model.WinCriteriaDays + 2);
		GameTime gt = new GameTime(jump, jump);
		model.Advance(gt);
		Assert.IsTrue(won);
		Assert.IsFalse(model.WinTimerRunning);
		Assert.IsTrue(model.PostWin);

		GameModel model2 = new GameModel("lost1", 3000, GameDifficulty.Normal, start);
		bool lost = false;
		LoseReason? reason = null;
		model2.GameLost += (object? sender, LoseReason r) => {
			reason = r;
			lost = true;
		};
		model2.AnimalCount = 0;
		Assert.IsTrue(lost);
		Assert.IsNotNull(reason);
		Assert.AreEqual(LoseReason.Animals, reason);
		Assert.IsFalse(model2.CheckWinLose);

		GameModel model3 = new GameModel("lost2", 3000, GameDifficulty.Normal, start);
		lost = false;
		reason = null;
		model3.GameLost += (object? sender, LoseReason r) => {
			reason = r;
			lost = true;
		};
		model3.Funds = 0;
		Assert.IsTrue(lost);
		Assert.IsNotNull(reason);
		Assert.AreEqual(LoseReason.Money, reason);
		Assert.IsFalse(model3.CheckWinLose);
	}
}

