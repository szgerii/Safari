using Engine;
using Engine.Components;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Safari.Components;
using Safari.Model.Entities.Animals;
using Safari.Persistence;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Model.Entities;

/// <summary>
/// The possible states of the tourist entity
/// </summary>
public enum RangerState {
    Wandering,
    Chasing
}

/// <summary>
/// A class representing rangers, people who in exchange for a monthly salary protect the safari from poachers and control animal population
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Ranger : Entity {
    /// <summary>
    /// The monthly salary of all rangers, payed in advance
    /// </summary>
    public const int SALARY = 4000;
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
    [StaticSavedProperty]
    public static AnimalSpecies? DefaultTarget { get; set; } = null;

    [JsonProperty]
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

    [GameobjectReferenceProperty]
    private Entity? chaseTargetBuffer = null;
    /// <summary>
    /// The Animal or Poacher the ranger is currently chasing
    /// </summary>
    [GameobjectReferenceProperty]
    public Entity? ChaseTarget { get; private set; } = null;

    /// <summary>
    /// The in-game date of the last successful animal killing
    /// </summary>
    [JsonProperty]
    public DateTime LastSuccessfulHunt { get; private set; } = DateTime.MinValue;
    /// <summary>
    /// Whether the ranger can try to hunt for an animal again
    /// </summary>
    public bool CanHunt => (GameScene.Active.Model.IngameDate - LastSuccessfulHunt).TotalHours >= HUNT_COOLDOWN_HRS;

    /// <summary>
    /// The animated sprite component of the ranger
    /// </summary>
    public AnimatedSpriteCmp AnimatedSprite => (AnimatedSpriteCmp)Sprite!;
    /// <summary>
    /// The state machine used for transitioning between the different ranger behavior types
    /// </summary>
    [JsonProperty]
    public StateMachineCmp<RangerState> StateMachine { get; init; }
    /// <summary>
    /// Shorthand for StateMachine.State
    /// </summary>
    public RangerState State => StateMachine.CurrentState;

    [JsonProperty]
    private int lastSalaryMonth = -1;

    /// <summary>
    /// Initializes the static state of rangers
    /// </summary>
    public static void Init() {
        DefaultTarget = null;
    }

    [JsonConstructor]
    public Ranger() : base() {
        SetupSprite();
        StateMachine = new();
    }

    public Ranger(Vector2 pos) : base(pos) {
        DisplayName = "Ranger";

        SetupSprite();

        Bounds = new Vectangle(0, 0, 25, 64);
        SightDistance = 8;
        ReachDistance = 3;
        NavCmp.Speed *= SPEED;

        StateMachine = new(RangerState.Wandering);

        TryGetPaid();
    }

    private void SetupSprite() {
        AnimatedSpriteCmp animSprite = new(Game.LoadTexture("Assets/Ranger/Walk"), 8, 2, 10);
        animSprite.Animations["walk-right"] = new Animation(0, 8, true);
        animSprite.Animations["walk-left"] = new Animation(1, 8, true);
        Sprite = animSprite;
        Sprite.LayerDepth = Animal.ANIMAL_LAYER;
        Sprite.YSortEnabled = true;
        Sprite.YSortOffset = 64;
        Attach(Sprite);

        animSprite.CurrentAnimation = "walk-right";
    }

    [PostPersistenceSetup]
    public void PostPeristenceSetup(Dictionary<string, List<GameObject>> refObjs) {
        chaseTargetBuffer = (Entity)refObjs["chaseTargetBuffer"][0];
        ChaseTarget = (Entity)refObjs["ChaseTarget"][0];

        if (StateMachine.CurrentState == RangerState.Wandering) {
            NavCmp.ReachedTarget += OnWanderingTargetReached;
        }

        if (StateMachine.CurrentState == RangerState.Chasing) {
            NavCmp.ReachedTarget += OnChaseTargetReached;
            if (ChaseTarget != null) {
                ChaseTarget.Died += OnChaseTargetDied;
            }
            NavCmp.TargetObject = ChaseTarget;
        }
    }

    public override void Load() {
        GameScene.Active.Model.RangerCount++;

        Attach(StateMachine);

        LightEntityCmp lightCmp = new(GameScene.Active.Model.Level!, 5);
        Attach(lightCmp);

        base.Load();
    }

    public override void Unload() {
        GameScene.Active.Model.RangerCount--;

        base.Unload();
    }

    public override void Update(GameTime gameTime) {
        TryGetPaid();

        if (NavCmp.LastIntendedDelta != Vector2.Zero) {
            bool right = NavCmp.LastIntendedDelta.X > -0.075f;
            string anim = $"walk-{(right ? "right" : "left")}";

            if (!AnimatedSprite.IsPlaying || (AnimatedSprite.CurrentAnimation != anim && AnimatedSprite.CurrentAnimation!.StartsWith("walk"))) {
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
        return $"{DisplayName}, {State}, target: {(NavCmp.Target != null ? (NavCmp.TargetObject?.ToString() + " " + Utils.Format(NavCmp.Target.Value, false, false)) : "none")}";
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
        NavCmp.TargetPosition = GameScene.Active.Model.Level!.GetRandomPosition();
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
            if (entity.IsDead || this == entity) continue;

            if (entity is Poacher poacher &&
               (chaseTargetBuffer is not Poacher ||
               (Vector2.DistanceSquared(CenterPosition, entity.CenterPosition) < Vector2.DistanceSquared(CenterPosition, chaseTargetBuffer.CenterPosition)))) {
                chaseTargetBuffer = poacher;
                StateMachine.Transition(RangerState.Chasing);
            }

            if (CanHunt && chaseTargetBuffer == null && entity is Animal animal && animal.Species == TargetSpecies) {
                chaseTargetBuffer = animal;
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

    private void OnWanderingTargetReached(object? sender, NavigationTargetEventArgs e) {
        NavCmp.TargetPosition = GameScene.Active.Model.Level!.GetRandomPosition();
        NavCmp.Moving = true;
    }

    /// <summary>
    /// Delegate method for transitioning into Chasing state
    /// </summary>
    [StateBegin(RangerState.Chasing)]
    public void StartChasing() {
        if (chaseTargetBuffer == null) {
            StateMachine.Transition(RangerState.Wandering);
            return;
        }

        ChaseTarget = chaseTargetBuffer;
        chaseTargetBuffer = null;
        NavCmp.TargetObject = ChaseTarget;
        NavCmp.Moving = true;
        NavCmp.ReachedTarget += OnChaseTargetReached;
        ChaseTarget.Died += OnChaseTargetDied;
    }

    /// <summary>
    /// Delegate method for handling frame-by-frame Chasing state logic
    /// </summary>
    /// <param name="gameTime"></param>
    [StateUpdate(RangerState.Chasing)]
    public void ChasingUpdate(GameTime gameTime) {
        if (ChaseTarget is Animal animal && animal.Species != TargetSpecies) {
            StateMachine.Transition(RangerState.Wandering);
            return;
        }

        foreach (Entity entity in GetEntitiesInSight()) {
            if (entity.IsDead || this == entity) continue;

            // chasing animal - saw poacher -> always
            // chasing poacher - saw poacher -> if seen poacher is closer
            bool shouldSwitch =
                (entity is Poacher && (
                ChaseTarget is Animal ||
                (ChaseTarget is Poacher &&
                    (
                        Vector2.DistanceSquared(CenterPosition, ChaseTarget.CenterPosition) >
                        Vector2.DistanceSquared(CenterPosition, entity.CenterPosition)
                    )
                )
            ));

            if (shouldSwitch) {
                chaseTargetBuffer = entity;
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
        if (ChaseTarget != null) {
            ChaseTarget.Died -= OnChaseTargetDied;
        }

        ChaseTarget = null;
        NavCmp.TargetObject = null;
        NavCmp.ReachedTarget -= OnChaseTargetReached;
    }

    private void OnChaseTargetDied(object? sender, EventArgs e) {
        StateMachine.Transition(RangerState.Wandering);
    }

    private void OnChaseTargetReached(object? sender, NavigationTargetEventArgs e) {
        if (ChaseTarget is Animal) {
            LastSuccessfulHunt = GameScene.Active.Model.IngameDate;

            // don't kill the target if it's the only animal left in the park
            if (GameScene.Active.Model.AnimalCount == 1) {
                StateMachine.Transition(RangerState.Wandering);
                return;
            }
        }

        if (ChaseTarget != null && !ChaseTarget.IsDead) {
            ChaseTarget.Die();
        }

        StateMachine.Transition(RangerState.Wandering);
    }

    /// <summary>
    /// The amount of money the ranger should get if hired right now (for the remainder of the current month)
    /// </summary>
    public static int SalaryIfHiredNow() {
        DateTime date = GameScene.Active.Model.IngameDate;
        int monthDays = DateTime.DaysInMonth(date.Year, date.Month);
        float salaryRatio = (float)(monthDays - date.Day + 1) / monthDays;
        int result = (int)(SALARY * salaryRatio);
        return result;
    }

    private void TryGetPaid() {
        DateTime date = GameScene.Active.Model.IngameDate;

        if (lastSalaryMonth != date.Month) {
            int monthDays = DateTime.DaysInMonth(date.Year, date.Month);
            float salaryRatio = (float)(monthDays - date.Day + 1) / monthDays;
            GameScene.Active.Model.Funds -= (int)(SALARY * salaryRatio);
            lastSalaryMonth = date.Month;
        }
    }
}
