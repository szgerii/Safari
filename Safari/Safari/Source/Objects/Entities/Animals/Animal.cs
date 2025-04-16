using Engine;
using Engine.Components;
using Engine.Collision;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;
using Safari.Popups;

namespace Safari.Objects.Entities.Animals;

public enum Gender {
	Male,
	Female
}

public abstract class Animal : Entity {
	/// <summary>
	/// The base layer depth for animals
	/// </summary>
	public const float ANIMAL_LAYER = 0.4f;

	protected const int ANIMATION_SPEED = 7;

	/// <summary>
	/// The level of hunger at which an animal will become hungry
	/// </summary>
	private const int HUNGER_THRESHOLD = 50;
	private const float INITIAL_HUNGER_DECAY = 0.04f;
	/// <summary>
	/// The level of hunger at which an animal will become thirsty
	/// </summary>
	private const int THIRST_THRESHOLD = 50;
	private const float INITIAL_THIRST_DECAY = 0.08f;
	/// <summary>
	/// The number of days that have to pass before an animal can mate again
	/// </summary>
	private const int MATING_COOLDOWN_DAYS = 10;
	/// <summary>
	/// The number of days an animal can live
	/// </summary>
	private const int MAX_AGE = 100;

	private const float FEEDING_SPEED = 10f;
	private const float DRINKING_SPEED = 10f;

	private const int INDICATOR_HEIGHT = 8;

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

	/// <summary>
	/// Represents how hungry the animal currently is (goes down with time)s
	/// </summary>
	public float HungerLevel { get; protected set; } = 100f;
	/// <summary>
	/// Represents how thirsty the animal currently is (goes down with time)s
	/// </summary>
	public float ThirstLevel { get; protected set; } = 100f;
	/// <summary>
	/// Determines how fast the animal's hunger level drops
	/// </summary>
	public float HungerDecay => (1f + (float)Age / MAX_AGE) * INITIAL_HUNGER_DECAY;
	/// <summary>
	/// Determines how fast the animal's thirst level drops
	/// </summary>
	public float ThirstDecay => (1f + (float)Age / MAX_AGE) * INITIAL_THIRST_DECAY;

	/// <summary>
	/// Whether the animal is currently hungry
	/// (i.e. hunger level has fallen under the hunger threshold)
	/// </summary>
	public bool IsHungry => HungerLevel < HUNGER_THRESHOLD;
	/// <summary>
	/// Whether the animal is currently thirsty
	/// (i.e. thirst level has fallen under the thirst threshold)
	/// </summary>
	public bool IsThirsty => ThirstLevel < THIRST_THRESHOLD;

	/// <summary>
	/// Whether the animal is currently being escorted by a poacher
	/// </summary>
	public bool IsCaught { get; protected set; }

	/// <summary>
	/// The gender of the animal
	/// </summary>
	public Gender Gender { get; init; }
	/// <summary>
	/// The species of this animal
	/// </summary>
	public AnimalSpecies Species { get; init; }
	/// <summary>
	/// Whether the animal feeds on other, herbivorous animals
	/// </summary>
	public bool IsCarnivorous => Species.IsCarnivorous();
	/// <summary>
	/// The age of this animal in in-game days!
	/// </summary>
	public int Age => (int)(GameScene.Active.Model.IngameDate - birthTime).TotalDays;
	/// <summary>
	/// Whether the animal is old enough to mate
	/// </summary>
	public bool IsMature => Age >= 15;
	/// <summary>
	/// Whether the animal is capable of mating currently
	/// </summary>
	public bool CanMate => IsMature && (lastMatingTime == null || (GameScene.Active.Model.IngameDate - lastMatingTime.Value).TotalDays > MATING_COOLDOWN_DAYS);

	/// <summary>
	/// The current selling price of the animal
	/// </summary>
	public int Price => Utils.Round(Species.GetPrice() * ((float)Age / MAX_AGE));

	private bool hasChip;
	/// <summary>
	/// Controls whether this animal has a chip attached
	/// (Chips make animals visible even in the dark)
	/// </summary>
	public bool HasChip {
		get => hasChip;
		set {
			if (hasChip != value) {
				VisibleAtNight = value;
			}
			hasChip = value;
		}
	}

