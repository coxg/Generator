namespace Generator
{
    public class Resource
        // Health, stamina, and capacity
    {
        private int baseValue;

        private float current;

        private int max;

        // Constructor
        public Resource(
            string name,
            int baseValue,
            int regeneration = 0
        )
        {
            Name = name;
            BaseValue = baseValue;
            Current = baseValue;
            Max = baseValue;
            Regeneration = regeneration;
        }

        // One of "Health", "Stamina", "Electricity"
        public string Name { get; set; }

        public int BaseValue
            // This is the person's base for the resource.
        {
            get => baseValue;
            set
            {
                Max += value - baseValue;
                baseValue = value;
            }
        }

        public float Current
            // Current value. 0 < Current < Max
        {
            get => current;
            set
            {
                if (value > Max)
                    value = Max;
                else if (value < 0) value = 0;
                current = value;
            }
        }

        public int Max
            // Max for value
        {
            get => max;
            set
            {
                // Min for value
                if (value < 1 && Name == "Health")
                    value = 1;
                else if (value < 0) value = 0;

                // Update other attributes
                if (Current > value)
                    Current = value;
                else if (Current < value && value > max) Current += value - max;
                max = value;
            }
        }

        // How quickly this stat regenerates per second
        public int Regeneration { get; set; }

        // Each frame
        public void Update()
        {
            Current += Regeneration * Timing.GameSpeed / Globals.RefreshRate;
        }
    }
}