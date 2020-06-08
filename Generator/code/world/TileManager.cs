using System;
using System.Threading.Tasks;
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
        private int?[,] IdMap;
        public TileSheet TileSheet;
        private int? BaseTileId;
        private bool isPopulatingVertices;

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
        {
            IdMap[x, y] = tileId;
            for (var _x = x - 1; _x <= x + 1; _x++)
            {
                for (var _y = y - 1; _y <= y + 1; _y++)
                {
                    AppendVertices(_x, _y, DynamicVertices);
                    AppendAllEdgeVertices(_x, _y, DynamicVertices);
                }
            }
        }

        [JsonIgnore]
        public VertexPositionTexture[] Vertices;
        
        [JsonIgnore]
        public List<VertexPositionTexture> DynamicVertices;
        
        [JsonIgnore]
        public int[] Indices;

        public void PopulateAllVertices(int dynamicVericesToKeep=0)
        {
            isPopulatingVertices = true;
            var vertices = new List<VertexPositionTexture>();
            var indices = new List<int>();
            
            for (var x = 0; x < Globals.Zone.Width; x++)
            {
                for (var y = 0; y < Globals.Zone.Height; y++)
                {
                    AppendVertices(x, y, vertices, indices);
                    AppendAllEdgeVertices(x, y, vertices, indices);
                }
            }

            var vertexArray = vertices.ToArray();
            var indexArray = indices.ToArray();
            
            PopulateBuffers(indexArray, vertexArray);
            Vertices = vertexArray;
            Indices = indexArray;
            DynamicVertices = DynamicVertices.GetRange(
                dynamicVericesToKeep, DynamicVertices.Count - dynamicVericesToKeep);
            isPopulatingVertices = false;
        }

        private void AppendVertices(int x, int y, List<VertexPositionTexture> vertices, List<int> indices=null, 
            string orientation = "Bottom", int? tileId = null)
        {
            tileId = tileId ?? IdMap[x, y];
            if (tileId == null)
            {
                return;
            }
            var textureCoordinates = TileSheet.TextureVerticesFromId((int)tileId, orientation);

            if (indices != null)
            {
                // Just do this first because it's less confusing
                indices.Add(vertices.Count);
                indices.Add(vertices.Count + 1);
                indices.Add(vertices.Count + 2);
                indices.Add(vertices.Count + 1);
                indices.Add(vertices.Count + 3);
                indices.Add(vertices.Count + 2);
                
                vertices.Add(new VertexPositionTexture(new Vector3(x, y, 0), textureCoordinates[0]));
                vertices.Add(new VertexPositionTexture(new Vector3(x, y + 1, 0), textureCoordinates[1]));
                vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y, 0), textureCoordinates[2]));
                vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y + 1, 0), textureCoordinates[3]));
            }
            else
            {
                vertices.Add(new VertexPositionTexture(new Vector3(x, y, 0), textureCoordinates[0]));
                vertices.Add(new VertexPositionTexture(new Vector3(x, y + 1, 0), textureCoordinates[1]));
                vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y, 0), textureCoordinates[2]));
                vertices.Add(new VertexPositionTexture(new Vector3(x, y + 1, 0), textureCoordinates[1]));
                vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y + 1, 0), textureCoordinates[3]));
                vertices.Add(new VertexPositionTexture(new Vector3(x + 1, y, 0), textureCoordinates[2]));
            }
        }
        
        private void AppendAllEdgeVertices(int x, int y, List<VertexPositionTexture> vertices, List<int> indices=null)
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
            foreach (var surroundingTile in uniqueSurroundingTileMap
                .Where(uniqueSurroundingTile => uniqueSurroundingTile.Key.FirstEdgeId != null)
                .OrderBy(uniqueSurroundingTile => uniqueSurroundingTile.Key.Layer))
            {
                // If we are being drawn over the tiles on all sides
                if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Bottom")
                    && surroundingTile.Value.Contains("Left") && surroundingTile.Value.Contains("Right"))
                    AppendVertices(x, y, vertices, indices, "Bottom", surroundingTile.Key.GetOEdgeId());

                else
                {
                    // If we are being drawn over by the bottom three sides
                    if (surroundingTile.Value.Contains("Bottom") && surroundingTile.Value.Contains("Left")
                        && surroundingTile.Value.Contains("Right"))
                        AppendVertices(x, y, vertices, indices, "Bottom", surroundingTile.Key.GetUEdgeId());

                    // If we are being drawn over by the left three sides
                    else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Bottom")
                        && surroundingTile.Value.Contains("Left"))
                        AppendVertices(x, y, vertices, indices, "Left", surroundingTile.Key.GetUEdgeId());

                    // If we are being drawn over by the top three sides
                    else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Left")
                        && surroundingTile.Value.Contains("Right"))
                        AppendVertices(x, y, vertices, indices, "Top", surroundingTile.Key.GetUEdgeId());

                    // If we are being drawn over by the bottom right sides
                    else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Bottom")
                        && surroundingTile.Value.Contains("Right"))
                        AppendVertices(x, y, vertices, indices, "Right", surroundingTile.Key.GetUEdgeId());

                    else
                    {
                        // If we are being drawn over the bottom and the left
                        if (surroundingTile.Value.Contains("Bottom") && surroundingTile.Value.Contains("Left"))
                        {
                            AppendVertices(x, y, vertices, indices, "Bottom", surroundingTile.Key.GetLEdgeId());

                            // If we are being drawn over the tile on the top right
                            if (surroundingTile.Value.Contains("Top Right") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendVertices(x, y, vertices, indices, "Top", surroundingTile.Key.GetCornerEdgeId());
                        }

                        // If we are being drawn over the top and the left
                        else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Left"))
                        {
                            AppendVertices(x, y, vertices, indices, "Left", surroundingTile.Key.GetLEdgeId());

                            // If we are being drawn over the tile on the bottom right
                            if (surroundingTile.Value.Contains("Bottom Right") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendVertices(x, y, vertices, indices, "Right", surroundingTile.Key.GetCornerEdgeId());
                        }

                        // If we are being drawn over the top and the right
                        else if (surroundingTile.Value.Contains("Top") && surroundingTile.Value.Contains("Right"))
                        {
                            AppendVertices(x, y, vertices, indices, "Top", surroundingTile.Key.GetLEdgeId());

                            // If we are being drawn over the tile on the bottom left
                            if (surroundingTile.Value.Contains("Bottom Left") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendVertices(x, y, vertices, indices, "Bottom", surroundingTile.Key.GetCornerEdgeId());
                        }

                        // If we are being drawn over the bottom and the right
                        else if (surroundingTile.Value.Contains("Bottom") && surroundingTile.Value.Contains("Right"))
                        {
                            AppendVertices(x, y, vertices, indices, "Right", surroundingTile.Key.GetLEdgeId());

                            // If we are being drawn over the tile on the top left
                            if (surroundingTile.Value.Contains("Top Left") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendVertices(x, y, vertices, indices, "Left", surroundingTile.Key.GetCornerEdgeId());
                        }

                        else
                        {
                            // If we are being drawn over the tile on the right
                            if (surroundingTile.Value.Contains("Right"))
                                AppendVertices(x, y, vertices, indices, "Right", surroundingTile.Key.GetSideEdgeId());

                            // If we are being drawn over the tile on the left
                            if (surroundingTile.Value.Contains("Left"))
                                AppendVertices(x, y, vertices, indices, "Left", surroundingTile.Key.GetSideEdgeId());

                            // If we are being drawn over the tile on the bottom
                            if (surroundingTile.Value.Contains("Bottom"))
                                AppendVertices(x, y, vertices, indices, "Bottom", surroundingTile.Key.GetSideEdgeId());

                            // If we are being drawn over the tile on the top
                            if (surroundingTile.Value.Contains("Top"))
                                AppendVertices(x, y, vertices, indices, "Top", surroundingTile.Key.GetSideEdgeId());

                            // If we are being drawn over the tile on the top right
                            if (surroundingTile.Value.Contains("Top Right") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendVertices(x, y, vertices, indices, "Top", surroundingTile.Key.GetCornerEdgeId());

                            // If we are being drawn over the tile on the top left
                            if (surroundingTile.Value.Contains("Top Left") && !surroundingTile.Value.Contains("Top")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendVertices(x, y, vertices, indices, "Left", surroundingTile.Key.GetCornerEdgeId());

                            // If we are being drawn over the tile on the bottom right
                            if (surroundingTile.Value.Contains("Bottom Right") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Right"))
                                AppendVertices(x, y, vertices, indices, "Right", surroundingTile.Key.GetCornerEdgeId());

                            // If we are being drawn over the tile on the bottom left
                            if (surroundingTile.Value.Contains("Bottom Left") && !surroundingTile.Value.Contains("Bottom")
                                && !surroundingTile.Value.Contains("Left"))
                                AppendVertices(x, y, vertices, indices, "Bottom", surroundingTile.Key.GetCornerEdgeId());
                        }
                    }
                }
            }
        }

        private static void PopulateBuffers(int[] indices, VertexPositionTexture[] vertices)
        {
            Globals.Log("Populating buffers");
            var indexBuffer = new IndexBuffer(
                GameControl.graphics.GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            var vertexBuffer = new VertexBuffer(GameControl.graphics.GraphicsDevice, 
                typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            vertexBuffer.SetData(vertices);
            GameControl.graphics.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GameControl.graphics.GraphicsDevice.Indices = indexBuffer;
            Globals.Log("Done populating buffers");
        }
        
        public TileManager(TileSheet tileSheet, int? baseTileId=0)
        {
            TileSheet = tileSheet;
            BaseTileId = baseTileId;
            DynamicVertices = new List<VertexPositionTexture>();
            
            // Populate with random instances of the base tile
            IdMap = new int?[Globals.Zone.Width, Globals.Zone.Height];
            if (BaseTileId != null)
            {
                for (var x = 0; x < Globals.Zone.Width; x++)
                {
                    for (var y = 0; y < Globals.Zone.Height; y++)
                    {
                        IdMap[x, y] = TileSheet.Tiles[(int)BaseTileId].GetRandomBaseId();
                    }
                }
            }

            PopulateAllVertices();
        }

        [JsonConstructor]
        public TileManager(int?[,] idMap, TileSheet tileSheet, int baseTileId)
        {
            TileSheet = tileSheet;
            BaseTileId = baseTileId;
            IdMap = idMap;
            DynamicVertices = new List<VertexPositionTexture>();
            
            PopulateAllVertices();
        }
        
        public void Update() {
            if (!isPopulatingVertices && DynamicVertices.Count > 5000)
            {
                isPopulatingVertices = true;
                Task.Run( () => PopulateAllVertices(DynamicVertices.Count) );
            }
        }
    }
}