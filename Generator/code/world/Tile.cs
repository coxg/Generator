using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Tile
        // 2D objects lying flat on the ground
    {
        // Constructor
        public Tile(
            string id,
            Cached<Texture2D> sprite,
            string baseTileName
        )
        {
            ID = id;
            Sprite = sprite;
            BaseTileName = baseTileName;
        }

        // Attributes
        public string ID;
        public Cached<Texture2D> Sprite;
        public string BaseTileName;
    }
}