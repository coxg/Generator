using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Resource
    // Health, stamina, and capacity
    {
        // Only for the sake of changing health floor to 1
        public string Name { get; set; }

        private int baseValue;
        public int BaseValue
        // This is the person's base for the resource.
        {
            get {
                return baseValue;
            }
            set
            {
                Max += value - baseValue;
                baseValue = value;
            }
        }

        private int current;
        public int Current
        // Current value. 0 < Current < Max
        {
            get { return current; }
            set
            {
                if (value > Max)
                {
                    value = Max;
                }
                else if (value < 0)
                {
                    value = 0;
                }
                current = value;
            }
        }

        private int max;
        public int Max
        // Max for value
        {
            get {
                return max;
            }
            set
            {
                // Min for value
                if (value < 1 && Name == "Health")
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                // Update other attributes
                if (Current > value)
                {
                    Current = value;
                }
                else if (Current < value && value > max)
                {
                    Current += (value - max);
                }
                max = value;
            }
        }

        // How quickly this stat regenerates per turn
        public int Regeneration { get; set; }

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
    }
}
