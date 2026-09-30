using System;
using Fosters.Studio;
using Fosters.Bellavolta;
class ModelChecks {
 static int failures=0,awards=0;
 static void Check(bool ok,string label){Console.WriteLine((ok?"PASS ":"FAIL ")+label);if(!ok)failures++;}
 static void Main(){
 var module=new Bellavolta();
 for(int outing=0;outing<3;outing++){
  var c=module.CreateCourse(outing);c.Validate();int stumbles=0;var m=new MopedMotor(c,(n,p)=>awards++,s=>stumbles++);m.Reset(1);
  for(int i=0;i<120*90;i++)m.Step(1f/120,default);
  Check(stumbles==0 && m.Pose.X>=450,"Outing "+outing+" neutral route completes without failures");
 }
 {var c=module.CreateCourse(0);int fail=0;var m=new MopedMotor(c,(n,p)=>awards++,s=>fail++);m.Reset(1);
 for(int i=0;i<120;i++)m.Step(1f/120,new Gesture{Held=true,HeldTime=1,BeganGrounded=true});
 float pitch=m.Pose.Pitch;for(int i=0;i<180;i++)m.Step(1f/120,default);
 Check(pitch>20 && m.Pose.Pitch<5 && awards>0 && fail==0,"Wheelie raises, release settles, award only after settlement");}
 {var c=module.CreateCourse(0);int fail=0;var m=new MopedMotor(c,(n,p)=>awards++,s=>fail++);m.Reset(122.6f);
 m.Step(1f/120,new Gesture{Up=true,BeganGrounded=true});
 for(int i=0;i<180;i++)m.Step(1f/120,default);
 Check(m.UpperDiscovered && fail==0,"Assisted pop reaches optional elevated promenade");}
 Environment.ExitCode=failures>0?1:0;
 }
}
