using System.Linq;
using System.Collections.Generic;

namespace Generator
{
    public class Party
    {
        public List<string> MemberIDs;
        public List<Item> Inventory;
        public bool InCombat { get { return Globals.Zone.Enemies.Count != 0; } }
        public int Junk = 0;

        // Constructor
        public Party(List<string> memberNames = null)
        {
            MemberIDs = memberNames ?? new List<string>();
        }

        public IEnumerable<GameObject> GetMembers()
        {
            return MemberIDs.Select((member) => Globals.Zone.GameObjects.Objects[member]);
        }

        public void Say(string text)
            // Make the whole party say/emote something
        {
            foreach (var member in GetMembers())
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
            foreach (var member in GetMembers())
            {
                if (member.Health.Current > 0)
                {
                    member.Experience += experience;
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