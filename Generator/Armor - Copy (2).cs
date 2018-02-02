using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Armor
    // Weapons, equipped by GameObjects
    {
        // Sprite
        public Texture2D Sprite { get; set; }

        // Stats
        public string Name { get; set; }
        public int Range { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public string Type { get; set; }
        public float Spread { get; set; }
        public int Area { get; set; }
        public int Weight { get; set; }

        // Resources
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Capacity { get; set; }

        // Primary Attributes
        public int Strength { get; set; }
        public int Intellect { get; set; }
        public int Speed { get; set; }
        public int Perception { get; set; }

        // Constructor
        public Armor(

            // Sprite
            string spriteFile = "Sprites/black_dot",

            // Stats
            string name = "Fists",
            int range = 1,
            int damage = 0,
            int defense = 0,
            string type = "Bash",
            float spread = 0,
            int area = 0,
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
            Range = range;
            Damage = damage;
            Defense = defense;
            Type = type;
            Spread = spread;
            Area = area;
            Weight = weight;

            // Resources
            Health = health;
            Stamina = stamina;
            Capacity = capacity;

            // Primary Attributes
            Strength = strength;
            Intellect = intellect;
            Speed = speed;
            Perception = perception;
        }
    }
}
