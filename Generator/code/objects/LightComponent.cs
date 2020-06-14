using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Generator.code.objects
{
    public class LightComponent : Component
    {
        public LightComponent(
            Vector3 size,
            Color? color = null,
            float brightness = 1,
            bool flicker = false
        ) : base(
            Globals.LightSprite, 
            size * 3)
        {
            Sprite = Globals.LightSprite;
            Color = color ?? Color.White;
            Brightness = brightness;
            Flicker = flicker;
            if (Flicker)
            {
                Animations = new Dictionary<string, Animation>
                {
                    {
                        "flicker", new Animation(
                            updateFrames: new Frames(
                                baseResizes: new List<Vector3>
                                {
                                    Vector3.One,
                                    new Vector3(1.1f),
                                    Vector3.One,
                                    new Vector3(1.05f),
                                    Vector3.One,
                                    new Vector3(1.05f),
                                    Vector3.One,
                                    new Vector3(.9f),
                                    Vector3.One,
                                    new Vector3(1.1f),
                                    Vector3.One,
                                    new Vector3(.95f),
                                    Vector3.One,
                                },
                                duration: 20))
                    }
                };
                Animations["flicker"].AnimatedElement = this;
                Animations["flicker"].Start();
            }
        }

        public Color Color;
        public float Brightness;
        public bool Flicker;
    }
}