using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class TileManager: Manager<Tile>
    {
        // Mappings to help move between the base tile ("Grass") and the unique sprites for it
        public static Dictionary<string, int> BaseTileIndexFromName = new Dictionary<string, int>();
        public static List<int> BaseTileIndexes = new List<int>();

        // Maps each base tile to its other variants and sides, allowing better organization in creative mode
        public static Dictionary<int, Dictionary<string, List<int>>> TileInfo = new Dictionary<int, Dictionary<string, List<int>>>();

        // Adds all tiles from a directory to all necessary manager attributes
        public static void AddAllTilesInDirectory(string tileName, string directoryName, int baseTileIndex)
        {
            foreach (var individualTileFile in Directory.GetFiles(
                Globals.Directory + "/Content/Tiles/" + tileName + "/" + directoryName, "*.png", 
                SearchOption.TopDirectoryOnly
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
                TileInfo[baseTileIndex][directoryName].Add(Count);

                // Add it to the mapping dictionaries, allowing us to reference it by name/id
                Globals.Log(tileName + " " + individualTileFile);
                AddNewObject(
                    tileName + " " + individualTileFile, 
                    new Tile(
                        tileName + " " + individualTileFile,
                        Globals.Content.Load<Texture2D>(
                            "Tiles/" + tileName + "/" + directoryName + "/" + individualTileFile),
                        baseTileIndex,
                        tileName
                    )
                );
            }
        }

        // Gets a random base tile for a particular tile type
        public static int GetRandomBaseIndex(string baseObject)
        {
            var baseObjects = TileInfo[BaseTileIndexFromName[baseObject]]["Base"];
            return baseObjects[MathTools.RandInt(baseObjects.Count)];
        }

        public static void Initialize()
        {
            // Set the name
            Name = "Tiles";

            // Load in the tile sprites
            // TODO: Is there a better way to do this?
            foreach (var tileName in new List<string>{ "Grass", "Clay" })
            {

                // Add the base tiles - these will be placed in creative mode and by certain abilities
                var baseObjectIndex = Count;
                BaseTileIndexes.Add(baseObjectIndex);  // Note that the first tile is the default
                BaseTileIndexFromName.Add(tileName, baseObjectIndex);
                AddAllTilesInDirectory(tileName, "Base", baseObjectIndex);

                // Add the sides of the tiles - these will be drawn around the base tiles automatically
                AddAllTilesInDirectory(tileName, "Sides", baseObjectIndex);

                // Add all other tiles as miscellaneous
                AddAllTilesInDirectory(tileName, "Misc", baseObjectIndex);
            }

            // Populate the Acres
            PopulateAcres();
        }
    }
}