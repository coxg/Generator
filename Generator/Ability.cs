using System;

namespace Generator
{
    public class Ability
    {
        // Resource costs
        public int HealthCost { get; set; }
        public int StaminaCost { get; set; }
        public int ElectricityCost { get; set; }

        // If it's channeled... duh
        public bool Channeled { get; set; }

        // What's using the ability
        public GameObject SourceObject { get; set; }

        // TODO: I might need to make an animation class
        public Action WeaponAnimation { get; set; }

        // What actually happens when the ability is used
        public Action Script { get; set; }

        // Constructor
        public Ability(
            int healthCost = 0,
            int staminaCost = 0,
            int electricityCost = 0,
            bool channeled = false,
            GameObject sourceObject = null,
            Action weaponAnimation = null,
            Action script = null)
        {
            HealthCost = healthCost;
            StaminaCost = staminaCost;
            ElectricityCost = electricityCost;
            Channeled = channeled;
            SourceObject = sourceObject;
            WeaponAnimation = weaponAnimation;
            Script = script;
        }
    }
}
