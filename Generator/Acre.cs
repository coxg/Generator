using Microsoft.Xna.Framework;
using System.IO;
using System.Text;

namespace Generator
{
    public class Acre
    {
        public static Vector2 AcreSize = new Vector2(50, 50);
        public string Name;
        public string FileName;
        public int[,] Values = new int[(int)AcreSize.X, (int)AcreSize.Y];
        public int MinX;
        public int MinY;

        public Acre(string level, int x, int y)
        {
            Name = x.ToString() + "_" + y.ToString();
            FileName = Globals.Directory + "/Maps/" + level + "/" + Name + ".csv";
            MinX = x * (int)AcreSize.X;
            MinY = y * (int)AcreSize.Y;

            // If the file exists then load it in
            if (File.Exists(FileName))
            {
                Read();
            }

            // If the file doesn't exist then populate with the default values
            else
            {
                var defaultValue = 0;
                for (int rowNumber = 0; rowNumber < (int)AcreSize.Y; rowNumber++)
                {
                    // Fill the values with the default value
                    for (int columnNumber = 0; columnNumber < (int)AcreSize.X; columnNumber++)
                    {
                        if (level == "Tiles" && Globals.Tiles != null) defaultValue = Globals.Tiles.GetRandomBaseIndex("Grass");
                        Values[rowNumber, columnNumber] = defaultValue;
                    }
                }
            }
        }

        // Reads in the Acre from a file
        public void Read() // TODO: Populate list of active objects on read
        {
            using (var sr = new StreamReader(FileName))
            {
                var rows = sr.ReadToEnd().Split('\n');
                for (int rowNumber = 0; rowNumber < (int)AcreSize.Y; rowNumber++)
                {
                    var row = rows[rowNumber].Split(',');
                    for (int columnNumber = 0; columnNumber < (int)AcreSize.X; columnNumber++)
                        Values[rowNumber, Values.GetLength(1) - columnNumber - 1] = int.Parse(row[columnNumber]);
                }
            }
        }

        // Writes the acre to a file
        public void Write()
        {
            var csv = new StringBuilder();
            for (var rowNumber = 0; rowNumber < Values.GetLength(0); rowNumber++)
            {
                var row = new StringBuilder();
                for (var colNumber = 0; colNumber < Values.GetLength(1); colNumber++)
                {
                    row.Append(Values[rowNumber, Values.GetLength(1) - colNumber - 1]);
                    row.Append(',');
                }
                csv.AppendLine(row.ToString());
            }
            File.WriteAllText(FileName, csv.ToString());
        }

        // "Getter"
        // TODO: Replace this with a real getter
        public int Get(int x, int y)
        {
            return Values[x - MinX, y - MinY];
        }

        // "Setter"
        // TODO: Replace this with a real setter
        public void Set(int x, int y, int value)
        {
            Values[x - MinX, y - MinY] = value;
        }

        // For debugging
        public override string ToString()
        {
            var returnString = new StringBuilder();

            for (var rowNumber = 0; rowNumber < Values.GetLength(0); rowNumber++)
            {
                var row = new StringBuilder();
                for (var colNumber = 0; colNumber < Values.GetLength(1); colNumber++)
                {
                    row.Append(Values[rowNumber, Values.GetLength(1) - colNumber - 1]);
                    row.Append('\t');
                }
                returnString.AppendLine(row.ToString());
            }
            return returnString.ToString();
        }
    }
}