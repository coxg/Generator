using System.Collections.Generic;

namespace Generator
{
    public class Party
    {
        public Dictionary<string, GameObject> Members;
        public bool InCombat = true;

        // Constructor
        public Party(Dictionary<string, GameObject> members = null)
        {
            Members = members ?? new Dictionary<string, GameObject>();
        }

        public void Say(string text)
            // Make the whole party say/emote something
        {

        }

        public void AddExperience(int experience)
            // Grants experience to all conscious members of the party
        {
            foreach (var member in Members.Values)
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
            foreach (var member in Members.Values)
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

        }

        public void AddItem(Item item)
            // Gives an item to the party
        {

        }
    }
}