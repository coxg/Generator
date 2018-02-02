using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Weapon : Equipment
    // Weapons, equipped by GameObjects
    {
        // Stats
        int Range { get; set; }
        string Type { get; set; } // {"Bash", "Cut", "Shoot"}
        float Spread { get; set; }
        int Area { get; set; }

        // Constructor
        public Weapon(

            // Sprite
            string spriteFile = "Sprites/black_dot",

            // Stats
            string name = "Fists",
            int range = 1,
            string type = "Bash", // {"Bash", "Cut", "Shoot"}
            float spread = 0,
            int area = 0,
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
            Range = range;
            Type = type; // {"Bash", "Cut", "Shoot"}
            Spread = spread;
            Area = area;
            Damage = damage;
            Defense = defense;
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

            // Populate relevant dictionary
            Globals.WeaponsDict[name] = this;
        }
    }
}
