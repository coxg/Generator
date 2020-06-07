using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    public class Component : GameElement
    {
        public Component(
            string id,
            Vector3 relativePosition,
            float relativeSize,
            Vector3 baseRotationPoint,
            Vector3? relativeRotation = null,
            Sprite sprite = null,
            bool directional = false,
            GameObject sourceObject = null,
            float yOffset = 0,
            Dictionary<String, Animation> animations = null
        )
        {
            RotationPoint = new Vector3(
                baseRotationPoint.X * 6,
                baseRotationPoint.Y * 6,
                baseRotationPoint.Z * 18); // TODO: What's going on here?
            var idParts = id.Split('/');
            ID = idParts[0];
            if (idParts.Length == 2)
            {
                Side = idParts[1];
            }
            CurrentFrame = 0;
            Directional = directional;
            RelativePosition = relativePosition;
            RelativeRotation = relativeRotation ?? Vector3.Zero;
            Size = relativeSize * 6; // TODO: Why is this necessary? Why is it 6?
            SourceObject = sourceObject;
            YOffset = yOffset;
            Animations = animations ?? new Dictionary<String, Animation>();
            Sprite = sprite;
        }

        public int CurrentFrame;
        public bool Directional;
        public string Side;
        new public Sprite Sprite;
        public Vector3 RelativePosition;
        public Vector3 RelativeRotation;
        new public float Size;
        [JsonIgnore]
        public GameObject SourceObject;
        public float YOffset;
        public Dictionary<String, Animation> Animations;

        [JsonIgnore]
        override public float Direction
        {
            set { throw new NotSupportedException("Set the SourceObject Direction instead."); }
            get { return SourceObject.Direction; }
        }

        [JsonIgnore]
        override public Vector3 Position
        {
            set { throw new NotImplementedException("Cannot set Component position directly; use an animation instead."); }

            get
            {
                // Get the center point of the source object - this is what we're rotating around
                var ObjectOffsets = new Vector3(
                    SourceObject.Size.X / 2,
                    SourceObject.Size.Y / 2,
                    0);

                // Get the center point for the component - this is what we'll be rotating
                var ComponentSize = SourceObject.Size * Size;
                var ComponentOffsets = ObjectOffsets + new Vector3(
                    (RelativePosition.X - .5f) * ComponentSize.X,
                    (RelativePosition.Y - .5f) * ComponentSize.Y,
                    0);

                // Rotate the center of the component around the center of the object
                var RotatedPosition = MathTools.PointRotatedAroundPoint(
                    SourceObject.Position + ComponentOffsets, // Center of component
                    SourceObject.Position + ObjectOffsets, // Center of object
                    new Vector3(0, 0, SourceObject.Direction - MathHelper.PiOver2));

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
        }
    }
}
