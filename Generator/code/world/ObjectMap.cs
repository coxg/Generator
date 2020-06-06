using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Generator.code.world
{
    public class ObjectMap
    {
        public ObjectMap(int width, int height, IEnumerable<GameObject> objects)
        {
            Values = new HashSet<GameObject>[width, height];
            foreach (GameObject gameObject in objects)
            {
                Add(gameObject);
            }
        }

        public HashSet<GameObject>[,] Values;

        public bool InBounds(float x, float y)
        {
            return x >= 0 && x < Values.GetLength(0) && y >= 0 && y < Values.GetLength(1);
        }

        public void Add(GameObject gameObject)
        {
            for (int x = (int)gameObject.Area.Left; x <= (int)gameObject.Area.Right; x++)
            {
                for (int y = (int)gameObject.Area.Top; y <= (int)gameObject.Area.Bottom; y++)
                {
                    if (InBounds(x, y))
                    {
                        if (Values[x, y] == null)
                        {
                            Values[x, y] = new HashSet<GameObject> { gameObject };
                        }
                        else
                        {
                            Values[x, y].Add(gameObject);
                        }
                    }
                }
            }
        }

        public void Remove(GameObject gameObject)
        {
            for (int x = (int)gameObject.Area.Left; x <= (int)gameObject.Area.Right; x++)
            {
                for (int y = (int)gameObject.Area.Top; y <= (int)gameObject.Area.Bottom; y++)
                {
                    if (InBounds(x, y) && Values[x, y] != null && Values[x, y].Contains(gameObject))
                    {
                        Values[x, y].Remove(gameObject);
                    }
                }
            }
        }

        public IEnumerable<GameObject> Get(float x, float y)
        {
            if (InBounds(x, y))
            {
                var values = Values[(int)x, (int)y] ?? new HashSet<GameObject>();
                return values.Where(value => value.Area.Contains(x, y));
            }
            return new HashSet<GameObject>();
        }
        
        public HashSet<GameObject> Get(RectangleF area)
        {
            var results = new HashSet<GameObject>();
            for (int x = (int)area.Left; x <= (int)area.Right; x++)
            {
                for (int y = (int)area.Top; y <= (int)area.Bottom; y++)
                {
                    results.UnionWith(Get(x, y));
                }
            }
            return results;
        }
    }
}
