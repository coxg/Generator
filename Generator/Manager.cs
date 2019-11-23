using System;
using System.Collections.Generic;

namespace Generator
{
    public class Manager <T>
    {
        // TODO: This shouldn't be by name - what if the player names their character something that's already in here?
        public static Dictionary<string, T> ObjectFromID = new Dictionary<string, T>();
        public static Dictionary<int, string> IDFromIndex = new Dictionary<int, string>();
        public static Dictionary<string, int> IndexFromID = new Dictionary<string, int>();

        public static string Name;

        public static int Count = 0;

        public static int CenterAcreX;
        public static int CenterAcreY;

        public static Acre[,] Acres = new Acre[3, 3];

        public static string String()
        {
            return Name + ":\n" 
                + Acres[0, 2].Name + ", " + Acres[1, 2].Name + ", " + Acres[2, 2].Name + "\n"
                + Acres[0, 1].Name + ", " + Acres[1, 1].Name + ", " + Acres[2, 1].Name + "\n"
                + Acres[0, 0].Name + ", " + Acres[1, 0].Name + ", " + Acres[2, 0].Name;
        }

        // Gets the index of the acre from the X position
        public static int AcreX(int x)
        {
            return (int)Math.Floor(x / Acre.AcreSize.X) - CenterAcreX + 1;
        }

        // Gets the index of the acre from the Y position
        public static int AcreY(int y)
        {
            return (int)Math.Floor(y / Acre.AcreSize.Y) - CenterAcreY + 1;
        }

        // Gets the index of the object at the location
        // TODO: Negative indices aren't a thing, so I need to wrap around manually. What should the max distance be?
        public static int GetIndex(int x, int y)
        {
            var acre = Acres[AcreX(x), AcreY(y)];
            var index = acre.Get(x, y);
            return index;
        }

        // Gets the name of the object at the location
        public static string GetName(int x, int y)
        {
            return IDFromIndex[GetIndex(x, y)];
        }

        // "Getter" - get the object at the location
        // TODO: Replace this with a real getter
        public static T Get(int x, int y)
        {
            return ObjectFromID[GetName(x, y)];
        }

        // "Setter" - set the object by name
        // TODO: Replace this with a real setter
        public static void Set(int x, int y, string name)
        {
            var acre = Acres[AcreX(x), AcreY(y)];
            var index = IndexFromID[name];
            acre.Set(x, y, index);
        }

        // Adds a new object to the mappings
        public static void AddNewObject(string Name, T Object)
        {
            ObjectFromID.Add(Name, Object);
            IDFromIndex.Add(Count, Name);
            IndexFromID.Add(Name, Count);
            Count += 1;
        }

        // Attempts to remove an object from the mappings
        // Note that an object is not guaranteed to be in the mappings - an object can die for multiple reasons within the same Update, for example
        // TODO: Make this impact the count, as that will probably cause some headaches in the future. Would currently cause us to overwrite existing values
        public static void RemoveObject(string Name)
        {
            var removedSuccessfully = ObjectFromID.Remove(Name);
            if (removedSuccessfully)
            {
                var index = IndexFromID[Name];
                IndexFromID.Remove(Name);
                IDFromIndex.Remove(index);
            }
        }

        // Initially loads in the acres
        public static void PopulateAcres()
        {
            // Get center coordinates
            CenterAcreX = (int)Math.Floor((GameControl.camera.VisibleArea.Left + GameControl.camera.VisibleArea.Right) / 100);
            CenterAcreY = (int)Math.Floor((GameControl.camera.VisibleArea.Top + GameControl.camera.VisibleArea.Bottom) / 100);

            // Populate acres based off of the center
            Acres[0, 0] = new Acre(Name, CenterAcreX - 1, CenterAcreY - 1); // Bottom-left
            Acres[1, 0] = new Acre(Name, CenterAcreX, CenterAcreY - 1);     // Bottom-middle
            Acres[2, 0] = new Acre(Name, CenterAcreX + 1, CenterAcreY - 1); // Bottom-right
            Acres[0, 1] = new Acre(Name, CenterAcreX - 1, CenterAcreY);     // Middle-left
            Acres[1, 1] = new Acre(Name, CenterAcreX, CenterAcreY);         // Middle-middle
            Acres[2, 1] = new Acre(Name, CenterAcreX + 1, CenterAcreY);     // Middle-right
            Acres[0, 2] = new Acre(Name, CenterAcreX - 1, CenterAcreY + 1); // Top-left
            Acres[1, 2] = new Acre(Name, CenterAcreX, CenterAcreY + 1);     // Top-middle
            Acres[2, 2] = new Acre(Name, CenterAcreX + 1, CenterAcreY + 1); // Top-right
        }

