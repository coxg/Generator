using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Helmet : Equipment
        // Helmets, equipped by GameObjects
    {
        // Constructor
        public Helmet(
            // Sprite
            string spriteFile = "Sprites/black_dot",

            // Stats
            string name = "",
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
        )
        {
            // Sprite
            Sprite = Globals.Content.Load<Texture2D>(spriteFile);

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
            Globals.HelmetDict[Name] = this;
        }
    }
}