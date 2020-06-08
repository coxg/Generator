using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class GeneratorObj : Equipment
        // Weapons, equipped by GameObjects
    {
        // Constructor
        public GeneratorObj(
            string name,
            Sprite sprite, 
            int quantity = 1,

            // Stats
            int damage = 0,
            int defense = 0,

            // Resources
            int health = 0,
            int capacity = 0,

            // Primary Attributes
            int strength = 0,
            int speed = 0,
            int perception = 0
        ) : base(name, sprite, quantity)
        {
            Slot = "Generator";

            // Sprite
            Sprite = sprite;

            // Stats
            Name = name;
            Damage = damage;
            Defense = defense;

            // Resources
            Health = health;
            Capacity = capacity;

            // Primary Attributes
            Strength = strength;
            Speed = speed;
            Sense = perception;
        }
    }
}