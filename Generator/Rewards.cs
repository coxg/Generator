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
        public int ClassPoints;
        public int Junk;
        public List<Item> Items;
        public bool Rewarded = false;
        public bool Repeatable = false;

        public Rewards(int experience = 0, int classPoints = 0, int junk = 0, List<Item> items = null, bool repeatable = false)
        {
            Experience = experience;
            ClassPoints = classPoints;
            Junk = junk;
            Items = items ?? new List<Item>();
            Repeatable = repeatable;
        }

        public void Award()
        {
            if (!Rewarded || Repeatable)
            {
                Globals.Party.AddExperience(Experience);
                Globals.Party.AddClassPoints(ClassPoints);
                Globals.Party.AddJunk(Junk);
                foreach (var item in Items)
                {
                    Globals.Party.AddItem(item);
                }
            }
            else
            {
                Globals.Log("This has already been rewarded!");
            }
        }
    }
}
