using System;
using System.Collections.Generic;

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

        public bool InBounds(int x, int y)
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

        public HashSet<GameObject> Get(int x, int y)
        {
            if (InBounds(x, y))
            {
                return Values[x, y] ?? new HashSet<GameObject>();
            }
            else
            {
                return new HashSet<GameObject>();
            }
        }
    }
}
