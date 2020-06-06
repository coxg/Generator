using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public abstract class GameElement
        // GameObjects and their components
    {
        public string ID;
        public string Name;
        public Vector3 AnimationOffset;
        public Vector3 RotationOffset;
        public Vector3 RotationPoint;
        public Vector3 Size;
        public abstract Texture2D Sprite { get; set; }
        public abstract float Direction { get; set; }

        protected Vector3 _Center;
        public Vector3 Center { get => _Center; }
        protected Vector3 _Position;
        public virtual Vector3 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                _Center = Position + Size / 2;
            }
        }
    }
}