using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Equipment : Item
        // Anything equipped by GameObjects
    {
        public string Slot;

        // Stats
        public int Damage;
        public int Defense;
        public int Weight;

        // Resources
        public int Health;
        public int Stamina;
        public int Capacity;

        // Primary Attributes
        public int Strength;
        public int Speed;
        public int Perception;

        // Constructor
        public Equipment(string name, Texture2D sprite, int quantity = 1) : base(name, sprite, quantity)
        {
        }
    }
}