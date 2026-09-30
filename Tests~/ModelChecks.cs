// Deterministic model checks for Bellavolta. Compiled outside Unity against UnityEngine.CoreModule.
using System;
using System.Collections.Generic;
using Fosters.Studio;
using Fosters.Bellavolta;
class ModelChecks
{
    static int failures;
    static void Check(bool ok,string label){Console.WriteLine((ok?"PASS ":"FAIL ")+label);if(!ok)failures++;}
    const float Dt=1f/120;
    static Gesture Hold=>new Gesture{Held=true,HeldTime=1,BeganGrounded=true};
    static void Main()
    {
        var module=new Bellavolta();
        for(int outing=0;outing<3;outing++)
        {
            var c=module.CreateCourse(outing);c.Validate();int stumbles=0;var m=new MopedMotor(c,(n,p)=>{},s=>stumbles++);m.Reset(1);
            for(int i=0;i<120*95 && m.Pose.X<c.Length;i++)m.Step(Dt,default);
            Check(stumbles==0 && m.Pose.X>=c.Length,"Outing "+outing+" neutral route completes without failures (~"+(c.Length/m.Speed).ToString("0")+" s)");
        }
        {
            var c=module.CreateCourse(0);var awards=new List<string>();int fail=0;var m=new MopedMotor(c,(n,p)=>awards.Add(n),s=>fail++);m.Reset(1);
            for(int i=0;i<120;i++)m.Step(Dt,Hold);
            float pitch=m.Pose.Pitch;Check(awards.Count==0,"No wheelie credit while still held");
            for(int i=0;i<180;i++)m.Step(Dt,default);
            Check(pitch>20 && m.Pose.Pitch<5 && awards.Contains("Wheelie") && fail==0,"Wheelie raises, release settles, award only after settlement");
        }
        {
            var c=module.CreateCourse(0);int fail=0,wobbles=0;var awards=new List<string>();
            var m=new MopedMotor(c,(n,p)=>awards.Add(n),s=>fail++);m.Wobbled=w=>wobbles++;m.Reset(1);
            for(int i=0;i<120*4;i++)m.Step(Dt,Hold);
            Check(wobbles>=1 && fail==0 && m.Pose.Pitch<30,"Holding too long wobbles back to two wheels instead of ending the run");
            Check(!awards.Contains("Wheelie"),"An over-balanced wheelie earns nothing");
            for(int i=0;i<120;i++)m.Step(Dt,Hold);
            Check(m.Pose.Pitch<20,"After a wobble the same hold cannot relift; a fresh touch is needed");
        }
        {
            var c=module.CreateCourse(0);int fail=0;var m=new MopedMotor(c,(n,p)=>{},s=>fail++);m.Reset(122.6f);
            m.Step(Dt,new Gesture{Up=true,BeganGrounded=true});
            for(int i=0;i<180;i++)m.Step(Dt,default);
            Check(m.UpperDiscovered && fail==0,"Assisted pop reaches optional elevated promenade");
        }
        {
            var c=module.CreateCourse(0);int fail=0;var awards=new List<string>();var m=new MopedMotor(c,(n,p)=>awards.Add(n),s=>fail++);m.Reset(122.6f);
            m.Step(Dt,new Gesture{Up=true,BeganGrounded=true});
            for(int i=0;i<14;i++)m.Step(Dt,default);
            m.Step(Dt,new Gesture{Up=true,BeganGrounded=false});
            Check(awards.Count==0,"Tabletop does not award in the air");
            for(int i=0;i<240 && !awards.Contains("Tabletop");i++)m.Step(Dt,default);
            Check(awards.Contains("Tabletop") && fail==0,"Early tabletop after a pop lands and scores");
        }
        {
            var c=module.CreateCourse(0);var m=new MopedMotor(c,(n,p)=>{},s=>{});m.Reset(1);
            m.Step(Dt,new Gesture{Up=true,BeganGrounded=true});
            Check(m.Pose.Trick==0 && m.Pose.Grounded,"A grounded flick away from a lip is not an aerial trick");
        }
        {
            var s=new LineScore(4);s.Add("Wheelie",80);s.Add("Promenade pop",60);s.Add("Tabletop",120);s.Add("Other",1);s.Add("Fifth",1);
            Check(s.Multiplier==4,"Line multiplier caps at x4");
            var d=new LineScore(4);d.Add("Wheelie",80);d.Add("Wheelie",80);Check(d.Pending<160,"Repeated wheelies diminish");
        }
        Environment.ExitCode=failures>0?1:0;
    }
}
