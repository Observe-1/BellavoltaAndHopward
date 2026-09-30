// Deterministic model checks for Bellavolta's endless ride. Compiled outside Unity against UnityEngine.CoreModule.
using System;
using System.Collections.Generic;
using Fosters.Studio;
using Fosters.Bellavolta;
class ModelChecks
{
    static int failures;
    static void Check(bool ok,string label){Console.WriteLine((ok?"PASS ":"FAIL ")+label);if(!ok)failures++;}
    const float Dt=1f/120;

    // A simple "reads the road" bot: hops at lips, before gaps and before crates. No flips.
    static bool BotWantsJump(MopedMotor m,PortoRoad road)
    {
        float x=m.Pose.X,v=m.Speed,front=x+MopedMotor.Base;
        foreach(var s in road.Surfaces)if(s.Lip && !s.Upper && x>=s.End-.3f && x<=s.End)return true;
        foreach(var g in road.Gaps)if(g.x-front>0 && g.x-front<v*Dt*3+.05f)return true;
        foreach(var p in road.Props)if(p.X-front>0 && p.X-front<v*.16f+.25f)return true;
        return false;
    }
    static (float dist,string cause,int awards,int lemons,Dictionary<string,int> names) Ride(int seed,float until,Func<MopedMotor,PortoRoad,Gesture> pilot)
    {
        var road=new PortoRoad(seed);string cause=null;int awards=0;var names=new Dictionary<string,int>();
        MopedMotor m=null;
        m=new MopedMotor(road,(n,p)=>{awards++;names.TryGetValue(n,out int c);names[n]=c+1;},s=>cause=s);m.Reset(1);
        while(m.Pose.X<until && cause==null){road.EnsureAhead(m.Pose.X+60);m.Step(Dt,pilot(m,road));}
        return (m.Pose.X,cause,awards,m.Pickups,names);
    }
    static Func<MopedMotor,PortoRoad,Gesture> Bot()
    {
        bool held=false;
        return (m,r)=>{bool want=(m.Pose.Grounded)&&BotWantsJump(m,r);var g=new Gesture{Pressed=want&&!held,Held=want,Released=!want&&held};held=want;return g;};
    }

