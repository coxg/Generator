using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Equipment
        // Anything equipped by GameObjects
    {

        // Sprite
        public Texture2D Sprite { get; set; }

        // Stats
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public int Weight { get; set; }

        // Resources
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Capacity { get; set; }

        // Primary Attributes
        public int Strength { get; set; }
        public int Speed { get; set; }
        public int Perception { get; set; }
    }
}