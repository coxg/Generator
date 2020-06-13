using Microsoft.Xna.Framework;

namespace Generator.code.objects
{
    public class LightComponent : Component
    {
        public LightComponent(
            Vector3 size,
            Color? color = null,
            float brightness = 1
        ) : base(Globals.LightSprite, size * 3)
        {
            Sprite = Globals.LightSprite;
            Color = color ?? Color.White;
            Brightness = brightness;
        }

        public Color Color;
        public float Brightness;
    }
}