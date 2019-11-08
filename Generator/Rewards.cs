using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    public class Rewards
    {
        public int Experience;
        public int ClassPoints;
        public int Junk;
        public List<Item> Items;

        public Rewards(int experience=0, int classPoints=0, int junk=0, List<Item> items=null)
        {
            Experience = experience;
            ClassPoints = classPoints;
            Junk = junk;
            Items = items ?? new List<Item>();
        }

        public void Award()
        {
            Globals.Party.AddExperience(Experience);
            Globals.Party.AddClassPoints(ClassPoints);
            Globals.Party.AddJunk(Junk);
            foreach (var item in Items)
            {
                Globals.Party.AddItem(item);
            }
        }
    }
}
