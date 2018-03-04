using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Generator
{
    public class Frames
    // The frames of the animation
    {
        public List<Vector3> Offsets { get; set; }
        public Animation SourceAnimation { get; set; }
        public int Duration { get; set; }
        public int CurrentFrame { get; set; }

        public Frames(
            List<Vector3> offsets,
            float duration,
            Animation sourceAnimation = null)
        // Constructor
        {
            SourceAnimation = sourceAnimation;
            Duration = (int)(duration * Globals.RefreshRate);
            CurrentFrame = 0;
            Offsets = SmoothFrames(offsets, Duration);
        }

        public void Play()
        // Plays a frame of the animation
        {
            // Rotate the difference between the last frame and this one
            Vector3 PositionDifference = Offsets[CurrentFrame]
                - Offsets[(int)Globals.Mod(CurrentFrame - 1, Offsets.Count)];
            PositionDifference = Globals.PointRotatedAroundPoint(
                RotatedPoint: PositionDifference,
                AroundPoint: new Vector3(0, 0, 0),
                Radians: SourceAnimation.SourceObject.Direction);

            // Move the object in that direction
            Vector3 NewPosition = SourceAnimation.SourceObject.Position + PositionDifference;
            if (SourceAnimation.SourceObject.CanMoveTo(NewPosition))
            {
                SourceAnimation.SourceObject.Position = NewPosition;
            }

            // Update animation logic
            SourceAnimation.TotalOffset += PositionDifference;
            CurrentFrame = (int)Globals.Mod(CurrentFrame + 1, Offsets.Count);
        }

        public static List<Vector3> SmoothFrames(List<Vector3> Frames, int Duration)
        // Lengthen the frames to the specified duration, smoothing along the way.
        {
            // Always start and end with (0, 0, 0)
            Frames.Insert(0, new Vector3(0, 0, 0));
            Frames.Add(new Vector3(0, 0, 0));

            // Create lists of values for each dimension
            List<float> XValues = new List<float>();
            List<float> YValues = new List<float>();
            List<float> ZValues = new List<float>();

            // But wait - time is also a dimension! We're in 4D, people!
            float[] TimeInputs = Globals.FloatRange(Frames.Count);
            for (int FrameIndex = 0; FrameIndex < Frames.Count; FrameIndex++)
            {
                TimeInputs[FrameIndex] *= (float)Duration / (Frames.Count - 1);
            }
            float[] TimeOutputs = Globals.FloatRange(Duration);

            // Append X, Y, and Z values from the FrameType to their lists
            foreach (Vector3 Frame in Frames)
            {
                XValues.Add(Frame.X);
                YValues.Add(Frame.Y);
                ZValues.Add(Frame.Z);
            }

            // Create splines from the dimension lists
            float[] XSpline = Spline.Compute(
                x: TimeInputs,
                y: XValues.ToArray(),
                xs: TimeOutputs);
            float[] YSpline = Spline.Compute(
                x: TimeInputs,
                y: YValues.ToArray(),
                xs: TimeOutputs);
            float[] ZSpline = Spline.Compute(
                x: TimeInputs,
                y: ZValues.ToArray(),
                xs: TimeOutputs);

            // Combine dimension lists into a list of Vector3s
            Frames = new List<Vector3>();
            for (int FrameIndex = 0; FrameIndex < Duration; FrameIndex++)
            {
                Frames.Add(new Vector3(
                    XSpline[FrameIndex],
                    YSpline[FrameIndex],
                    ZSpline[FrameIndex]));
            }
            return Frames;
        }
    }

    public class Animation
    {
        // Animation name
        public string Name { get; set; }

        // What's being animated
        public GameObject SourceObject { get; set; }

        // What it does
        private Frames _startFrames { get; set; }
        public Frames StartFrames
        {
            get
            {
                return _startFrames;
            }
            set
            {
                _startFrames = value;
                if (_startFrames != null)
                {
                    _startFrames.SourceAnimation = this;
                }
            }
        }
        private Frames _updateFrames { get; set; }
        public Frames UpdateFrames
        {
            get
            {
                return _updateFrames;
            }
            set
            {
                _updateFrames = value;
                if (_updateFrames != null)
                {
                    _updateFrames.SourceAnimation = this;
                }
            }
        }
        private Frames _stopFrames { get; set; }
        public Frames StopFrames
        {
            get
            {
                return _stopFrames;
            }
            set
            {
                _stopFrames = value;
                if (_stopFrames != null)
                {
                    _stopFrames.SourceAnimation = this;
                }
            }
        }

        // How it does it
        public Vector3 TotalOffset { get; set; }
        public bool IsStarting { get; set; }
        public bool IsUpdating { get; set; }
        public bool IsStopping { get; set; }

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
            if (StartFrames != null)
            {
                StartFrames.SourceAnimation = this;
            }
            if (UpdateFrames != null)
            {
                UpdateFrames.SourceAnimation = this;
            }
            if (StopFrames != null)
            {
                StopFrames.SourceAnimation = this;
            }
        }

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
            if (!IsStarting)
            {
                IsUpdating = true;
            }
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
            if (StartFrames != null)
            {
                StartFrames.CurrentFrame = 0;
            }
            if (UpdateFrames != null)
            {
                UpdateFrames.CurrentFrame = 0;
            }
            if (StopFrames != null)
            {
                StopFrames.CurrentFrame = 0;
            }
        }

        public void Update()
        // What happens on each frame
        {
            // Starting
            if (IsStarting)
            {
                // Play the animation
                if (StartFrames != null)
                {
                    StartFrames.Play();
                }

                // If it was the last frame of the animation
                if (StartFrames == null || StartFrames.CurrentFrame == 0)
                {
                    IsStarting = false;
                }
            }

            // Updating
            else if (IsUpdating)
            {
                // Play the animation
                if (UpdateFrames != null)
                {
                    UpdateFrames.Play();
                }
            }

            // Stopping
            if (IsStopping)
            {
                // Let the updating animation finish playing
                if (IsUpdating && (UpdateFrames == null || UpdateFrames.CurrentFrame == 0))
                {
                    IsUpdating = false;
                    if (StopFrames == null)
                    {
                        IsStopping = false;
                    }
                }

                // If we're playing the stopping animation
                else
                {
                    // Play the animation
                    if (StopFrames != null)
                    {
                        StopFrames.Play();
                        if (StopFrames.CurrentFrame == 0)
                        {
                            IsStopping = false;
                        }
                    }
                }
            }
        }
    }
}
