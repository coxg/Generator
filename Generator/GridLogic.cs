using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public static class GridLogic
    {
        private static GameObject[,] _grid = new GameObject[100, 100];
        public static Dictionary<string, Texture2D> TileNameToTexture { get; set; }
        public static Dictionary<int, string> TileIndexToTexture { get; set; }

        public static class Grid
        {
            // "Getter"
            public static GameObject GetObject(int XVal, int YVal)
            {
                return _grid[
                    (int)MathTools.Mod(XVal, _grid.GetLength(0)),
                    (int)MathTools.Mod(YVal, _grid.GetLength(1))];
            }

            // "Setter"
            public static void SetObject(int XVal, int YVal, GameObject gameObject)
            {
                _grid[
                    (int)MathTools.Mod(XVal, _grid.GetLength(0)),
                    (int)MathTools.Mod(YVal, _grid.GetLength(1))] = gameObject;
            }

            // GetLength
            public static int GetLength(int Dimension)
            {
                return _grid.GetLength(Dimension);
            }
        }
    }
}