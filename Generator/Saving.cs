﻿using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Generator
{
    public static class Saving
    {
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
                    var slot = getSlot("manual");
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
                    var slot = getSlot("quick");
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
                    var slot = getSlot("auto");
                    autosaveSlot = slot;
                }
                return (int)autosaveSlot;
            }
            set { autosaveSlot = value; }
        }

        public static int getSlot(string category)
        {
            var mostRecentSlot = 0;
            var mostRecentTime = System.DateTime.MinValue;
            foreach (var filepath in Directory.GetFiles(Globals.SaveDirectory))
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

        public static void Save(string category, int slot)
        {
            var saveDir = Globals.SaveDirectory + category + "_" + slot;
            Globals.Log("Saving to " + saveDir);
            // To get accurate timing information we delete first
            if (Directory.Exists(saveDir)) Directory.Delete(saveDir, true);
            Directory.CreateDirectory(saveDir);

            GameObjectManager.Save(saveDir);
            foreach (Acre acre in TileManager.Acres) acre.Save();
        }

        public static void Load(string category, int slot)
        {
            var saveDir = Globals.SaveDirectory + category + "_" + slot;
            if (Directory.Exists(saveDir))
            {
                Globals.Log("Loading from " + saveDir);

                GameObjectManager.Load(saveDir);
                TileManager.PopulateAcres();
            }
            else
            {
                Globals.Log(saveDir + " does not exist.");
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
            if (Directory.Exists(Globals.SaveDirectory + saveName))
            {
                return saveName.Split('_').Last() + ": " + Directory.GetLastWriteTime(Globals.SaveDirectory + saveName);
            }
            else
            {
                return saveName.Split('_').Last() + ": NULL";
            }
        }
    }
}
