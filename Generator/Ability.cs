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
            bool isChanneled = false,
            bool isToggleable = false,

            // What it looks like
            Animation animation = null,

            // What it does
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
            IsChanneled = isChanneled;
            IsToggleable = isToggleable;
            IsActive = false;

            // What's using the ability
            SourceObject = sourceObject;

            // What it looks like
            Animation = animation;

            // What it does
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
        public bool IsChanneled { get; set; }
        public bool IsToggleable { get; set; }
        private bool IsActive { get; set; }
        private bool WasPressed { get; set; }
        private bool _isPressed { get; set; }

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
            return SourceObject.Health.Current >= HealthCost
                   && SourceObject.Stamina.Current >= StaminaCost
                   && SourceObject.Electricity.Current >= ElectricityCost;
        }

        public void Use()
            // This is what happens when the ability is used.
        {
            if (CanUse())
            {
                Globals.Log(SourceObject + " uses " + this);

                // If toggle and on, turn off
                if (IsToggleable && IsActive)
                {
                    IsActive = false;
                }

                // If you can use it
                else if (IsChanneled || IsToggleable)
                {
                    IsActive = true;
                }
                else
                {
                    // Use resources
                    SourceObject.Health.Current -= HealthCost;
                    SourceObject.Stamina.Current -= StaminaCost;
                    SourceObject.Electricity.Current -= ElectricityCost;

                    // Perform the ability
                    Start();
                }
            }
            else
            {
                Globals.Log(SourceObject + " can't use " + this);
            }
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
                if (!WasActive && !WasPressed && IsPressed && CanUse()) IsNowActive = true;
            }

            // What happens when we start
            if (!WasActive && IsNowActive)
            {
                Globals.Log(SourceObject + " uses " + this);
                Start();
                if (Animation != null) Animation.Start();
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
                if (Animation != null) Animation.OnUpdate();
            }

            // Update variable
            IsActive = IsNowActive;

            // Use resources
            if (IsActive)
            {
                SourceObject.Health.Current -= HealthCost;
                SourceObject.Stamina.Current -= StaminaCost;
                SourceObject.Electricity.Current -= ElectricityCost;
            }

            // Play the animation
            if (Animation != null) Animation.Update();
        }
    }
}