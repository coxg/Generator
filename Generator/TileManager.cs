using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class TileManager: Manager<Texture2D>
    {
        public TileManager()
        {
            // Set the name
            Name = "Tiles";

            // Load in the objects - default comes first
            foreach (var tileName in Directory.GetDirectories(
                Globals.Directory + "/Content/Tiles", "*", SearchOption.TopDirectoryOnly
                ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
            {
                foreach (var baseTile in Directory.GetFiles(
                    Globals.Directory + "/Content/Tiles/" + tileName + "/Base", "*.png", SearchOption.TopDirectoryOnly
                    ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
                {

                    AddNewObject(baseTile, Globals.Content.Load<Texture2D>("Tiles/" + tileName + "/Base/" + baseTile));
                }
            }

            // Populate the Acres
            PopulateAcres();
        }
    }
}