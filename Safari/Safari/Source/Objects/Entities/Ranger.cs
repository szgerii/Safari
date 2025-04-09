using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;
using System;

namespace Safari.Objects.Entities;

public enum RangerState {
	Wandering,
	Chasing
}

public class Ranger : Entity {
	/// <summary>
	/// The monthly salary of all rangers, payed in advance
	/// </summary>
	public const int SALARY = 1500;
	/// <summary>
	/// The amount of hours that must pass after a successful hunt (killing an ANIMAL) before the ranger will try again
	/// </summary>
	public const int HUNT_COOLDOWN_HRS = 12;
	/// <summary>
	/// The walking speed of rangers (relative to the default NavCmp speed)
	/// </summary>
	public const float SPEED = 1f;

	/// <summary>
	/// The species of animals rangers should target when they
	/// don't have a specific target (can be changed by the player)
	/// </summary>
	public static AnimalSpecies? DefaultTarget { get; set; } = null;

	private AnimalSpecies? targetSpecies = null;
	/// <summary>
	/// The species of animals this particular ranger should target <br/>
	/// Set to null to use <see cref="DefaultTarget"/>
	/// </summary>
	public AnimalSpecies? TargetSpecies {
		get {
			return targetSpecies ?? DefaultTarget;
		}
		set {
			targetSpecies = value;
		}
	}

	/// <summary>
	/// The Animal or Poacher the ranger is currently chasing
	/// </summary>
	public Entity ChaseTarget { get; private set; } = null;

	/// <summary>
	/// The in-game date of the last successful animal killing
	/// </summary>
	public DateTime LastSuccessfulHunt { get; private set; } = DateTime.MinValue;
	/// <summary>
	/// Whether the ranger can try to hunt for an animal again
	/// </summary>
	public bool CanHunt => (GameScene.Active.Model.IngameDate - LastSuccessfulHunt).TotalHours >= HUNT_COOLDOWN_HRS;

	/// <summary>
	/// The animated sprite component of the ranger
	/// </summary>
	public AnimatedSpriteCmp AnimatedSprite => Sprite as AnimatedSpriteCmp;
	/// <summary>
	/// The state machine used for transitioning between the different ranger behavior types
	/// </summary>
	public StateMachineCmp<RangerState> StateMachine { get; init; }
	/// <summary>
	/// Shorthand for StateMachine.State
	/// </summary>
	public RangerState State => StateMachine.CurrentState;

	private int lastSalaryMonth = -1;

	public Ranger(Vector2 pos) : base(pos) {
		DisplayName = "Ranger";

		AnimatedSpriteCmp animSprite = new(Game.ContentManager.Load<Texture2D>("Assets/Ranger/Walk"), 8, 2, 10);
		animSprite.Animations["walk-right"] = new Animation(0, 8, true);
		animSprite.Animations["walk-left"] = new Animation(1, 8, true);
		Sprite = animSprite;
		Sprite.LayerDepth = Animal.ANIMAL_LAYER;
		Sprite.YSortEnabled = true;
		Sprite.YSortOffset = 64;
		Attach(Sprite);

		animSprite.CurrentAnimation = "walk-right";

		Bounds = new Rectangle(0, 0, 25, 64);
		SightDistance = 8;
		ReachDistance = 3;
		NavCmp.Speed *= SPEED;

		StateMachine = new(RangerState.Wandering);
		Attach(StateMachine);

		LightEntityCmp lightCmp = new(GameScene.Active.Model.Level, 5);
		Attach(lightCmp);
	}

	public override void Load() {
		GameScene.Active.Model.RangerCount++;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.RangerCount--;

		base.Unload();
	}

