using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Equipment
    // Weapons, equipped by GameObjects
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

        // Constructor
        public Equipment(

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
            int intellect = 0,
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
        }
    }
}
