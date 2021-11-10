using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Generator
{
    public class Conversation
    {
        public int StartingChoicesIndex;
        public int CurrentChoicesIndex;
        public List<Choices> ChoicesList;
        [JsonIgnore]
        public GameObject SourceObject;

        // Constructor - most flexible
        [JsonConstructor]
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

            // If we haven't selected a choice yet then select it
            if (!choices.ChoiceSelected)
            {
                choices.ChoiceSelected = true;
            }

            // If we've already selected a choice, and there's more messages to display, then advance the message
            if (node.MessageIndex < node.Text.Count - 1)
            {
                node.MessageIndex += 1;
            }

            // If we're at the final message
            else
            {
                node.Effects?.Invoke();
                node.Rewards?.Award();
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
            Globals.Log(this + " starts talking");
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
            [JsonIgnore]
            public Conversation SourceConversation;
            public int CurrentNodeIndex;
            public bool ChoiceSelected = false;

            private List<Node> nodes;
            public List<Node> Nodes
            {
                get
                {
                    return nodes.Where(node => node.Conditional == null || (bool) node.Conditional?.Invoke()).ToList();
                }
                set { nodes = value; }
            }

            public Node GetCurrentNode()
            {
                return Nodes[CurrentNodeIndex];
            }

            // Constructor - list of nodes
            [JsonConstructor]
            public Choices(List<Node> nodes, int index = 0)
            {
                Index = index;
                Nodes = nodes;
                foreach (Node node in Nodes)
                {
                    node.SourceChoices = this;
                }
            }

            // Constructor - single node
            public Choices(Node node, int index = 0)
            {
                Index = index;
                Nodes = new List<Node> { node };
                node.SourceChoices = this;
            }

            // Constructor - single message
            public Choices(string message, int index = 0)
            {
                Index = index;
                Nodes = new List<Node> { new Node(message, exitsConversation: true) };
                foreach (Node node in Nodes)
                {
                    node.SourceChoices = this;
                }
            }

            public class Node
            {
                [JsonIgnore]
                public Func<bool> Conditional = null;
                public Rewards Rewards = null;
                [JsonIgnore]
                public Action Effects = null;
                public int? GoToChoicesIndex = null;
                [JsonIgnore]
                public Choices SourceChoices;
                public bool ExitsConversation;
                public int MessageIndex = 0;

                private List<string> text;
                public List<string> Text
                {
                    get
                    {
                        var output = new List<string>();
                        foreach (string elem in text)
                        {
                            output.Add(Keywords.Aggregate(elem, (current, value) => 
                                current.Replace("{" + value.Key + "}", value.Value().ToString())));
                        }
                        return output;
                    }
                }

                public string GetCurrentMessage()
                {
                    var fullMessage = Text[MessageIndex];
                    var messageParts = fullMessage.Split(new string[] { ": " }, 2, StringSplitOptions.None);
                    return messageParts[messageParts.Count() - 1];
                }

                public GameObject GetCurrentSpeaker()
                {
                    var fullMessage = Text[MessageIndex];
                    var messageParts = fullMessage.Split(new string[] { ": " }, 2, StringSplitOptions.None);
                    var talkingObject = Globals.GameObjectManager.Get(messageParts[0]);
                    return talkingObject ?? SourceChoices.SourceConversation.SourceObject;
                }

                // Constructor - single message
                public Node(string text, Func<bool> conditional = null, Rewards rewards = null,
                    Action effects = null, int? goToChoicesIndex = null, bool exitsConversation = false)
                {
                    this.text = new List<string>() { text };
                    Conditional = conditional;
                    Rewards = rewards;
                    Effects = effects;
                    GoToChoicesIndex = goToChoicesIndex;
                    ExitsConversation = exitsConversation;
                }

                // Constructor - list of messages
                [JsonConstructor]
                public Node(List<string> text, Func<bool> conditional = null, Rewards rewards = null,
                    Action effects = null, int? goToChoicesIndex = null, bool exitsConversation = false)
                {
                    this.text = text;
                    Conditional = conditional;
                    Rewards = rewards;
                    Effects = effects;
                    GoToChoicesIndex = goToChoicesIndex;
                    ExitsConversation = exitsConversation;
                }
            }
        }

        public static Dictionary<string, Func<object>> Keywords = new Dictionary<string, Func<object>>
        // Used for string replacement - for example, "{inCombat}" might be replaced with CombatManager.InCombat
        {
            { "wasCreativeMode", () => !Globals.CreativeMode }
        };
    }
}