        // Updates your acres to surround the camera
        public static void Update()
        {
            // Check to see if we need to make updates to the acres
            var NewCenterAcreX = (int)Math.Floor((GameControl.camera.VisibleArea.Left + GameControl.camera.VisibleArea.Right) / 100);
            var NewCenterAcreY = (int)Math.Floor((GameControl.camera.VisibleArea.Top + GameControl.camera.VisibleArea.Bottom) / 100);

            if (NewCenterAcreX != CenterAcreX | NewCenterAcreY != CenterAcreY)
            {
                if (Name == "GameObjects")
                    Globals.Log("Camera moved to acre " + NewCenterAcreX.ToString() + ", " + NewCenterAcreY.ToString());

                // If we're now further to the left then slide acres to the right
                if (NewCenterAcreX + 1 == CenterAcreX & NewCenterAcreY == CenterAcreY)
                {
                    // Write the acres to unload to disk
                    Acres[2, 0].Save();
                    Acres[2, 1].Save();
                    Acres[2, 2].Save();

                    // Slide acres we're keeping over
                    Acres[2, 0] = Acres[1, 0];
                    Acres[2, 1] = Acres[1, 1];
                    Acres[2, 2] = Acres[1, 2];
                    Acres[1, 0] = Acres[0, 0];
                    Acres[1, 1] = Acres[0, 1];
                    Acres[1, 2] = Acres[0, 2];

                    // Read in new acres from disk
                    Acres[0, 0] = new Acre(Name, NewCenterAcreX - 1, NewCenterAcreY - 1);
                    Acres[0, 1] = new Acre(Name, NewCenterAcreX - 1, NewCenterAcreY);
                    Acres[0, 2] = new Acre(Name, NewCenterAcreX - 1, NewCenterAcreY + 1);
                }

                // If we're now further to the right then slide acres to the left
                else if (NewCenterAcreX - 1 == CenterAcreX & NewCenterAcreY == CenterAcreY)
                {
                    // Write the acres to unload to disk
                    Acres[0, 0].Save();
                    Acres[0, 1].Save();
                    Acres[0, 2].Save();

                    // Slide acres we're keeping over
                    Acres[0, 0] = Acres[1, 0];
                    Acres[0, 1] = Acres[1, 1];
                    Acres[0, 2] = Acres[1, 2];
                    Acres[1, 0] = Acres[2, 0];
                    Acres[1, 1] = Acres[2, 1];
                    Acres[1, 2] = Acres[2, 2];

                    // Read in new acres from disk
                    Acres[2, 0] = new Acre(Name, NewCenterAcreX + 1, NewCenterAcreY - 1);
                    Acres[2, 1] = new Acre(Name, NewCenterAcreX + 1, NewCenterAcreY);
                    Acres[2, 2] = new Acre(Name, NewCenterAcreX + 1, NewCenterAcreY + 1);
                }

                // If we're now further down then slide acres up
                else if (NewCenterAcreX == CenterAcreX & NewCenterAcreY + 1 == CenterAcreY)
                {
                    // Write the acres to unload to disk
                    Acres[0, 2].Save();
                    Acres[1, 2].Save();
                    Acres[2, 2].Save();

                    // Slide acres we're keeping over
                    Acres[0, 2] = Acres[0, 1];
                    Acres[1, 2] = Acres[1, 1];
                    Acres[2, 2] = Acres[2, 1];
                    Acres[0, 1] = Acres[0, 0];
                    Acres[1, 1] = Acres[1, 0];
                    Acres[2, 1] = Acres[2, 0];

                    // Read in new acres from disk
                    Acres[0, 0] = new Acre(Name, NewCenterAcreX - 1, NewCenterAcreY - 1);
                    Acres[1, 0] = new Acre(Name, NewCenterAcreX, NewCenterAcreY - 1);
                    Acres[2, 0] = new Acre(Name, NewCenterAcreX + 1, NewCenterAcreY - 1);
                }

                // If we're now further up then slide acres down
                else if (NewCenterAcreX == CenterAcreX & NewCenterAcreY - 1 == CenterAcreY)
                {
                    // Write the acres to unload to disk
                    Acres[0, 0].Save();
                    Acres[1, 0].Save();
                    Acres[2, 0].Save();

                    // Slide acres we're keeping over
                    Acres[0, 0] = Acres[0, 1];
                    Acres[1, 0] = Acres[1, 1];
                    Acres[2, 0] = Acres[2, 1];
                    Acres[0, 1] = Acres[0, 2];
                    Acres[1, 1] = Acres[1, 2];
                    Acres[2, 1] = Acres[2, 2];

                    // Read in new acres from disk
                    Acres[0, 2] = new Acre(Name, NewCenterAcreX - 1, NewCenterAcreY + 1);
                    Acres[1, 2] = new Acre(Name, NewCenterAcreX, NewCenterAcreY + 1);
                    Acres[2, 2] = new Acre(Name, NewCenterAcreX + 1, NewCenterAcreY + 1);
                }

                // If we're not in one of the above cases then we moved the camera somewhere completely new
                else
                {
                    PopulateAcres();
                }

                // Update the Center stats to reflect the current position
                CenterAcreX = NewCenterAcreX;
                CenterAcreY = NewCenterAcreY;
            }
        }
    }
}