using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class Frames
        // The frames of the animation
    {
        public Frames(
                List<Vector3> offsets,
                float duration,
                Animation sourceAnimation = null)
            // Constructor
        {
            SourceAnimation = sourceAnimation;
            Duration = (int) (duration * Globals.RefreshRate);
            CurrentFrame = 0;
            Offsets = SmoothFrames(offsets, Duration);
        }

        public List<Vector3> Offsets { get; set; }
        public Animation SourceAnimation { get; set; }
        public int Duration { get; set; }
        public int CurrentFrame { get; set; }

        public void Play()
            // Plays a frame of the animation
        {
            // Rotate the difference between the last frame and this one
            var positionDifference = Offsets[CurrentFrame]
                                     - Offsets[(int) Globals.Mod(CurrentFrame - 1, Offsets.Count)];
            positionDifference = Globals.PointRotatedAroundPoint(
                positionDifference,
                new Vector3(0, 0, 0),
                SourceAnimation.SourceObject.Direction);

            // Move the object in that direction
            var newPosition = SourceAnimation.SourceObject.Position + positionDifference;
            if (SourceAnimation.SourceObject.CanMoveTo(newPosition))
                SourceAnimation.SourceObject.Position = newPosition;

            // Update animation logic
            SourceAnimation.TotalOffset += positionDifference;
            CurrentFrame = (int) Globals.Mod(CurrentFrame + 1, Offsets.Count);
        }

        public static List<Vector3> SmoothFrames(List<Vector3> frames, int duration)
            // Lengthen the frames to the specified duration, smoothing along the way.
        {
            // Always start and end with (0, 0, 0)
            frames.Insert(0, new Vector3(0, 0, 0));
            frames.Add(new Vector3(0, 0, 0));

            // Create lists of values for each dimension
            var xValues = new List<float>();
            var yValues = new List<float>();
            var zValues = new List<float>();

            // But wait - time is also a dimension! We're in 4D, people!
            var timeInputs = Globals.FloatRange(frames.Count);
            for (var frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                timeInputs[frameIndex] *= (float) duration / (frames.Count - 1);
            var timeOutputs = Globals.FloatRange(duration);

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
            for (var frameIndex = 0; frameIndex < duration; frameIndex++)
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
            GameObject sourceObject = null,

            // What it does
            Frames startFrames = null,
            Frames updateFrames = null,
            Frames stopFrames = null)
        {
            // Animation name
            Name = name;

            // What's being animated
            SourceObject = sourceObject;

            // How it does it
            TotalOffset = new Vector3(0, 0, 0);
            IsStarting = false;
            IsUpdating = false;
            IsStopping = false;

            // What it does
            StartFrames = startFrames;
            UpdateFrames = updateFrames;
            StopFrames = stopFrames;
            if (StartFrames != null) StartFrames.SourceAnimation = this;
            if (UpdateFrames != null) UpdateFrames.SourceAnimation = this;
            if (StopFrames != null) StopFrames.SourceAnimation = this;
        }

        // Animation name
        public string Name { get; set; }

        // What's being animated
        public GameObject SourceObject { get; set; }

        // What it does
        private Frames _startFrames { get; set; }
        public Frames StartFrames
        {
            get => _startFrames;
            set
            {
                _startFrames = value;
                if (_startFrames != null) _startFrames.SourceAnimation = this;
            }
        }

        private Frames _updateFrames { get; set; }
        public Frames UpdateFrames
        {
            get => _updateFrames;
            set
            {
                _updateFrames = value;
                if (_updateFrames != null) _updateFrames.SourceAnimation = this;
            }
        }

        private Frames _stopFrames { get; set; }
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
        public Vector3 TotalOffset { get; set; }
        public bool IsStarting { get; set; }
        public bool IsUpdating { get; set; }
        public bool IsStopping { get; set; }

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

        public void OnUpdate()
            // When the ability is channeled/toggled on
        {
            // Just because the ability's updating doesn't mean we are
            if (!IsStarting) IsUpdating = true;
        }

        private void Reset()
            // Kills the animation wherever it is, resetting frames and positions.
            // Outside the animation class, one should use Stop().
        {
            // Reset position
            SourceObject.Position -= TotalOffset;
            TotalOffset = new Vector3(0, 0, 0);

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
                if (StartFrames == null || StartFrames.CurrentFrame == 0) IsStarting = false;
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
                // Let the updating animation finish playing
                if (IsUpdating && (UpdateFrames == null || UpdateFrames.CurrentFrame == 0))
                {
                    IsUpdating = false;
                    if (StopFrames == null) IsStopping = false;
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
    }
}