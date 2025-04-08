using Engine;
using Engine.Components;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using Safari.Objects.Entities.Animals;
using Safari.Popups;
using Safari.Scenes;
using System;

namespace Safari.Objects.Entities;

/// <summary>
/// Enum for showing which behavior the poacher is currently performing
/// </summary>
public enum PoacherState {
	Wandering,
	Chasing,
	Smuggling
}

/// <summary>
/// Class for poacher entities
/// </summary>
public class Poacher : Entity {
	/// <summary>
	/// The chance of shooting the chased animal upon reaching them (versus smuggling them out of the park)
	/// </summary>
	private const float SHOOT_CHANCE = 0.3f;
	/// <summary>
	/// The speed of the poachers (relative to the default NavCmp speed)
	/// </summary>
	private const float SPEED = 0.7f;

	/// <summary>
	/// The poacher's state machine for handling behavior
	/// </summary>
	public StateMachineCmp<PoacherState> StateMachine { get; init; }
	/// <summary>
	/// Shorthand for StateMachine.State
	/// </summary>
	public PoacherState State => StateMachine.CurrentState;
	/// <summary>
	/// The currently chased animal (or null if not chasing any right now)
	/// </summary>
	public Animal ChaseTarget { get; private set; } = null;
	/// <summary>
	/// The animal that is currently being smuggled out of the park (or null if State != Smuggling)
	/// </summary>
	public Animal CaughtAnimal { get; private set; } = null;

	private AnimatedSpriteCmp AnimatedSprite => Sprite as AnimatedSpriteCmp;

	/// <summary>
	/// Whether to force reveal all poacher, regardless of not being seen by any friendly entity
	/// </summary>
	private static bool revealAll = false;

	public Poacher(Vector2 pos) : base(pos) {
		DisplayName = "Poacher";
		VisibleAtNight = false;

		Texture2D walkSheet = Game.ContentManager.Load<Texture2D>("Assets/Poacher/Walk");

		AnimatedSpriteCmp animSprite = new(walkSheet, 7, 2, 10);
		animSprite.Animations["walk-right"] = new Animation(0, 7, true);
		animSprite.Animations["walk-left"] = new Animation(1, 7, true);
		Sprite = animSprite;
		Sprite.LayerDepth = Animal.ANIMAL_LAYER;
		Sprite.YSortEnabled = true;
		Sprite.YSortOffset = 64;
		Attach(Sprite);

		animSprite.CurrentAnimation = "walk-right";

		/*Collider collider = (new Collider(6, 54, 20, 12)).WithSpriteScale(Sprite.Scale);
		CollisionCmp collisionCmp = new CollisionCmp(collider) {
			Tags = CollisionTags.Poacher,
			Targets = CollisionTags.World
		};
		Attach(collisionCmp);*/

		Bounds = new Rectangle(0, 0, 32, 64);
		SightDistance = 6;
		ReachDistance = 2;
		NavCmp.Speed *= SPEED;

		StateMachine = new StateMachineCmp<PoacherState>(PoacherState.Wandering);
		Attach(StateMachine);
	}

	static Poacher() {
		DebugMode.AddFeature(new ExecutedDebugFeature("reveal-poachers", () => revealAll = true));
		DebugMode.AddFeature(new ExecutedDebugFeature("hide-poachers", () => revealAll = false));

		DebugMode.AddFeature(new ExecutedDebugFeature("kill-poachers", () => {
			foreach (GameObject obj in GameScene.Active.GameObjects) {
				if (obj is Poacher p) {
					p.Die();
				}
			}
		}));
	}

	public override void Load() {
		GameScene.Active.Model.PoacherCount++;
		GameScene.Active.PreUpdate += HideOnPreUpdate;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.PoacherCount--;
		GameScene.Active.PreUpdate -= HideOnPreUpdate;

		base.Unload();
	}

	public override void Update(GameTime gameTime) {
		if (NavCmp.LastIntendedDelta != Vector2.Zero) {
			bool right = NavCmp.LastIntendedDelta.X > 0;
			string anim = $"walk-{(right ? "right" : "left")}";

			if (!AnimatedSprite.IsPlaying || (AnimatedSprite.CurrentAnimation != anim && AnimatedSprite.CurrentAnimation.StartsWith("walk"))) {
				AnimatedSprite.CurrentAnimation = anim;
			}
		}

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (revealAll) Sprite.Visible = true;

		base.Draw(gameTime);
	}

	public override string ToString() {
		string target;
		target = State switch {
			PoacherState.Wandering => Utils.Format(NavCmp.Target.Value, false, false),
			PoacherState.Chasing => ChaseTarget.ToString(),
			PoacherState.Smuggling => $"{Utils.Format(NavCmp.Target.Value, false, false)} [{CaughtAnimal}]",
			_ => ""
		};

		return $"{DisplayName}, {State}, target: {target}";
	}

	/// <summary>
	/// Reveal the poacher (visually) for the current frame <br/>
	/// Can be called during Update, PostUpdate or PreDraw
	/// </summary>
	public void Reveal() {
		Sprite.Visible = true;
	}

	/// <summary>
	/// Delegate method for transitioning into Wandering state
	/// </summary>
	[StateBegin(PoacherState.Wandering)]
	public void StartWandering() {
		NavCmp.TargetPosition = GameScene.Active.Model.Level.GetRandomPosition();
		NavCmp.ReachedTarget += OnWanderingTargetReached;
		NavCmp.StopOnTargetReach = false;
		NavCmp.Moving = true;
	}

