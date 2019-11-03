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

        // Resource max values modifiers
        public int Health;
        public int Stamina;
        public int Capacity;

        // Primary attribute modifiers
        public int Strength;
        public int Speed;
        public int Sense;
        public int Style;

        // Constructor
        public Equipment(string name, Texture2D sprite, int quantity = 1) : base(name, sprite, quantity)
        {
        }
    }
}