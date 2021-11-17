using System.Linq;
using System.Collections.Generic;

namespace Generator
{
    public class Party
    {
        public List<string> MemberIDs;
        public string LeaderID;
        public List<Item> Inventory = new List<Item>();
        public int Gold;

        // Constructor
        public Party(List<string> memberIDs, string leaderID = null)
        {
            MemberIDs = memberIDs;
            LeaderID = leaderID ?? MemberIDs[0];
        }

        public IEnumerable<GameObject> GetMembers()
        {
            return MemberIDs.Select(member => Globals.GameObjectManager.Get(member));
        }

        public GameObject GetLeader()
        {
            return Globals.GameObjectManager.Get(LeaderID);
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

        public void AddGold(int gold)
            // Gives junk to the party
        {
            Globals.Log("The party gains " + gold + " gold!");
            Gold += gold;
        }

        public void AddItem(Item item)
            // Gives an item to the party
        {
            Globals.Log(item.Name + " added to inventory.");
        }
    }
}