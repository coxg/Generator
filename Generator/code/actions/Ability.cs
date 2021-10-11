using System;
using System.Collections.Generic;
using Generator.code.objects;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Generator
{
    public enum Type { Untyped, Physical, Fire, Water, Electrical }

    public class Ability
    {
        // Constructor
        public Ability(
            string description,
            Type type,
            int healthCost = 0,
            int manaCost = 0,
            int damage = 0,
            int healing = 0,
            int range = 1,
            int radius = 0,
            bool collision = true,
            float castTime = 0,
            float recharge = 0,
            float cooldown = 0,
            List<Ailment> ailments = null,
            Animation animation = null,
            Action<GameObject, Vector3> locationEffect = null,
            Action<GameObject, GameObject> objectEffect = null)
        {
            Name = GetType().Name;
            Description = description;
            Type = type;

            HealthCost = healthCost;
            ManaCost = manaCost;

            Damage = damage;
            Healing = healing;
            
            Range = range;
            Radius = radius;
            Collision = collision;
            
            CastTime = castTime;
            Recharge = recharge;
            Cooldown = cooldown;

            Ailments = ailments;
            LocationEffect = locationEffect;
            ObjectEffect = objectEffect;
            Animation = animation;

            Abilities.set(this);
        }

        public string Name;
        public string Description;
        public Type Type;
        public int HealthCost;
        public int ManaCost;
        public int Damage;
        public int Healing;
        public List<Ailment> Ailments;
        public Action<GameObject, Vector3> LocationEffect;
        public Action<GameObject, GameObject> ObjectEffect;
        public float CastTime;
        public float Cooldown;
        public float Recharge;
        public int Range;  // not including the user's position, so 0 is self-target
        public int Radius;  // not including the target itself, so 0 is 1 square
        public bool Collision;
        public Animation Animation;

        public override string ToString()
        // Return name, useful for debugging.
        {
            return Name;
        }

        public bool CanUse(GameObject gameObject)
        {
            return gameObject.AbilityCooldowns.ContainsKey(Name) 
                   && gameObject.AbilityCooldowns[Name] <= 0
                   && gameObject.Health.Current > HealthCost
                   && gameObject.Mana.Current >= ManaCost;
        }
    }
}