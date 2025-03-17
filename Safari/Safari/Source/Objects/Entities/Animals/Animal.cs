using Engine;
using Engine.Components;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;

namespace Safari.Objects.Entities.Animals;

public enum Gender {
	Male,
	Female
}

public abstract class Animal : Entity {
	/// <summary>
	/// The level of hunger at which an animal will become hungry
	/// </summary>
	private const int HUNGER_THRESHOLD = 50;
	private const float INITIAL_HUNGER_DECAY = 0.5f;
	/// <summary>
	/// The level of hunger at which an animal will become thirsty
	/// </summary>
	private const int THIRST_THRESHOLD = 50;
	private const float INITIAL_THIRST_DECAY = 0.75f;
	/// <summary>
	/// The number of days that have to pass before an animal can mate again
	/// </summary>
	private const int MATING_COOLDOWN_DAYS = 10;
	/// <summary>
	/// The number of days an animal can live
	/// </summary>
	private const int MAX_AGE = 100;
	private const float FEEDING_SPEED = 1f;
	private const float DRINKING_SPEED = 1f;

	protected DateTime birthTime;
	protected DateTime? lastMatingTime = null;

	/// <summary>
	/// Invoked when this animal gets hungry
	/// </summary>
	public event EventHandler GotHungry;
	/// <summary>
	/// Invoked when this animal gets thirsty
	/// </summary>
	public event EventHandler GotThirsty;

	public float HungerLevel { get; protected set; } = 100f;
	public float ThirstLevel { get; protected set; } = 100f;
	public float HungerDecay => (1f + (float)Age / MAX_AGE) * INITIAL_HUNGER_DECAY;
	public float ThirstDecay => (1f + (float)Age / MAX_AGE) * INITIAL_THIRST_DECAY;

	public bool IsHungry => HungerLevel < HUNGER_THRESHOLD;
	public bool IsThirsty => ThirstLevel < THIRST_THRESHOLD;

	public bool IsCaught { get; protected set; }

	/// <summary>
	/// The gender of the animal
	/// </summary>
	public Gender Gender { get; init; }
	/// <summary>
	/// The species of this animal
	/// </summary>
	public AnimalSpecies Species { get; init; }
	public bool IsCarnivorous => Species.IsCarnivorous();
	/// <summary>
	/// The age of this animal in in-game days!
	/// </summary>
	public int Age => (int)(GameScene.Active.Model.IngameDate - birthTime).TotalDays;
	public bool IsMature => Age >= 15;
	public bool CanMate => IsMature && (lastMatingTime == null || (GameScene.Active.Model.IngameDate - lastMatingTime.Value).TotalDays > MATING_COOLDOWN_DAYS);

	public int Price => Utils.Round(Species.GetPrice() * ((float)Age / MAX_AGE));

	public Animal(Vector2 pos, AnimalSpecies species, Gender gender) : base(pos) {
		Species = species;
		Gender = gender;

		birthTime = GameScene.Active.Model.IngameDate;

		sprite = new SpriteCmp(null);
		sprite.LayerDepth = 0.5f;
		sprite.YSortEnabled = true;
		Attach(sprite);
	}

	static Animal() {
		DebugMode.AddFeature(new LoopedDebugFeature("animal-indicators", (object sender, GameTime gameTime) => {
			foreach (Entity e in ActiveEntities) {
				if (e is Animal a) a.DrawIndicators(gameTime);
			}
		}, GameLoopStage.POST_DRAW));
	}

	public override void Load() {
		GameModel model = GameScene.Active.Model;

		model.AnimalCount++;
		if (IsCarnivorous) {
			model.CarnivoreCount++;
		} else {
			model.HerbivoreCount++;
		}

		base.Load();
	}

	public override void Unload() {
		GameModel model = GameScene.Active.Model;

		model.AnimalCount--;
		if (IsCarnivorous) {
			model.CarnivoreCount--;
		} else {
			model.HerbivoreCount--;
		}

		base.Unload();
	}

