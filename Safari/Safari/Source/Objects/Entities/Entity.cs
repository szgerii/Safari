using Engine;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Scenes;
using System;

namespace Safari.Objects.Entities;

public abstract class Entity : GameObject {
	protected string displayName;
	private DateTime lastHourUpdate;
	private DateTime lastDayUpdate;
	private DateTime lastWeekUpdate;

	public Entity(Vector2 pos) : base(pos) { }

	public event EventHandler? HourPassed;
	public event EventHandler? DayPassed;
	public event EventHandler? WeekPassed;

	public string DisplayName => displayName;

	public override void Load() {
		GameModel model = GameScene.Active.Model;
		lastHourUpdate = model.IngameDate;
		lastDayUpdate = model.IngameDate;
		lastWeekUpdate = model.IngameDate;
		base.Load();
	}

	public override void Update(GameTime gameTime) {
		DateTime ingameDate = GameScene.Active.Model.IngameDate;
		if (ingameDate - lastHourUpdate > TimeSpan.FromHours(1)) {
			HourPassed?.Invoke(this, EventArgs.Empty);
			lastHourUpdate = ingameDate;
		}
		if (ingameDate - lastDayUpdate > TimeSpan.FromDays(1)) {
			DayPassed?.Invoke(this, EventArgs.Empty);
			lastDayUpdate = ingameDate;
		}
		if (ingameDate - lastWeekUpdate > TimeSpan.FromDays(7)) {
			WeekPassed?.Invoke(this, EventArgs.Empty);
			lastWeekUpdate = ingameDate;
		}
		base.Update(gameTime);
	}
}
