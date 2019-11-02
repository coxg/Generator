using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Armor : Equipment
        // Weapons, equipped by GameObjects
    {
        // Constructor
        public Armor(
            string name,
            Texture2D sprite,
            int quantity = 1,

            // Stats
            int damage = 0,
            int defense = 0,
            int weight = 0,

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
            Damage = damage;
            Defense = defense;
            Weight = weight;

            // Resources
            Health = health;
            Stamina = stamina;
            Capacity = capacity;

            // Primary Attributes
            Strength = strength;
            Speed = speed;
            Perception = perception;

            // Populate relevant dictionary
            Globals.ArmorDict[Name] = this;
        }
    }
}