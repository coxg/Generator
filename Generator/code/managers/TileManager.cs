using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class TileManager: Manager<Tile>
    {
        public TileManager(List<string> baseTileNames, Vector2 size)
        {
            // Load in the tile sprites
            // TODO: Is there a better way to do this?
            foreach (var tileName in baseTileNames)
            {
                // Add the base tiles - these will be placed in creative mode and by certain abilities
                AddAllTilesInDirectory(tileName, "Base");

                // Add the sides of the tiles - these will be drawn around the base tiles automatically
                AddAllTilesInDirectory(tileName, "Sides");

                // Add all other tiles as miscellaneous
                AddAllTilesInDirectory(tileName, "Misc");
            }

            // Create mapping from ID to index
            BaseTileNames = baseTileNames;
            for (int i = 0; i < BaseTileNames.Count; i++)
            {
                BaseTileIndices[BaseTileNames[i]] = i;
            }

            // Populate the mapping with random tiles from the base set
            IDs = new string[(int)size.X, (int)size.Y];
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    IDs[x, y] = GetRandomBaseName(BaseTileNames[0]);
                }
            }
        }

        [Newtonsoft.Json.JsonConstructor]
        public TileManager(Dictionary<string, Tile> objects, string[,] ids, List<string> baseTileNames, Dictionary<string, int> baseTileIndices)
        {
            Objects = objects;
            IDs = ids;
            BaseTileNames = baseTileNames;
            BaseTileIndices = baseTileIndices;
        }

        // Maps each base tile to its other variants and sides, allowing better organization in creative mode
        public Dictionary<string, Dictionary<string, List<string>>> TileInfo = new Dictionary<string, Dictionary<string, List<string>>>();

        // Adds all tiles from a directory to all necessary manager attributes
        public void AddAllTilesInDirectory(string tileName, string directoryName)
        {
            var fullDirectoryName = Globals.Directory + "/Content/Tiles/" + tileName + "/" + directoryName;

            if (Directory.Exists(fullDirectoryName))
            {
                foreach (var individualTileFile in Directory.GetFiles(fullDirectoryName, "*.png", SearchOption.TopDirectoryOnly
                    ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
                {
                    // Add it to the tile info dictionary, allowing us to access different sides
                    if (!TileInfo.ContainsKey(tileName))
                    {
                        TileInfo.Add(tileName, new Dictionary<string, List<string>>());
                    }
                    if (!TileInfo[tileName].ContainsKey(directoryName))
                    {
                        TileInfo[tileName].Add(directoryName, new List<string>());
                    }
                    var tileId = tileName + " " + individualTileFile;
                    TileInfo[tileName][directoryName].Add(tileId);

                    // Add it to the mapping dictionaries, allowing us to reference it by name/id
                    Objects[tileId] = new Tile(
                        tileId,
                        new Cached<Texture2D>("Tiles/" + tileName + "/" + directoryName + "/" + individualTileFile),
                        tileName);
                }
            }
        }

        public List<string> BaseTileNames;
        public Dictionary<string, int> BaseTileIndices = new Dictionary<string, int>();

        public string[,] IDs;
        public Tile Get(int x, int y)
        {
            string id = null;
            if (x >= 0 && y >= 0 && x <= IDs.GetUpperBound(0) && y <= IDs.GetUpperBound(1))
            {
                id = IDs[x, y];
            }
            return Objects[id ?? "Grass grass_01_tile_256_09"];
        }

        // Gets a random base tile name for a particular tile type
        public string GetRandomBaseName(string baseObject)
        {
            var baseObjects = TileInfo[baseObject]["Base"];
            return baseObjects[MathTools.RandInt(baseObjects.Count)];
        }
    }
}