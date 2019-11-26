using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Weapon : Equipment
        // Weapons, equipped by GameObjects
    {
        // Constructor
        public Weapon(
            string name,
            Loaded<Texture2D> sprite,
            int quantity = 1,

            // Stats
            int range = 1,
            float spread = 0,
            int area = 0,
            int damage = 0,
            int defense = 0,

            // Resources
            int health = 0,
            int stamina = 0,
            int capacity = 0,

            // Primary Attributes
            int strength = 0,
            int speed = 0,
            int perception = 0
        ) : base(name, sprite, quantity)
        {
            Slot = "Armor";

            // Sprite
            Sprite = sprite;

            // Stats
            Name = name;
            Range = range;
            Spread = spread;
            Area = area;
            Damage = damage;
            Defense = defense;

            // Resources
            Health = health;
            Stamina = stamina;
            Capacity = capacity;

            // Primary Attributes
            Strength = strength;
            Speed = speed;
            Sense = perception;

            // Populate relevant dictionary
            Globals.WeaponsDict[name] = this;
        }

        // Stats
        public int Range { get; set; }
        public float Spread { get; set; }
        public int Area { get; set; }
    }
}