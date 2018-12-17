using System;
using System.Collections.Generic;

namespace Generator
{
    public class Manager <T>
    {
        public Dictionary<string, T> NameToObject = new Dictionary<string, T>();
        public Dictionary<int, string> IndexToName = new Dictionary<int, string>();
        public Dictionary<string, int> NameToIndex = new Dictionary<string, int>();

        public string Name;

        public int Count = 0;

        public int CenterAcreX;
        public int CenterAcreY;

        public Acre[,] Acres = new Acre[3, 3];

        public override string ToString()
        {
            return Name + ":\n" 
                + Acres[0, 2].Name + ", " + Acres[1, 2].Name + ", " + Acres[2, 2].Name + "\n"
                + Acres[0, 1].Name + ", " + Acres[1, 1].Name + ", " + Acres[2, 1].Name + "\n"
                + Acres[0, 0].Name + ", " + Acres[1, 0].Name + ", " + Acres[2, 0].Name;
        }

        // Gets the index of the acre from the X position
        public int AcreX(int x)
        {
            return (int)Math.Floor(x / Acre.AcreSize.X) - CenterAcreX + 1;
        }

        // Gets the index of the acre from the Y position
        public int AcreY(int y)
        {
            return (int)Math.Floor(y / Acre.AcreSize.Y) - CenterAcreY + 1;
        }

        // "Getter" - get the object value
        // TODO: Replace this with a real getter
        public T Get(int x, int y)
        {
            var acre = Acres[AcreX(x), AcreY(y)];
            var index = acre.Get(x, y);
            return NameToObject[IndexToName[index]];
        }

        // "Setter" - set the object by name
        // TODO: Replace this with a real setter
        public void Set(int x, int y, string name)
        {
            var acre = Acres[AcreX(x), AcreY(y)];
            var index = NameToIndex[name];
            acre.Set(x, y, index);
        }

        // Adds a new object to the mappings
        public void AddNewObject(string Name, T Object)
        {
            NameToObject.Add(Name, Object);
            IndexToName.Add(Count, Name);
            NameToIndex.Add(Name, Count);
            Count += 1;
        }

        // Initially loads in the acres
        public void PopulateAcres()
        {
            // Get center coordinates
            var viewMin = GameControl.camera.ViewMinCoordinates();
            var viewMax = GameControl.camera.ViewMaxCoordinates();
            CenterAcreX = (int)Math.Floor((viewMax.X + viewMin.X) / 100);
            CenterAcreY = (int)Math.Floor((viewMax.Y + viewMin.Y) / 100);

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
        public void Update()
        {
            // Check to see if we need to make updates to the acres
            var viewMin = GameControl.camera.ViewMinCoordinates();
            var viewMax = GameControl.camera.ViewMaxCoordinates();
            var NewCenterAcreX = (int)Math.Floor((viewMax.X + viewMin.X) / 100);
            var NewCenterAcreY = (int)Math.Floor((viewMax.Y + viewMin.Y) / 100);

            if (NewCenterAcreX != CenterAcreX | NewCenterAcreY != CenterAcreY)
            {
                if (Name == "GameObjects")
                    Globals.Log("Camera moved to acre " + NewCenterAcreX.ToString() + ", " + NewCenterAcreY.ToString());

                // If we're now further to the left then slide acres to the right
                if (NewCenterAcreX + 1 == CenterAcreX & NewCenterAcreY == CenterAcreY)
                {
                    // Write the acres to unload to disk
                    Acres[2, 0].Write();
                    Acres[2, 1].Write();
                    Acres[2, 2].Write();

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
                    Acres[0, 0].Write();
                    Acres[0, 1].Write();
                    Acres[0, 2].Write();

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
                    Acres[0, 2].Write();
                    Acres[1, 2].Write();
                    Acres[2, 2].Write();

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
                    Acres[0, 0].Write();
                    Acres[1, 0].Write();
                    Acres[2, 0].Write();

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
                    foreach (Acre acre in Globals.Tiles.Acres) acre.Write();
                    PopulateAcres();
                }

                // Update the Center stats to reflect the current position
                CenterAcreX = NewCenterAcreX;
                CenterAcreY = NewCenterAcreY;

                if (Name == "GameObjects")
                {
                    Globals.Log("Loaded in:");
                    Globals.Log(this);
                }
            }
        }
    }
}