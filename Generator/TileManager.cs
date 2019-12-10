using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class TileManager: Manager<Tile>
    {
        public TileManager(List<string> objects, string[,] ids)
        {
            // Load in the tile sprites
            // TODO: Is there a better way to do this?
            foreach (var tileName in objects)
            {

                // Add the base tiles - these will be placed in creative mode and by certain abilities
                var baseObjectIndex = Objects.Count;
                BaseTileIndexes.Add(baseObjectIndex);  // Note that the first tile is the default
                BaseTileIndexFromName.Add(tileName, baseObjectIndex);
                AddAllTilesInDirectory(tileName, "Base", baseObjectIndex);

                // Add the sides of the tiles - these will be drawn around the base tiles automatically
                AddAllTilesInDirectory(tileName, "Sides", baseObjectIndex);

                // Add all other tiles as miscellaneous
                AddAllTilesInDirectory(tileName, "Misc", baseObjectIndex);
            }
            IDs = ids;
        }

        [Newtonsoft.Json.JsonConstructor]
        public TileManager(Dictionary<string, Tile> objects, string[,] ids)
        {
            Objects = objects;
            IDs = ids;
        }

        // Mappings to help move between the base tile (ex: "Grass") and the unique sprites for it
        public Dictionary<string, int> BaseTileIndexFromName = new Dictionary<string, int>();
        public List<int> BaseTileIndexes = new List<int>();

        // Maps each base tile to its other variants and sides, allowing better organization in creative mode
        public Dictionary<int, Dictionary<string, List<int>>> TileInfo = new Dictionary<int, Dictionary<string, List<int>>>();

        // Adds all tiles from a directory to all necessary manager attributes
        public void AddAllTilesInDirectory(string tileName, string directoryName, int baseTileIndex)
        {
            var fullDirectoryName = Globals.Directory + "/Content/Tiles/" + tileName + "/" + directoryName;

            if (Directory.Exists(fullDirectoryName))
            {
                foreach (var individualTileFile in Directory.GetFiles(fullDirectoryName, "*.png", SearchOption.TopDirectoryOnly
                    ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
                {
                    // Add it to the tile info dictionary, allowing us to access different sides
                    if (!TileInfo.ContainsKey(baseTileIndex))
                    {
                        TileInfo.Add(baseTileIndex, new Dictionary<string, List<int>>());
                    }
                    if (!TileInfo[baseTileIndex].ContainsKey(directoryName))
                    {
                        TileInfo[baseTileIndex].Add(directoryName, new List<int>());
                    }
                    TileInfo[baseTileIndex][directoryName].Add(Objects.Count);

                    // Add it to the mapping dictionaries, allowing us to reference it by name/id
                    var tileId = tileName + " " + individualTileFile;
                    Globals.Log(tileId);
                    Objects[tileId] = new Tile(
                        tileId,
                        new Cached<Texture2D>("Tiles/" + tileName + "/" + directoryName + "/" + individualTileFile),
                        baseTileIndex,
                        tileName);
                }
            }
        }

        public string[,] IDs;
        public Tile Get(int x, int y)
        {
            string id = null;
            if (x >= 0 && y >= 0 && x <= IDs.GetUpperBound(0) && x <= IDs.GetUpperBound(1))
            {
                id = IDs[x, y];
            }
            return Objects[id ?? "Grass grass_01_tile_256_09"];
        }

        // Gets a random base tile for a particular tile type
        public int GetRandomBaseIndex(string baseObject)
        {
            var baseObjects = TileInfo[BaseTileIndexFromName[baseObject]]["Base"];
            return baseObjects[MathTools.RandInt(baseObjects.Count)];
        }
    }
}