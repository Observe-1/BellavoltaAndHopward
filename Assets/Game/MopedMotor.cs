using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    public sealed class MopedMotor : Motor
    {
        public override float Speed=>5;
        const float Gravity=16,TabletopDuration=.65f;
        float pitchVelocity,wheelieTime,airtime,trickClock,heldDuration;
        float lastLip=-100;bool wheeliePending,tabletopUsed,tabletopComplete,popPending,needsRelease;float wobbleClock;
        public MopedMotor(Course c,System.Action<string,int> a,System.Action<string> s):base(c,a,s){}
        public override void Reset(float x)
        {
            Pose=new RiderPose{X=x,Y=Course.Ground(x),Grounded=true};Previous=RenderPose=Pose;
            pitchVelocity=wheelieTime=airtime=trickClock=heldDuration=0;lastLip=x-3;
            wheeliePending=tabletopUsed=tabletopComplete=popPending=needsRelease=UpperDiscovered=false;ContactPulse=0;wobbleClock=0;Pose.Trick=0;Pose.TrickKind=0;
        }
        public override void CancelInput(){heldDuration=0;needsRelease=false;}
        public override void Step(float dt,Gesture input)
        {
            ContactPulse=Mathf.Max(0,ContactPulse-dt*7);
            float beforeX=Pose.X;Pose.X+=Speed*dt;
            if(Pose.Grounded)
            {
                if(!input.Held)needsRelease=false;
                bool hold=input.Held && input.BeganGrounded && input.HeldTime>.12f && !input.Up && !needsRelease;
                heldDuration=hold?heldDuration+dt:0;
                float target=0;
                if(Course.Support(Pose.X,Pose.Y+.3f,out Surface surface))
                {
                    Pose.Y=surface.Height(Pose.X);
                    target=Mathf.Atan2(surface.Height(Pose.X+.94f)-Pose.Y,.94f)*Mathf.Rad2Deg;
                    if(hold){wheelieTime+=dt;target+=Mathf.Min(58,25+heldDuration*12);wheeliePending|=wheelieTime>.35f;}
                    else if(Pose.Pitch<target+5 && wheeliePending){Award("Wheelie",80);wheelieTime=0;wheeliePending=false;}
                    else if(!hold && !wheeliePending)wheelieTime=0;
                    if(input.Up && input.BeganGrounded && Course.NearLip(Pose.X))Launch(7.8f,true);
                    else if(surface.Lip && Pose.X+Speed*dt>=surface.End && lastLip<surface.End-.1f)
                    {lastLip=surface.End;Launch(4.0f,false);}
                    if(surface.Upper)UpperDiscovered=true;
                }
                else Launch(0,false);
                pitchVelocity+=(target-Pose.Pitch)*80*dt-pitchVelocity*15*dt;
                Pose.Pitch+=pitchVelocity*dt;
                // Recoverable mistakes: a wobble drops the front wheel and clears the unbanked line.
                if(Pose.Pitch>49){Slip("Wobble. Release a little sooner to keep your balance.");}
                foreach(float arch in Course.Arches)
                    if(Pose.X+.94f>=arch && Pose.X<arch+2.2f && Pose.Pitch>14){Slip("Wobble. Settle both wheels before the low arch.");break;}
            }
            else
            {
                airtime+=dt;
                if(input.Up && !input.BeganGrounded && !tabletopUsed && airtime>.10f)
                {tabletopUsed=true;trickClock=TabletopDuration;Pose.TrickKind=1;}
                if(trickClock>0){trickClock=Mathf.Max(0,trickClock-dt);Pose.Trick=Mathf.Sin((1-trickClock/TabletopDuration)*Mathf.PI);if(trickClock==0)tabletopComplete=true;}
                else Pose.Trick=0;
                float oldY=Pose.Y;Pose.VelocityY-=Gravity*dt;float next=Pose.Y+Pose.VelocityY*dt;
                float desired=Mathf.Clamp(Pose.VelocityY*2.4f,-12,18);Pose.Pitch=Mathf.Lerp(Pose.Pitch,desired,1-Mathf.Exp(-dt*5));
                if(Pose.VelocityY<=0 && Course.Sweep(Pose.X,oldY,next,out float y,out Surface support))
                {
                    bool late=trickClock>.055f;
                    if(late && trickClock>TabletopDuration*.5f){Fail("The tabletop needed more air. Restore the wheels before landing.");return;}
                    Land(y,support.Upper);Pose.Compression=.16f;pitchVelocity=0;
                    if(late){tabletopComplete=false;popPending=false;Slip("Wobble. Start the tabletop a little earlier.");}
                    if(tabletopComplete)Award("Tabletop",120);
                    if(popPending && support.Upper)Award("Promenade pop",60);
                    tabletopUsed=tabletopComplete=popPending=false;Pose.Trick=0;
                }
                else Pose.Y=next;
                if(Pose.Y<-7){Fail("That line missed the road. Try the lower promenade.");return;}
            }
            Pose.Compression=Mathf.MoveTowards(Pose.Compression,0,dt*.65f);
            wobbleClock=Mathf.Max(0,wobbleClock-dt);
            Pose.Lean=Mathf.Lerp(Pose.Lean,Pose.Pitch/50+(wobbleClock>0?Mathf.Sin(wobbleClock*34)*.35f*wobbleClock/.6f:0),1-Mathf.Exp(-dt*8));
        }
        void Launch(float impulse,bool pop)
        {Pose.Grounded=false;Pose.VelocityY=impulse;airtime=0;heldDuration=0;tabletopUsed=tabletopComplete=false;trickClock=0;popPending=pop;wheelieTime=0;wheeliePending=false;}
        void Fail(string why){Stumble(why);}
        void Slip(string why)
        {
            Wobble(why);wobbleClock=.6f;needsRelease=true;heldDuration=0;wheelieTime=0;wheeliePending=false;
            Pose.Pitch=Mathf.Min(Pose.Pitch,8);pitchVelocity=-40;
        }
    }
}
