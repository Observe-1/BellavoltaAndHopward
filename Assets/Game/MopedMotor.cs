using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    // Endless-run moped. One finger:
    //   press on the ground (or a rope)   -> hop
    //   keep holding in the air           -> backflip rotation; release to stop
    //   quick tap in the air              -> trick (tabletop, then no-hander); finish it before landing
    // Land roughly level with the road to ride away. Land nose-high for a wheelie. Over-rotate,
    // land on the nose, hit a crate or drop into the harbour and the run ends.
    // Clean tricks chain into a combo and give a short speed boost. The road speeds up with distance.
    public sealed class MopedMotor : Motor
    {
        public const float Gravity=20f,JumpSpeed=7.6f,Wheel=.225f,Base=.94f;
        const float SpinRate=500f,SpinAccel=2600f,SpinBrake=3400f,HoldToSpin=.13f,TapMax=.16f,Coyote=.09f;
        const float TrickTime=.5f;

        public enum Mode{Ride=0,Grind=1,Crash=2}
        float v,pressTime,coyote,angVel,spun,airTime,trickClock,wheelie,boost,groundQuiet,railTime,crashClock,landSquash;
        bool pressing,pressFromGround,trickDone,crashReported;int trickKind,trickCount,chain;bool grinding;PortoRoad.Rail rail;string crashCause;
        float crashVx,crashVy;
        readonly PortoRoad road;

        public MopedMotor(Course c,System.Action<string,int> a,System.Action<string> s):base(c,a,s){road=c as PortoRoad;}
        public override float Speed=>v;
        public float Boost=>boost;
        public bool Crashed=>Pose.Mode==(int)Mode.Crash;
        public float Spun=>spun;
        public int Chain=>chain;

        public override void Reset(float x)
        {
            float g=Course.Ground(x);if(g<-50)g=0;
            Pose=new RiderPose{X=x,Y=g,Grounded=true};Previous=RenderPose=Pose;
            v=PortoRoad.TargetSpeed(x);pressTime=coyote=angVel=spun=airTime=trickClock=wheelie=boost=groundQuiet=railTime=crashClock=landSquash=0;
            pressing=pressFromGround=trickDone=crashReported=false;trickKind=0;trickCount=0;chain=0;grinding=false;crashCause=null;
            ContactPulse=0;JumpPulse=0;Pickups=0;UpperDiscovered=false;
        }
        public override void CancelInput(){pressing=false;pressTime=0;}

        static float Wrap(float a){a=Mathf.Repeat(a+180,360)-180;return a;}
        // Chassis angle from the road under the machine. Only surfaces close to the rear contact count,
        // so a front wheel hanging over an edge or a gap never pulls the nose down.
        float SlopeDeg(float x,float y,out bool found)
        {
            found=Course.Support(x,y+.3f,out Surface s);
            if(!found)return 0;
            float hr=s.Height(x),mid=x+Base*.5f,slope=s.Slope(x);
            if(Course.Support(mid,hr+.35f,out Surface m) && Mathf.Abs(m.Height(mid)-(hr+slope*Base*.5f))<.25f)slope=(m.Height(mid)-hr)/(Base*.5f);
            return Mathf.Atan(slope)*Mathf.Rad2Deg;
        }
        public override void Step(float dt,Gesture input)
        {
            ContactPulse=Mathf.Max(0,ContactPulse-dt*6);JumpPulse=Mathf.Max(0,JumpPulse-dt*6);
            landSquash=Mathf.MoveTowards(landSquash,0,dt*.9f);
            if(Crashed){CrashStep(dt);return;}
            boost=Mathf.Max(0,boost-dt);

            // ---- input ----
            bool onRail=grinding;
            if(input.Pressed)
            {
                pressing=true;pressTime=0;pressFromGround=Pose.Grounded||onRail||coyote>0;
                if(pressFromGround)Jump();
            }
            if(pressing)pressTime+=dt;
            if(input.Released && pressing)
            {
                if(!pressFromGround && pressTime<TapMax && !Pose.Grounded && !grinding)StartTrick();
                pressing=false;
            }
            if(!input.Held && !input.Pressed)pressing=false;
            if(input.Up && !Pose.Grounded && !grinding)StartTrick();

            if(grinding)RailStep(dt);
            else if(Pose.Grounded)GroundStep(dt);
            else AirStep(dt);
            if(Crashed)return;

            Collect();
            Pose.Compression=landSquash;
            Pose.Trick=trickClock>0?Mathf.Sin((1-trickClock/TrickTime)*Mathf.PI):0;
            Pose.VelocityY=Pose.Grounded?0:Pose.VelocityY;
        }

        void Jump()
        {
            float slope=0;
            if(grinding)slope=rail.Slope;
            else if(Course.Support(Pose.X,Pose.Y+.3f,out Surface s))slope=s.Slope(Pose.X);
            Pose.VelocityY=Mathf.Max(0,v*slope)+JumpSpeed;
            if(grinding)EndGrind();
            Pose.Grounded=false;coyote=0;airTime=0;spun=0;angVel=0;trickDone=false;trickClock=0;wheelie=0;
            JumpPulse=1;
        }

        void GroundStep(float dt)
        {
            groundQuiet+=dt;if(groundQuiet>2.5f)chain=0;
            float slope=SlopeDeg(Pose.X,Pose.Y,out bool found);
            float target=PortoRoad.TargetSpeed(Pose.X)+(boost>0?2.2f:0);
            float accel=(target-v)*(v<target?1.1f:.6f)-Gravity*Mathf.Sin(slope*Mathf.Deg2Rad)*.3f;
            v=Mathf.Clamp(v+accel*dt,4.5f,19f);
            float oldX=Pose.X,oldY=Pose.Y;
            float groundVy=0;if(Course.Support(oldX,oldY+.3f,out Surface cur))groundVy=v*cur.Slope(oldX);
            Pose.X+=v*dt;
            // Stay on the surface unless it falls away faster than gravity would (lips, crests, edges).
            if(Course.Support(Pose.X,oldY+.3f+Mathf.Max(0,groundVy*dt),out Surface next))
            {
                float y=next.Height(Pose.X),ballistic=oldY+groundVy*dt-.5f*Gravity*dt*dt;
                if(y<ballistic-.006f){Launch(groundVy);return;}
                Pose.Y=y;
            }
            else{Launch(groundVy);return;}
            float nowSlope=SlopeDeg(Pose.X,Pose.Y,out _);
            if(wheelie>0)
            {
                wheelie-=dt;float lift=Mathf.Clamp01(wheelie/.35f)*22f;
                Pose.Pitch=Mathf.Lerp(Pose.Pitch,nowSlope+lift,1-Mathf.Exp(-dt*6));
                if(wheelie<=0){Award("Wheelie",60);chain++;groundQuiet=0;}
            }
            else Pose.Pitch=Mathf.Lerp(Pose.Pitch,nowSlope,1-Mathf.Exp(-dt*18));
            Pose.Lean=Mathf.Lerp(Pose.Lean,0,1-Mathf.Exp(-dt*6));
            if(HitProp())return;
        }
        void Launch(float vy)
        {
            Pose.Grounded=false;Pose.VelocityY=vy;coyote=Coyote;airTime=0;spun=0;angVel=0;trickDone=false;trickClock=0;wheelie=0;
        }

        void AirStep(float dt)
        {
            coyote=Mathf.Max(0,coyote-dt);airTime+=dt;
            // backflip: rotate while the press is held (after a short delay, so a tap stays a tap)
            bool spinning=pressing && pressTime>HoldToSpin;
            angVel=spinning?Mathf.MoveTowards(angVel,SpinRate,SpinAccel*dt):Mathf.MoveTowards(angVel,0,SpinBrake*dt);
            Pose.Pitch+=angVel*dt;spun+=angVel*dt;
            if(trickClock>0){trickClock=Mathf.Max(0,trickClock-dt);if(trickClock==0)trickDone=true;}
            Pose.Lean=Mathf.Lerp(Pose.Lean,Mathf.Clamp(Pose.VelocityY*.04f,-.3f,.3f),1-Mathf.Exp(-dt*4));
            float oldY=Pose.Y;
            Pose.VelocityY-=Gravity*dt;
            v=Mathf.Max(4.5f,v-v*.02f*dt);
            Pose.X+=v*dt;
            float newY=Pose.Y+Pose.VelocityY*dt;
            // ropes: land on top while descending, nose roughly along the line
            if(road!=null && Pose.VelocityY<=0)
                for(int i=0;i<road.Rails.Count;i++)
                {
                    var r=road.Rails[i];if(Pose.X<r.X0||Pose.X>r.X1-.3f)continue;
                    float ry=r.Y(Pose.X);
                    if(oldY>=ry-.02f && newY<=ry)
                    {
                        float diff=Wrap(Pose.Pitch-Mathf.Atan(r.Slope)*Mathf.Rad2Deg);
                        if(Mathf.Abs(diff)<40 && trickClock<=TrickTime*.35f){StartGrind(r,ry);return;}
                    }
                }
            if(Pose.VelocityY<=0 && Course.Sweep(Pose.X,oldY,newY,out float y,out Surface s)){Touchdown(y,s);return;}
            Pose.Y=newY;
            if(Pose.Y<-4.2f){Crash("Into the harbour");return;}
            HitProp();
        }

        void Touchdown(float y,Surface s)
        {
            Pose.Y=y;
            float slope=Mathf.Atan(s.Slope(Pose.X))*Mathf.Rad2Deg;
            float diff=Wrap(Pose.Pitch-slope);
            int flips=Mathf.FloorToInt((Mathf.Abs(spun)+50)/360f);
            if(trickClock>TrickTime*.35f){Crash("Finish the trick before landing");return;}
            if(diff>55){Crash("Over-rotated");return;}
            if(diff<-32){Crash("Nose first");return;}
            Pose.Grounded=true;Pose.VelocityY=0;ContactPulse=1;landSquash=Mathf.Clamp(.08f+airTime*.08f,.08f,.2f);
            if(s.Upper)UpperDiscovered=true;
            bool clean=false;
            if(flips>0){Award(flips==1?"Backflip":flips==2?"Double backflip":"Triple backflip",flips==1?150:flips==2?400:800);clean=true;}
            if(trickDone){Award(trickKind==1?"Tabletop":"No-hander",100);clean=true;}
            if(airTime>1.25f && !clean){Award("Big air",40);clean=true;}
            if(Mathf.Abs(diff)<8 && (flips>0||trickDone)){Award("Perfect landing",50);v+=.6f;}
            if(diff>16){wheelie=.7f;Pose.Pitch=slope+Mathf.Min(diff,30);}else Pose.Pitch=slope+diff*.3f;
            if(clean){chain++;groundQuiet=0;boost=Mathf.Min(4.5f,1.6f+.5f*chain);}
            spun=0;angVel=0;trickClock=0;trickDone=false;airTime=0;
        }

        void StartTrick()
        {
            if(trickClock>0||trickDone||airTime<.08f)return;
            trickKind=trickCount%2==0?1:2;trickCount++;Pose.TrickKind=trickKind;trickClock=TrickTime;
        }

        void StartGrind(PortoRoad.Rail r0,float y)
        {
            rail=r0;grinding=true;railTime=0;Pose.Y=y;Pose.VelocityY=0;Pose.Mode=(int)Mode.Grind;ContactPulse=1;
            if(spun>300){int flips=Mathf.FloorToInt((spun+50)/360f);Award(flips==1?"Backflip":"Double backflip",flips==1?150:400);chain++;}
            if(trickDone){Award(trickKind==1?"Tabletop":"No-hander",100);chain++;}
            spun=0;angVel=0;trickDone=false;trickClock=0;
            Pose.Pitch=Mathf.Atan(rail.Slope)*Mathf.Rad2Deg;
        }
        void RailStep(float dt)
        {
            var r=rail;railTime+=dt;
            Pose.X+=v*dt;Pose.ModeTime=railTime;
            if(Pose.X>=r.X1){float vy=v*r.Slope;EndGrind();Launch(vy);return;}
            Pose.Y=r.Y(Pose.X);Pose.Pitch=Mathf.Atan(r.Slope)*Mathf.Rad2Deg;
        }
        void EndGrind()
        {
            if(!grinding)return;
            int pts=40+Mathf.RoundToInt(railTime*80);Award("Grind",pts);chain++;groundQuiet=0;boost=Mathf.Min(4.5f,1.6f+.5f*chain);
            grinding=false;Pose.Mode=(int)Mode.Ride;Pose.ModeTime=0;
        }

        bool HitProp()
        {
            if(road==null)return false;
            float x0=Pose.X-.25f,x1=Pose.X+Base+.25f;
            foreach(var p in road.Props)
            {
                if(x1<p.X||x0>p.X+p.W)continue;
                float bottom=Pose.Y+Mathf.Min(0,Mathf.Sin(Pose.Pitch*Mathf.Deg2Rad))*.3f;
                if(bottom<p.Base+p.H-.04f){Crash(p.Kind==1?"Clipped the lemon crates":"Hit a crate");return true;}
            }
            return false;
        }
        void Collect()
        {
            if(road==null)return;
            Vector2 c=new Vector2(Pose.X+.45f,Pose.Y+.7f);
            foreach(var l in road.Lemons)
            {
                if(l.Taken||Mathf.Abs(l.X-c.x)>1)continue;
                if((new Vector2(l.X,l.Y)-c).sqrMagnitude<.62f*.62f){l.Taken=true;Pickups++;}
            }
        }

        void Crash(string cause)
        {
            grinding=false;
            crashCause=cause;crashClock=0;crashReported=false;Pose.Mode=(int)Mode.Crash;Pose.ModeTime=0;
            crashVx=v*.55f;crashVy=Mathf.Max(2.5f,Pose.VelocityY*.3f+3f);Pose.Grounded=false;
        }
        void CrashStep(float dt)
        {
            crashClock+=dt;Pose.ModeTime=crashClock;
            // the machine tumbles on a short arc and comes to rest; the rider is thrown clear (drawn by the rig)
            Pose.X+=crashVx*dt;crashVx=Mathf.MoveTowards(crashVx,0,dt*6);
            float ground=Course.Ground(Pose.X);bool water=ground<-50;
            crashVy-=Gravity*dt;float y=Pose.Y+crashVy*dt;
            float floor=water?-4.5f:ground;
            if(y<=floor){y=floor;crashVy=-crashVy*.25f;}
            Pose.Y=y;Pose.Pitch+=(water?-160f:-300f)*dt*Mathf.Clamp01(1-crashClock*.7f);
            if(crashClock>1.15f && !crashReported){crashReported=true;Stumble(crashCause);}
        }
    }
}
