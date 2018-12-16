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
            foreach (var fileName in Directory.GetFiles(
                Globals.Directory + "/Content/Tiles", "*.png", SearchOption.TopDirectoryOnly
                ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
            {
                AddNewObject(fileName, Globals.Content.Load<Texture2D>("Tiles/" + fileName));
            }

            // Populate the Acres
            PopulateAcres();
        }
    }
}