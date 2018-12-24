using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Tile
        // 2D objects lying flat on the ground
    {
        // Constructor
        public Tile(
            string name,
            Texture2D sprite,
            int baseTileIndex,
            string baseTileName
        )
        {
            Name = name;
            Sprite = sprite;
            BaseTileIndex = baseTileIndex;
            BaseTileName = baseTileName;
        }

        // Attributes
        public string Name;
        public Texture2D Sprite;
        public int BaseTileIndex;
        public string BaseTileName;
    }
}