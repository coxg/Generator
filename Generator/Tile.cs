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
            int baseTileIndex,
            string baseTileName
        )
        {
            ID = id;
            Sprite = sprite;
            BaseTileIndex = baseTileIndex;
            BaseTileName = baseTileName;
        }

        // Attributes
        public string ID;
        public Cached<Texture2D> Sprite;
        public int BaseTileIndex;
        public string BaseTileName;
    }
}