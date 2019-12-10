using System;
using System.Collections.Generic;

namespace Generator
{
    public class Manager<T>
    {
        public Dictionary<string, T> Objects = new Dictionary<string, T>();
    }
}