    static void Main()
    {
        // ---- the road ----
        {
            var a=new PortoRoad(42);a.EnsureAhead(2000);var b=new PortoRoad(42);b.EnsureAhead(2000);
            Check(a.Surfaces.Count==b.Surfaces.Count && a.Props.Count==b.Props.Count && a.Gaps.Count==b.Gaps.Count,"Same seed, same coastline");
            var c=new PortoRoad(7);c.EnsureAhead(2000);
            Check(c.Surfaces.Count!=a.Surfaces.Count||c.Props.Count!=a.Props.Count,"Different seeds give different roads");
            Check(PortoRoad.TargetSpeed(0)<7 && PortoRoad.TargetSpeed(3000)>10 && PortoRoad.TargetSpeed(12000)<=PortoRoad.TopSpeed,"The ride speeds up slowly but surely ("+PortoRoad.TargetSpeed(0).ToString("0.0")+" to "+PortoRoad.TargetSpeed(3000).ToString("0.0")+" to "+PortoRoad.TargetSpeed(9000).ToString("0.0")+" u/s)");
            var d=new PortoRoad(3){KeepHistory=true};d.EnsureAhead(9000);int gaps=0,props=0,rails=0;foreach(var g in d.Gaps)if(g.x>6000)gaps++;foreach(var p in d.Props)if(p.X>6000)props++;foreach(var r in d.Rails)if(r.X0>6000)rails++;
            Check(gaps>0 && props>0 && rails>0,"Far down the road there are still gaps, crates and bunting ("+gaps+"/"+props+"/"+rails+" past 6 km)");
        }
        // ---- survivability: every hazard is clearable by a player who reads the road ----
        int seedsOk=0;float worst=1e9f;string worstCause="";int lemons=0,grinds=0;
        for(int seed=1;seed<=8;seed++)
        {
            var r=Ride(seed,4000,Bot());if(r.cause==null)seedsOk++;else if(r.dist<worst){worst=r.dist;worstCause=r.cause;}
            lemons+=r.lemons;if(r.names.TryGetValue("Grind",out int gr))grinds+=gr;
        }
        Check(seedsOk==8,"A road-reading rider survives 4 km on 8 random coastlines"+(seedsOk<8?" (worst: "+worst.ToString("0")+" m, "+worstCause+")":""));
        Check(lemons>50,"Lemons sit on the natural lines ("+lemons+" collected)");
        Check(grinds>0,"Kicking off bunting ramps lands on the rope and grinds ("+grinds+" grinds)");
        {
            // How often does the road offer air long enough for a backflip (~1 s)?
            var road=new PortoRoad(5);string cause=null;var m=new MopedMotor(road,(n,p)=>{},s2=>cause=s2);m.Reset(1);var bot=Bot();
            int big=0,airs=0;float air=0;bool was=true;
            while(m.Pose.X<3000 && cause==null){road.EnsureAhead(m.Pose.X+60);m.Step(Dt,bot(m,road));bool g=m.Pose.Grounded||m.Pose.Mode==1;if(!g)air+=Dt;if(g&&!was){airs++;if(air>.95f)big++;air=0;}was=g;}
            Check(big>=25,"Flip-sized air comes often: "+big+" of "+airs+" airs over 3 km last longer than 0.95 s");
        }
        {
            int crashed=0;for(int seed=1;seed<=4;seed++){var r=Ride(seed,4000,(m,rd)=>default);if(r.cause!=null && r.dist<600)crashed++;}
            Check(crashed==4,"Doing nothing ends the run early: the road demands input");
        }
        // ---- controls on a test ledge: 3 m drop onto flat road ----
        float ledge=3;
        PortoRoad Ledge(){var r=new PortoRoad(1,false);r.Surfaces.Clear();r.Surfaces.Add(new Surface(-10,20,ledge,ledge));r.Surfaces.Add(new Surface(20,200,0,0));return r;}
        (string cause,List<string> awards,float pitch) Hold(float holdTime,bool tap=false,float tapAt=.2f)
        {
            var r=Ledge();string cause=null;var aw=new List<string>();var m=new MopedMotor(r,(n,p)=>aw.Add(n),s=>cause=s);m.Reset(12);
            while(m.Pose.X<19.2f)m.Step(Dt,default);
            m.Step(Dt,new Gesture{Pressed=true,Held=true});float t=0;
            while(!m.Pose.Grounded && cause==null && t<4)
            {
                t+=Dt;bool held=!tap && t<holdTime;
                var g=new Gesture{Held=held,Released=!tap && Math.Abs(t-holdTime)<Dt*.5f};
                if(tap && t>=tapAt-Dt*.5f && t<tapAt+.06f)g=new Gesture{Pressed=Math.Abs(t-tapAt)<Dt*.5f,Held=true};
                if(tap && Math.Abs(t-tapAt-.06f)<Dt*.5f)g=new Gesture{Released=true};
                m.Step(Dt,g);
            }
            for(int i=0;i<200 && cause==null;i++)m.Step(Dt,default);
            return (cause,aw,m.Pose.Pitch);
        }
        {
            var quick=Hold(.05f);Check(quick.cause==null && !quick.awards.Contains("Backflip"),"A quick tap on the ground is just a hop");
            bool flipped=false;float good=0;
            for(float h=.5f;h<1.2f;h+=.02f){var r=Hold(h);if(r.cause==null && r.awards.Contains("Backflip")){flipped=true;good=h;break;}}
            Check(flipped,"Holding in the air rotates a full backflip that lands clean (hold "+good.ToString("0.00")+" s)");
            ledge=7;var over=Hold(3f);ledge=3;Check(over.cause!=null,"Holding too long over-rotates and ends the run ("+over.cause+")");
            var half=Hold(.62f);Check(half.cause!=null,"Letting go half-way round lands on the head and ends the run");
            var trick=Hold(0,true,.2f);Check(trick.cause==null && (trick.awards.Contains("Tabletop")||trick.awards.Contains("No-hander")),"A tap in the air performs a trick, credited on landing");
            var late=Hold(0,true,.82f);Check(late.cause!=null,"A trick started too late is unfinished at landing and ends the run");
        }
        {
            // crate without a hop
            var r=new PortoRoad(1,false);r.Surfaces.Clear();r.Surfaces.Add(new Surface(-10,200,0,0));r.Props.Add(new PortoRoad.Prop{X=20,W=.9f,H=.7f,Base=0});
            string cause=null;var m=new MopedMotor(r,(n,p)=>{},s=>cause=s);m.Reset(5);for(int i=0;i<900 && cause==null;i++)m.Step(Dt,default);
            Check(cause!=null,"Riding into a crate ends the run");
            cause=null;m=new MopedMotor(r,(n,p)=>{},s=>cause=s);m.Reset(5);bool jumped=false;
            for(int i=0;i<900 && cause==null;i++){bool j=!jumped && 20-(m.Pose.X+MopedMotor.Base)<m.Speed*.16f+.25f;if(j)jumped=true;m.Step(Dt,new Gesture{Pressed=j,Held=j});}
            Check(cause==null,"A timed hop clears a crate");
        }
        {
            // a clean landing hands a speed boost, a combo builds
            var r=Hold(.88f);
            Check(r.awards.Count>0,"Landing tricks awards points");
            var s=new LineScore(5);s.Add("Backflip",150);s.Add("Grind",80);s.Add("Tabletop",100);
            Check(s.Multiplier==3,"Different tricks in one line multiply (x"+s.Multiplier+")");
        }
        Environment.ExitCode=failures>0?1:0;
    }
}
