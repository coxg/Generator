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
        public Rewards Rewards;

        // Constructor
        public Quest(
            string name, string description, GameObject giver, GameObject receiver, 
            Requirements completion, Requirements reception, Rewards rewards=null)
        {
            Name = name;
            Description = description;
            Giver = giver;
            Receiver = receiver;
            Rewards = rewards;
            Completion = completion;
            Reception = reception;
        }

        public void TryComplete()
            // Tries to complete a quest, dispersing its reward if applicable
        {
            if (Completion.IsComplete())
            {
                // TODO: Give completion screen, acceptance screen, whatever
                // TODO: Parameter for what to say when complete?
                Rewards?.Award();
            }
            else
            {
                // TODO: Parameter for what to say when not complete?
                Globals.Log("Cannot complete quest " + Name);
            }
        }
    }
}