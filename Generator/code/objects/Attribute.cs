namespace Generator
{
    public class Attribute
        // Strength, Speed, Sense, Style
    {
        public int BaseValue;
        public int Multiplier = 1;
        public int Modifier;

        // Constructor
        public Attribute(int baseValue)
        {
            BaseValue = baseValue;
        }

        // This is the current value
        public int CurrentValue
        {
            get
            {
                var currentValue = (BaseValue + Modifier) * Multiplier;
                if (currentValue < 1)
                {
                    return 0;
                }
                else
                {
                    return currentValue;
                }
            }
        }
    }
}