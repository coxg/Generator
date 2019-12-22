using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Generator
{
    public abstract class Saved<T>
    {
        public string Name;
        public T DefaultValue;

        [JsonIgnore]
        public T Value;

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

        public static void Load()
        {
            using (StreamReader file = File.OpenText(Saving.CurrentSaveDirectory + "/ints.json"))
            {
                Ints = (Dictionary<string, int>)Globals.Serializer.Deserialize(file, typeof(Dictionary<string, int>));
            }
            using (StreamReader file = File.OpenText(Saving.CurrentSaveDirectory + "/strings.json"))
            {
                Strings = (Dictionary<string, string>)Globals.Serializer.Deserialize(file, typeof(Dictionary<string, string>));
            }
            using (StreamReader file = File.OpenText(Saving.CurrentSaveDirectory + "/parties.json"))
            {
                Parties = (Dictionary<string, Party>)Globals.Serializer.Deserialize(file, typeof(Dictionary<string, Party>));
            }
        }

        public static void Save()
        {
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/ints.json"))
            {
                Globals.Serializer.Serialize(file, Ints);
            }
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/strings.json"))
            {
                Globals.Serializer.Serialize(file, Strings);
            }
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/parties.json"))
            {
                Globals.Serializer.Serialize(file, Parties);
            }
        }
    }

    public class SavedString : Saved<string>
    {
        public SavedString(string name, string defaultValue) : base(name, defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public new string Value
        {
            get {
                if (SavedDicts.Strings.TryGetValue(Name, out string value))
                {
                    return value;
                };
                return DefaultValue;
            }
            set { SavedDicts.Strings[Name] = value; }
        }
    }

    public class SavedParty : Saved<Party>
    {
        public SavedParty(string name, Party defaultValue) : base(name, defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public new Party Value
        {
            get
            {
                if (SavedDicts.Parties.TryGetValue(Name, out Party value))
                {
                    return value;
                };
                return DefaultValue;
            }
            set { SavedDicts.Parties[Name] = value; }
        }
    }

    public class SavedInt : Saved<int>
    {
        public SavedInt(string name, int defaultValue) : base(name, defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public new int Value
        {
            get
            {
                if (SavedDicts.Ints.TryGetValue(Name, out int value))
                {
                    return value;
                };
                return DefaultValue;
            }
            set { SavedDicts.Ints[Name] = value; }
        }
    }
}
