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
        
        [JsonIgnore]
        public VertexPositionColorTexture[] Vertices;

        public void PopulateVertices()
        {
            // TODO: Give each tile its own layer?
            Vertices = new VertexPositionColorTexture[6 * IdMap.Length];
            var vertexIndex = 0;
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var textureCoordinates = TileSheet.TextureCoordinatesFromId(IdMap[x, y]);
                    
                    // Bottom left
                    Vertices[vertexIndex].Position = new Vector3(x, y, 0);
                    Vertices[vertexIndex].Color = Color.White;
                    Vertices[vertexIndex++].TextureCoordinate = textureCoordinates[0];

                    // Top left
                    Vertices[vertexIndex].Position = new Vector3(x, y + 1, 0);
                    Vertices[vertexIndex].Color = Color.White;
                    Vertices[vertexIndex++].TextureCoordinate = textureCoordinates[1];

                    // Bottom right
                    Vertices[vertexIndex].Position = new Vector3(x + 1, y, 0);
                    Vertices[vertexIndex].Color = Color.White;
                    Vertices[vertexIndex++].TextureCoordinate = textureCoordinates[2];
                    Vertices[vertexIndex].Color = Color.White;
                    Vertices[vertexIndex].Position = new Vector3(x, y + 1, 0);
                    Vertices[vertexIndex++].TextureCoordinate = textureCoordinates[3];

                    // Top right
                    Vertices[vertexIndex].Position = new Vector3(x + 1, y + 1, 0);
                    Vertices[vertexIndex].Color = Color.White;
                    Vertices[vertexIndex++].TextureCoordinate = textureCoordinates[4];
                    Vertices[vertexIndex].Position = new Vector3(x + 1, y, 0);
                    Vertices[vertexIndex].Color = Color.White;
                    Vertices[vertexIndex++].TextureCoordinate = textureCoordinates[5];
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

            PopulateVertices();
        }

        [JsonConstructor]
        public TileManager(int width, int height, int[,] idMap, TileSheet tileSheet)
        {
            TileSheet = tileSheet;
            Width = width;
            Height = height;
            IdMap = idMap;
            
            PopulateVertices();
        }
    }
}