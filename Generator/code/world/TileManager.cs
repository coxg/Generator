using System.Collections.Generic;
using System.Linq;
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
            if (x < 0 || y < 0)
            {
                return null;
            }
            var id = IdMap[x, y];
            return TileSheet.TileFromId(id);
        }

        public void Set(int x, int y, int tileId, bool setBuffers=true)
        // setBuffers is expensive - if an ability needs to call Set a bunch of times then only the last call should
        // do the setBuffers call
        {
            IdMap[x, y] = tileId;
            UpdateVertices(x, y, updateSurroundingTiles: true);
            if (setBuffers)
            {
                SetBuffers();
            }
        }

        [JsonIgnore]
        public VertexPositionTexture[] Vertices;
        
        [JsonIgnore]
        public int[] Indices;

        private void UpdateVertices(int x, int y, string orientation = "Bottom", int? startIndex = null,
            int? tileId = null, bool updateSurroundingTiles = false)
        {
            var textureCoordinates = TileSheet.TextureVerticesFromId(tileId ?? IdMap[x, y], orientation);
            var vi = (startIndex ?? 4 * (x * Width + y));  // vertex index
            var ii = (startIndex ?? 6 * (x * Width + y));  // index index
            
            // Just do this first because it's less confusing
            Indices[ii++] = vi;
            Indices[ii++] = vi + 1;
            Indices[ii++] = vi + 2;
            Indices[ii++] = vi + 1;
            Indices[ii++] = vi + 3;
            Indices[ii] = vi + 2;
                    
            // Bottom left
            Vertices[vi].Position = new Vector3(x, y, 0);
            Vertices[vi++].TextureCoordinate = textureCoordinates[0];

            // Top left
            Vertices[vi].Position = new Vector3(x, y + 1, 0);
            Vertices[vi++].TextureCoordinate = textureCoordinates[1];

            // Bottom right
            Vertices[vi].Position = new Vector3(x + 1, y, 0);
            Vertices[vi++].TextureCoordinate = textureCoordinates[2];

            // Top right
            Vertices[vi].Position = new Vector3(x + 1, y + 1, 0);
            Vertices[vi].TextureCoordinate = textureCoordinates[3];

            // If we also need to update the layers around it then do that
            if (updateSurroundingTiles)
            {
                UpdateVertices(x, y - 1);
                UpdateVertices(x, y + 1);
                UpdateVertices(x - 1, y);
                UpdateVertices(x + 1, y);
                UpdateVertices(x - 1, y - 1);
                UpdateVertices(x - 1, y + 1);
                UpdateVertices(x + 1, y - 1);
                UpdateVertices(x + 1, y + 1);
            }
        }
        
        // TODO
        /*public void DrawTile(int x, int y)
        {
            var tile = Get(x, y);

            // Figure which tiles surround the current tile
            var surroundingTileMap = new Dictionary<string, Tile>
            {
                { "Top", Get(x, y + 1) },
                { "Bottom", Get(x, y - 1) },
                { "Right", Get(x + 1, y) },
                { "Left", Get(x - 1, y) },
                { "Top Left", Get(x - 1, y + 1) },
                { "Top Right", Get(x + 1, y + 1) },
                { "Bottom Right", Get(x + 1, y - 1) },
                { "Bottom Left", Get(x - 1, y - 1) },
            };

            // Figure out which unique tiles surround the current tile
            var uniqueSurroundingTileMap = new Dictionary<string, HashSet<string>>();
            foreach (var surroundingTile in surroundingTileMap)
            {
                if (BaseTileIndices[surroundingTile.Value.BaseTileName] > BaseTileIndices[tile.BaseTileName])
                {
                    if (!uniqueSurroundingTileMap.ContainsKey(surroundingTile.Value.BaseTileName))
                    {
                        uniqueSurroundingTileMap.Add(surroundingTile.Value.BaseTileName, new HashSet<string>());
                    }
                    uniqueSurroundingTileMap[surroundingTile.Value.BaseTileName].Add(surroundingTile.Key);
                }
            }

            // Loop through each unique tile from smallest to largest index, applying all layers for each
            foreach (var uniqueSurroundingTile in uniqueSurroundingTileMap.OrderBy(uniqueSurroundingTile => uniqueSurroundingTile.Key))
            {
                var tileName = uniqueSurroundingTile.Key;

                // If we are being drawn over the tiles on all sides
                if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Bottom")
                    && uniqueSurroundingTile.Value.Contains("Left") && uniqueSurroundingTile.Value.Contains("Right"))
                    DrawTileLayer(tileName + " O", bottomLeft);

                else
                {
                    // If we are being drawn over by the bottom three sides
                    if (uniqueSurroundingTile.Value.Contains("Bottom") && uniqueSurroundingTile.Value.Contains("Left")
                        && uniqueSurroundingTile.Value.Contains("Right"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Bottom");

                    // If we are being drawn over by the left three sides
                    else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Bottom")
                        && uniqueSurroundingTile.Value.Contains("Left"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Left");

                    // If we are being drawn over by the top three sides
                    else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Left")
                        && uniqueSurroundingTile.Value.Contains("Right"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Top");

                    // If we are being drawn over by the bottom right sides
                    else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Bottom")
                        && uniqueSurroundingTile.Value.Contains("Right"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Right");

                    else
                    {
                        // If we are being drawn over the bottom and the left
                        if (uniqueSurroundingTile.Value.Contains("Bottom") && uniqueSurroundingTile.Value.Contains("Left"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Bottom");

                            // If we are being drawn over the tile on the top right
                            if (uniqueSurroundingTile.Value.Contains("Top Right") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Top");
                        }

                        // If we are being drawn over the top and the left
                        else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Left"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Left");

                            // If we are being drawn over the tile on the bottom right
                            if (uniqueSurroundingTile.Value.Contains("Bottom Right") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Right");
                        }

                        // If we are being drawn over the top and the right
                        else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Right"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Top");

                            // If we are being drawn over the tile on the bottom left
                            if (uniqueSurroundingTile.Value.Contains("Bottom Left") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Bottom");
                        }

                        // If we are being drawn over the bottom and the right
                        else if (uniqueSurroundingTile.Value.Contains("Bottom") && uniqueSurroundingTile.Value.Contains("Right"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Right");

                            // If we are being drawn over the tile on the top left
                            if (uniqueSurroundingTile.Value.Contains("Top Left") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Left");
                        }

                        else
                        {
                            // If we are being drawn over the tile on the right
                            if (uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Right");

                            // If we are being drawn over the tile on the left
                            if (uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Left");

                            // If we are being drawn over the tile on the bottom
                            if (uniqueSurroundingTile.Value.Contains("Bottom"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Bottom");

                            // If we are being drawn over the tile on the top
                            if (uniqueSurroundingTile.Value.Contains("Top"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Top");

                            // If we are being drawn over the tile on the top right
                            if (uniqueSurroundingTile.Value.Contains("Top Right") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Top");

                            // If we are being drawn over the tile on the top left
                            if (uniqueSurroundingTile.Value.Contains("Top Left") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Left");

                            // If we are being drawn over the tile on the bottom right
                            if (uniqueSurroundingTile.Value.Contains("Bottom Right") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Right");

                            // If we are being drawn over the tile on the bottom left
                            if (uniqueSurroundingTile.Value.Contains("Bottom Left") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Bottom");
                        }
                    }
                }
            }
        }*/

        public void PopulateAllVertices()
        {
            Vertices = new VertexPositionTexture[4 * IdMap.Length];
            Indices = new int[6 * IdMap.Length];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    UpdateVertices(x, y);
                }
            }
            PopulateBuffers();
            SetBuffers();
        }

        private void PopulateBuffers()
        {
            GameControl.IndexBuffer = new IndexBuffer(
                GameControl.graphics.GraphicsDevice, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            GameControl.VertexBuffer = new VertexBuffer(GameControl.graphics.GraphicsDevice, 
                typeof(VertexPositionTexture), Vertices.Length, BufferUsage.WriteOnly);
            GameControl.graphics.GraphicsDevice.SetVertexBuffer(GameControl.VertexBuffer);
            GameControl.graphics.GraphicsDevice.Indices = GameControl.IndexBuffer;
        }
        
        private void SetBuffers()
        {
            GameControl.IndexBuffer.SetData(Indices);
            GameControl.VertexBuffer.SetData(Vertices);
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