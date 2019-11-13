using System.Collections.Generic;

namespace Generator
{
    public class Party
    {
        public List<GameObject> Members;
        public List<Item> Inventory;
        public bool InCombat = false;
        public int Junk = 0;

        // Constructor
        public Party(List<GameObject> members = null)
        {
            Members = members ?? new List<GameObject>();
        }

        public void Say(string text)
            // Make the whole party say/emote something
        {
            foreach (var member in Members)
            {
                if (member.Health.Current > 0)
                {
                    member.Say(text);
                }
            }
        }

        public void AddExperience(int experience)
            // Grants experience to all conscious members of the party
        {
            Globals.Log("The party gains " + experience + " experience!");
            foreach (var member in Members)
            {
                if (member.Health.Current > 0)
                {
                    member.Experience += experience;
                }
            }
        }

        public void AddClassPoints(int classPoints)
            // Grants CP to all conscious members of the party
        {
            foreach (var member in Members)
            {
                if (member.Health.Current > 0)
                {
                    // TODO: This!
                }
            }
        }

        public void AddJunk(int junk)
            // Gives junk to the party
        {
            Globals.Log("The party gains " + junk + " junk!");
            Junk += junk;
        }

        public void AddItem(Item item)
            // Gives an item to the party
        {
            Globals.Log(item.Name + " added to inventory.");
        }
    }
}