	/// <summary>
	/// The group this animal belongs to
	/// </summary>
	public AnimalGroup Group { get; set; }

	/// <summary>
	/// Shorthand for Group.State
	/// </summary>
	public AnimalGroupState State => Group.State;

	/// <summary>
	/// The animal's sprite component cast to an animated sprite
	/// (which all animals have by default)
	/// </summary>
	protected AnimatedSpriteCmp AnimSprite => Sprite as AnimatedSpriteCmp;
	/// <summary>
	/// The animal's collision detection component
	/// </summary>
	protected CollisionCmp collisionCmp;

	public Animal(Vector2 pos, AnimalSpecies species, Gender gender) : base(pos) {
		// data
		Species = species;
		Gender = gender;
		Group = new AnimalGroup(this);
		VisibleAtNight = false;

		birthTime = GameScene.Active.Model.IngameDate;

		Died += (object sender, EventArgs e) => {
			Group?.Leave(this);
		};

		// animations
		AnimatedSpriteCmp animSprite = new AnimatedSpriteCmp(null, 3, 4, ANIMATION_SPEED);
		Sprite = animSprite;
		Attach(Sprite);
		animSprite.LayerDepth = ANIMAL_LAYER;
		animSprite.YSortEnabled = true;
		animSprite.Animations["idle-right"] = new Animation(0, 1, true);
		animSprite.Animations["idle-left"] = new Animation(1, 1, true);
		animSprite.Animations["idle-up-right"] = new Animation(2, 1, true);
		animSprite.Animations["idle-up-left"] = new Animation(3, 1, true);
		animSprite.Animations["walk-right"] = new Animation(0, 3, true);
		animSprite.Animations["walk-left"] = new Animation(1, 3, true);
		animSprite.Animations["walk-up-right"] = new Animation(2, 3, true);
		animSprite.Animations["walk-up-left"] = new Animation(3, 3, true);

		// collision
		collisionCmp = new CollisionCmp(Collider.Empty) {
			Tags = CollisionTags.Animal,
			Targets = CollisionTags.World // | CollisionTags.Animal
		};
		Attach(collisionCmp);
	}

