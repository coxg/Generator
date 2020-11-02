using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace Generator
{
    public class Frames
        // The frames of the animation
    {
        public Frames(
                float duration,  // in seconds
                List<Vector3> baseOffsets = null,
                List<Vector3> baseRotations = null,
                List<Vector3> baseResizes = null,
                Animation sourceAnimation = null,
                int smoothing = 100)
            // Constructor
        {
            SourceAnimation = sourceAnimation;
            Duration = duration;
            Smoothing = smoothing;  // How many frames to generate per 1 frame under 100% speed
            
            BaseOffsets = baseOffsets ?? new List<Vector3> { Vector3.Zero, Vector3.Zero };
            BaseRotations = baseRotations ?? new List<Vector3> { Vector3.Zero, Vector3.Zero };
            BaseResizes = baseResizes ?? new List<Vector3> { Vector3.One, Vector3.One };
            
            Terminators = GetTerminators(BaseOffsets, BaseRotations, BaseResizes);
            SmoothedOffsets = GetSmoothedFrames(BaseOffsets);
            SmoothedRotations = GetSmoothedFrames(BaseRotations);
            SmoothedResizes = GetSmoothedFrames(BaseResizes);
        }

        public List<Vector3> BaseOffsets;
        public List<Vector3> BaseRotations;
        public List<Vector3> BaseResizes;
        [JsonIgnore]
        public List<Vector3> SmoothedOffsets;
        [JsonIgnore]
        public List<Vector3> SmoothedRotations;
        [JsonIgnore]
        public List<Vector3> SmoothedResizes;
        public IEnumerable<int> Terminators;
        [JsonIgnore]
        public Animation SourceAnimation;
        public float Duration;
        public float CurrentFrame;
        public int Smoothing;

        public float FramesPerUpdate()
            // How many animation frames should actually be played per update of the game clock
        {
            float frames = (float)Math.Sqrt(SourceAnimation.SourceObject.Speed.CurrentValue) * Timing.GameSpeed * Smoothing;
            if (SourceAnimation.Name == "Walk")  // TODO: this
            {
                frames *= SourceAnimation.SourceObject.MovementSpeed ?? 1;
            }
            return frames;
        }

        public void Play()
        // Plays a frame of the animation
        {
            // See if there are any new animation frames to play
            var newFrame = MathTools.Mod(CurrentFrame + FramesPerUpdate(), SmoothedOffsets.Count);
            if ((int)newFrame != (int)CurrentFrame)
            {
                // Rotate the difference between the last frame and this one
                var positionDifference = MathTools.PointRotatedAroundPoint(
                    SmoothedOffsets[(int)newFrame],
                    Vector3.Zero,
                    new Vector3(0, 0, -SourceAnimation.AnimatedElement.Direction));
                
                SourceAnimation.AnimatedElement.AnimationOffset = positionDifference;
                SourceAnimation.AnimatedElement.RotationOffset = SmoothedRotations[(int)newFrame];
                SourceAnimation.AnimatedElement.ResizeOffset = SmoothedResizes[(int)newFrame];
                SourceAnimation.TotalOffset = positionDifference;
            }

            // Update the current frame
            CurrentFrame = newFrame;
        }

        public IEnumerable<int> GetTerminators(List<Vector3> offsets, List<Vector3> rotations, List<Vector3> resizes)
        {
            var potentialTerminators = new List<List<int>>
            {
                GetBaseTerminaters(offsets, Vector3.Zero), 
                GetBaseTerminaters(rotations, Vector3.Zero), 
                GetBaseTerminaters(resizes, Vector3.One)
            };
            potentialTerminators = potentialTerminators.Where(x => x.Count > 2).ToList();
            switch (potentialTerminators.Count)
            {
                case 0:
                    return new[] { 0 };
                case 1:
                    return potentialTerminators[0];
                case 2:
                    return potentialTerminators[0].Intersect(potentialTerminators[1]);
                default:
                    return potentialTerminators[0].Intersect(potentialTerminators[1]).Intersect(potentialTerminators[2]);
            }
        }

        private List<int> GetBaseTerminaters(List<Vector3> baseVectors, Vector3 terminator)
        {
            var baseTerminators = new List<int>();
            for (int i = 0; i < baseVectors.Count; i++)
            {
                if (baseVectors[i] == terminator)
                {
                    baseTerminators.Add((int)(i * Globals.RefreshRate * Smoothing * Duration / (baseVectors.Count - 1)));
                }
            }
            return baseTerminators;
        }

        public bool CanTerminate()
            // Checks whether or not the CurrentFrame meets any of the termination conditions
        {
            foreach (var terminator in Terminators)
            {
                if (terminator > CurrentFrame)
                {
                    return false;
                }
                else if (terminator <= CurrentFrame && CurrentFrame < terminator + FramesPerUpdate())
                {
                    return true;
                }
            }
            return false;
        }

        public List<Vector3> GetSmoothedFrames(List<Vector3> frames)
            // Lengthen the frames to the specified duration, smoothing along the way.
        {

            // Create lists of values for each dimension
            var xValues = new List<float>();
            var yValues = new List<float>();
            var zValues = new List<float>();

            // But wait - time is also a dimension! We're in 4D, people!
            var numberOfFrames = (int) (Duration * Globals.RefreshRate * Smoothing);
            var timeInputs = MathTools.FloatRange(frames.Count);
            for (var frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                timeInputs[frameIndex] *= (float) numberOfFrames / (frames.Count - 1);
            var timeOutputs = MathTools.FloatRange(numberOfFrames);

            // Append X, Y, and Z values from the FrameType to their lists
            foreach (var frame in frames)
            {
                xValues.Add(frame.X);
                yValues.Add(frame.Y);
                zValues.Add(frame.Z);
            }

            // Create splines from the dimension lists
            var xSpline = Spline.Compute(
                timeInputs,
                xValues.ToArray(),
                timeOutputs);
            var ySpline = Spline.Compute(
                timeInputs,
                yValues.ToArray(),
                timeOutputs);
            var zSpline = Spline.Compute(
                timeInputs,
                zValues.ToArray(),
                timeOutputs);

            // Combine dimension lists into a list of Vector3s
            frames = new List<Vector3>();
            for (var frameIndex = 0; frameIndex < numberOfFrames; frameIndex++)
                frames.Add(new Vector3(
                    xSpline[frameIndex],
                    ySpline[frameIndex],
                    zSpline[frameIndex]));
            return frames;
        }
    }

    public class Animation
    {
        // Constructor
        public Animation(
            // Animation name
            string name = null,

            // What's being animated
            GameElement animatedElement = null,
            GameObject sourceObject = null,

            // What it does
            Frames startFrames = null,
            Frames updateFrames = null,
            Frames stopFrames = null)
        {
            // Animation name
            Name = name;

            // What's being animated
            AnimatedElement = animatedElement;

            // How it does it
            TotalOffset = Vector3.Zero;
            IsStarting = false;
            IsUpdating = false;
            IsStopping = false;

            // What it does
            StartFrames = startFrames;
            UpdateFrames = updateFrames;
            StopFrames = stopFrames;
            SetSource(sourceObject);
        }

        // Animation name
        public string Name;

        // What's being animated
        [JsonIgnore]
        public GameElement AnimatedElement;  // This is the element that's actually moving
        [JsonIgnore]
        public GameObject SourceObject;  // Can be the same as the AnimatedElement, controls animation speed

        // What it does
        private Frames _startFrames;
        public Frames StartFrames
        {
            get => _startFrames;
            set
            {
                _startFrames = value;
                if (_startFrames != null) _startFrames.SourceAnimation = this;
            }
        }

        private Frames _updateFrames;
        public Frames UpdateFrames
        {
            get => _updateFrames;
            set
            {
                _updateFrames = value;
                if (_updateFrames != null) _updateFrames.SourceAnimation = this;
            }
        }

        private Frames _stopFrames;
        public Frames StopFrames
        {
            get => _stopFrames;
            set
            {
                _stopFrames = value;
                if (_stopFrames != null) _stopFrames.SourceAnimation = this;
            }
        }

        // How it does it
        public Vector3 TotalOffset;
        public bool IsStarting;
        public bool IsUpdating;
        public bool IsStopping;

        public void Start()
            // When the ability is started
        {
            // Make it only starting
            Reset();
            IsStarting = true;
        }

        public void Stop()
            // When the ability is stopped
        {
            IsStopping = true;
        }

        private void Reset()
            // Kills the animation wherever it is, resetting frames and positions.
            // Outside the animation class, one should use Stop().
        {
            // Reset position
            AnimatedElement.AnimationOffset -= TotalOffset;
            TotalOffset = Vector3.Zero;
            AnimatedElement.RotationOffset = Vector3.Zero;

            // Stop all animations
            IsStarting = false;
            IsUpdating = false;
            IsStopping = false;

            // Reset animation frames
            if (StartFrames != null) StartFrames.CurrentFrame = 0;
            if (UpdateFrames != null) UpdateFrames.CurrentFrame = 0;
            if (StopFrames != null) StopFrames.CurrentFrame = 0;
        }

        public void Update()
            // What happens on each frame
        {
            // Starting
            if (IsStarting)
            {
                // Play the animation
                if (StartFrames != null) StartFrames.Play();

                // If it was the last frame of the animation
                if (StartFrames == null || StartFrames.CurrentFrame == 0)
                {
                    IsStarting = false;
                    IsUpdating = true;
                }
            }

            // Updating
            else if (IsUpdating)
            {
                // Play the animation
                if (UpdateFrames != null) UpdateFrames.Play();
            }

            // Stopping
            if (IsStopping)
            {
                // If we're ending the stopping animation
                if (IsUpdating && (UpdateFrames == null  || UpdateFrames.CanTerminate()))
                {
                    Reset();
                }

                // If we're playing the stopping animation
                else
                {
                    // Play the animation
                    if (StopFrames != null)
                    {
                        StopFrames.Play();
                        if (StopFrames.CurrentFrame == 0) IsStopping = false;
                    }
                }
            }
        }

        public void SetSource(GameObject sourceObject)
        {
            SourceObject = sourceObject;
            if (StartFrames != null) StartFrames.SourceAnimation = this;
            if (UpdateFrames != null) UpdateFrames.SourceAnimation = this;
            if (StopFrames != null) StopFrames.SourceAnimation = this;
        }
    }
}