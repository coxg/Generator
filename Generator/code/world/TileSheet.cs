using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class TileSheet
    {
        public TileSheet(
            string spriteName,
            List<Tile> tiles,
            int tileSize=256
        )
        {
            SpriteName = spriteName;
            Sprite = Globals.ContentManager.Load<Texture2D>(SpriteName);
            TileSize = tileSize;
            Tiles = tiles;
            Height = Sprite.Height / TileSize;
            Width = Sprite.Width / TileSize;
        }
        
        public string SpriteName;
        public int TileSize;
        public int Height;  // In tiles
        public int Width;  // In tiles
        public List<Tile> Tiles;  // In the order they appear in the sheet
        
        [Newtonsoft.Json.JsonIgnore]
        public Texture2D Sprite;

        public int GetRandomBaseTileId(int row=0)
        {
            return row * Width + MathTools.RandInt(Tiles[row].NumBaseTiles);
        }

        public Tile TileFromId(int id)
        {
            return Tiles[id / Width];
        }

        public Rectangle TextureCoordinatesFromId(int id)
        {
            var row = id / Width;
            var col = (int)MathTools.Mod(id, Width);
            return new Rectangle(
                row * TileSize,
                col * TileSize,
                TileSize,
                TileSize);
        }

        public Vector2[] TextureVerticesFromId(int id, string orientation="Bottom")
        {
            // Get the coordinates of the texture on the sheet
            var row = id / Width;
            var col = (int)MathTools.Mod(id, Width);
            
            // TODO: Without this you see graphical bugs, but there's no reason to think this works for all sheet sizes
            var roundingOffset = .001f;
            
            var xMin = (float)col / Width + roundingOffset;
            var xMax = (float)(col + 1) / Width - roundingOffset;
            var yMin = (float)row / Height + roundingOffset;
            var yMax = (float)(row + 1) / Height - roundingOffset;
            
            var bottomLeft = new Vector2(xMin, yMax);
            var topLeft = new Vector2(xMin, yMin);
            var bottomRight = new Vector2(xMax, yMax);
            var topRight = new Vector2(xMax, yMin);
            
            // Rotate the coordinates as necessary
            var textureCoordinates = new Vector2[6];
            switch (orientation)
            {
                case "Bottom":
                    textureCoordinates[0] = bottomLeft; // Bottom left
                    textureCoordinates[1] = topLeft; // Top left
                    textureCoordinates[2] = bottomRight; // Bottom right
                    textureCoordinates[3] = textureCoordinates[1];
                    textureCoordinates[4] = topRight; // Top right
                    textureCoordinates[5] = textureCoordinates[2];
                    break;
                case "Top":
                    textureCoordinates[0] = topRight; // Bottom left
                    textureCoordinates[1] = bottomRight; // Top left
                    textureCoordinates[2] = topLeft; // Bottom right
                    textureCoordinates[3] = textureCoordinates[1];
                    textureCoordinates[4] = bottomLeft; // Top right
                    textureCoordinates[5] = textureCoordinates[2];
                    break;
                case "Left":
                    textureCoordinates[0] = bottomRight; // Bottom left
                    textureCoordinates[1] = bottomLeft; // Top left
                    textureCoordinates[2] = topRight; // Bottom right
                    textureCoordinates[3] = textureCoordinates[1];
                    textureCoordinates[4] = topLeft; // Top right
                    textureCoordinates[5] = textureCoordinates[2];
                    break;
                case "Right":
                    textureCoordinates[0] = topLeft; // Bottom left
                    textureCoordinates[1] = topRight; // Top left
                    textureCoordinates[2] = bottomLeft; // Bottom right
                    textureCoordinates[3] = textureCoordinates[1];
                    textureCoordinates[4] = bottomRight; // Top right
                    textureCoordinates[5] = textureCoordinates[2];
                    break;
                case "Component":
                    textureCoordinates[0] = bottomLeft; // Bottom left
                    textureCoordinates[1] = topLeft; // Top left
                    textureCoordinates[2] = bottomRight; // Bottom right
                    textureCoordinates[3] = textureCoordinates[1];
                    textureCoordinates[4] = topRight; // Top right
                    textureCoordinates[5] = textureCoordinates[2];
                    break;
            }
            return textureCoordinates;
        }
    }
}