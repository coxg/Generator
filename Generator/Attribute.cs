namespace Generator
{
    public class Attribute
        // Strength, Intellect, Speed, Perception, Weight
    {
        private int baseValue;

        // Constructor
        public Attribute(int baseValue)
        {
            BaseValue = baseValue;
            CurrentValue = baseValue;
        }

        public int BaseValue
            // This is the person's base for the attribute.
        {
            get => baseValue;
            set
            {
                CurrentValue += value - baseValue;
                baseValue = value;
            }
        }

        // This is the current value
        public int CurrentValue { get; set; }
    }
}