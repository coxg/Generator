using System;
using System.Collections.Generic;

namespace Generator
{
    public class Quest
    {
        public string Name;
        public string Description;
        public GameObject Giver;
        public GameObject Receiver;
        public Requirements Completion;
        public Requirements Reception;
        public int Experience;
        public int ClassPoints;
        public int Junk;
        public List<Item> Items;

        // Constructor
        public Quest(
            string name, string description, GameObject giver, GameObject receiver, 
            Requirements completion, Requirements reception, List<Item> items = null)
        {
            Name = name;
            Description = description;
            Giver = giver;
            Receiver = receiver;
            Items = items ?? new List<Item>();
            Completion = completion;
            Reception = reception;
        }

        public void AwardRewards()
        {
            Globals.Party.AddExperience(Experience);
            Globals.Party.AddClassPoints(ClassPoints);
            Globals.Party.AddJunk(Junk);
            foreach (var item in Items)
            {
                Globals.Party.AddItem(item);
            }
        }

        public void TryComplete()
            // Tries to complete a quest, dispersing its reward if applicable
        {
            if (Completion.IsComplete())
            {
                // TODO: Give completion screen, acceptance screen, whatever
                // TODO: Parameter for what to say when complete?
                AwardRewards();
            }
            else
            {
                // TODO: Parameter for what to say when not complete?
            }
        }
    }
}