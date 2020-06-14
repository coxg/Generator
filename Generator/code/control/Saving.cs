using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public static class Saving
    {
        public static string BaseSaveDirectory = Globals.ProjectDirectory + "/Saves/";
        public static string CurrentSaveDirectory;
        public static string TempSaveDirectory { get { return BaseSaveDirectory + "tmp/"; } }

        public static Dictionary<string, int> numSaves = new Dictionary<string, int>
        {
            { "manual", 5 },
            { "quick", 5 },
            { "auto", 5 }
        };

        private static int? manualsaveSlot;
        public static int ManualSaveSlot
        {
            get
            {
                if (manualsaveSlot == null)
                {
                    var slot = GetSlot("manual");
                    manualsaveSlot = slot;
                }
                return (int)manualsaveSlot;
            }
            set { manualsaveSlot = value; }
        }

        private static int? quicksaveSlot;
        public static int QuicksaveSlot
        {
            get
            {
                if (quicksaveSlot == null)
                {
                    var slot = GetSlot("quick");
                    quicksaveSlot = slot;
                }
                return (int)quicksaveSlot;
            }
            set { quicksaveSlot = value; }
        }

        private static int? autosaveSlot;
        public static int AutosaveSlot
        {
            get
            {
                if (autosaveSlot == null)
                {
                    var slot = GetSlot("auto");
                    autosaveSlot = slot;
                }
                return (int)autosaveSlot;
            }
            set { autosaveSlot = value; }
        }

        public static int GetSlot(string category)
        {
            var mostRecentSlot = 0;
            var mostRecentTime = System.DateTime.MinValue;
            foreach (var filepath in Directory.GetFiles(BaseSaveDirectory))
            {
                if (filepath.Split('\\').Last().Split('_')[0] == category)
                {
                    var saveTime = Directory.GetLastWriteTime(filepath);
                    if (saveTime > mostRecentTime)
                    {
                        mostRecentTime = saveTime;
                        mostRecentSlot = int.Parse(filepath.Split('_').Last());
                    }
                }
            }
            return mostRecentSlot;
        }

        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            // Delete and recreate the new destination directory if it already exists
            if (Directory.Exists(destDirName))
            {
                try
                {
                    Directory.Delete(destDirName, true);
                }
                catch (IOException)
                {
                    Globals.Warn("Could not delete " + destDirName + "; files may be open.");
                }
            }
            Directory.CreateDirectory(destDirName);

            // If the source directory doesn't exist then warn and move on
            // This will be the case when creating a new game, for example
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                Globals.Warn(sourceDirName + " does not exist.");
                return;
            }
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // Call recursively on other directories in the directory
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        public static void Save(string saveType, int slot)
        {
            CurrentSaveDirectory = BaseSaveDirectory + saveType + "_" + slot;
            Globals.Log("Saving to " + CurrentSaveDirectory);

            // TODO: Why can't I do this as a Task? If not then add blocking
            // Create save slot out of tmp directory
            CopyDirectory(TempSaveDirectory, CurrentSaveDirectory);
            SaveAreaToDisk();
            SavedDicts.Save();
            Input.Save();
        }

        public static void SaveAreaToDisk()
        {
            var saveDir = Saving.CurrentSaveDirectory + "/Zones/" + Globals.Zone.Name + "/";
            Directory.CreateDirectory(saveDir);
            using (StreamWriter file = File.CreateText(saveDir + "zone.json"))
            {
                Globals.Serializer.Serialize(file, Globals.Zone);
            }
            
            using (StreamWriter file = File.CreateText(saveDir + "objects.json"))
            {
                Globals.Serializer.Serialize(file, Globals.GameObjectManager);
            }
            
            using (StreamWriter file = File.CreateText(saveDir + "tiles.json"))
            {
                Globals.Serializer.Serialize(file, Globals.TileManager);
            }
        }
        
        public static void LoadAreaFromDisk(string name)
        {
            var saveDir = CurrentSaveDirectory + "/Zones/" + name + "/";
            if (Directory.Exists(saveDir))
            {
                Globals.Log("Loading " + name);
                using (StreamReader file = File.OpenText(saveDir + "zone.json"))
                {
                    Globals.Zone = (Zone)Globals.Serializer.Deserialize(file, typeof(Zone));
                }
                
                Globals.Log("Loading GameObjects");
                GameObjectManager.Load(saveDir + "objects.json");
                
                Globals.Log("Loading Tiles");
                using (StreamReader file = File.OpenText(saveDir + "tiles.json"))
                {
                    Globals.TileManager = (TileManager)Globals.Serializer.Deserialize(file, typeof(TileManager));
                }
            }
            else
            {
                Globals.Log(name + " not found; initializing.");
                Zone.Initialize(name);
            }
        }

        public static void Load(string saveType, int slot)
        {
            CurrentSaveDirectory = BaseSaveDirectory + saveType + "_" + slot;
            if (Directory.Exists(CurrentSaveDirectory))
            {
                Globals.Log("Loading from " + CurrentSaveDirectory);

                // Load the game from the save file
                SavedDicts.Load();
                Input.Load();
                LoadAreaFromDisk(Globals.ZoneName.Value);

                // Recreate the tmp directory based on the save we're loading
                CopyDirectory(CurrentSaveDirectory, TempSaveDirectory);
            }
            else
            {
                Globals.Log(CurrentSaveDirectory + " does not exist.");
            }
        }

        public static void Quicksave()
        {
            QuicksaveSlot = (int)MathTools.Mod(QuicksaveSlot + 1, numSaves["quick"]);
            Save("quick", QuicksaveSlot);
        }

        public static void Quickload()
            // TODO: Make config option to load most recent save or load most recent quicksave
        {
            Load("quick", QuicksaveSlot);
        }

        public static void Autosave()
        {
            AutosaveSlot = (int)MathTools.Mod(AutosaveSlot + 1, numSaves["auto"]);
            Save("auto", AutosaveSlot);
            Timing.AddEvent(300, Autosave);
        }

        public static void PopulateSaveKeywords()
        {
            foreach (var category in numSaves.Keys)
            {
                for (int i = 0; i < numSaves[category]; i++)
                {
                    var saveName = category + "_" + i;
                    Conversation.Keywords[saveName] = () => GetSaveStr(saveName);
                }
            }
        }

        public static string GetSaveStr(string saveName)
        {
            if (Directory.Exists(BaseSaveDirectory + saveName))
            {
                return saveName.Split('_').Last() + ": " + Directory.GetLastWriteTime(BaseSaveDirectory + saveName);
            }
            else
            {
                return saveName.Split('_').Last() + ": NULL";
            }
        }
    }
}
