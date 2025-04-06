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

public enum PoacherState {
	Wandering,
	Chasing,
	Smuggling
}

public class Poacher : Entity {
	private const float SHOOT_CHANCE = 0.3f;

	public StateMachineCmp<PoacherState> StateMachine { get; init; }
	public PoacherState State => StateMachine.CurrentState;
	public Animal ChaseTarget { get; private set; } = null;
	public Animal CaughtAnimal { get; private set; } = null;

	private AnimatedSpriteCmp AnimatedSprite => Sprite as AnimatedSpriteCmp;

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
		Sprite.YSortOffset = 32;
		Attach(Sprite);

		animSprite.CurrentAnimation = "walk-right";

		Bounds = new Rectangle(0, 0, 16, 64);
		SightDistance = 6;
		ReachDistance = 3;
		NavCmp.Speed *= 0.75f;

		StateMachine = new StateMachineCmp<PoacherState>(PoacherState.Wandering);
		Attach(StateMachine);
	}

	static Poacher() {
		DebugMode.AddFeature(new ExecutedDebugFeature("reveal-poachers", () => revealAll = true));
		DebugMode.AddFeature(new ExecutedDebugFeature("hide-poachers", () => revealAll = false));
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
			bool right = NavCmp.LastIntendedDelta.X > -0.075f;
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

	public void Reveal() {
		Sprite.Visible = true;
	}

	private void OnWanderingTargetReached(object sender, ReachedTargetEventArgs e) {
		if (e.TargetPosition != null) {
			NavCmp.TargetPosition = GameScene.Active.Model.Level.GetRandomPosition();
		}
	}

	[StateBegin(PoacherState.Wandering)]
	public void StartWandering() {
		NavCmp.TargetPosition = GameScene.Active.Model.Level.GetRandomPosition();
		NavCmp.ReachedTarget += OnWanderingTargetReached;
		NavCmp.StopOnTargetReach = false;
		NavCmp.Moving = true;
	}

	[StateEnd(PoacherState.Wandering)]
	public void EndWandering() {
		NavCmp.ReachedTarget -= OnWanderingTargetReached;
		NavCmp.Moving = false;
	}

	[StateUpdate(PoacherState.Wandering)]
	public void WanderingUpdate(GameTime gameTime) {
		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is Animal a) {
				ChaseTarget = a;
				StateMachine.Transition(PoacherState.Chasing);
			}

			return;
		}
	}

	private Random rand = new();
	private void OnChaseTargetReached(object sender, ReachedTargetEventArgs e) {
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

	[StateBegin(PoacherState.Chasing)]
	public void OnBeginChasing() {
		if (ChaseTarget == null) {
			StateMachine.Transition(PoacherState.Wandering);
			return;
		}

		NavCmp.TargetObject = ChaseTarget;
		NavCmp.Moving = true;
		NavCmp.StopOnTargetReach = true;
		NavCmp.ReachedTarget += OnChaseTargetReached;
	}

	[StateEnd(PoacherState.Chasing)]
	public void OnEndChasing() {
		ChaseTarget = null;
		NavCmp.TargetObject = null;
		NavCmp.Moving = false;
		NavCmp.ReachedTarget -= OnChaseTargetReached;
	}

	[StateUpdate(PoacherState.Chasing)]
	public void ChasingUpdate(GameTime gameTime) {
		if (!CanSee(ChaseTarget)) {
			StateMachine.Transition(PoacherState.Wandering);
		}
	}

	[StateBegin(PoacherState.Smuggling)]
	public void OnBeginSmuggling() {
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

	[StateEnd(PoacherState.Smuggling)]
	public void OnEndSmuggling() {
		CaughtAnimal = null;
		NavCmp.ReachedTarget -= OnEscapeReached;
		Died -= OnDiedWhileEscaping;

		if (!Dead) {
			Die();
		}
	}

	private void HideOnPreUpdate(object sender, GameTime e) {
		Sprite.Visible = false;
	}
}
