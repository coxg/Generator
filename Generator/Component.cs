using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Component : GameElement
    {
        public Component(
            string name,
            Vector3 relativePosition,
            float relativeSize,
            Vector3 rotationPoint,
            Vector3? relativeRotation = null,
            string spriteFile = null,
            bool directional = false,
            string side = null,
            bool castsShadow = true,
            GameObject sourceObject = null,
            float yOffset = 0,
            Dictionary<String, Animation> animations = null
        )
        {
            RotationPoint = new Vector3(
                rotationPoint.X * 6,
                rotationPoint.Y * 6,
                rotationPoint.Z * 18); // TODO: What's going on here?
            Name = name;
            Side = side;
            CurrentFrame = 0;
            Directional = directional;
            RelativePosition = relativePosition;
            RelativeRotation = relativeRotation ?? Vector3.Zero;
            Size = relativeSize * 6; // TODO: Why is this necessary? Why is it 6?
            SourceObject = sourceObject;
            YOffset = yOffset;
            Animations = animations ?? new Dictionary<String, Animation>();
            Sprites = new Dictionary<string, Texture2D>();
            SpriteFile = spriteFile;
            CastsShadow = castsShadow;

        }

        public int CurrentFrame;
        public bool Directional;
        public string Side;
        public Vector3 RelativePosition;
        public Vector3 RelativeRotation;
        new public float Size;
        public GameObject SourceObject;
        public float YOffset;
        public Dictionary<String, Animation> Animations;
        public Dictionary<String, Texture2D> Sprites;

        private float _Direction;
        override public float Direction {
            set { _Direction = value; }
            get { return SourceObject.Direction; }
        }

        private string _spriteFile;
        public string SpriteFile
        {
            get { return _spriteFile; }

            set
            {
                // Determine the base path for the component based on the input and what files exists
                var ComponentPath = "Components/" + Name + "/";
                if (value != null && Directory.Exists(Globals.Directory + "/Content/" + ComponentPath + value))
                {
                    ComponentPath += value + "/";
                }
                else if (Directory.Exists(Globals.Directory + "/Content/" + ComponentPath + "Default"))
                {
                    ComponentPath += "Default/";
                }

                // Components can have different sprites for each side they're on
                if (Side != null && Directory.Exists(Globals.Directory + "/Content/" + ComponentPath + Side))
                {
                    ComponentPath += Side + "/";
                }

                // Load up the sprites for each specified direction
                if (Directional)
                {
                    Sprites["Front"] = Globals.Content.Load<Texture2D>(ComponentPath + "Front");
                    Sprites["Back"] = Globals.Content.Load<Texture2D>(ComponentPath + "Back");
                    Sprites["Left"] = Globals.Content.Load<Texture2D>(ComponentPath + "Left");
                    Sprites["Right"] = Globals.Content.Load<Texture2D>(ComponentPath + "Right");
                }
                else
                {
                    Sprites[""] = Globals.Content.Load<Texture2D>(ComponentPath + Name);
                }
                _spriteFile = value;
            }
        }

        override public Texture2D Sprite
        {
            get {
                if (Directional)
                {
                    return Sprites[MathTools.StringFromRadians(Direction)];
                }
                else
                {
                    return Sprites[""];
                }
            }

            set { throw new NotImplementedException("Cannot set Component Sprite directly; use SpriteFile instead."); }
        }

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
        }
    }
}
