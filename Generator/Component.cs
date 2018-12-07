using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Component
    {
        public Component(
            string spriteFile,
            Vector3 relativePosition,
            float relativeSize,
            bool directional = false,
            GameObject sourceObject = null,
            float yOffset = 0,
            float sizeMultiplier = 2
        )
        {
            CurrentFrame = 0;
            Directional = directional;
            SpriteFile = spriteFile; // Fix - System.IO.Directory.GetFiles
            RelativePosition = new Vector3(relativePosition.X, relativePosition.Y, relativePosition.Z * sizeMultiplier);
            RelativeSize = relativeSize * sizeMultiplier;
            SourceObject = sourceObject;
            YOffset = yOffset;
        }

        public int CurrentFrame { get; set; }
        public bool Directional { get; set; }
        public string SpriteFile { get; set; }
        public Texture2D Sprite { get; set; }
        public Vector3 RelativePosition { get; set; }
        public float RelativeSize { get; set; }
        public Vector3 RelativeRotationPoint { get; set; }
        public GameObject SourceObject { get; set; }
        public float YOffset { get; set; }

        public Vector3 Position
        {
            get
            {
                // Get the center point of the object itself - this is what we're rotating around
                var ObjectOffsets = new Vector3(
                    SourceObject.Size.X / 2,
                    SourceObject.Size.Y / 2,
                    0);

                // Get the center point for the component itself - this is what we'll be rotating
                var ComponentOffsets = ObjectOffsets + new Vector3(
                    (RelativePosition.X - .5f) * SourceObject.Size.X * RelativeSize,
                    (RelativePosition.Y - .5f) * SourceObject.Size.Y * RelativeSize,
                    0);

                // Rotate the center of the component around the center of the object
                var RotatedPosition = Globals.PointRotatedAroundPoint(
                    SourceObject.Position + ComponentOffsets, // Center of component
                    SourceObject.Position + ObjectOffsets, // Center of object
                    -SourceObject.Direction);

                // Subtract the offsets we used before - we draw from the bottom left corner, not the center
                var OffsetCorrectedPosition = RotatedPosition - ComponentOffsets;

                // Finally, move the component to its position relative to the object itself
                OffsetCorrectedPosition += new Vector3(
                    SourceObject.Size.X / 2 + (RelativePosition.X - .5f) * SourceObject.Size.X * RelativeSize - .5f * SourceObject.Size.X * RelativeSize,
                    SourceObject.Size.Y / 2 + YOffset,
                    SourceObject.Size.Z * (RelativePosition.Z - RelativeSize / 2));

                return OffsetCorrectedPosition;
            }
        }

        public void Update()
        {
            Sprite = Globals.Content.Load<Texture2D>(
                "Sprites/" + SpriteFile + (
                Directional ? "-" + Globals.StringFromRadians(SourceObject.Direction) : ""));
        }
    }
}
