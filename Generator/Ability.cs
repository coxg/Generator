using System;

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
            Action start = null,
            Action onUpdate = null,
            Action stop = null)
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
        public string Name { get; set; }

        // What's using the ability
        private GameObject _sourceObject { get; set; }
        public GameObject SourceObject
        {
            get => _sourceObject;

            set
            {
                _sourceObject = value;
                if (Animation != null) Animation.SourceElement = SourceObject;
            }
        }

        // Resource costs
        public int HealthCost { get; set; }
        public int StaminaCost { get; set; }
        public int ElectricityCost { get; set; }

        // How it works
        public bool OffCooldown;
        public bool KeepCasting { get; set; }
        public bool IsChanneled { get; set; }
        public bool IsToggleable { get; set; }
        private bool IsActive { get; set; }
        private bool WasPressed { get; set; }
        private bool _isPressed { get; set; }
        private bool RequiresWalking { get; set; }

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
        private Animation _animation { get; set; }
        public Animation Animation
        {
            get => _animation;

            set
            {
                _animation = value;
                if (_animation != null)
                {
                    _animation.SourceElement = SourceObject;
                    if (_animation.Name == "") _animation.Name = Name;
                }
            }
        }

        // What it does
        public float Cooldown;
        public Action Start { get; set; }
        public Action OnUpdate { get; set; }
        public Action Stop { get; set; }

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
                Start();
                if (Animation != null) Animation.Start();

                // Start the cooldown
                if (Cooldown != 0)
                {
                    OffCooldown = false;
                    Timer.AddEvent(Cooldown, delegate { OffCooldown = true; });
                }
            }

            // What happens when we stop
            else if (WasActive && !IsNowActive)
            {
                Globals.Log(SourceObject + " stops using " + this);
                Stop();
                if (Animation != null) Animation.Stop();
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
                SourceObject.Stamina.Current -= StaminaCost;
                SourceObject.Electricity.Current -= ElectricityCost;
            }

            // Play the animation
            if (Animation != null) Animation.Update();
        }
    }
}