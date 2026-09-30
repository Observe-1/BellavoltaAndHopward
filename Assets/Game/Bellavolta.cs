using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    public sealed class Bellavolta : GameModule
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){new GameObject("Bellavolta").AddComponent<StudioApp>().Initialize(new Bellavolta());}
        public override string Title=>"Bellavolta";
        public override string Slug=>"bellavolta";
        public override string Place=>"Porto Chiaro · Italian coast";
        public override string Tagline=>"Take the long way around.";
        public override string[] Outings=>new[]{"The harbour road","The planted promenade","Evening on the sea wall"};
        public override string[] Lessons=>new[]{"Hold to lift the front wheel. Release to settle.","Keep a little balance. Long holds can tip the line.","Flick up at a pale road lip for the higher path.","Start a fresh upward flick in the air for a tabletop."};
        public override string PrimaryControl=>"Wheelie · hold";
        public override string SecondaryControl=>"Pop / tabletop";
        public override bool IsMotorbike=>true;
        public override int MultiplierCap=>4;
        public override Color[] Palette(bool dusk)
        {
            string[] colors=dusk?new[]{"C99483","F1D3AB","9292AD","AF8E96","70929F","BE8A76","423C50","C77D66"}:new[]{"EAB991","F4DDB1","A7A4B8","B8A0A1","76A2AD","D19B83","493F54","C77D66"};
            var p=new Color[8];for(int i=0;i<8;i++)ColorUtility.TryParseHtmlString("#"+colors[i],out p[i]);return p;
        }
        public override Course CreateCourse(int outing)
        {
            var c=new Course{Length=450};
            for(int i=0;i<6;i++)
            {
                float x=i*75;
                c.Surfaces.Add(new Surface(x,x+24,0,0));
                c.Surfaces.Add(new Surface(x+24,x+36,0,-.35f));
                c.Surfaces.Add(new Surface(x+36,x+42,-.35f,0));
                c.Surfaces.Add(new Surface(x+42,x+48,0,1,false,true));
                c.Surfaces.Add(new Surface(x+48,x+57,1,0));
                c.Surfaces.Add(new Surface(x+57,x+75,0,0));
                if(i==1 || (outing>0 && i==3) || (outing>1 && i==4))
                {c.Surfaces.Add(new Surface(x+51,x+62,1.85f,1.85f,true));c.Surfaces.Add(new Surface(x+62,x+72,1.85f,0,true));}
                if(i==2 || i==4)c.Arches.Add(x+20);
            }
            return c;
        }
        public override Motor CreateMotor(Course c,System.Action<string,int> a,System.Action<string> s)=>new MopedMotor(c,a,s);
        public override void DrawRider(Ink ink,Motor motor,Vector2 position,float time)=>MopedRig.Draw(ink,motor.RenderPose,position,time);
    }
}
