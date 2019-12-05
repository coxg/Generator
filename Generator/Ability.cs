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
            Cached<Action<GameObject>> start = null,
            Cached<Action<GameObject>> onUpdate = null,
            Cached<Action<GameObject>> stop = null)
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
            Start = start;
            OnUpdate = onUpdate;
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
        public Cached<Action<GameObject>> Start;
        public Cached<Action<GameObject>> OnUpdate;
        public Cached<Action<GameObject>> Stop;

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
                Start?.Value(SourceObject);
                Animation?.Start();

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
                Stop?.Value(SourceObject);
                Animation?.Stop();
            }

            // What happens when we stay on
            else if (WasActive && IsNowActive)
            {
                OnUpdate?.Value(SourceObject);
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

        public static Dictionary<String, Ability> Abilities = new Dictionary<string, Ability>()
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
                            baseOffsets: new List<Vector3>
                            {
                                Vector3.Zero,
                                new Vector3(0, 0, .2f),
                                Vector3.Zero
                            },
                            duration: 1.5f)),
                    start: new Cached<Action<GameObject>>("SprintStart"),
                    stop: new Cached<Action<GameObject>>("SprintStop"))
            },
            {
                "Attack",
                new Ability(
                    "Attack",
                    staminaCost: 10,
                    start: new Cached<Action<GameObject>>("Attack"))
            },
            {
                "Shoot",
                new Ability(
                    "Shoot",
                    staminaCost: 3,
                    cooldown: .1f,
                    keepCasting: true,
                    start: new Cached<Action<GameObject>>("Shoot"))
            },
            {
                "Place Object",
                new Ability(
                    "Place Object",
                    start: new Cached<Action<GameObject>>("Place Object"))
            },
            {
                "Always Sprint",
                new Ability(
                    "Always Sprint",
                    staminaCost: 1,
                    isToggleable: true,
                    requiresWalking: true,
                    start: new Cached<Action<GameObject>>("SprintStart"),
                    stop: new Cached<Action<GameObject>>("SprintStop"),
                    animation: new Animation(
                        startFrames: new Frames(
                            baseOffsets: new List<Vector3>
                            {
                                Vector3.Zero,
                                new Vector3(0, 0, 1),
                                Vector3.Zero,
                            },
                            duration: 1),
                        updateFrames: new Frames(
                            baseOffsets: new List<Vector3>
                            {
                                Vector3.Zero,
                                new Vector3(-.2f, 0, 0),
                                Vector3.Zero,
                                new Vector3(.2f, 0, 0),
                                Vector3.Zero
                            },
                            duration: .5f),
                        stopFrames: new Frames(
                            baseOffsets: new List<Vector3>
                            {
                                Vector3.Zero,
                                new Vector3(0, 0, 1),
                                Vector3.Zero
                            },
                            duration: 1))
                )
            }
        };
    }
}