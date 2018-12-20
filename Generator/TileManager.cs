using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class TileManager: Manager<Texture2D>
    {
        public void AddTileDirectory(string tileName, string directoryName)
        {
            foreach (var baseTile in Directory.GetFiles(
                Globals.Directory + "/Content/Tiles/" + tileName + "/" + directoryName, "*.png", 
                SearchOption.TopDirectoryOnly
                ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
            {
                AddNewObject(baseTile, Globals.Content.Load<Texture2D>(
                    "Tiles/" + tileName + "/" + directoryName + "/" + baseTile));
            }
        }

        public TileManager()
        {
            // Set the name
            Name = "Tiles";

            // Load in the tile sprites
            foreach (var tileName in Directory.GetDirectories(
                Globals.Directory + "/Content/Tiles", "*", SearchOption.TopDirectoryOnly
                ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
            {

                // Add the base tiles - these will be placed in creative mode and by certain abilities
                BaseObjectIndexes.Add(Count);  // Note that the first tile is the default
                AddTileDirectory(tileName, "Base");

                // Add the sides of the tiles - these will be drawn around the base tiles automatically
                AddTileDirectory(tileName, "Sides");

                // Add all other tiles as miscellaneous
                AddTileDirectory(tileName, "Misc");
            }

            // Populate the Acres
            PopulateAcres();
        }
    }
}