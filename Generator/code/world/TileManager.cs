using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    public class TileManager
    // Middle man between Zone and TileSheet
    {
        public int Width;
        public int Height;
        public int[,] IdMap;
        public TileSheet TileSheet;

        public Tile Get(int x, int y)
        {
            var id = IdMap[x, y];
            return TileSheet.TileFromId(id);
        }

        public void Set(int x, int y, int tileId)
        {
            IdMap[x, y] = tileId;
            UpdateVertices(x, y);
        }
        
        [JsonIgnore]
        public VertexPositionColorTexture[] Vertices;

        public void UpdateVertices(int x, int y)
        {
            var textureCoordinates = TileSheet.TextureVerticesFromId(IdMap[x, y]);
            int i = 6 * (x * Width + y);
                    
            // Bottom left
            Vertices[i].Position = new Vector3(x, y, 0);
            Vertices[i].Color = Color.White;
            Vertices[i++].TextureCoordinate = textureCoordinates[0];

            // Top left
            Vertices[i].Position = new Vector3(x, y + 1, 0);
            Vertices[i].Color = Color.White;
            Vertices[i++].TextureCoordinate = textureCoordinates[1];

            // Bottom right
            Vertices[i].Position = new Vector3(x + 1, y, 0);
            Vertices[i].Color = Color.White;
            Vertices[i++].TextureCoordinate = textureCoordinates[2];
                    
            // Top left
            Vertices[i].Color = Color.White;
            Vertices[i].Position = new Vector3(x, y + 1, 0);
            Vertices[i++].TextureCoordinate = textureCoordinates[3];

            // Top right
            Vertices[i].Position = new Vector3(x + 1, y + 1, 0);
            Vertices[i].Color = Color.White;
            Vertices[i++].TextureCoordinate = textureCoordinates[4];
                    
            // Bottom right
            Vertices[i].Position = new Vector3(x + 1, y, 0);
            Vertices[i].Color = Color.White;
            Vertices[i].TextureCoordinate = textureCoordinates[5];
        }

        public void PopulateAllVertices()
        {
            // TODO: Give each tile its own layer?
            Vertices = new VertexPositionColorTexture[6 * IdMap.Length];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    UpdateVertices(x, y);
                }
            }
        }
        
        public TileManager(int width, int height, TileSheet tileSheet)
        {
            TileSheet = tileSheet;
            Width = width;
            Height = height;
            
            // Populate with random instances of the base tile
            IdMap = new int[Width, Height];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    IdMap[x, y] = TileSheet.GetRandomBaseTileId();
                }
            }

            PopulateAllVertices();
        }

        [JsonConstructor]
        public TileManager(int width, int height, int[,] idMap, TileSheet tileSheet)
        {
            TileSheet = tileSheet;
            Width = width;
            Height = height;
            IdMap = idMap;
            
            PopulateAllVertices();
        }
    }
}