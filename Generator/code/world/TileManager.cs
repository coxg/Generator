using System;
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
        public int[,] IdMap;
        public TileSheet TileSheet;
        public int BaseTileId;

        public Tile Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Globals.Zone.Width || y >= Globals.Zone.Height)
            {
                return null;
            }
            var id = IdMap[x, y];
            return TileSheet.TileFromId(id);
        }

        public void Set(int x, int y, int tileId)
        // setBuffers is expensive - if an ability needs to call Set a bunch of times then only the last call should
        // do the setBuffers call
        {
            IdMap[x, y] = tileId;
        }

        [JsonIgnore]
        public VertexPositionTexture[] Vertices;
        
        [JsonIgnore]
        public int[] Indices;

        public void PopulateAllVertices()
        {
            var numBaseVertices = 4 * IdMap.Length;
            var numBaseIndices = 6 * IdMap.Length;
            Vertices = new VertexPositionTexture[numBaseVertices];
            Indices = new int[numBaseIndices];
            var edgeVertices = new List<VertexPositionTexture>();
            var edgeIndices = new List<int>();
            
            for (var x = 0; x < Globals.Zone.Width; x++)
            {
                for (var y = 0; y < Globals.Zone.Height; y++)
                {
                    UpdateVertices(x, y);
                    AppendAllEdgeVertices(x, y, edgeIndices, edgeVertices);
                }
            }
            
            Array.Resize(ref Vertices, numBaseVertices + edgeVertices.Count);
            edgeVertices.CopyTo(Vertices, numBaseVertices);
            Array.Resize(ref Indices, numBaseIndices + edgeIndices.Count);
            edgeIndices.CopyTo(Indices, numBaseIndices);
            
            PopulateBuffers();
            SetBuffers();
        }

        private void UpdateVertices(int x, int y, string orientation = "Bottom", int? tileId = null)
        {
            var textureCoordinates = TileSheet.TextureVerticesFromId(tileId ?? IdMap[x, y], orientation);
            var vi = 4 * (x * Globals.Zone.Width + y);  // vertex index
            var ii = 6 * (x * Globals.Zone.Width + y);  // index index
            
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
        }

        private void AppendEdgeVertices(int tileId, int x, int y, List<int> indices, 
            List<VertexPositionTexture> vertices, string orientation)
        {
            var textureCoordinates = TileSheet.TextureVerticesFromId(tileId, orientation);
            
            // Just do this first because it's less confusing
            indices.Add(Vertices.Length + vertices.Count);
            indices.Add(Vertices.Length + vertices.Count + 1);
            indices.Add(Vertices.Length + vertices.Count + 2);
            indices.Add(Vertices.Length + vertices.Count + 1);
            indices.Add(Vertices.Length + vertices.Count + 3);
            indices.Add(Vertices.Length + vertices.Count + 2);
                    
            // Bottom left
            vertices.Add(new VertexPositionTexture(new Vector3(x, y, 0), textureCoordinates[0]));
            vertices.Add(new VertexPositionTexture(new Vector3(x, y + 1, 0), textureCoordinates[1]));
            vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y, 0), textureCoordinates[2]));
            vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y + 1, 0), textureCoordinates[3]));
        }
        
        private void AppendAllEdgeVertices(int x, int y, List<int> indices, List<VertexPositionTexture> vertices)
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
            var uniqueSurroundingTileMap = new Dictionary<Tile, HashSet<string>>();
            foreach (var surroundingTile in surroundingTileMap)
            {
                if (surroundingTile.Value != null)
                {
                    if (surroundingTile.Value.Layer > tile.Layer)
                    {
                        if (!uniqueSurroundingTileMap.ContainsKey(surroundingTile.Value))
                        {
                            uniqueSurroundingTileMap.Add(surroundingTile.Value, new HashSet<string>());
                        }
                        uniqueSurroundingTileMap[surroundingTile.Value].Add(surroundingTile.Key);
                    }
                }
            }

            // Loop through each unique tile from smallest to largest index, applying all layers for each
            foreach (var surroundingTile in uniqueSurroundingTileMap.OrderBy(uniqueSurroundingTile => uniqueSurroundingTile.Key))
            {
                // If we are being drawn over the tiles on all sides
                if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Bottom")
                    && surroundingTile.Value.Contains("Left") && surroundingTile.Value.Contains("Right"))
                    AppendEdgeVertices(surroundingTile.Key.GetOEdgeId(), x, y, indices, vertices, "Bottom");

                else
                {
                    // If we are being drawn over by the bottom three sides
                    if (surroundingTile.Value.Contains("Bottom") && surroundingTile.Value.Contains("Left")
                        && surroundingTile.Value.Contains("Right"))
                        AppendEdgeVertices(surroundingTile.Key.GetUEdgeId(), x, y, indices, vertices, "Bottom");

                    // If we are being drawn over by the left three sides
                    else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Bottom")
                        && surroundingTile.Value.Contains("Left"))
                        AppendEdgeVertices(surroundingTile.Key.GetUEdgeId(), x, y, indices, vertices, "Left");

                    // If we are being drawn over by the top three sides
                    else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Left")
                        && surroundingTile.Value.Contains("Right"))
                        AppendEdgeVertices(surroundingTile.Key.GetUEdgeId(), x, y, indices, vertices, "Top");

                    // If we are being drawn over by the bottom right sides
                    else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Bottom")
                        && surroundingTile.Value.Contains("Right"))
                        AppendEdgeVertices(surroundingTile.Key.GetUEdgeId(), x, y, indices, vertices, "Right");

                    else
                    {
                        // If we are being drawn over the bottom and the left
                        if (surroundingTile.Value.Contains("Bottom") && surroundingTile.Value.Contains("Left"))
                        {
                            AppendEdgeVertices(surroundingTile.Key.GetLEdgeId(), x, y, indices, vertices, "Bottom");

                            // If we are being drawn over the tile on the top right
                            if (surroundingTile.Value.Contains("Top Right") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Top");
                        }

                        // If we are being drawn over the top and the left
                        else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Left"))
                        {
                            AppendEdgeVertices(surroundingTile.Key.GetLEdgeId(), x, y, indices, vertices, "Left");

                            // If we are being drawn over the tile on the bottom right
                            if (surroundingTile.Value.Contains("Bottom Right") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Right");
                        }

                        // If we are being drawn over the top and the right
                        else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Right"))
                        {
                            AppendEdgeVertices(surroundingTile.Key.GetLEdgeId(), x, y, indices, vertices, "Top");

                            // If we are being drawn over the tile on the bottom left
                            if (surroundingTile.Value.Contains("Bottom Left") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Bottom");
                        }

                        // If we are being drawn over the bottom and the right
                        else if (surroundingTile.Value.Contains("Bottom") && surroundingTile.Value.Contains("Right"))
                        {
                            AppendEdgeVertices(surroundingTile.Key.GetLEdgeId(), x, y, indices, vertices, "Right");

                            // If we are being drawn over the tile on the top left
                            if (surroundingTile.Value.Contains("Top Left") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Left");
                        }

                        else
                        {
                            // If we are being drawn over the tile on the right
                            if (surroundingTile.Value.Contains("Right"))
                                AppendEdgeVertices(surroundingTile.Key.GetSideEdgeId(), x, y, indices, vertices, "Right");

                            // If we are being drawn over the tile on the left
                            if (surroundingTile.Value.Contains("Left"))
                                AppendEdgeVertices(surroundingTile.Key.GetSideEdgeId(), x, y, indices, vertices, "Left");

                            // If we are being drawn over the tile on the bottom
                            if (surroundingTile.Value.Contains("Bottom"))
                                AppendEdgeVertices(surroundingTile.Key.GetSideEdgeId(), x, y, indices, vertices, "Bottom");

                            // If we are being drawn over the tile on the top
                            if (surroundingTile.Value.Contains("Top"))
                                AppendEdgeVertices(surroundingTile.Key.GetSideEdgeId(), x, y, indices, vertices, "Top");

                            // If we are being drawn over the tile on the top right
                            if (surroundingTile.Value.Contains("Top Right") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Top");

                            // If we are being drawn over the tile on the top left
                            if (surroundingTile.Value.Contains("Top Left") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Left");

                            // If we are being drawn over the tile on the bottom right
                            if (surroundingTile.Value.Contains("Bottom Right") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Right");

                            // If we are being drawn over the tile on the bottom left
                            if (surroundingTile.Value.Contains("Bottom Left") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendEdgeVertices(surroundingTile.Key.GetCornerEdgeId(), x, y, indices, vertices, "Bottom");
                        }
                    }
                }
            }
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
        
        public TileManager(TileSheet tileSheet, int baseTileId=0)
        {
            TileSheet = tileSheet;
            BaseTileId = baseTileId;
            
            // Populate with random instances of the base tile
            IdMap = new int[Globals.Zone.Width, Globals.Zone.Height];
            for (var x = 0; x < Globals.Zone.Width; x++)
            {
                for (var y = 0; y < Globals.Zone.Height; y++)
                {
                    IdMap[x, y] = TileSheet.Tiles[BaseTileId].GetRandomBaseId();
                }
            }

            PopulateAllVertices();
        }

        [JsonConstructor]
        public TileManager(int[,] idMap, TileSheet tileSheet, int baseTileId)
        {
            TileSheet = tileSheet;
            BaseTileId = baseTileId;
            IdMap = idMap;
            
            PopulateAllVertices();
        }
    }
}