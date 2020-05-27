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
            bool hasCorners,
            float layer
        )
        {
            Name = name;
            NumBaseTiles = numBaseTiles;
            HasCorners = hasCorners;
            Layer = layer;
        }

        // Attributes
        public string Name;
        public int NumBaseTiles;
        public bool HasCorners;
        public float Layer;  // Order to draw them - higher is drawn on top of lower
    }
}