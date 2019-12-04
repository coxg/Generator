using System;
using Newtonsoft.Json;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Cached <T>
        // A strategy for lazy loading based off of a string identifier
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                // Can't use case/switch because typeof isn't a constant
                object baseValue = null;
                var ttype = typeof(T);
                if (ttype == typeof(Action<GameObject, GameObject>))
                {
                    baseValue = Actions.TargetedActions[value];
                }
                else if (ttype == typeof(Action<GameObject>))
                {
                    baseValue = Actions.SelfActions[value];
                }
                else if (ttype == typeof(Texture2D))
                {
                    if (Globals.Textures.ContainsKey(value))
                    {
                        baseValue = Globals.Textures[value];
                    }
                    else
                    {
                        baseValue = Globals.Content.Load<Texture2D>(value);
                        Globals.Textures[value] = (Texture2D)baseValue;
                    }
                }
                else
                {
                    throw new TypeLoadException(typeof(T) + " is not a known Loaded type.");
                }
                _value = (T)Convert.ChangeType(baseValue, typeof(T));
                name = value;
            }
        }

        [JsonIgnore]
        private T _value;
        [JsonIgnore]
        public T Value
        {
            get { return _value; }
        }

        public Cached(string name)
        {
            Name = name;
        }
    }
}
