using System.Collections.Generic;

namespace Generator
{
    public class Requirements
    {
        private bool _hasBeenCompleted;
        public Dictionary<string, int> Tasks;
        public Dictionary<string, int> Progress;

        // Constructor
        public Requirements(Dictionary<string, int> tasks)
        {
            Tasks = tasks;
            foreach (var requirement in Tasks.Keys)
            {
                Progress.Add(requirement, 0);
            }
        }

        public bool IsComplete()
            // Sees if all of the requirements have been completed
        {
            // If we've already done the check then don't do it again
            if (_hasBeenCompleted)
            {
                return true;
            }

            // See if any requirements have not been met
            foreach (var requirement in Tasks.Keys)
            {
                if (Progress[requirement] < Tasks[requirement])
                {
                    return false;
                }
            }

            // Cache the value - once complete, always complete
            _hasBeenCompleted = true;
            return true;
        }
    }
}