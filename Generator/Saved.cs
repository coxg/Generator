using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Generator
{
    public abstract class Saved<T>
    {
        public string Name;
        public T DefaultValue;
        public Dictionary<string, T> SavedDict;

        [JsonIgnore]
        public T Value
        {
            get {
                T returnValue;
                if (SavedDict.TryGetValue(Name, out returnValue))
                {
                    return returnValue;
                }
                else
                {
                    return DefaultValue;
                }
            }
            set { SavedDict[Name] = value; }
        }

        public Saved(string name, T defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
    }

    public static class SavedDicts
    {
        public static Dictionary<string, string> Strings = new Dictionary<string, string>();
        public static Dictionary<string, Party> Parties = new Dictionary<string, Party>();
        public static Dictionary<string, int> Ints = new Dictionary<string, int>();

        public static void Load(string saveDir)
        {
            using (StreamReader file = File.OpenText(saveDir + "/misc.json"))
            {
                Ints = (Dictionary<string, int>)Globals.Serializer.Deserialize(file, typeof(Dictionary<string, int>));
                Strings = (Dictionary<string, string>)Globals.Serializer.Deserialize(file, typeof(Dictionary<string, string>));
                Parties = (Dictionary<string, Party>)Globals.Serializer.Deserialize(file, typeof(Dictionary<string, Party>));
            }
        }

        public static void Save(string saveDir)
        {
            using (StreamWriter file = File.CreateText(saveDir + "/misc.json"))
            {
                Globals.Serializer.Serialize(file, Ints);
                Globals.Serializer.Serialize(file, Strings);
                Globals.Serializer.Serialize(file, Parties);
            }
        }
    }

    public class SavedString : Saved<string>
    {
        public SavedString(string name, string defaultValue) : base(name, defaultValue)
        {
            SavedDict = SavedDicts.Strings;
            Name = name;
            DefaultValue = defaultValue;
        }
    }

    public class SavedParty : Saved<Party>
    {
        public SavedParty(string name, Party defaultValue) : base(name, defaultValue)
        {
            SavedDict = SavedDicts.Parties;
            Name = name;
            DefaultValue = defaultValue;
        }
    }

    public class SavedInt : Saved<int>
    {
        public SavedInt(string name, int defaultValue) : base(name, defaultValue)
        {
            SavedDict = SavedDicts.Ints;
            Name = name;
            DefaultValue = defaultValue;
        }
    }
}
