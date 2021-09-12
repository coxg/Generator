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

            // Resource costs
            int healthCost = 0,
            int manaCost = 0,

            // How it works
            bool keepCasting = false,
            bool isChanneled = false,
            bool isToggleable = false,
            bool requiresWalking = false,
            Vector3? target = null,
            float cooldown = 0,
            Animation animation = null)
        {
            // Ability name
            Name = name;

            // Resource costs
            HealthCost = healthCost;
            ManaCost = manaCost;

            // How the ability works
            KeepCasting = keepCasting;
            IsChanneled = isChanneled;
            IsToggleable = isToggleable;
            IsActive = false;
            RequiresWalking = requiresWalking;
            Target = target;

            // What it looks like
            Animation = animation;

            // What it does
            OffCooldown = true;
            Cooldown = cooldown;
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
        public int ManaCost;

        // How it works
        public bool OffCooldown = true;
        public bool KeepCasting;
        public bool IsChanneled;
        public bool IsToggleable;
        public Vector3? Target = null;
        private bool IsActive;
        private bool WasTryingToUse;
        private bool _isTryingToUse;
        private bool RequiresWalking;

        public bool IsTryingToUse
        {
            get => _isTryingToUse;
            set
            {
                WasTryingToUse = _isTryingToUse;
                _isTryingToUse = value;
                if (Target != null)
                {
                    if (SourceObject != Globals.Player)
                    {
                        SourceObject.DirectionOverride = (float)MathTools.Angle(SourceObject.Center, (Vector3)Target);
                        SourceObject.MovementTarget = (Vector3)Target;
                    }
                    
                }
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
        public virtual void Start() { }
        public virtual void OnUpdate() { }
        public virtual void Stop() { }

        // Effect calculations
        public virtual int Damage() { return 0; }
        public virtual int Healing() { return 0; }
        public virtual List<code.objects.Ailment> Ailments() { return new List<code.objects.Ailment>(); }

        public virtual Dictionary<string, float> GetPriorityValues(
            IEnumerable<GameObject> allies, IEnumerable<GameObject> enemies, List<GameObject> projectiles)
        {
            return new Dictionary<string, float>
            {
                { "Damage",     0f },
                { "Healing",    0f },
                { "Ailments",   0f },
                { "Slows",      0f },
                { "Distance",   0f }
            };
        }

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
                   && SourceObject.Mana.Current >= ManaCost
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
                else if (!WasActive && !WasTryingToUse && IsTryingToUse && CanUse()) IsNowActive = true;

                // Turning off now
                if (WasActive && !WasTryingToUse && IsTryingToUse && CanUse()) IsNowActive = false;
            }

            // Channeled abilities
            else if (IsChanneled)
            {
                // It was already active
                if (WasActive && IsTryingToUse && CanUse())
                    IsNowActive = true;

                // Activating now
                else if (!WasActive && !WasTryingToUse && IsTryingToUse && CanUse()) IsNowActive = true;
            }

            // Activated abilities
            else
            {
                if (IsTryingToUse && CanUse())
                {
                    if (!WasActive && !WasTryingToUse)
                        IsNowActive = true;

                    else if (KeepCasting)
                        IsNowActive = true;
                }
            }

            // What happens when we start
            if ((!WasActive || KeepCasting) && IsNowActive)
            {
                Globals.Log(SourceObject + " uses " + this);
                Start();
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
                Stop();
                Animation?.Stop();
            }

            // What happens when we stay on
            else if (WasActive && IsNowActive)
            {
                OnUpdate();
            }

            // Update variable
            IsActive = IsNowActive;

            // Use resources
            if (IsActive)
            {
                SourceObject.TakeDamage(HealthCost);
                SourceObject.Mana.Current -= ManaCost;
            }

            // Play the animation
            if (Animation != null) Animation.Update();
        }

        // TODO: Something better than this!
        public static Ability GetTyped(Ability ability)
        {
            switch (ability.Name)
            {
                case "Attack":
                    Globals.CopyTo(ability, out code.abilities.Attack attack);
                    return attack;
                case "PlaceObject":
                    Globals.CopyTo(ability, out code.abilities.PlaceObject placeObject);
                    return placeObject;
                case "Shoot":
                    Globals.CopyTo(ability, out code.abilities.Shoot shoot);
                    return shoot;
                case "Sprint":
                    Globals.CopyTo(ability, out code.abilities.Sprint sprint);
                    return sprint;
                case "Jump":
                    Globals.CopyTo(ability, out code.abilities.Jump jump);
                    return jump;
                case "Dash":
                    Globals.CopyTo(ability, out code.abilities.Dash dash);
                    return dash;
                case "Blink":
                    Globals.CopyTo(ability, out code.abilities.Blink blink);
                    return blink;
                default:
                    throw new InvalidCastException(ability.Name + " must be added to Ability.GetTyped");
            }
        }
    }
}