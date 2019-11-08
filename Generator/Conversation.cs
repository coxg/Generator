﻿using System;
using System.Linq;
using System.Collections.Generic;


namespace Generator
{
    public class Conversation
    {
        public int StartingChoicesIndex;
        public int CurrentChoicesIndex;
        public List<Choices> ChoicesList;
        public GameObject SourceObject;

        // Constructor - most flexible
        public Conversation(List<Choices> choicesList, int startingChoicesIndex = 0)
        {
            StartingChoicesIndex = startingChoicesIndex;
            CurrentChoicesIndex = startingChoicesIndex;
            ChoicesList = choicesList ?? new List<Choices>();
            SetChoicesSource();
            SelectOnlyChoice();
        }

        // Constructor - single node
        public Conversation(Choices.Node node)
        {
            ChoicesList = new List<Choices>() {
                new Choices(nodes: new List<Choices.Node>() { node })};
            SetChoicesSource();
            SelectOnlyChoice();
        }

        // Constructor - linear conversation
        public Conversation(List<string> text)
        {
            ChoicesList = new List<Choices>() {
                new Choices(nodes: new List<Choices.Node>() { new Choices.Node(text, exitsConversation: true) }) };
            SetChoicesSource();
            SelectOnlyChoice();
        }

        // Constructor - single message
        public Conversation(string text)
        {
            ChoicesList = new List<Choices>() {
                new Choices(nodes: new List<Choices.Node>() { new Choices.Node(text, exitsConversation: true) }) };
            SetChoicesSource();
            SelectOnlyChoice();
        }

        public void SelectOnlyChoice()
        {
            var currentChoices = CurrentChoices;
            if (currentChoices.Nodes.Count == 1)
            {
                currentChoices.ChoiceSelected = true;
            }
        }

        public void SetChoicesSource()
        {
            foreach (Choices choices in ChoicesList)
            {
                choices.SourceConversation = this;
            }
        }

        public void Advance()
            // Advance the conversation based on what's currently selected
        {
            Choices choices = CurrentChoices;
            Choices.Node node = CurrentNode;
            Globals.Log("Choice selected: " + choices.ChoiceSelected);
            Globals.Log("MessageIndex: " + node.MessageIndex);
            Globals.Log("Exits conversation: " + node.ExitsConversation);

            // If we haven't selected a choice yet then select it
            if (!choices.ChoiceSelected)
            {
                choices.ChoiceSelected = true;
                return;
            }

            // If we've already selected a choice, and there's more messages to display, then advance the message
            else if (node.MessageIndex < node.Text.Count - 1)
            {
                node.MessageIndex += 1;
            }

            // If we're at the final message
            else
            {
                choices.ChoiceSelected = false;
                node.MessageIndex = 0;

                // If it ends the conversation then end it
                if (node.ExitsConversation)
                {
                    Globals.CurrentConversation = null;
                }

                // If it doesn't end the conversation then continue it
                else
                {
                    if (node.GoToChoicesIndex != null)
                    {
                        Globals.Log("Going to choice: " + (int)node.GoToChoicesIndex);
                        CurrentChoicesIndex = (int)node.GoToChoicesIndex;
                    }
                    SelectOnlyChoice();
                }
            }
        }

        public void Reset()
            // Reset all indices to restart the conversation
        {
            CurrentChoicesIndex = StartingChoicesIndex;
            foreach (Choices choices in ChoicesList)
            {
                choices.CurrentNodeIndex = 0;
                foreach (Choices.Node node in choices.Nodes)
                {
                    node.MessageIndex = 0;
                }
            }
            SelectOnlyChoice();
        }

        public Choices CurrentChoices
        {
            get => ChoicesList[CurrentChoicesIndex];
        }

        public Choices.Node CurrentNode
        {
            get => CurrentChoices.Nodes[CurrentChoices.CurrentNodeIndex];
        }

        public void Start()
        {
            Globals.CurrentConversation = this;
        }

        public void End()
        {
            CurrentChoicesIndex = StartingChoicesIndex;
            Globals.CurrentConversation = null;
        }

        public class Choices
        {
            public int Index;
            public Conversation SourceConversation;
            public int CurrentNodeIndex;
            public bool ChoiceSelected = false;

            private List<Node> nodes;
            public List<Node> Nodes
            {
                get
                {
                    return nodes.Where(node => node.Requirements == null || node.Requirements.IsComplete()).ToList();
                }
                set { nodes = value; }
            }

            // Constructor - list of nodes
            public Choices(List<Node> nodes, int index = 0)
            {
                Index = index;
                Nodes = nodes;
                foreach (Node node in Nodes)
                {
                    node.SourceChoice = this;
                }
            }

            // Constructor - single node
            public Choices(Node node, int index = 0)
            {
                Index = index;
                Nodes = new List<Node>() { node };
                node.SourceChoice = this;
            }

            // Constructor - single message
            public Choices(string message, int index = 0)
            {
                Index = index;
                Nodes = new List<Node>() { new Node(message, exitsConversation: true) };
                foreach (Node node in Nodes)
                {
                    node.SourceChoice = this;
                }
            }

            public class Node
            {
                public List<string> Text;
                public Requirements Requirements = null;
                public Rewards Rewards = null;
                public Action Effects = null;
                public int? GoToChoicesIndex = null;
                public Choices SourceChoice;
                public bool ExitsConversation;
                public int MessageIndex = 0;

                // Constructor - single message
                public Node(string text, Requirements requirements = null, Rewards rewards = null,
                    Action effects = null, int? goToChoicesIndex = null, bool exitsConversation = false)
                {
                    Text = new List<string>() { text };
                    Requirements = requirements;
                    Rewards = rewards;
                    Effects = effects;
                    GoToChoicesIndex = goToChoicesIndex;
                    ExitsConversation = exitsConversation;
                }

                // Constructor - list of messages
                public Node(List<string> text, Requirements requirements = null, Rewards rewards = null,
                    Action effects = null, int? goToChoicesIndex = null, bool exitsConversation = false)
                {
                    Text = text;
                    Requirements = requirements;
                    Rewards = rewards;
                    Effects = effects;
                    GoToChoicesIndex = goToChoicesIndex;
                    ExitsConversation = exitsConversation;
                }
            }
        }
    }
}