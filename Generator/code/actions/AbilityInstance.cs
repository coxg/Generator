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
            GameObject initialTargetObject,
            Vector3 targetOffset)
        {
            Name = name;
            _ability = Abilities.get(Name);

            SourceObject = sourceObject;
            InitialTargetObject = initialTargetObject;
            TargetOffset = targetOffset;
        }

        public String Name;
        
        public GameObject SourceObject;
        public GameObject InitialTargetObject;
        
        public Vector3 InitialTargetPos;
        public Vector3 TargetOffset;
        
        public float RemainingCastTime;
        public float RemainingRecharge;
        public float RemainingCooldown;

        [JsonIgnore] 
        private Ability _ability;
        public Ability Ability
        {
            get => _ability;
        }

        public bool CanUse()
        {
            return RemainingCooldown <= 0
                   && SourceObject.Health.Current > Ability.HealthCost
                   && SourceObject.Mana.Current >= Ability.ManaCost;
        }

        public void Start()
        {
            Globals.Log(SourceObject + " starts casting " + this);
            SourceObject.TakeDamage(Ability.HealthCost, Type.Untyped);
            SourceObject.Mana.Current -= Ability.ManaCost;
            Ability.Animation?.Start();

            RemainingRecharge = Ability.Recharge;
            RemainingCastTime = Ability.CastTime;
            RemainingCooldown = Ability.Cooldown;
        }

        public void Update()
        {
            Ability.Animation?.Update();

            if (Timing.SecondsPassed > RemainingCooldown && RemainingCooldown > 0)
            {
                Stop();
            }
            
            RemainingRecharge = Math.Min(0, RemainingRecharge - Timing.SecondsPassed);
            RemainingCastTime = Math.Min(0, RemainingCastTime - Timing.SecondsPassed);
            RemainingCooldown = Math.Min(0, RemainingCooldown - Timing.SecondsPassed);
        }

        public void Stop()
        {
            Globals.Log(SourceObject + " casts " + this);
            Ability.Animation?.Stop();

            var targetPosition = SourceObject.Position += TargetOffset;
            if (Ability.Collision)
            {
                targetPosition = SourceObject.GetFirstTargetInRange(TargetOffset.Length())?.Position ?? targetPosition;
            }
            
            var targetPositions = MathTools.GetCoordinatesInCircle(
                (int)targetPosition.X, (int)targetPosition.Y, Ability.Radius);

            var targetObjects = new List<GameObject>();
            foreach (var eachTargetPosition in targetPositions)
            {
                Ability.LocationEffect?.Invoke(SourceObject, eachTargetPosition);
                targetObjects.Union(Globals.GameObjectManager.Get(eachTargetPosition.X, eachTargetPosition.Y));
            }

            foreach (var targetObject in targetObjects)
            {
                Ability.ObjectEffect?.Invoke(SourceObject, targetObject);
                SourceObject.DealDamage(targetObject, Ability.Damage, Ability.Type);
                targetObject.Heal(Ability.Healing);
                targetObject.Ailments.UnionWith(Ability.Ailments);  // TODO: Ailments need their own getter/setter logic
            }
        }
    }
}