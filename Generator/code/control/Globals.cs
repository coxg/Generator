﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    // C# doesn't have globals. I do!
    public static class Globals
    {
        // The player's party
        public static SavedParty Party = new SavedParty("party", new Party(new List<string> { "niels", "farrah", "old man"}));
        public static SavedInt PlayerPartyNumber = new SavedInt("playerPartyNumber", 0);
        public static GameObject Player
        {
            get { return Zone.GameObjects.Objects[Party.Value.MemberIDs[PlayerPartyNumber.Value]]; }
            set { PlayerPartyNumber.Value = Party.Value.MemberIDs.IndexOf(value.ID); }
        }

        // The current conversation - there can be only one
        private static Conversation currentConversation;
        public static Conversation CurrentConversation
        {
            get { return currentConversation; }
            set
            {
                if (value != null)
                {
                    value.Reset();
                }
                currentConversation = value;
            }
        }

        // Regular old variables
        public static Vector2 Resolution = new Vector2(1600, 900);
        public static ContentManager ContentManager;
        public static bool Logging = true;
        public static int RefreshRate = 60;
        public static string Directory = "/Generator/Generator/";
        // TODO: Remove formatting before release - this roughly doubles the size of the save files
        public static JsonSerializer Serializer = new JsonSerializer { Formatting = Formatting.Indented };

        // World management
        public static SavedString ZoneName = new SavedString("zoneName", "testingZone");
        private static Zone zone;
        public static Zone Zone
        {
            get { return zone; }
            set
            {
                // Remove party from zone before serializing
                var partyMembers = new List<GameObject>();
                if (zone != null)
                {
                    foreach (var memberID in Party.Value.MemberIDs)
                    {
                        partyMembers.Add(Zone.GameObjects.Objects[memberID]);
                        Zone.GameObjects.Objects.Remove(memberID);
                    }
                    Saving.SaveToTmp();
                }

                // Set the new zone
                zone = value;
                ZoneName.Value = value.Name;
                GameControl.lightingRenderTargets = new Dictionary<GameObject, RenderTarget2D>();

                // Add the party to the new zone
                foreach (var partyMember in partyMembers)
                {
                    Zone.GameObjects.Objects[partyMember.ID] = partyMember;
                }
            }
        }
        public static void LoadZone()
        {
            Zone.GameObjects.Objects = new Dictionary<string, GameObject>();
            zone = Zone.Load(ZoneName.Value);
            GameControl.lightingRenderTargets = new Dictionary<GameObject, RenderTarget2D>();
        }
        public static IEnumerable<GameObject> Objects
        {
            get { return Zone.GameObjects.Objects.Values; }
        }

        // Configure the world building mode
        public static bool CreativeMode = true;
        public static int CreativeObjectIndex;

        // Loading assets
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static Texture2D LightTexture;
        public static Texture2D WhiteDot;
        public static SpriteFont Font;

        // Data storage
        public static Dictionary<string, Weapon> WeaponsDict = new Dictionary<string, Weapon>();
        public static Dictionary<string, Armor> ArmorDict = new Dictionary<string, Armor>();
        public static Dictionary<string, GeneratorObj> GeneratorDict = new Dictionary<string, GeneratorObj>();
        public static Dictionary<string, Accessory> AccessoryDict = new Dictionary<string, Accessory>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Log(object text = null)
        // Logs to console with debugging information
        {
            if (Logging)
            {
                var CallingFrame = new StackTrace(1, true).GetFrame(0);
                Console.WriteLine(
                    CallingFrame.GetFileName().Split('\\').Last() + " line "
                    + CallingFrame.GetFileLineNumber() + ", in "
                    + CallingFrame.GetMethod().ToString().Split(" ".ToCharArray())[1].Split("(".ToCharArray()).First()
                    + ": " + text);
            }
        }

        public static void Warn(object text = null)
        {
            Log("[WARNING] " + text);
        }

        public static object Copy(object copyObj)
            // C# doesn't have a native copy method, so just serialize and deserialize
        {
            using (StreamWriter file = File.CreateText(Directory + "tmp.json"))
            {
                Serializer.Serialize(file, copyObj);
            }
            using (StreamReader file = File.OpenText(Directory + "tmp.json"))
            {
                copyObj = Serializer.Deserialize(file, copyObj.GetType());
            }
            return copyObj;
        }
    }
}