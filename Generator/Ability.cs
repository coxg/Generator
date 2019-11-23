using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Generator
{
    public class Ability
    {
        // Constructor
        public Ability(

            // Ability name
            string name,

            // What's using the ability
            GameObject sourceObject = null,

            // Resource costs
            int healthCost = 0,
            int staminaCost = 0,
            int electricityCost = 0,

            // How it works
            bool keepCasting = false,
            bool isChanneled = false,
            bool isToggleable = false,
            bool requiresWalking = false,

            // What it looks like
            Animation animation = null,

            // What it does
            float cooldown = 0,
            Action<GameObject> start = null,
            Action<GameObject> onUpdate = null,
            Action<GameObject> stop = null)
        {
            // Ability name
            Name = name;

            // Resource costs
            HealthCost = healthCost;
            StaminaCost = staminaCost;
            ElectricityCost = electricityCost;

            // How the ability works
            KeepCasting = keepCasting;
            IsChanneled = isChanneled;
            IsToggleable = isToggleable;
            IsActive = false;
            RequiresWalking = requiresWalking;
          
            // What's using the ability
            SourceObject = sourceObject;

            // What it looks like
            Animation = animation;

            // What it does
            OffCooldown = true;
            Cooldown = cooldown;
            if (start == null) start = delegate { };
            Start = start;
            if (onUpdate == null) onUpdate = delegate { };
            OnUpdate = onUpdate;
            if (stop == null) stop = delegate { };
            Stop = stop;
        }

        // Ability name
        public string Name;

        // What's using the ability
        [JsonIgnore]
        private GameObject _sourceObject;
        [JsonIgnore]
        public GameObject SourceObject
        {
            get => _sourceObject;

            set
            {
                _sourceObject = value;
                if (Animation != null)
                {
                    Animation.AnimatedElement = SourceObject;
                    Animation.SourceObject = SourceObject;
                }
            }
        }

        // Resource costs
        public int HealthCost;
        public int StaminaCost;
        public int ElectricityCost;

        // How it works
        public bool OffCooldown;
        public bool KeepCasting;
        public bool IsChanneled;
        public bool IsToggleable;
        private bool IsActive;
        private bool WasPressed;
        private bool _isPressed;
        private bool RequiresWalking;

        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                WasPressed = _isPressed;
                _isPressed = value;
            }
        }

        // What it looks like
        [JsonIgnore]
        private Animation _animation;
        public Animation Animation
        {
            get => _animation;

            set
            {
                _animation = value;
                if (_animation != null)
                {
                    _animation.AnimatedElement = SourceObject;
                    _animation.SourceObject = SourceObject;
                    if (_animation.Name == "") _animation.Name = Name;
                }
            }
        }

        // What it does
        public float Cooldown;
        public Action<GameObject> Start;
        public Action<GameObject> OnUpdate;
        public Action<GameObject> Stop;

        public override string ToString()
            // Return name, useful for debugging.
        {
            return Name;
        }

        public bool CanUse()
            // Can the SourceObject use the ability?
        {
            return OffCooldown
                   && SourceObject.Health.Current >= HealthCost
                   && SourceObject.Stamina.Current >= StaminaCost
                   && SourceObject.Electricity.Current >= ElectricityCost
                   && (SourceObject.IsWalking || !RequiresWalking);
        }

        public void Update()
            // This is what happens on each update.
        {
            // See if it was active
            var WasActive = IsActive;

            // See if we are now active
            var IsNowActive = false;

            // Toggled abilities
            if (IsToggleable)
            {
                // It was already active
                if (WasActive && CanUse())
                    IsNowActive = true;

                // Activating now
                else if (!WasActive && !WasPressed && IsPressed && CanUse()) IsNowActive = true;

                // Turning off now
                if (WasActive && !WasPressed && IsPressed && CanUse()) IsNowActive = false;
            }

            // Channeled abilities
            else if (IsChanneled)
            {
                // It was already active
                if (WasActive && IsPressed && CanUse())
                    IsNowActive = true;

                // Activating now
                else if (!WasActive && !WasPressed && IsPressed && CanUse()) IsNowActive = true;
            }

            // Activated abilities
            else
            {
                if (IsPressed && CanUse())
                {
                    if (!WasActive && !WasPressed)
                        IsNowActive = true;

                    else if (KeepCasting)
                        IsNowActive = true;
                }
            }

            // What happens when we start
            if ((!WasActive || KeepCasting) && IsNowActive)
            {
                Globals.Log(SourceObject + " uses " + this);
                Start(SourceObject);
                if (Animation != null) Animation.Start();

                // Start the cooldown
                if (Cooldown != 0)
                {
                    OffCooldown = false;
                    Timing.AddEvent(Cooldown, delegate { OffCooldown = true; });
                }
            }

            // What happens when we stop
            else if (WasActive && !IsNowActive)
            {
                Globals.Log(SourceObject + " stops using " + this);
                Stop(SourceObject);
                if (Animation != null) Animation.Stop();
            }

            // What happens when we stay on
            else if (WasActive && IsNowActive)
            {
                OnUpdate(SourceObject);
            }

            // Update variable
            IsActive = IsNowActive;

            // Use resources
            if (IsActive)
            {
                SourceObject.TakeDamage(HealthCost);
                SourceObject.Stamina.Current -= StaminaCost;
                SourceObject.Electricity.Current -= ElectricityCost;
            }

            // Play the animation
            if (Animation != null) Animation.Update();
        }

        static void BulletAI(GameObject bullet)
        {
            bullet.MoveInDirection(bullet.Direction);
        }

        static void BulletCollision(GameObject bullet, GameObject other)
        {
            bullet.DealDamage(other, (int)Math.Sqrt(bullet.Speed.CurrentValue));
            bullet.Die();
        }

        public static Dictionary<String, Ability> StandardAbilities = new Dictionary<string, Ability>()
        // Eventually I'll want to serialize this or move it to its own file or something
        {
            {
                "Sprint",
                new Ability(
                    "Sprint",
                    staminaCost: 0,
                    isChanneled: true,
                    requiresWalking: true,
                    animation: new Animation(
                        updateFrames: new Frames(
                            offsets: new List<Vector3>
                            {
                                new Vector3(0, 0, .2f)
                            },
                            duration: 1.5f)),
                    start: (GameObject gameObject) =>
                    {
                        gameObject.Speed.Multiplier *= 3;
                        gameObject.IsWalking = true;
                    },
                    stop: (GameObject gameObject) =>
                    {
                        gameObject.Speed.Multiplier /= 3;
                        gameObject.IsWalking = false;
                    })
            },
            {
                "Attack",
                new Ability(
                    "Attack",
                    staminaCost: 10,
                    start: (GameObject gameObject) =>
                    {
                        gameObject.IsSwinging = true;

                        // Figure out which one you hit
                        var target = gameObject.GetTargetInRange(gameObject.EquippedWeapon.Range);

                        // Deal damage
                        if (target != null)
                        {
                            Globals.Log(gameObject + " attacks, hitting " + target + ".");
                            gameObject.DealDamage(target, gameObject.EquippedWeapon.Damage + gameObject.Strength.CurrentValue);
                        }
                        else
                        {
                            Globals.Log(gameObject + " attacks and misses.");
                        }
                    }
                )
            },
            {
                "Shoot",
                new Ability(
                    "Shoot",
                    staminaCost: 3,
                    cooldown: .1f,
                    keepCasting: true,
                    start: (GameObject gameObject) =>
                    {
                        gameObject.IsShooting = true;
                        var position = gameObject.GetTargetCoordinates(1);
                        position.Z += gameObject.Size.Z / 2;
                        new GameObject(
                            baseHealth: 1,
                            position: position,
                            size: new Vector3(.05f, .05f, .05f),
                            direction: gameObject.Direction,
                            baseSpeed: 100,
                            ai: BulletAI,
                            collisionEffect: BulletCollision,
                            brightness: new Vector3(.5f, .1f, .5f),
                            castsShadow: false,
                            temporary: true,
                            components: new Dictionary<string, Component>()
                            {
                                {"body", new Component(
                                    id: "Hand",
                                    relativePosition: new Vector3(.5f, .5f, .5f),
                                    relativeSize: 1,
                                    rotationPoint: new Vector3(.5f, .5f, .5f))
                                }
                            }
                        );
                    }
                )
            },
            {
                "Place Object",
                new Ability(
                    "Place Object",
                    start: (GameObject gameObject) =>
                    {
                        var baseTileName = TileManager.IDFromIndex[TileManager.BaseTileIndexes[Globals.CreativeObjectIndex]];
                        var randomBaseTile = TileManager.GetRandomBaseIndex(TileManager.ObjectFromID[baseTileName].BaseTileName);
                        var targetCoordinates = gameObject.GetTargetCoordinates(1);
                        TileManager.Set((int)targetCoordinates.X, (int)targetCoordinates.Y, TileManager.IDFromIndex[randomBaseTile]);
                    },
                    animation: new Animation(
                        startFrames: new Frames(
                            rotations: new List<Vector3>
                            {
                                new Vector3(0, 0, 1)
                            },
                            duration: .5f))
                )
            },
            {
                "Always Sprint",
                new Ability(
                    "Always Sprint",
                    staminaCost: 1,
                    isToggleable: true,
                    requiresWalking: true,
                    start: (GameObject gameObject) =>
                    {
                        gameObject.Speed.Multiplier *= 4;
                        gameObject.IsWalking = true;
                    },
                    stop: (GameObject gameObject) =>
                    {
                        gameObject.Speed.Multiplier /= 4;
                        gameObject.IsWalking = false;
                    },
                    animation: new Animation(
                        startFrames: new Frames(
                            offsets: new List<Vector3>
                            {
                                new Vector3(0, 0, 1)
                            },
                            duration: 1),
                        updateFrames: new Frames(
                            offsets: new List<Vector3>
                            {
                                new Vector3(-.2f, 0, 0),
                                Vector3.Zero,
                                new Vector3(.2f, 0, 0)
                            },
                            duration: .5f),
                        stopFrames: new Frames(
                            offsets: new List<Vector3>
                            {
                                new Vector3(0, 0, 1)
                            },
                            duration: 1))
                )
            }
        };
    }
}