	public override void Update(GameTime gameTime) {
		DateTime date = GameScene.Active.Model.IngameDate;
		if (lastSalaryMonth != date.Month) {
			int monthDays = DateTime.DaysInMonth(date.Year, date.Month);
			float salaryRatio = (float)(monthDays - date.Day + 1) / monthDays;
			GameScene.Active.Model.Funds -= (int)(SALARY * salaryRatio);
			lastSalaryMonth = date.Month;
		}

		if (NavCmp.LastIntendedDelta != Vector2.Zero) {
			bool right = NavCmp.LastIntendedDelta.X > -0.075f;
			string anim = $"walk-{(right ? "right" : "left")}";

			if (!AnimatedSprite.IsPlaying || (AnimatedSprite.CurrentAnimation != anim && AnimatedSprite.CurrentAnimation.StartsWith("walk"))) {
				AnimatedSprite.CurrentAnimation = anim;
			}
		}

		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is Poacher poacher) {
				poacher.Reveal();
			}
		}

		base.Update(gameTime);
	}

	public override string ToString() {
		return $"{DisplayName}, {State}, target: {(NavCmp.Target != null ? Utils.Format(NavCmp.Target.Value, false, false) : "none")}";
	}

	/// <summary>
	/// Releases the ranger from the park
	/// </summary>
	public void Fire() {
		Die();
	}

	/// <summary>
	/// Delegate method for transitioning into Wandering state
	/// </summary>
	[StateBegin(RangerState.Wandering)]
	public void StartWandering() {
		NavCmp.TargetPosition = GameScene.Active.Model.Level.GetRandomPosition();
		NavCmp.Moving = true;
		NavCmp.ReachedTarget += OnWanderingTargetReached;
	}

	/// <summary>
	/// Delegate method for handling frame-by-frame wandering logic
	/// </summary>
	/// <param name="gameTime">The current MonoGame game time</param>
	[StateUpdate(RangerState.Wandering)]
	public void WanderingUpdate(GameTime gameTime) {
		foreach (Entity entity in GetEntitiesInSight()) {
			if (CanHunt && ChaseTarget == null && entity is Animal animal && animal.Species == TargetSpecies) {
				ChaseTarget = animal;
				StateMachine.Transition(RangerState.Chasing);
			}

			if (entity is Poacher poacher) {
				ChaseTarget = poacher;
				StateMachine.Transition(RangerState.Chasing);
			}
		}
	}

	/// <summary>
	/// Delegate method for transitioning out of Wandering state
	/// </summary>
	[StateEnd(RangerState.Wandering)]
	public void EndWandering() {
		NavCmp.TargetPosition = null;
		NavCmp.ReachedTarget -= OnWanderingTargetReached;
	}

	private void OnWanderingTargetReached(object sender, ReachedTargetEventArgs e) {
		NavCmp.TargetPosition = GameScene.Active.Model.Level.GetRandomPosition();
		NavCmp.Moving = true;
	}

	/// <summary>
	/// Delegate method for transitioning into Chasing state
	/// </summary>
	[StateBegin(RangerState.Chasing)]
	public void StartChasing() {
		if (ChaseTarget == null) {
			StateMachine.Transition(RangerState.Wandering);
			return;
		}

		NavCmp.TargetObject = ChaseTarget;
		NavCmp.Moving = true;
		NavCmp.ReachedTarget += OnChaseTargetReached;
		ChaseTarget.Died += OnChaseTargetDied;
	}

	private void OnChaseTargetDied(object sender, EventArgs e) {
		ChaseTarget.Died -= OnChaseTargetDied;
		ChaseTarget = null;
		NavCmp.TargetObject = null;
		StateMachine.Transition(RangerState.Wandering);
	}

	/// <summary>
	/// Delegate method for handling frame-by-frame Chasing state logic
	/// </summary>
	/// <param name="gameTime"></param>
	[StateUpdate(RangerState.Chasing)]
	public void ChasingUpdate(GameTime gameTime) {
		if (ChaseTarget is Poacher) return;

		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is Poacher) {
				ChaseTarget.Died -= OnChaseTargetDied;
				ChaseTarget = entity;
				StateMachine.Transition(RangerState.Chasing);
				break;
			}
		}
	}

	/// <summary>
	/// Delegate method for transitioning out of Chasing state
	/// </summary>
	[StateEnd(RangerState.Chasing)]
	public void EndChasing() {
		if (ChaseTarget != null && ChaseTarget == NavCmp.TargetObject) {
			ChaseTarget.Died -= OnChaseTargetDied;
		}

		NavCmp.TargetObject = null;
		NavCmp.ReachedTarget -= OnChaseTargetReached;
	}

	private void OnChaseTargetReached(object sender, ReachedTargetEventArgs e) {
		if (ChaseTarget is Animal) {
			LastSuccessfulHunt = GameScene.Active.Model.IngameDate;
		}

		if (ChaseTarget != null && !ChaseTarget.IsDead) {
			ChaseTarget.Die();
		}

		StateMachine.Transition(RangerState.Wandering);
	}
}
