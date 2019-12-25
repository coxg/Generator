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
            int electricityCost = 0,

            // How it works
            bool keepCasting = false,
            bool isChanneled = false,
            bool isToggleable = false,
            bool requiresWalking = false,
            float cooldown = 0,
            Animation animation = null)
        {
            // Ability name
            Name = name;

            // Resource costs
            HealthCost = healthCost;
            ElectricityCost = electricityCost;

            // How the ability works
            KeepCasting = keepCasting;
            IsChanneled = isChanneled;
            IsToggleable = isToggleable;
            IsActive = false;
            RequiresWalking = requiresWalking;

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
        public int ElectricityCost;

        // How it works
        public bool OffCooldown = true;
        public bool KeepCasting;
        public bool IsChanneled;
        public bool IsToggleable;
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

        public virtual float GetPriority(List<GameObject> targets, List<GameObject> projectiles)
        {
            return 1f;
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
                SourceObject.Electricity.Current -= ElectricityCost;
            }

            // Play the animation
            if (Animation != null) Animation.Update();
        }

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
                default:
                    throw new InvalidCastException(ability.Name + " not recognized.");
            }
        }
    }
}