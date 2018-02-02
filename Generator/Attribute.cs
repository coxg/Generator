using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Attribute
    // Strength, Intellect, Speed, Perception, Weight
    {

        private int baseValue;
        public int BaseValue
        // This is the person's base for the attribute.
        {
            get
            {
                return baseValue;
            }
            set
            {
                CurrentValue += value - baseValue;
                baseValue = value;
            }
        }

        // This is the current value
        public int CurrentValue { get; set; }

        // Constructor
        public Attribute(int baseValue)
        {
            BaseValue = baseValue;
            CurrentValue = baseValue;
        }
    }
}