	public override void Update(GameTime gameTime) {
		if (IsCaught) return;

		if (Age > MAX_AGE) {
			Die();
			return;
		}

		bool wasHungry = IsHungry, wasThirsty = IsThirsty;

		HungerLevel = Math.Max(0, HungerLevel - HungerDecay * (float)gameTime.ElapsedGameTime.TotalSeconds);
		ThirstLevel = Math.Max(0, ThirstLevel - ThirstDecay * (float)gameTime.ElapsedGameTime.TotalSeconds);

		if (!wasHungry && IsHungry) {
			GotHungry?.Invoke(this, EventArgs.Empty);
		}

		if (!wasThirsty && IsThirsty) {
			GotThirsty?.Invoke(this, EventArgs.Empty);
		}

		if (ThirstLevel <= 0f || HungerLevel <= 0f) {
			Die();
		}

		CheckSurroundings();

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (IsCaught) return;

		base.Draw(gameTime);
	}

	public void Feed(GameTime gameTime) {
		HungerLevel += FEEDING_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (HungerLevel > 100f) {
			HungerLevel = 100f;
		}
	}

	public void Drink(GameTime gameTime) {
		ThirstLevel += DRINKING_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (ThirstLevel > 100f) {
			ThirstLevel = 100f;
		}
	}

	public void Sell() {
		GameScene.Active.Model.Funds += Price;
		Die();
	}

	public void Die() {
		Game.RemoveObject(this);
	}

	public void Mate() {
		if (!CanMate) {
			throw new InvalidOperationException("Trying to invoke mating on an animal that doesn't meet the required criteria");
		}

		lastMatingTime = GameScene.Active.Model.IngameDate;
	}

	public void Catch() {
		if (IsCaught) {
			throw new InvalidOperationException("Cannot catch an animal that has already been caught");
		}

		// TODO: leave group

		IsCaught = true;
	}

	public void Release(Vector2 releasePosition) {
		if (!IsCaught) {
			throw new InvalidOperationException("Cannot release an animal that hasn't been caught");
		}

		Position = releasePosition;
		IsCaught = false;
	}

	private Texture2D indicatorTex = null, indicatorOutline = null;
	public void DrawIndicators(GameTime gameTime) {
		if (IsCaught) return;

		int indicatorWidth = (sprite.SourceRectangle?.Width ?? sprite.Texture.Width) / 2;
		int fullHeight = sprite.SourceRectangle?.Height ?? sprite.Texture.Height;

		if (indicatorTex == null || indicatorTex.Width != indicatorWidth) {
			indicatorTex = Utils.GenerateTexture(indicatorWidth, fullHeight, Color.White);
			indicatorOutline = Utils.GenerateTexture(indicatorWidth, fullHeight, Color.White, true);
		}

		int thirstHeight = (int)(ThirstLevel / 100f * fullHeight);
		int hungerHeight = (int)(HungerLevel / 100f * fullHeight);

		// thirst level
		Game.SpriteBatch.Draw(indicatorTex, new Rectangle(Position.ToPoint() + new Point(0, fullHeight - thirstHeight), new Point(indicatorWidth, thirstHeight)), null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
		Game.SpriteBatch.Draw(indicatorOutline, new Rectangle(Position.ToPoint(), new Point(indicatorWidth, fullHeight)), null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
		// hunger level
		Game.SpriteBatch.Draw(indicatorTex, new Rectangle(Position.ToPoint() + new Point(indicatorWidth, fullHeight - hungerHeight), new Point(indicatorWidth, hungerHeight)), null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
		Game.SpriteBatch.Draw(indicatorOutline, new Rectangle(Position.ToPoint() + new Point(indicatorWidth, 0), new Point(indicatorWidth, fullHeight)), null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
	}

	private void CheckSurroundings() {
		foreach (Tile tile in GetTilesInSight()) {
			if (tile.IsFoodSource) {
				// TODO: save food source location to group
			}

			if (tile.IsWaterSource) {
				// TODO: save water source location to group
			}
		}

		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is not Animal anim) {
				continue;
			}

			// TODO: group merging

			if (IsCarnivorous && IsHungry && !anim.IsCarnivorous && !anim.IsCaught) {
				if (CanReach(anim)) {
					HungerLevel = 100f;
					anim.Die();
				} else {
					// TODO: set nav target to anim
				}
			}
		}
	}
}
