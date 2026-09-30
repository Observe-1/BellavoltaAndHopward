using System.Collections.Generic;
using UnityEngine;

namespace Fosters.Studio
{
    [System.Serializable]
    public struct Surface
    {
        public float Start, End, A, B;
        public bool Upper, Lip, Precision;
        public Surface(float start,float end,float a,float b,bool upper=false,bool lip=false,bool precision=false)
        { Start=start;End=end;A=a;B=b;Upper=upper;Lip=lip;Precision=precision; }
        public float Height(float x) { float t=Mathf.Clamp01((x-Start)/(End-Start)); return Mathf.Lerp(A,B,t*t*(3-2*t)); }
        public float Slope(float x) { float t=Mathf.Clamp01((x-Start)/(End-Start)); return (B-A)*6*t*(1-t)/(End-Start); }
    }
    public sealed class Course
    {
        public readonly List<Surface> Surfaces=new List<Surface>();
        public readonly List<float> Arches=new List<float>();
        public float Length;
        public float Ground(float x)
        { foreach(var s in Surfaces) if(!s.Upper && x>=s.Start && x<=s.End) return s.Height(x); return -100; }
        public bool Support(float x,float reference,out Surface result)
        {
            bool found=false; float best=-100; result=default;
            foreach(var s in Surfaces) if(x>=s.Start && x<=s.End)
            { float h=s.Height(x); if(h<=reference+.18f && h>best){best=h;result=s;found=true;} }
            return found;
        }
        // Swept contact: evaluate every surface at the new tip position, highest crossed support first.
        public bool Sweep(float x,float oldY,float newY,out float y,out Surface result)
        {
            y=-100; result=default; bool found=false;
            foreach(var s in Surfaces) if(x>=s.Start && x<=s.End)
            { float h=s.Height(x); if(oldY>=h-.025f && newY<=h && h>y){y=h;result=s;found=true;} }
            return found;
        }
        public bool NearLip(float x)
        { foreach(var s in Surfaces) if(s.Lip && x>=s.End-.9f && x<=s.End+.2f)return true; return false; }
        public float Checkpoint(float x) { return Mathf.Max(1,Mathf.Floor((x-2)/60)*60+1); }
        public void Validate()
        {
            if(Length<=0 || Surfaces.Count==0) throw new System.InvalidOperationException("Empty course");
            foreach(var s in Surfaces) if(s.End<=s.Start || float.IsNaN(s.A+s.B)) throw new System.InvalidOperationException("Invalid surface");
            for(float x=1;x<Length;x+=60) if(Ground(x)<-50) throw new System.InvalidOperationException("Checkpoint lacks ground");
        }
    }
}