	/// <summary>
	/// Delegate method for performing frame-by-frame Wandering logic
	/// </summary>
	/// <param name="gameTime">The current MonoGame game time</param>
	[StateUpdate(PoacherState.Wandering)]
	public void WanderingUpdate(GameTime gameTime) {
		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is Animal a && !a.IsCaught && !a.IsDead) {
				ChaseTarget = a;
				StateMachine.Transition(PoacherState.Chasing);
				break;
			}
		}
	}

	/// <summary>
	/// Delegate method for transitioning out of Wandering state
	/// </summary>
	[StateEnd(PoacherState.Wandering)]
	public void EndWandering() {
		NavCmp.ReachedTarget -= OnWanderingTargetReached;
		NavCmp.Moving = false;
	}

	private void OnWanderingTargetReached(object sender, ReachedTargetEventArgs e) {
		if (e.TargetPosition != null) {
			NavCmp.TargetPosition = GameScene.Active.Model.Level.GetRandomPosition();
		}
	}

	/// <summary>
	/// Delegate method for transitioning into Chasing state
	/// </summary>
	[StateBegin(PoacherState.Chasing)]
	public void StartChasing() {
		if (ChaseTarget == null) {
			StateMachine.Transition(PoacherState.Wandering);
			return;
		}

		NavCmp.TargetObject = ChaseTarget;
		NavCmp.Moving = true;
		NavCmp.StopOnTargetReach = true;
		NavCmp.ReachedTarget += OnChaseTargetReached;
	}

	/// <summary>
	/// Delegate method for handling frame-by-frame Chasing logic
	/// </summary>
	/// <param name="gameTime">The current MonoGame game time</param>
	[StateUpdate(PoacherState.Chasing)]
	public void ChasingUpdate(GameTime gameTime) {
		SightDistance++;
		bool canSeeTarget = CanSee(ChaseTarget);
		SightDistance--;

		if (!canSeeTarget) {
			StateMachine.Transition(PoacherState.Wandering);
		}
	}

	/// <summary>
	/// Delegate method for transitioning out of Chasing state
	/// </summary>
	[StateEnd(PoacherState.Chasing)]
	public void EndChasing() {
		ChaseTarget = null;
		NavCmp.TargetObject = null;
		NavCmp.Moving = false;
		NavCmp.ReachedTarget -= OnChaseTargetReached;
	}

	private readonly Random rand = new();
	private void OnChaseTargetReached(object sender, ReachedTargetEventArgs e) {
		if (ChaseTarget.IsCaught || ChaseTarget.IsDead) {
			StateMachine.Transition(PoacherState.Wandering);
			return;
		}

		bool shoot = rand.NextSingle() <= SHOOT_CHANCE;

		if (shoot) {
			ChaseTarget.Die();
			StateMachine.Transition(PoacherState.Wandering);
		} else {
			ChaseTarget.Catch();
			CaughtAnimal = ChaseTarget;
			StateMachine.Transition(PoacherState.Smuggling);
		}
	}

	/// <summary>
	/// Delegate method for transitioning into Smuggling state
	/// </summary>
	[StateBegin(PoacherState.Smuggling)]
	public void StartSmuggling() {
		Rectangle lvlBounds = GameScene.Active.Model.Level.PlayAreaBounds;

		Vector2[] potentialEscapes = [
			new Vector2(lvlBounds.X, CenterPosition.Y),
			new Vector2(lvlBounds.Right, CenterPosition.Y),
			new Vector2(CenterPosition.X, lvlBounds.Y),
			new Vector2(CenterPosition.X, lvlBounds.Bottom)
		];

		Vector2 closestEscape = potentialEscapes[0];
		float minDist = Vector2.Distance(CenterPosition, closestEscape);
		for (int i = 1; i < potentialEscapes.Length; i++) {
			float dist = Vector2.Distance(CenterPosition, potentialEscapes[i]);

			if (dist < minDist) {
				closestEscape = potentialEscapes[i];
				minDist = dist;
			}
		}

		NavCmp.TargetPosition = closestEscape;
		NavCmp.Moving = true;
		NavCmp.StopOnTargetReach = true;
		ReachDistance = 1;
		NavCmp.ReachedTarget += OnEscapeReached;
		Died += OnDiedWhileEscaping;
	}

	private void OnDiedWhileEscaping(object sender, EventArgs e) {
		CaughtAnimal.Release(Position);
	}

	private void OnEscapeReached(object sender, ReachedTargetEventArgs e) {
		CaughtAnimal.Die();
		StateMachine.Transition(PoacherState.Wandering);
		ReachDistance = 4;
	}

	/// <summary>
	/// Delegate method for transitioning out of Smuggling state
	/// </summary>
	[StateEnd(PoacherState.Smuggling)]
	public void EndSmuggling() {
		CaughtAnimal = null;
		NavCmp.ReachedTarget -= OnEscapeReached;
		Died -= OnDiedWhileEscaping;

		if (!IsDead) {
			Die();
		}
	}

	private void HideOnPreUpdate(object sender, GameTime e) {
		Sprite.Visible = false;
	}
}
