using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Manager <T>
    {
        // TODO: This shouldn't be by name - what if the player names their character something that's already in here?
        public static Dictionary<string, T> ObjectFromName = new Dictionary<string, T>();
        public static Dictionary<int, string> NameFromIndex = new Dictionary<int, string>();
        public static Dictionary<string, int> IndexFromName = new Dictionary<string, int>();

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
            return NameFromIndex[GetIndex(x, y)];
        }

        // "Getter" - get the object at the location
        // TODO: Replace this with a real getter
        public static T Get(int x, int y)
        {
            return ObjectFromName[GetName(x, y)];
        }

        // "Setter" - set the object by name
        // TODO: Replace this with a real setter
        public static void Set(int x, int y, string name)
        {
            var acre = Acres[AcreX(x), AcreY(y)];
            var index = IndexFromName[name];
            acre.Set(x, y, index);
        }

        // Adds a new object to the mappings
        public static void AddNewObject(string Name, T Object)
        {
            ObjectFromName.Add(Name, Object);
            NameFromIndex.Add(Count, Name);
            IndexFromName.Add(Name, Count);
            Count += 1;
        }

        // Attempts to remove an object from the mappings
        // Note that an object is not guaranteed to be in the mappings - an object can die for multiple reasons within the same Update, for example
        // TODO: Make this impact the count, as that will probably cause some headaches in the future. Would currently cause us to overwrite existing values
        public static void RemoveObject(string Name)
        {
            var removedSuccessfully = ObjectFromName.Remove(Name);
            if (removedSuccessfully)
            {
                var index = IndexFromName[Name];
                IndexFromName.Remove(Name);
                NameFromIndex.Remove(index);
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

        // Getting the min/max values
        public static int MinX()
        {
            return Acres[0, 0].MinX;
        }
        public static int MinY()
        {
            return Acres[0, 0].MinY;
        }
        public static int MaxX()
        {
            return Acres[2, 2].MinX + (int)Acre.AcreSize.X;
        }
        public static int MaxY()
        {
            return Acres[2, 2].MinY + (int)Acre.AcreSize.Y;
        }

        public static Int16[] indices = new Int16[(int)Acre.AcreSize.X * (int)Acre.AcreSize.Y * 6 * 9];
        public static void SetUpIndices()
        // Sets up the indices. This should only be run once (?)
        {
            int counter = 0;
            for (Int16 y = 0; y < ((Int16)Acre.AcreSize.Y * 3) - 1; y++)
            {
                for (Int16 x = 0; x < ((Int16)Acre.AcreSize.X * 3) - 1; x++)
                {
                    Int16 lowerLeft = (Int16)(x + y * Acre.AcreSize.X);
                    Int16 lowerRight = (Int16)((x + 1) + y * Acre.AcreSize.X);
                    Int16 topLeft = (Int16)(x + (y + 1) * Acre.AcreSize.X);
                    Int16 topRight = (Int16)((x + 1) + (y + 1) * Acre.AcreSize.X);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        public static VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(int)Acre.AcreSize.X * (int)Acre.AcreSize.Y * 6 * 9];
        public static void SetUpVertices()
        // Sets up the vertices. This should be run every time the acres change.
        {
            var counter = 0;
            for (int x = MinX(); x < MaxX(); x++)
            {
                for (int y = MinY(); y < MaxY(); y++)
                {
                    // top left
                    var topLeft = vertices[counter++];
                    topLeft.Position = new Vector3(x, y + 1, 0);
                    topLeft.Color = Color.White; // TODO: Is this the default or do we need to specify?
                    topLeft.TextureCoordinate.X = 0;
                    topLeft.TextureCoordinate.Y = 0;

                    // bottom right
                    var bottomRight = vertices[counter++];
                    bottomRight.Position = new Vector3(x + 1, y, 0);
                    bottomRight.Color = Color.White;
                    bottomRight.TextureCoordinate.X = 1;
                    bottomRight.TextureCoordinate.Y = 1;

                    // bottom left
                    var bottomLeft = vertices[counter++];
                    bottomLeft.Position = new Vector3(x, y, 0);
                    bottomLeft.Color = Color.White;
                    bottomLeft.TextureCoordinate.X = 0;
                    bottomLeft.TextureCoordinate.Y = 1;

                    // bottom right
                    vertices[counter++] = bottomRight;

                    // top left
                    vertices[counter++] = topLeft;

                    // top right
                    var topRight = vertices[counter++];
                    topRight.Position = new Vector3(x + 1, y + 1, 0);
                    topRight.Color = Color.White;
                    topRight.TextureCoordinate.X = 1;
                    topRight.TextureCoordinate.Y = 0;
                }
            }
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
                    PopulateAcres();
                }

                // Update the Center stats to reflect the current position
                CenterAcreX = NewCenterAcreX;
                CenterAcreY = NewCenterAcreY;

                SetUpVertices();
            }
        }
    }
}