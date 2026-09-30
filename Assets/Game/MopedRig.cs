using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    public static class MopedRig
    {
        public static void Draw(Ink ink,RiderPose p,Vector2 pos,float time)
        {
            Vector2 pivot=pos+new Vector2(0,.23f);
            float tilt=p.Pitch,fold=1-p.Trick*.20f;
            Vector2 At(float x,float y)=>pivot+Ink.Rotate(new Vector2(x,y*fold),tilt);
            Color tyre=ink.Dark,metal=Color.Lerp(ink.Dark,ink.Cream,.23f),skin=Color.Lerp(ink.Cream,ink.Coral,.32f),body=Color.Lerp(ink.Teal,ink.Dark,.48f),trousers=Color.Lerp(ink.Teal,ink.Dark,.28f);
            // Two contact anchors are stable across every pose; wheels turn independently.
            for(int wheelIndex=0;wheelIndex<2;wheelIndex++)
            {
                float x=wheelIndex*.94f;Vector2 wheel=At(x,0);ink.Ellipse(wheel,.225f,.225f,tyre,28);ink.Ellipse(wheel,.135f,.135f,metal,24);ink.Ellipse(wheel,.035f,.035f,tyre,12);
                float spin=-p.X/.225f;Vector2 spoke=new Vector2(Mathf.Cos(spin),Mathf.Sin(spin))*.115f;ink.Line(wheel-spoke,wheel+spoke,.022f,Color.Lerp(metal,ink.Cream,.45f));
            }
            ink.Capsule(At(0,0),At(.19f,.39f),.065f,metal);ink.Capsule(At(.94f,0),At(.79f,.69f),.058f,metal);
            ink.Quad(At(-.07f,.20f),At(.43f,.19f),At(.48f,.49f),At(.05f,.52f),body);
            ink.Line(At(.35f,.16f),At(.72f,.16f),.065f,body);
            ink.Quad(At(.67f,.19f),At(.81f,.32f),At(.78f,.7f),At(.68f,.65f),body);
            ink.Capsule(At(.77f,.64f),At(.74f,.83f),.045f,metal);ink.Capsule(At(.64f,.83f),At(.83f,.83f),.055f,tyre);
            ink.Ellipse(At(.83f,.64f),.074f,.08f,ink.Cream,18);
            ink.Capsule(At(.07f,.56f),At(.41f,.56f),.095f,tyre);
            float crouch=p.Compression+p.Trick*.06f;
            Vector2 hip=At(.24f-p.Lean*.14f,.74f-crouch),shoulder=At(.40f-p.Lean*.20f,1.08f-crouch);
            Vector2 foot=At(.55f,.24f),hand=At(.72f,.82f);
            ink.Limb(hip+new Vector2(-.06f,.01f),foot+new Vector2(-.04f,.025f),.31f,-1,.074f,Color.Lerp(ink.Teal,tyre,.5f));
            ink.Limb(shoulder+new Vector2(-.045f,0),hand+new Vector2(-.03f,.015f),.25f,1,.06f,Color.Lerp(ink.Coral,tyre,.25f));
            ink.Capsule(hip,shoulder,.19f,ink.Coral);
            ink.Limb(hip,foot,.31f,-1,.088f,trousers);ink.Capsule(foot+new Vector2(-.03f,0),foot+new Vector2(.09f,0),.07f,ink.Cream);
            ink.Limb(shoulder,hand,.25f,1,.067f,ink.Coral);ink.Ellipse(hand,.039f,.039f,skin,12);
            Vector2 head=shoulder+Ink.Rotate(new Vector2(.055f,.225f),tilt);
            ink.Capsule(shoulder,head,.065f,skin);ink.Ellipse(head,.113f,.132f,skin,24);
            ink.Ellipse(head+new Vector2(-.014f,.057f),.137f,.105f,ink.Cream,26);ink.Capsule(head+new Vector2(.02f,-.035f),head+new Vector2(.16f,-.03f),.034f,ink.Cream);
        }
    }
}

