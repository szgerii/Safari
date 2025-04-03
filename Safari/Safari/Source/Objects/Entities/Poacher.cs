using Engine;
using Engine.Components;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;

namespace Safari.Objects.Entities;

public enum PoacherState {
	Wandering,
	Chasing,
	Smuggling
}

public class Poacher : Entity {
	public StateMachineCmp<PoacherState> StateMachine { get; init; } = new(PoacherState.Wandering);
	public PoacherState State => StateMachine.CurrentState;
	public Animal ChaseTarget { get; private set; } = null;

	private AnimatedSpriteCmp AnimatedSprite => Sprite as AnimatedSpriteCmp;

	private static bool revealAll = false;

	public Poacher(Vector2 pos) : base(pos) {
		DisplayName = "Poacher";

		Texture2D walkSheet = Game.ContentManager.Load<Texture2D>("Assets/Poacher/Walk");
		Texture2D attackSheet = Game.ContentManager.Load<Texture2D>("Assets/Poacher/Attack");
		Texture2D shotSheet = Game.ContentManager.Load<Texture2D>("Assets/Poacher/Shot");

		AnimatedSpriteCmp animSprite = new(null, 7, 1, 10);
		animSprite.Animations["walk"] = new Animation(0, 7, true, texture: walkSheet);
		Sprite = animSprite;
		Sprite.LayerDepth = Animal.ANIMAL_LAYER;
		Sprite.YSortEnabled = true;
		Sprite.YSortOffset = 32;
		Sprite.Scale = 1.75f;
		Attach(Sprite);

		animSprite.CurrentAnimation = "walk";

		Bounds = new Rectangle(0, 0, 16, 32);
		SightDistance = 6;
		ReachDistance = 3;
		NavCmp.Speed *= 0.75f;

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
			if (!AnimatedSprite.IsPlaying || AnimatedSprite.CurrentAnimation != "walk") {
				AnimatedSprite.CurrentAnimation = "walk";
			}

			bool right = NavCmp.LastIntendedDelta.X > 0;
			AnimatedSprite.Flip = right ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (revealAll) Sprite.Visible = true;

		base.Draw(gameTime);
	}

	public override string ToString() {
		return $"{DisplayName}, {State}, target: {(State == PoacherState.Wandering ? Utils.Format(NavCmp.Target.Value, false, false) : ChaseTarget.ToString())}";
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

	private void OnChaseTargetReached(object sender, ReachedTargetEventArgs e) {
		// TODO: shoot or smuggle
		StateMachine.Transition(PoacherState.Wandering);
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

	private void HideOnPreUpdate(object sender, GameTime e) {
		//Sprite.Visible = false;
	}
}
