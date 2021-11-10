using System.Collections.Generic;

namespace Generator
{
    public class Sprite
    {
        public Sprite(
            string name, bool directional, int width, int height, int x, int y, int numAnimationFrames=1, 
            int? currentFrame=null, int animationLength=1, List<string> directions = null)
        {
            Name = name;
            Directional = directional;
            Width = width;
            Height = height;
            X = x;
            Y = y;
            Directions = directions ?? new List<string>{ "Back", "Front", "Left", "Right" };
            CurrentFrame = currentFrame ?? MathTools.RandInt(numAnimationFrames);
            AnimationLength = animationLength;
        }
        
        public string Name;
        public bool Directional;
        public int Width;  // in blocks for all of these
        public int Height;
        public int X;
        public int Y;
        private int NumAnimationFrames;
        private float CurrentFrame;
        private int AnimationLength;  // in seconds
        public List<string> Directions;

        public int GetAnimationFrame()
        {
            if (NumAnimationFrames == 0)
            {
                return 0;
            }
            
            var frameLength = (float)AnimationLength / NumAnimationFrames;
            CurrentFrame += Timing.GameSpeed / Globals.RefreshRate / frameLength;
            CurrentFrame = MathTools.Mod(CurrentFrame, NumAnimationFrames);
            return (int)CurrentFrame;
        }
    }
}