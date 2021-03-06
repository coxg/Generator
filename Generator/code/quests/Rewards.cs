﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    public class Rewards
    {
        public int Experience;
        public int Junk;
        public List<Item> Items;
        public bool Rewarded = false;
        public bool Repeatable = false;

        public Rewards(int experience = 0, int junk = 0, List<Item> items = null, bool repeatable = false)
        {
            Experience = experience;
            Junk = junk;
            Items = items ?? new List<Item>();
            Repeatable = repeatable;
        }

        public void Award()
        {
            if (!Rewarded || Repeatable)
            {
                Globals.Party.Value.AddExperience(Experience);
                Globals.Party.Value.AddJunk(Junk);
                foreach (var item in Items)
                {
                    Globals.Party.Value.AddItem(item);
                }
            }
            else
            {
                Globals.Log("This has already been rewarded!");
            }
        }
    }
}
