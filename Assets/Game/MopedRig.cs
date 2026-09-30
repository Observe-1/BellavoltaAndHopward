using System.Collections.Generic;
using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    // Original rider-and-moped rig in flat filled shapes. Local frame: rear axle at the origin,
    // x forward, y up, rotated by pitch. Wheel base 0.94 and radius 0.225 match MopedMotor contact.
    // Slim step-through moped: rear rack, long seat, teal frame and fenders, round lamp, tall fork.
    public static class MopedRig
    {
        const float Wheel=.225f,Base=.94f;
        static readonly List<Vector2> pts=new List<Vector2>(40);
        public static void Draw(Ink ink,RiderPose p,Vector2 pos,float time)
        {
            var P=PortoArt.P;
            Vector2 axle=pos+new Vector2(0,Wheel);
            float tilt=p.Pitch,depth=p.Trick*55f*Mathf.Deg2Rad;
            float bikeFold=Mathf.Cos(depth),riderFold=Mathf.Cos(depth*.45f);
            Vector2 B(float x,float y)=>axle+Ink.Rotate(new Vector2(x,y*bikeFold),tilt);
            float crouch=p.Compression*.8f+p.Trick*.05f,lean=p.Lean;
            Vector2 R(float x,float y)=>axle+Ink.Rotate(new Vector2(x-lean*(y-.5f)*.28f,(y-crouch)*riderFold),tilt);
            Vector2 D(Vector2 v)=>Ink.Rotate(v,tilt);
            Color frame=Color.Lerp(P.Moped,P.MopedShade,p.Trick*.4f),frameLight=Color.Lerp(frame,P.MopedLight,.5f);
            Color dark=P.Tyre,mech=Color.Lerp(P.Tyre,P.MopedShade,.45f);

            // --- far side: arm and leg in shade ---
            Vector2 hip=R(.12f,.66f),knee=R(.46f,.74f),foot=B(.44f,.18f);
            Vector2 farKnee=R(.42f,.72f),farFoot=B(.38f,.2f);
            Vector2 shoulder=R(.3f,1.06f),grip=B(.77f,.93f),farGrip=B(.72f,.95f);
            ink.Capsule(hip,farKnee,.13f,P.TrousersShade);ink.Capsule(farKnee,farFoot,.11f,P.TrousersShade);
            ink.Capsule(farFoot+D(new Vector2(-.02f,-.01f)),farFoot+D(new Vector2(.1f,-.01f)),.07f,Color.Lerp(P.Shoe,P.Trousers,.4f));
            ink.Capsule(shoulder+D(new Vector2(-.02f,-.03f)),Vector2.Lerp(shoulder,farGrip,.5f)+D(new Vector2(0,-.05f)),.1f,P.JacketShade);
            ink.Capsule(Vector2.Lerp(shoulder,farGrip,.5f)+D(new Vector2(0,-.05f)),farGrip,.08f,P.JacketShade);

            // --- wheels: thick tyre, dark hub, thin rim line ---
            float spin=-p.X/Wheel;
            for(int w=0;w<2;w++)
            {
                Vector2 c=B(w*Base,0);float ry=Wheel*bikeFold;
                ink.Ellipse(c,Wheel,ry,dark);
                ink.Ellipse(c,Wheel*.7f,ry*.7f,Color.Lerp(P.Rim,P.Metal,.2f));
                ink.Ellipse(c,Wheel*.64f,ry*.64f,Color.Lerp(dark,P.Rim,.35f));
                for(int s=0;s<2;s++){float a=spin+s*Mathf.PI*.5f;Vector2 d=new Vector2(Mathf.Cos(a)*Wheel*.6f,Mathf.Sin(a)*ry*.6f);ink.Line(c-d,c+d,.014f,Color.Lerp(P.Rim,dark,.3f));}
                ink.Ellipse(c,.055f,.055f*bikeFold+.001f,P.Metal);
                ink.Ellipse(c,.025f,.025f*bikeFold+.001f,dark);
            }
            // --- mechanicals: exhaust, engine, rear shock, pedals ---
            ink.Capsule(B(.3f,.06f),B(-.26f,.1f),.055f,Color.Lerp(P.Metal,dark,.5f));
            ink.Capsule(B(-.26f,.1f),B(-.36f,.12f),.07f,Color.Lerp(P.Metal,dark,.35f));
            pts.Clear();foreach(var v in new[]{new Vector2(.18f,.04f),new Vector2(.46f,.05f),new Vector2(.5f,.16f),new Vector2(.42f,.26f),new Vector2(.2f,.24f)})pts.Add(B(v.x,v.y));ink.Polygon(pts,mech);
            ink.Ellipse(B(.34f,.13f),.07f,.07f*bikeFold+.001f,Color.Lerp(mech,P.Metal,.3f));
            ink.Line(B(0,0),B(.28f,.08f),.06f,mech);
            ink.Line(B(-.08f,.48f),B(.02f,.06f),.045f,dark);
            // --- rear fender, rack, seat ---
            pts.Clear();
            for(int i=0;i<=10;i++){float a=Mathf.Lerp(40,170,i/10f)*Mathf.Deg2Rad;pts.Add(B(Mathf.Cos(a)*.3f,Mathf.Sin(a)*.3f));}
            for(int i=10;i>=0;i--){float a=Mathf.Lerp(40,170,i/10f)*Mathf.Deg2Rad;pts.Add(B(Mathf.Cos(a)*.255f,Mathf.Sin(a)*.255f));}
            ink.Polygon(pts,frame);
            ink.Line(B(-.36f,.46f),B(.02f,.46f),.035f,P.Rail);ink.Line(B(-.36f,.4f),B(-.02f,.4f),.022f,P.Rail);
            ink.Line(B(-.34f,.46f),B(-.3f,.14f),.025f,P.Rail);ink.Line(B(-.02f,.46f),B(.03f,.28f),.025f,P.Rail);
            ink.Ellipse(B(-.38f,.33f),.035f,.035f,P.Funnel,10);
            ink.Line(B(.12f,.3f),B(.18f,.5f),.05f,dark);
            ink.Capsule(B(-.04f,.55f),B(.32f,.56f),.1f,dark);ink.Capsule(B(-.02f,.585f),B(.3f,.59f),.035f,Color.Lerp(dark,P.Rim,.35f));
            // --- step-through frame: under the seat, down to the engine, up to the head tube ---
            pts.Clear();foreach(var v in new[]{new Vector2(.1f,.3f),new Vector2(.36f,.5f),new Vector2(.4f,.42f),new Vector2(.22f,.26f)})pts.Add(B(v.x,v.y));ink.Polygon(pts,frame);
            pts.Clear();foreach(var v in new[]{new Vector2(.08f,.22f),new Vector2(.34f,.2f),new Vector2(.54f,.24f),new Vector2(.72f,.62f),new Vector2(.8f,.72f),new Vector2(.74f,.76f),new Vector2(.63f,.6f),new Vector2(.5f,.34f),new Vector2(.3f,.3f),new Vector2(.12f,.32f)})pts.Add(B(v.x,v.y));ink.Polygon(pts,frame);
            pts.Clear();foreach(var v in new[]{new Vector2(.54f,.24f),new Vector2(.6f,.3f),new Vector2(.76f,.66f),new Vector2(.72f,.62f)})pts.Add(B(v.x,v.y));ink.Polygon(pts,frameLight);
            // --- fork, front fender, lamp, handlebar ---
            ink.Capsule(B(.8f,.74f),B(Base,0),.055f,Color.Lerp(frame,dark,.35f));
            pts.Clear();
            for(int i=0;i<=10;i++){float a=Mathf.Lerp(20,135,i/10f)*Mathf.Deg2Rad;pts.Add(B(Base+Mathf.Cos(a)*.3f,Mathf.Sin(a)*.3f));}
            for(int i=10;i>=0;i--){float a=Mathf.Lerp(20,135,i/10f)*Mathf.Deg2Rad;pts.Add(B(Base+Mathf.Cos(a)*.258f,Mathf.Sin(a)*.258f));}
            ink.Polygon(pts,frame);
            ink.Capsule(B(.78f,.74f),B(.8f,.9f),.05f,frame);
            ink.Ellipse(B(.9f,.8f),.085f,.085f*bikeFold+.001f,Color.Lerp(frame,dark,.3f));
            ink.Ellipse(B(.915f,.8f),.062f,.062f*bikeFold+.001f,P.Helmet);
            ink.Capsule(B(.72f,.93f),B(.82f,.92f),.04f,dark);ink.Line(B(.8f,.9f),B(.86f,.99f),.03f,dark);

            // --- near leg: thigh to a raised knee, shin down to the foot plate ---
            ink.Capsule(hip,knee,.15f,P.Trousers);
            ink.Capsule(knee,foot+D(new Vector2(0,.06f)),.12f,P.Trousers);
            ink.Capsule(foot+D(new Vector2(-.03f,-.005f)),foot+D(new Vector2(.12f,-.005f)),.08f,P.Shoe);
            // --- jacket: rounded back, open front, sleeve ---
            Vector2 waistBack=R(.0f,.64f),waistFront=R(.26f,.64f),chest=R(.42f,.98f),neckFront=R(.36f,1.12f),nape=R(.22f,1.14f),back=R(.02f,.92f);
            pts.Clear();pts.Add(waistBack);pts.Add(waistFront);pts.Add(Vector2.Lerp(waistFront,chest,.5f)+D(new Vector2(.04f,0)));pts.Add(chest);pts.Add(neckFront);pts.Add(nape);pts.Add(back+D(new Vector2(-.02f,.08f)));pts.Add(back);pts.Add(Vector2.Lerp(waistBack,back,.4f)+D(new Vector2(-.03f,0)));
            ink.Polygon(pts,P.Jacket);
            pts.Clear();pts.Add(waistBack);pts.Add(Vector2.Lerp(waistBack,waistFront,.3f));pts.Add(R(.1f,.9f));pts.Add(nape);pts.Add(back+D(new Vector2(-.02f,.08f)));pts.Add(back);pts.Add(Vector2.Lerp(waistBack,back,.4f)+D(new Vector2(-.03f,0)));
            ink.Polygon(pts,P.JacketShade);
            ink.Capsule(Vector2.Lerp(waistBack,waistFront,.05f),Vector2.Lerp(waistBack,waistFront,.95f),.07f,P.JacketShade);
            ink.Line(chest+D(new Vector2(-.02f,-.02f)),Vector2.Lerp(waistFront,chest,.2f),.025f,P.JacketShade);
            // collar and neck
            ink.Capsule(neckFront+D(new Vector2(-.02f,.02f)),D(new Vector2(-.03f,.02f))+nape,.06f,P.JacketShade);
            Vector2 head=R(.37f,1.3f);
            ink.Capsule(R(.31f,1.12f),head+D(new Vector2(-.02f,-.08f)),.08f,P.Skin);
            // --- long hair streaming back ---
            float breeze=Mathf.Sin(time*5.1f+p.X*.6f),lift=-p.VelocityY*4;
            Vector2 hairRoot=head+D(new Vector2(-.09f,-.02f));
            for(int s=0;s<3;s++)
            {
                float a=192+s*9+tilt+breeze*(4+s*2)+lift;float len=.34f+s*.05f;
                ink.Leaf(hairRoot+D(new Vector2(0,-.02f*s)),a,len,.1f-.015f*s,P.Hair);
            }
            ink.Ellipse(head,.108f,.12f,P.Skin);
            ink.Ellipse(head+D(new Vector2(-.055f,-.03f)),.07f,.09f,P.Hair);
            // --- helmet: dome with back guard, visor edge ---
            Vector2 hc=head+D(new Vector2(-.01f,.03f));
            ink.Sector(hc,.138f,.132f,-8+tilt,188+tilt,P.Helmet);
            pts.Clear();pts.Add(hc+D(new Vector2(-.136f,.02f)));pts.Add(hc+D(new Vector2(-.13f,-.08f)));pts.Add(hc+D(new Vector2(-.05f,-.065f)));pts.Add(hc+D(new Vector2(.02f,0)));ink.Polygon(pts,P.Helmet);
            ink.Line(hc+D(new Vector2(-.13f,-.005f)),hc+D(new Vector2(.12f,-.005f)),.016f,Color.Lerp(P.Helmet,P.Tyre,.22f));
            ink.Capsule(hc+D(new Vector2(.08f,-.008f)),hc+D(new Vector2(.17f,-.025f)),.028f,Color.Lerp(P.Helmet,P.Tyre,.12f));
            // --- near arm reaching to the grip ---
            Vector2 elbow=Vector2.Lerp(shoulder,grip,.5f)+D(new Vector2(0,-.06f));
            ink.Capsule(shoulder,elbow,.12f,P.Jacket);ink.Capsule(elbow,grip+D(new Vector2(-.05f,0)),.095f,P.Jacket);
            ink.Capsule(elbow+D(new Vector2(.02f,-.035f)),grip+D(new Vector2(-.06f,-.03f)),.03f,P.JacketShade);
            ink.Ellipse(grip,.048f,.048f,P.Skin);
        }
    }
}
