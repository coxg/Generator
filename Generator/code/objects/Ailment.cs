using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Generator.code.objects
{
    // TODO: This whole thing needs to be rewritten
    public class Ailment
    {
        public Ailment(
            string name, string key, GameObject sourceObject, GameObject targetObject,
            float totalDuration, float elapsedDuration=0, float frequency=1)
        {
            Name = name;
            Key = key;

            SourceObject = sourceObject;
            TargetObject = targetObject;
            TotalDuration = totalDuration;

            ElapsedDuration = elapsedDuration;
            Frequency = frequency;
        }

        public string Name;  // The displayed name
        public string Key;  // Two ailments with the same key can't stack

        public float ElapsedDuration;
        public float TotalDuration;
        public float Frequency;

        [JsonIgnore]
        public GameObject SourceObject;
        [JsonIgnore]
        public GameObject TargetObject;

        public virtual void ApplyEffects() { }
        public virtual int GetDamage() { return 1; }

        public virtual Dictionary<string, float> GetStatModifiers() { return new Dictionary<string, float>(); }

        public void Update()
        {
            if (MathTools.Mod(ElapsedDuration, Frequency) + Timing.GameSpeed / Globals.RefreshRate >= Frequency)
            {
                // TODO: Different ability damage types?
                SourceObject.DealDamage(TargetObject, GetDamage(), Type.Untyped);
                ApplyEffects();
            }
            ElapsedDuration += Timing.GameSpeed / Globals.RefreshRate;
        }
    }

    public class Burning : Ailment
    {
        public Burning(
            string name, string key, GameObject sourceObject, GameObject targetObject,
            float totalDuration, float elapsedDuration = 0, float frequency = 1) : base(
                name, key, sourceObject, targetObject, totalDuration, elapsedDuration, frequency) { }

        public override void ApplyEffects()
        {
        }

        public override int GetDamage()
        {
            return 1;
        }
    }

    public class Poisoned : Ailment
    {
        public Poisoned(
            string name, string key, GameObject sourceObject, GameObject targetObject,
            float totalDuration, float elapsedDuration = 0, float frequency = 1) : base(
                name, key, sourceObject, targetObject, totalDuration, elapsedDuration, frequency)
        { }

        public override Dictionary<string, float> GetStatModifiers()
        {
            return new Dictionary<string, float>
            {
                { "Strength", -.25f }
            };
        }

        public override void ApplyEffects()
        {
        }

        public override int GetDamage()
        {
            return 1;
        }
    }

    public class Chilled : Ailment
    {
        public Chilled(
            string name, string key, GameObject sourceObject, GameObject targetObject,
            float totalDuration, float elapsedDuration = 0, float frequency = 1) : base(
                name, key, sourceObject, targetObject, totalDuration, elapsedDuration, frequency)
        { }

        public override Dictionary<string, float> GetStatModifiers()
        {
            return new Dictionary<string, float>
            {
                { "Speed", -.25f }
            };
        }

        public override void ApplyEffects()
        {
        }

        public override int GetDamage()
        {
            return 0;
        }
    }
}
