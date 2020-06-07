using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Generator
{
    public class Component : GameElement
    {
        public Component(
            Sprite sprite,  // Can be null, but should be explicit when it is
            Vector3? size = null,
            Vector3? relativePosition = null,
            Vector3? rotationPoint = null,
            Vector3? relativeRotation = null,
            GameObject sourceObject = null,
            float yOffset = 0,
            Dictionary<String, Animation> animations = null
        )
        {
            RotationPoint = rotationPoint ?? new Vector3(.5f);
            RelativePosition = relativePosition ?? new Vector3(.5f);
            RelativeRotation = relativeRotation ?? Vector3.Zero;
            SourceObject = sourceObject;
            _size = size;
            YOffset = yOffset;
            Animations = animations ?? new Dictionary<String, Animation>();
            Sprite = sprite;
        }
        
        new public Sprite Sprite;
        public Vector3 RelativePosition;
        public Vector3 RelativeRotation;
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

        private Vector3? _size;
        
        [JsonIgnore]
        public Vector3 Size
        {
            set { _size = value; }
            get { return _size ?? SourceObject.Size; }
        }

        private Vector3 _position;

        [JsonIgnore]
        override public Vector3 Position
        {
            set { throw new NotSupportedException("Cannot set Component position directly; use an animation instead."); }
            get { return _position; }
        }

        private void UpdatePosition()
            // Gets the position when rotated around the sourceObject due to the sourceObject's direction.
            // This DOES NOT include offsets from animations/rotations.
        {
            // Get the center point of the source object - this is what we're rotating around
            var ObjectOffsets = new Vector3(
                SourceObject.Size.X / 2,
                SourceObject.Size.Y / 2,
                0);

            // Get the center point for the component - this is what we'll be rotating
            var ComponentOffsets = ObjectOffsets + new Vector3(
                (RelativePosition.X - .5f) * Size.X,
                (RelativePosition.Y - .5f) * Size.Y,
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
                SourceObject.Size.X / 2 + (RelativePosition.X - 1) * Size.X,
                SourceObject.Size.Y / 2 + YOffset,
                SourceObject.Size.Z * (RelativePosition.Z - Size.Z / 2));
            OffsetCorrectedPosition += AnimationOffset;
            _position = OffsetCorrectedPosition;
        }

        public void Update()
        {
            if (GameControl.camera.VisibleArea.IntersectsWith(SourceObject.Area))
            {
                UpdatePosition();
            }
            foreach (var animation in Animations) animation.Value.Update();
        }
    }
}
