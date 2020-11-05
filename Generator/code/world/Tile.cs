using System;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Tile
        // 2D objects lying flat on the ground
    {
        // Constructor
        public Tile(
            string name,
            int numBaseTiles,
            float layer,
            int firstBaseId,
            int? firstEdgeId=null,
            float friction=1f
        )
        {
            Name = name;
            NumBaseTiles = numBaseTiles;
            Layer = layer;
            FirstBaseId = firstBaseId;
            FirstEdgeId = firstEdgeId;
            Friction = friction;
        }

        // Attributes
        public string Name;
        public int NumBaseTiles;
        public float Layer;  // Order to draw them - higher is drawn on top of lower
        public int FirstBaseId;
        public int? FirstEdgeId;  // If null then it has no corners
        public float Friction;

        public int GetRandomBaseId()
        {
            return FirstBaseId + MathTools.RandInt(NumBaseTiles);
        }

        public int GetCornerEdgeId()
        {
            if (FirstEdgeId == null)
            {
                throw new ArgumentException(Name + " does not have edges defined!");
            }
            return (int)FirstEdgeId;
        }
        
        public int GetLEdgeId()
        {
            if (FirstEdgeId == null)
            {
                throw new ArgumentException(Name + " does not have edges defined!");
            }
            return (int)FirstEdgeId + 1;
        }
        
        public int GetOEdgeId()
        {
            if (FirstEdgeId == null)
            {
                throw new ArgumentException(Name + " does not have edges defined!");
            }
            return (int)FirstEdgeId + 2;
        }
        
        public int GetSideEdgeId()
        {
            if (FirstEdgeId == null)
            {
                throw new ArgumentException(Name + " does not have edges defined!");
            }
            return (int)FirstEdgeId + 3;
        }
        
        public int GetUEdgeId()
        {
            if (FirstEdgeId == null)
            {
                throw new ArgumentException(Name + " does not have edges defined!");
            }
            return (int)FirstEdgeId + 4;
        }
    }
}