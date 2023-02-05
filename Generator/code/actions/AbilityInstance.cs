using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Generator
{
    public class AbilityInstance
    {
        public AbilityInstance(
            String name,
            GameObject sourceObject,
            Vector3 target)
        {
            Name = name;
            _ability = Abilities.get(Name);

            SourceObject = sourceObject;
            Target = target;
        }

        public String Name;
        public GameObject SourceObject;
        public Vector3 Target;
        
        public float RemainingCastTime;
        public float RemainingRecharge;

        [JsonIgnore] 
        private Ability _ability;
        public Ability Ability
        {
            get => _ability;
        }
        
        public override string ToString()
        {
            return Name;
        }

        public List<Vector3> GetTargetPositions()
        {
            return MathTools.GetCoordinatesInCircle(Target, Ability.Radius);
        }
    }
}