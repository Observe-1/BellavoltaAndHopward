using UnityEngine;

namespace Fosters.Studio
{
    // Each product branch supplies one module; the foundation has no product-specific rules.
    public abstract class GameModule
    {
        public abstract string Title { get; }
        public abstract string Slug { get; }
        public abstract string Place { get; }
        public abstract string Tagline { get; }
        public abstract string[] Outings { get; }
        public abstract string[] Lessons { get; }
        public abstract string PrimaryControl { get; }
        public abstract string SecondaryControl { get; }
        public abstract Color[] Palette(bool dusk);
        public abstract Course CreateCourse(int outing);
        public abstract Motor CreateMotor(Course course, System.Action<string,int> award, System.Action<string> stumble);
        public abstract void DrawRider(Ink ink, Motor motor, Vector2 position, float time);
        public virtual bool IsMotorbike => false;
        // Line multiplier ceiling from each premise (Hopward x3, Bellavolta x4).
        public virtual int MultiplierCap => 3;
        // Short acknowledgement when the motor accepts a prepared input; null shows nothing.
        public virtual string PreparedHint => null;
        // Teaching lines per outing; defaults to the shared lesson list.
        public virtual string[] LessonsFor(int outing) => Lessons;
    }

    public struct RiderPose
    {
        public float X, Y, VelocityY, Pitch, Compression, Trick, Lean;
        public bool Grounded, Prepared;
        public int TrickKind;
        public static RiderPose Lerp(RiderPose a, RiderPose b, float t)
        {
            var p=b; p.X=Mathf.Lerp(a.X,b.X,t); p.Y=Mathf.Lerp(a.Y,b.Y,t);
            p.Pitch=Mathf.Lerp(a.Pitch,b.Pitch,t); p.Compression=Mathf.Lerp(a.Compression,b.Compression,t);
            p.Lean=Mathf.Lerp(a.Lean,b.Lean,t); p.Trick=Mathf.Lerp(a.Trick,b.Trick,t); return p;
        }
    }

    public abstract class Motor
    {
        public RiderPose Pose, Previous, RenderPose;
        public readonly Course Course;
        protected readonly System.Action<string,int> Award;
        protected readonly System.Action<string> Stumble;
        public bool UpperDiscovered;
        public float ContactPulse;
        public abstract float Speed { get; }
        public abstract void Step(float dt, Gesture input);
        public abstract void Reset(float x);
        public abstract void CancelInput();
        protected Motor(Course c, System.Action<string,int> award, System.Action<string> stumble)
        { Course=c; Award=award; Stumble=stumble; }
        public void Interpolate(float alpha) { RenderPose=RiderPose.Lerp(Previous,Pose,alpha); }
        protected void Land(float y, bool upper)
        { Pose.Y=y; Pose.VelocityY=0; Pose.Grounded=true; ContactPulse=1; if(upper) UpperDiscovered=true; }
    }
}
