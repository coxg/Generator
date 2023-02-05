using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Generator.code.world
{
    public class ObjectMap
    {
        public ObjectMap(int width, int height, IEnumerable<GameObject> objects)
        {
            Values = new GameObject[width, height];
            foreach (GameObject gameObject in objects)
            {
                Add(gameObject);
            }
        }

        public GameObject[,] Values;

        public bool InBounds(float x, float y)
        {
            return x >= 0 && x < Values.GetLength(0) && y >= 0 && y < Values.GetLength(1);
        }

        public void Add(GameObject gameObject)
        {
            for (int x = (int)gameObject.Area.Left; x < (int)gameObject.Area.Right; x++)
            {
                for (int y = (int)gameObject.Area.Top; y < (int)gameObject.Area.Bottom; y++)
                {
                    if (InBounds(x, y))
                    {
                        if (Values[x, y] == null)
                        {
                            Values[x, y] = gameObject;
                        }
                        else
                        {
                            throw new Exception("(" + x + "," + y + ") is already populated!");
                        }
                    }
                }
            }
        }

        public void Remove(GameObject gameObject)
        {
            for (int x = (int)gameObject.Area.Left; x < (int)gameObject.Area.Right; x++)
            {
                for (int y = (int)gameObject.Area.Top; y < (int)gameObject.Area.Bottom; y++)
                {
                    if (InBounds(x, y))
                    {
                        Debug.Assert(Values[x, y] == gameObject);
                        Values[x, y] = null;
                    }
                }
            }
        }

        public GameObject Get(int x, int y)
        {
            if (InBounds(x, y))
            {
                return Values[x, y];
            }
            return null;
        }
        
        public HashSet<GameObject> Get(RectangleF area)
        {
            var results = new HashSet<GameObject>();
            for (int x = (int)area.Left; x < (int)area.Right; x++)
            {
                for (int y = (int)area.Top; y < (int)area.Bottom; y++)
                {
                    var foundObject = Get(x, y);
                    if (foundObject != null)
                    {
                        results.Add(Get(x, y));
                    }
                }
            }
            return results;
        }
    }
}