	static Animal() {
		// add debug feature for drawing animal stats
		DebugMode.AddFeature(new LoopedDebugFeature("animal-indicators", (object sender, GameTime gameTime) => {
			foreach (Entity e in ActiveEntities) {
				if (e is Animal a) a.DrawIndicators(gameTime);
			}
		}, GameLoopStage.POST_DRAW));

		DebugMode.AddFeature(new ExecutedDebugFeature("list-animals", () => {
			List<Animal> animals = [];

			foreach (GameObject obj in GameScene.Active.GameObjects) {
				if (obj is Animal a) animals.Add(a);
			}

			animals.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));

			foreach (Animal a in animals) {
				DebugConsole.Instance.Info($"{a.DisplayName} - {Utils.Format(a.Position, false, false)}");
			}
		}));
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

		if (Age > MAX_AGE || ThirstLevel <= 0f || HungerLevel <= 0f) {
			Die();
			return;
		}

		// update anim
		if (NavCmp.Moving && NavCmp.LastIntendedDelta != Vector2.Zero) {
			bool rightish = NavCmp.LastIntendedDelta.X >= 0;
			bool upish = NavCmp.LastIntendedDelta.Y < 0;

			string newAnimName = $"walk-{(upish ? "up-" : "")}{(rightish ? "right" : "left")}";
			if (AnimSprite.CurrentAnimation != newAnimName) {
				AnimSprite.CurrentAnimation = newAnimName;
			}
		} else {
			if (AnimSprite.CurrentAnimation != "idle-right") {
				AnimSprite.CurrentAnimation = "idle-right";
			}
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

		CheckSurroundings();

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (IsCaught) return;

		base.Draw(gameTime);
	}

	/// <summary>
	/// Increases the animal's hunger level according to its feeding speed
	/// (for continous feeding)
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	public void Feed(GameTime gameTime) {
		HungerLevel += FEEDING_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (HungerLevel > 100f) {
			HungerLevel = 100f;
		}
	}

	/// <summary>
	/// Increases the animal's thirst level according to its drinking speed
	/// (for continous drinking)
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	public void Drink(GameTime gameTime) {
		ThirstLevel += DRINKING_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (ThirstLevel > 100f) {
			ThirstLevel = 100f;
		}
	}

	/// <summary>
	/// Sells the animal and removes it from the park
	/// </summary>
	public void Sell() {
		GameScene.Active.Model.Funds += Price;
		Die();
	}

	/// <summary>
	/// Resets the animal's mating cooldown
	/// </summary>
	/// <exception cref="InvalidOperationException"></exception>
	public void Mate() {
		if (!CanMate) {
			throw new InvalidOperationException("Trying to invoke mating on an animal that doesn't meet the required criteria");
		}

		lastMatingTime = GameScene.Active.Model.IngameDate;
	}

	/// <summary>
	/// Simulates a poacher catching an animal
	/// </summary>
	/// <exception cref="InvalidOperationException"></exception>
	public void Catch() {
		if (IsCaught) {
			throw new InvalidOperationException("Cannot catch an animal that has already been caught");
		}

		Group.Leave(this);

		IsCaught = true;
	}

	/// <summary>
	/// Simulates a poacher releasing an animal
	/// </summary>
	/// <param name="releasePosition">The position to release the animal at</param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Release(Vector2 releasePosition) {
		if (!IsCaught) {
			throw new InvalidOperationException("Cannot release an animal that hasn't been caught");
		}

		Position = releasePosition;
		IsCaught = false;
		Group = new AnimalGroup(this);
	}

	private Texture2D indicatorTex = null, indicatorOutlineTex = null;
	/// <summary>
	/// Draws an indicator for the animal's hunger and thirst levels to the screen (debug feature)
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	public void DrawIndicators(GameTime gameTime) {
		if (IsCaught) return;

		int maxWidth = Utils.Round(Bounds.Width * 0.8f);
		int margin = Utils.Round(Bounds.Width * 0.1f);

		if (indicatorTex == null || indicatorOutlineTex == null) {
			indicatorTex = Utils.GenerateTexture(maxWidth, INDICATOR_HEIGHT, Color.White);
			indicatorOutlineTex = Utils.GenerateTexture(maxWidth, INDICATOR_HEIGHT, Color.White, true);
		}

		int thirstWidth = (int)(ThirstLevel / 100f * maxWidth);
		int hungerWidth = (int)(HungerLevel / 100f * maxWidth);

		Vector2 thirstOffset = new Vector2(margin, -2f * INDICATOR_HEIGHT);
		Vector2 hungerOffset = new Vector2(margin, -INDICATOR_HEIGHT);

		// thirst level
		Game.SpriteBatch.Draw(indicatorTex, new Rectangle((Position + thirstOffset).ToPoint(), new Point(thirstWidth, INDICATOR_HEIGHT)), null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, Sprite.RealLayerDepth);
		Game.SpriteBatch.Draw(indicatorOutlineTex, new Rectangle((Position + thirstOffset).ToPoint(), new Point(maxWidth, INDICATOR_HEIGHT)), null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, Sprite.RealLayerDepth);
		// hunger level
		Game.SpriteBatch.Draw(indicatorTex, new Rectangle((Position + hungerOffset).ToPoint(), new Point(hungerWidth, INDICATOR_HEIGHT)), null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, Sprite.RealLayerDepth);
		Game.SpriteBatch.Draw(indicatorOutlineTex, new Rectangle((Position + hungerOffset).ToPoint(), new Point(maxWidth, INDICATOR_HEIGHT)), null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, Sprite.RealLayerDepth);
	}

	private void CheckSurroundings() {
		foreach (Tile tile in GetTilesInSight()) {
			if (tile.IsFoodSource) {
				Group.AddFoodSpot(tile.Position);
			}

			if (tile.IsWaterSource) {
				Group.AddWaterSpot(tile.Position);
			}
		}

		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is Poacher poacher) {
				poacher.Reveal();
			}

			if (entity is Animal anim && anim.Group != null) {
				if (Group != anim.Group && Group.CanMergeWith(anim.Group)) {
					Group.MergeWith(anim.Group);
				}

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
}
