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
            Vector3 size,
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
            Size = size;
            YOffset = yOffset;
            Animations = animations ?? new Dictionary<String, Animation>();
            Sprite = sprite;
        }
        
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

        private void UpdatePosition()
            // Gets the position when rotated around the sourceObject due to the sourceObject's direction.
        {
            Center = SourceObject.Position + RelativePosition * SourceObject.Size + AnimationOffset;
            Center = MathTools.PointRotatedAroundPoint(
                Center,
                SourceObject.Center,
                new Vector3(0, 0, SourceObject.Direction - MathHelper.PiOver2));
            Center += new Vector3(0, YOffset, 0);
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
