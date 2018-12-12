using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Component : GameElement
    {
        public Component(
            string spriteFile,
            Vector3 relativePosition,
            float relativeSize,
            Vector3 rotationPoint,
            string name = null,
            bool directional = false,
            GameObject sourceObject = null,
            float yOffset = 0,
            Dictionary<String, Animation> animations = null
        )
        {
            RotationPoint = rotationPoint * 6;
            RotationPoint = new Vector3(
                rotationPoint.X * 6,
                rotationPoint.Y * 6,
                rotationPoint.Z * 18);
            Name = name;
            CurrentFrame = 0;
            Directional = directional;
            SpriteFile = spriteFile;
            RelativePosition = relativePosition;
            Size = relativeSize * 6; // TODO: Why is this necessary? Why is it 6?
            SourceObject = sourceObject;
            YOffset = yOffset;
            Animations = animations ?? new Dictionary<String, Animation>();
        }

        public int CurrentFrame { get; set; }
        public bool Directional { get; set; }
        public string SpriteFile { get; set; }
        public Vector3 RelativePosition { get; set; }
        new public float Size { get; set; }
        public GameObject SourceObject { get; set; }
        public float YOffset { get; set; }
        public Dictionary<String, Animation> Animations { get; set; }

        private float _Direction { get; set; }
        override public float Direction {
            set { _Direction = value; }
            get { return SourceObject.Direction; }
        }
        
        private Vector3 PositionOffset { get; set; }
        override public Vector3 Position
        {
            set { throw new NotImplementedException("Cannot set Component position directly; use an animation instead."); }

            get
            {
                // Get the center point of the object itself - this is what we're rotating around
                var ObjectOffsets = new Vector3(
                    SourceObject.Size.X / 2,
                    SourceObject.Size.Y / 2,
                    0);

                // Get the center point for the component itself - this is what we'll be rotating
                var ComponentSize = SourceObject.Size * Size;
                var ComponentOffsets = ObjectOffsets + new Vector3(
                    (RelativePosition.X - .5f) * ComponentSize.X,
                    (RelativePosition.Y - .5f) * ComponentSize.Y,
                    0);

                // Rotate the center of the component around the center of the object
                var RotatedPosition = Globals.PointRotatedAroundPoint(
                    SourceObject.Position + ComponentOffsets, // Center of component
                    SourceObject.Position + ObjectOffsets, // Center of object
                    new Vector3(0, 0, -SourceObject.Direction));

                // Subtract the offsets we used before - we draw from the bottom left corner, not the center
                var OffsetCorrectedPosition = RotatedPosition - ComponentOffsets;

                // Finally, move the component to its position relative to the object itself
                OffsetCorrectedPosition += new Vector3(
                    SourceObject.Size.X / 2 + (RelativePosition.X - 1) * ComponentSize.X,
                    SourceObject.Size.Y / 2 + YOffset,
                    SourceObject.Size.Z * (RelativePosition.Z - Size / 2));
                OffsetCorrectedPosition += AnimationOffset;

                return OffsetCorrectedPosition;
            }
        }

        public void Update()
        {
            foreach (var animation in Animations) animation.Value.Update();

            // TODO: Only do this when we need to - maybe that's default behavior?
            Sprite = Globals.Content.Load<Texture2D>(
                "Sprites/" + SpriteFile + (
                Directional ? "-" + Globals.StringFromRadians(SourceObject.Direction) : ""));
        }
    }
}
