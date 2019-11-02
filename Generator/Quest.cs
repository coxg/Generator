using System;
using System.Collections.Generic;

namespace Generator
{
    public class Quest
    {
        public string Name;
        public string Description;
        public GameObject Giver;
        public GameObject TurnInner;
        public List<object> Rewards;
        public Requirements Completion;
        public Requirements Reception;

        // Constructor
        public Quest(
            string name, string description, GameObject giver, GameObject turnInner, 
            List<object> rewards, Requirements completion, Requirements reception)
        {
            Name = name;
            Description = description;
            Giver = giver;
            TurnInner = turnInner;
            Rewards = rewards;
            Completion = completion;
            Reception = reception;
        }

        public void AwardRewards()
        {
            foreach (var reward in Rewards)
            {
                // Most quests should award experience, which should apply to the party
                if (reward.GetType() == typeof(Experience))
                {
                    Globals.Party.AddExperience(reward as Experience);
                }

                // I guess some quests can give class points? I don't know
                else if (reward.GetType() == typeof(ClassPoints))
                {
                    Globals.Party.AddClassPoints(reward as ClassPoints);
                }

                // Sure, some of them might give junk, whatever
                else if (reward.GetType() == typeof(Junk))
                {
                    Globals.Party.AddJunk(reward as Junk);
                }

                // Plenty of quests will give items
                else if (reward.GetType() == typeof(Item))
                {
                    Globals.Party.AddItem(reward as Item);
                }

                // Rewards can be anything, so allow arbitrary code as rewards
                else if (reward.GetType() == typeof(Action))
                {
                    (reward as Action).Invoke();
                }

                // If it's not one of the above... that's not good
                else
                {
                    throw new ApplicationException("You can't reward a " + reward.GetType() + "!");
                }
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