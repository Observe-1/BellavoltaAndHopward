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
        public override string Tagline=>"One road along the coast. How far can you ride it?";
        public override string[] Outings=>new[]{"The coast road"};
        public override string[] Lessons=>new[]{"Tap to hop.","Keep holding in the air to backflip. Let go to stop.","Land level with the road. Nose-high lands a wheelie.","Tap in the air for a trick. Finish it before you land.","Kick off the ramps and land on the bunting to grind."};
        public override string PrimaryControl=>"Hop · hold to flip";
        public override string SecondaryControl=>"Trick";
        public override bool Endless=>true;
        public override bool IsMotorbike=>true;
        public override int MultiplierCap=>5;
        // Roles used by the shared shell (menus, HUD): 0 sky, 1 paper, 2 far, 3 town, 4 sea, 5 wall, 6 ink, 7 accent.
        public override Color[] Palette(bool dusk)
        {
            var p=dusk?PortoPalette.Dusk():PortoPalette.Day();
            return new[]{p.SkyTop,p.Paper,p.RidgeMid,p.VillageWall,p.Sea,p.NearWall,p.Ink,p.Jacket};
        }
        // Closer framing than the foundation default: rider near a third across, road at ~70% down.
        public override float CameraSize=>3.8f;
        public override float CameraLift=>PortoArt.CameraRest;
        public override IScenery CreateScenery(Transform root)=>new PortoScenery(root,this);
        // A fresh coastline every run.
        public override Course CreateCourse(int outing)=>new PortoRoad(System.Environment.TickCount&0x7fffffff);
        // Widen the view and give more look-ahead as the ride speeds up.
        public override float CameraSizeFor(Motor m)=>Mathf.Lerp(3.8f,4.7f,Mathf.InverseLerp(PortoRoad.StartSpeed,PortoRoad.TopSpeed+2,m.Speed));
        public override float RiderAnchorFor(Motor m)=>Mathf.Lerp(.32f,.25f,Mathf.InverseLerp(PortoRoad.StartSpeed,PortoRoad.TopSpeed+2,m.Speed));
        public override Motor CreateMotor(Course c,System.Action<string,int> a,System.Action<string> s)=>new MopedMotor(c,a,s);
        public override void DrawRider(Ink ink,Motor motor,Vector2 position,float time)=>MopedRig.Draw(ink,motor.RenderPose,position,time);
    }
}
