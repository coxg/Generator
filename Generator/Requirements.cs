using System.Collections.Generic;

namespace Generator
{
    public class Requirements
    {
        public bool HasBeenCompleted;
        public Dictionary<string, int> Tasks;
        public Dictionary<string, int> Progress;

        // Constructor
        public Requirements(Dictionary<string, int> tasks)
        {
            Tasks = tasks;
            Progress = new Dictionary<string, int>();
            foreach (var requirement in Tasks.Keys)
            {
                Progress.Add(requirement, 0);
            }
        }

        public bool IsComplete()
            // Sees if all of the requirements have been completed
        {
            // See if any requirements have not been met
            foreach (var requirement in Tasks.Keys)
            {
                if (Progress[requirement] < Tasks[requirement])
                {
                    return false;
                }
            }

            // Cache the value - once complete, always complete
            HasBeenCompleted = true;
            return true;
        }
    }
}