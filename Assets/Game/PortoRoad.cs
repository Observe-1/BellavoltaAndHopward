using System.Collections.Generic;
using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    // One endless coastal road, generated ahead of the rider in authored "moments" and pruned behind.
    // Difficulty rises with distance: faster target speed, wider gaps, more hazards, bigger drops.
    public sealed class PortoRoad : Course
    {
        public struct Prop{public float X,W,H,Base;public int Kind;}          // crates: hop over or crash
        public struct Rail{public float X0,Y0,X1,Y1;public int Kind;public float Y(float x)=>Mathf.Lerp(Y0,Y1,Mathf.InverseLerp(X0,X1,x));public float Slope=>(Y1-Y0)/(X1-X0);}
        public sealed class Lemon{public float X,Y;public bool Taken;}
        public readonly List<Prop> Props=new List<Prop>();
        public readonly List<Rail> Rails=new List<Rail>();
        public readonly List<Lemon> Lemons=new List<Lemon>();
        public readonly List<Vector2> Gaps=new List<Vector2>();                 // x ranges with open water
        public readonly int Seed;
        public float GeneratedTo;
        public bool KeepHistory;
        readonly System.Random rng;float h;bool generate;

        public const float StartSpeed=6.5f,TopSpeed=13.5f;
        // Cruise speed the road is designed around at a given distance.
        public static float TargetSpeed(float x)=>StartSpeed+(TopSpeed-StartSpeed)*(1-Mathf.Exp(-Mathf.Max(0,x)/3200f));
        public static float Difficulty(float x)=>Mathf.Clamp01(x/6000f);

        public PortoRoad(int seed,bool generated=true)
        {
            Seed=seed;rng=new System.Random(seed);Length=float.PositiveInfinity;generate=generated;
            // A calm run-up so the first seconds are about the view.
            Flat(-30,34);
        }
        float R(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
        bool Chance(float p)=>rng.NextDouble()<p;

        // ---------- building blocks (all heights absolute) ----------
        public void Flat(float x0,float len){Surfaces.Add(new Surface(x0,x0+len,h,h));GeneratedTo=x0+len;}
        void Slope(float len,float to,int shape=0,bool lip=false){Surfaces.Add(new Surface(GeneratedTo,GeneratedTo+len,h,to,false,lip,false,shape));GeneratedTo+=len;h=to;}
        void Run(float len){if(len>0)Flat(GeneratedTo,len);}
        void Gap(float w){Gaps.Add(new Vector2(GeneratedTo,GeneratedTo+w));GeneratedTo+=w;}
        void LemonArc(float x0,float y0,float v,float vy,int count,float spacing)
        {
            for(int i=0;i<count;i++){float t=i*spacing/v;Lemons.Add(new Lemon{X=x0+v*t,Y=y0+vy*t-10*t*t});}
        }
        void LemonLine(float x0,float y,int count,float spacing){for(int i=0;i<count;i++)Lemons.Add(new Lemon{X=x0+i*spacing,Y=y});}

        // Horizontal reach of a jump from a lip, landing `drop` lower, at speed v.
        public static float Reach(float v,float vy,float drop)
        {const float g=MopedMotor.Gravity;float t=(vy+Mathf.Sqrt(vy*vy+2*g*Mathf.Max(0,drop)))/g;return v*t;}

        public override void EnsureAhead(float x)
        {
            if(!generate)return;
            while(GeneratedTo<x)Moment();
            if(KeepHistory)return;
            // prune well behind the camera
            float behind=x-140;
            Surfaces.RemoveAll(s=>s.End<behind);Props.RemoveAll(p=>p.X+p.W<behind);Rails.RemoveAll(r=>r.X1<behind);
            Lemons.RemoveAll(l=>l.X<behind);Gaps.RemoveAll(g=>g.y<behind);
        }

        void Moment()
        {
            float x=GeneratedTo,d=Difficulty(x),v=TargetSpeed(x);
            // steer the road back towards sea-level bands so scenery and camera stay composed
            float pick=R(0,1);
            if(pick<.10f)Rollers(v);
            else if(pick<.28f)KickerGap(v,d);
            else if(pick<.38f)Launch(v,d);
            else if(pick<.46f)PlainGap(v,d);
            else if(pick<.58f)Crates(v,d);
            else if(pick<.72f)Drop(v,d);
            else if(pick<.86f)Bunting(v,d);
            else Promenade(v,d);
            Run(R(3,6)+(1-d)*R(2,6));
        }
        // A trick kicker with a long downhill landing: pure air time.
        void Launch(float v,float d)
        {
            Run(3);float rise=R(1.3f,1.8f),lip=h+rise;Slope(5.5f,lip,1,true);
            float vy=v*2*rise/5.5f;float x0=GeneratedTo;
            float land=Mathf.Clamp(h-R(.4f,1.2f),-1.2f,2f);
            // gentle drop-away so the landing matches the arc, then the road flattens
            Surfaces.Add(new Surface(GeneratedTo,GeneratedTo+14,lip,land));GeneratedTo+=14;h=land;
            LemonArc(x0+.5f,lip+1.1f,v,vy+MopedMotor.JumpSpeed*.7f,6,1.3f);
        }
        void Rollers(float v)
        {
            int n=Random(2,4);
            for(int i=0;i<n;i++)
            {
                float up=Mathf.Clamp(h+R(.5f,1.1f),-1f,2.4f);float len=R(6,9);
                Slope(len,up);Slope(len,Mathf.Clamp(up-R(.6f,1.3f),-1.2f,2f));
            }
            LemonLine(GeneratedTo-6,h+1.4f,3,.9f);
        }
        void KickerGap(float v,float d)
        {
            Run(4);float rise=R(1.0f,1.4f),lip=h+rise;Slope(4.5f,lip,1,true);
            float vy=v*Mathf.Min(.6f,2*rise/4.5f);
            float landH=Mathf.Clamp(lip-R(1.0f,2.0f),-1.2f,2f);
            float natural=Reach(v,vy,lip-landH),jumped=Reach(v,vy+MopedMotor.JumpSpeed,lip-landH);
            float w=Mathf.Lerp(natural*.55f,Mathf.Lerp(natural*.8f,jumped*.72f,d),Mathf.Clamp01(d*1.4f+.2f));
            w=Mathf.Clamp(w,1.8f,jumped*.78f);
            float x0=GeneratedTo;Gap(w);
            h=landH;Surfaces.Add(new Surface(GeneratedTo,GeneratedTo+7,landH+.5f,landH));GeneratedTo+=7;
            LemonArc(x0+.4f,lip+.9f,v,vy+MopedMotor.JumpSpeed*.8f,5,1.1f);
        }
        void PlainGap(float v,float d)
        {
            Run(3);float reach=Reach(v,MopedMotor.JumpSpeed,0);
            float w=Mathf.Clamp(Mathf.Lerp(reach*.35f,reach*.7f,d),1.6f,reach*.75f);
            float x0=GeneratedTo;Gap(w);Run(4);
            LemonArc(x0-v*.12f,h+.8f,v,MopedMotor.JumpSpeed*.85f,4,1.0f);
        }
        void Crates(float v,float d)
        {
            Run(5);int n=d>.4f&&Chance(.5f)?2:1;
            for(int i=0;i<n;i++)
            {
                float w=R(.7f,1.1f),hh=Chance(.35f+d*.3f)?R(.85f,.95f):R(.5f,.7f);
                Props.Add(new Prop{X=GeneratedTo,W=w,H=hh,Base=h,Kind=hh>.9f?1:0});
                LemonArc(GeneratedTo-v*.18f,h+hh+.6f,v,3.2f,3,.8f);
                Run(w+v*2*MopedMotor.JumpSpeed/MopedMotor.Gravity+R(1.5f,3.5f));
            }
        }
        void Drop(float v,float d)
        {
            Run(4);float fall=Mathf.Lerp(1.8f,3.0f,d)*R(.85f,1.1f);
            float to=h-fall;if(to<-1.3f){// climb first on a long ramp, then drop
                Slope(12,h+fall,0);to=h-fall;}
            float x0=GeneratedTo;h=to;
            Surfaces.Add(new Surface(GeneratedTo,GeneratedTo+6,to+.35f,to));GeneratedTo+=6;
            LemonArc(x0,to+fall+1.2f,v,MopedMotor.JumpSpeed*.6f,4,1.2f);
        }
        void Bunting(float v,float d)
        {
            // a festival line strung low over the road: kick up, land on the rope and grind
            Run(3);float lip=h+.7f;Slope(3.5f,lip,1,true);Slope(2.5f,h-.7f);
            float len=R(9,16)+d*6,x0=GeneratedTo-1.2f,y0=h+1.75f,y1=h+1.35f;
            Rails.Add(new Rail{X0=x0,Y0=y0,X1=x0+len,Y1=y1,Kind=0});
            LemonLine(x0+1.5f,y0+.45f,Mathf.FloorToInt(len/1.6f),1.5f);
            Run(len+2);
        }
        void Promenade(float v,float d)
        {
            // raised promenade on arcades: kick up and jump to reach it, drop off the far end for big air
            Run(3);float lip=h+1.0f;Slope(4.5f,lip,1,true);Slope(3,h-1.0f);
            float top=h+2.3f,a=GeneratedTo-1.5f,len=R(16,26);
            Surfaces.Add(new Surface(a,a+len,top,top,true));
            // the lower road carries on beneath, sometimes with a crate to make the upper line worth it
            float under=GeneratedTo;Run(len);
            if(d>.15f && Chance(.5f))Props.Add(new Prop{X=under+len*.55f,W=.9f,H=.7f,Base=h,Kind=0});
            LemonLine(a+2,top+.6f,Mathf.FloorToInt(len/1.8f),1.7f);
            if(Chance(.4f+d*.3f))Rails.Add(new Rail{X0=a+len*.3f,Y0=top+1.35f,X1=a+len*.3f+R(5,8),Y1=top+1.1f,Kind=0});
            Run(4);
        }
        int Random(int a,int b)=>rng.Next(a,b);

        // ---------- queries ----------
        public bool InGap(float x){foreach(var g in Gaps)if(x>g.x && x<g.y)return true;return false;}
        public bool Lower(float x,out float y){y=Ground(x);return y>-50;}
    }
}
