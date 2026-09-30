using System.Collections.Generic;
using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    // Porto Chiaro artwork: original flat-illustration vectors, generated deterministically.
    // Background layers are authored "at rest" (camera centre ArtRest above flat road) and drawn at
    // depth scale Back; the road and foreground are in world units at rider scale. Each layer scrolls
    // with its own parallax factors.
    // Level of detail follows depth: far layers are grouped silhouettes, near layers carry shutters,
    // rails and doors; curve tessellation also scales with on-screen size (Ink.Segments).
    public static class PortoArt
    {
        public const float ArtRest=1.95f,CameraRest=1.52f,Back=.79f,Horizon=1.66f;
        public static PortoPalette P=PortoPalette.Day();
        // Per-outing variation of the set dressing (the opening view stays fixed).
        public static int Variant;

        public sealed class LayerSpec
        {
            public string Name;public int Order;public float Fx,Fy,Width,Margin,Scale=Back,Rest=ArtRest;public System.Action<Ink,int,Course> Draw;
            public bool NeedsCourse;
        }
        public static readonly LayerSpec[] Layers=
        {
            new LayerSpec{Name="Far ridges",Order=1,Fx=.03f,Fy=.03f,Width=60,Margin=1,Draw=(i,k,c)=>Ridges(i,k)},
            new LayerSpec{Name="Coastal hills",Order=2,Fx=.07f,Fy=.06f,Width=40,Margin=1,Draw=(i,k,c)=>Hills(i,k)},
            new LayerSpec{Name="Hill town",Order=5,Fx=.2f,Fy=.15f,Width=36,Margin=3,Draw=(i,k,c)=>HillTown(i,k)},
            new LayerSpec{Name="Near town",Order=6,Fx=.5f,Fy=.45f,Width=28,Margin=4,Draw=(i,k,c)=>NearTown(i,k)},
            new LayerSpec{Name="Road",Order=9,Fx=1,Fy=1,Width=16,Margin=4,Scale=1,Rest=CameraRest,Draw=Road,NeedsCourse=true},
            new LayerSpec{Name="Foreground",Order=12,Fx=1.25f,Fy=1.05f,Width=20,Margin=3,Scale=1,Rest=CameraRest,Draw=(i,k,c)=>Foreground(i,k)},
        };
        public const int SkyOrder=0,SeaOrder=3,BoatOrder=4,LaundryOrder=7,RiderOrder=10;
        // Visible tile range and on-screen placement for a layer (fx may be zeroed for reduced motion).
        public static void TileRange(LayerSpec s,float fx,float camX,float hw,out int k0,out int k1)
        {float lx=camX*fx;k0=Mathf.FloorToInt((lx-hw-s.Margin)/s.Scale/s.Width);k1=Mathf.FloorToInt((lx+hw+s.Margin)/s.Scale/s.Width);}
        public static Vector2 Placement(LayerSpec s,float fx,float camX,float camY)
        =>new Vector2(-camX*fx,-s.Scale*s.Rest-(camY-CameraRest)*s.Fy);
        static float HorizonScreen(float camY)=>Back*(Horizon-ArtRest)-(camY-CameraRest)*.12f;

        // ---------- deterministic noise ----------
        public static float Hash(int n)
        {unchecked{uint h=(uint)n*0x9E3779B1u;h^=h>>15;h*=0x85EBCA77u;h^=h>>13;h*=0xC2B2AE3Du;h^=h>>16;return (h&0xFFFFFF)/16777216f;}}
        static float Hash(int a,int b)=>Hash(a*7919+b*104729+17);
        static float Noise(float x,int seed)
        {int i=Mathf.FloorToInt(x);float f=x-i,u=f*f*(3-2*f);return Mathf.Lerp(Hash(i,seed),Hash(i+1,seed),u);}
        static float Fbm(float x,int seed,int octaves=4)
        {float s=0,a=.5f,f=1,n=0;for(int o=0;o<octaves;o++){s+=a*Noise(x*f,seed+o*31);n+=a;a*=.5f;f*=2.03f;}return s/n;}
        sealed class Rng{int s;public Rng(int seed){s=seed;}public float Next(){return Hash(s++*2654435+911);}public float Range(float a,float b){return Mathf.Lerp(a,b,Next());}public int Int(int a,int b){return Mathf.Min(b-1,a+(int)(Next()*(b-a)));}public bool Chance(float p){return Next()<p;}}

        // ---------- sky, sun, sea (per frame, cheap) ----------
        public static void Sky(Ink ink,float hw,float hh,float camY)
        {
            float horizon=HorizonScreen(camY);
            ink.Gradient(-hw-1,horizon-.5f,2*hw+2,hh-horizon+1.5f,P.SkyLow,P.SkyTop);
            Vector2 sun=new Vector2(hw*.86f,Back*(3.3f-ArtRest)-(camY-CameraRest)*.02f);
            var glow=P.SunGlow;glow.a=.35f;ink.Ellipse(sun,.78f*Back,.78f*Back,glow);
            ink.Ellipse(sun,.54f*Back,.54f*Back,P.Sun);
        }
        public static void Sea(Ink ink,float hw,float hh,float camX,float camY,float time,bool still)
        {
            float top=HorizonScreen(camY);
            ink.Gradient(-hw-1,top-1.2f,2*hw+2,1.2f,P.Sea,P.SeaHorizon);
            ink.Rect(-hw-1,-hh-2,2*hw+2,top-1.2f+hh+2,P.Sea);
            // Light streaks: farther streaks are shorter, thinner and slower.
            var streak=P.SeaStreak;
            for(int j=0;j<26;j++)
            {
                float d=Hash(j,5),fx=.1f+d*.8f;
                float y=top-(.08f+d*d*4.6f)*Back;float len=(.35f+d*2.2f)*Back,thick=(.012f+d*.03f)*Back;
                float span=2*hw+len+6;
                float x=Mathf.Repeat(Hash(j,6)*span-camX*fx-(still?0:time*(.05f+d*.1f)),span)-hw-len-3;
                streak.a=.35f+.35f*Hash(j,7);ink.Rect(x,y,len,thick,streak);
            }
        }
        // Ferry with a long pale wake, crossing slowly; respawns along the coast.
        public static void Boats(Ink ink,float hw,float camX,float camY,float time,bool still)
        {
            float fx=.12f,yb=Back*(1.28f-ArtRest)-(camY-CameraRest)*.12f;
            float drift=still?0:time*.22f;
            for(int i=-1;i<=2;i++)
            {
                float lx=10+i*64+drift;float x=lx*Back-camX*fx;
                if(x<-hw-6||x>hw+2)continue;
                Ferry(ink,new Vector2(x,yb),Back);
            }
        }
        static void Ferry(Ink ink,Vector2 o,float s)
        {
            var wake=P.Wake;wake.a=.75f;
            ink.Quad(o+new Vector2(-.7f,.015f)*s,o+new Vector2(-.7f,.05f)*s,o+new Vector2(-4.2f,.035f)*s,o+new Vector2(-4.2f,.03f)*s,wake);
            wake.a=.35f;ink.Quad(o+new Vector2(-.7f,-.01f)*s,o+new Vector2(-.7f,.02f)*s,o+new Vector2(-2.4f,-.005f)*s,o+new Vector2(-2.4f,-.01f)*s,wake);
            ink.Polygon(new[]{o+new Vector2(-.72f,0)*s,o+new Vector2(.6f,0)*s,o+new Vector2(.8f,.19f)*s,o+new Vector2(-.76f,.19f)*s},P.Ferry);
            ink.Quad(o+new Vector2(-.72f,0)*s,o+new Vector2(.6f,0)*s,o+new Vector2(.63f,.035f)*s,o+new Vector2(-.73f,.035f)*s,P.FerryShade);
            ink.Rect(o.x-.55f*s,o.y+.19f*s,1.0f*s,.12f*s,P.Ferry);
            ink.Rect(o.x-.38f*s,o.y+.31f*s,.66f*s,.09f*s,P.Ferry);
            for(int w=0;w<11;w++)ink.Rect(o.x+(-.5f+w*.085f)*s,o.y+.23f*s,.045f*s,.035f*s,P.Dome);
            ink.Rect(o.x-.58f*s,o.y+.19f*s,1.06f*s,.012f*s,P.FerryShade);
            ink.Rect(o.x-.08f*s,o.y+.4f*s,.11f*s,.15f*s,P.Funnel);ink.Rect(o.x-.08f*s,o.y+.52f*s,.11f*s,.035f*s,P.Dome);
            ink.Rect(o.x+.28f*s,o.y+.4f*s,.012f*s,.22f*s,P.FerryShade);
        }

        // ---------- far layers ----------
        static float Peaks(float x,int seed){float n=Fbm(x,seed,3);return 1-Mathf.Abs(n*2-1);}
        static float RidgeFarY(float x)=>Horizon+.3f+3.1f*Mathf.Pow(.5f+.5f*Mathf.Cos((x+16)*Mathf.PI*2/64),1.3f)+.55f*Peaks(x*.08f,3)+.22f*Peaks(x*.3f,5)+.06f*Fbm(x*1.6f,4,2);
        static float RidgeMidY(float x)=>Horizon+.1f+2.4f*Mathf.Pow(.5f+.5f*Mathf.Cos((x+8)*Mathf.PI*2/52),1.5f)+.5f*Peaks(x*.11f,8)+.2f*Peaks(x*.36f,10)+.06f*Fbm(x*1.8f,9,2);
        static void Ridges(Ink ink,int k)
        {
            float x0=k*60-.05f,x1=(k+1)*60+.05f;
            ink.Ridge(x0,x1,.35f,RidgeFarY,Horizon-.4f,P.RidgeFar,Color.Lerp(P.RidgeFar,P.SkyLow,.18f));
            ink.Ridge(x0,x1,.3f,RidgeMidY,Horizon-.4f,P.RidgeMid,Color.Lerp(P.RidgeMid,P.RidgeFar,.35f));
        }
        // Opening view matches the reference: hills high on the left, falling to open sea on the right.
        static float HillEnvelope(float x)
        {
            float noise=Mathf.Clamp01((Fbm(x*.035f,21,2)-.3f)*3.4f);
            float opening=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-2,11,x));
            return Mathf.Lerp(opening,noise,Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,26,x)));
        }
        static float HillY(float x)=>Horizon-.2f+HillEnvelope(x)*(1.7f+1.1f*Fbm(x*.11f,22,3))+.06f*Fbm(x*1.3f,23,2);
        static float FoothillY(float x)=>Horizon-.2f+HillEnvelope(x+3)*(.55f+.7f*Fbm(x*.16f,24,3));
        static void Hills(Ink ink,int k)
        {
            float x0=k*40-.05f,x1=(k+1)*40+.05f;
            ink.Ridge(x0,x1,.2f,HillY,Horizon-.5f,P.Hill,Color.Lerp(P.Hill,P.RidgeMid,.25f));
            var r=new Rng(k*977+13+(k<-1||k>0?Variant*100003:0));
            // Wooded texture: soft darker tree clumps scattered over the slope (tiny at this depth).
            var clump=Color.Lerp(P.Hill,P.HillShade,.45f);
            for(int t=0;t<70;t++)
            {
                float tx=r.Range(x0,x1),top=HillY(tx);if(top<Horizon+.35f)continue;
                float ty=Mathf.Lerp(Horizon+.1f,top-.12f,r.Range(.35f,1f));
                for(int j=0;j<4;j++){float s=r.Range(.07f,.13f);ink.Ellipse(new Vector2(tx+j*.1f,ty+r.Range(-.03f,.03f)),s,s*.75f,clump,10);}
            }
            // Hillside villages: grouped low-detail blocks and tiny cypresses on the slope face.
            for(int v=0;v<14;v++)
            {
                float cx=Mathf.Lerp(x0+2,x1-2,r.Next());
                if(k==-1 && v<3)cx=-8f+v*2.6f;else if(k==0 && v<2)cx=1.2f+v*2.4f;
                if(HillEnvelope(cx)<.25f)continue;
                int n=r.Int(5,13);
                for(int h=0;h<n;h++)
                {
                    float hx=cx+r.Range(-1.2f,1.2f),top=HillY(hx);if(top<Horizon+.3f)continue;
                    float hy=Mathf.Lerp(Horizon+.05f,top-.2f,r.Range(.2f,.85f));
                    float w=r.Range(.14f,.28f),hh=r.Range(.1f,.18f);
                    ink.Rect(hx,hy,w,hh,Color.Lerp(P.VillageWall,P.Hill,r.Range(0,.25f)));
                    ink.Rect(hx+w*.7f,hy,w*.3f,hh,Color.Lerp(P.VillageWall,P.Hill,.35f));
                    ink.Triangle(new Vector2(hx-.02f,hy+hh),new Vector2(hx+w+.02f,hy+hh),new Vector2(hx+w*.5f,hy+hh+.07f),P.VillageRoof);
                }
                for(int c=0;c<r.Int(2,6);c++){float tx=cx+r.Range(-1.4f,1.4f),ty=Mathf.Lerp(Horizon,HillY(tx)-.1f,r.Range(.2f,.8f));ink.Ellipse(new Vector2(tx,ty+.1f),.035f,.13f,P.HillShade,8);}
            }
            ink.Ridge(x0,x1,.2f,FoothillY,Horizon-.5f,P.HillShade,Color.Lerp(P.HillShade,P.Hill,.3f));
            // A lighthouse islet where the hills open to the sea (always present in the opening view).
            if(k==0 || r.Chance(.55f))
            {
                float lx=k==0?9.6f:Mathf.Lerp(x0+4,x1-4,r.Next());
                if(HillEnvelope(lx)<.08f)
                {
                    var rock=Color.Lerp(P.RockShade,P.Hill,.35f);
                    ink.Polygon(new[]{new Vector2(lx-.9f,Horizon-.02f),new Vector2(lx-.45f,Horizon+.12f),new Vector2(lx-.1f,Horizon+.2f),new Vector2(lx+.35f,Horizon+.14f),new Vector2(lx+.8f,Horizon-.02f)},rock);
                    ink.Quad(new Vector2(lx-.07f,Horizon+.17f),new Vector2(lx+.07f,Horizon+.17f),new Vector2(lx+.05f,Horizon+.62f),new Vector2(lx-.05f,Horizon+.62f),Color.Lerp(P.Dome,P.Hill,.3f));
                    ink.Rect(lx-.075f,Horizon+.62f,.15f,.04f,Color.Lerp(P.Dome,P.Hill,.3f));
                    ink.Rect(lx-.04f,Horizon+.66f,.08f,.08f,P.Evening?P.LampGlass:Color.Lerp(P.Sun,P.Hill,.25f));
                    ink.Triangle(new Vector2(lx-.06f,Horizon+.74f),new Vector2(lx+.06f,Horizon+.74f),new Vector2(lx,Horizon+.82f),Color.Lerp(P.Dome,P.Hill,.3f));
                }
            }
        }

        // ---------- hill town on a promontory ----------
        static void Cypress(Ink ink,float x,float y,float h,float w,Color c,Color shade)
        {
            // Tall flame shape: stacked ellipse bands tapering to a point, lit on the left.
            var pts=new List<Vector2>();int n=Mathf.Clamp(Mathf.CeilToInt(h*Ink.PixelsPerUnit/14),5,14);
            for(int i=0;i<=n;i++){float t=i/(float)n;float r=w*.5f*Mathf.Sin(Mathf.Pow(1-t,.9f)*Mathf.PI*.62f+.35f)*(t<.08f?.8f:1);pts.Add(new Vector2(x+r,y+t*h));}
            for(int i=n;i>=0;i--){float t=i/(float)n;float r=w*.5f*Mathf.Sin(Mathf.Pow(1-t,.9f)*Mathf.PI*.62f+.35f)*(t<.08f?.8f:1);pts.Add(new Vector2(x-r,y+t*h));}
            ink.Polygon(pts,c);
            var half=new List<Vector2>();for(int i=0;i<=n;i++)half.Add(pts[i]);half.Add(new Vector2(x+w*.05f,y+h*.98f));for(int i=n;i>=0;i--){float t=i/(float)n;half.Add(new Vector2(x+w*.08f,y+t*h*.95f));}
            ink.Polygon(half,shade);
        }
        static void Palm(Ink ink,float x,float y,float h,Color c)
        {
            Vector2 prev=new Vector2(x,y);int n=8;Vector2 top=prev;
            for(int i=1;i<=n;i++){float t=i/(float)n;var p=new Vector2(x+Mathf.Sin(t*1.4f)*h*.12f,y+t*h);ink.Line(prev,p,.07f-.03f*t,c);prev=p;top=p;}
            float[] angles={10,40,75,105,140,170,-15,195};
            foreach(var a in angles)
            {
                Vector2 d=Ink.Rotate(Vector2.right,a);Vector2 mid=top+d*h*.22f+new Vector2(0,.04f);Vector2 tip=top+d*h*.4f-new Vector2(0,h*.12f*Mathf.Abs(Mathf.Cos(a*Mathf.Deg2Rad)));
                ink.Leaf(top,a,(mid-top).magnitude*1.1f,.09f,c);ink.Leaf(mid-d*.02f,Mathf.Atan2(tip.y-mid.y,tip.x-mid.x)*Mathf.Rad2Deg,(tip-mid).magnitude,.07f,c);
            }
        }
        static void BellTower(Ink ink,float x,float y,float w,float h)
        {
            ink.Rect(x,y,w,h,P.TownShade);ink.Rect(x,y,w*.62f,h,P.TownWall);
            // cornices, belfry openings, clock-less facade
            ink.Rect(x-.03f,y+h*.55f,w+.06f,.05f,Color.Lerp(P.TownWall,P.Sun,.3f));
            ink.Rect(x-.04f,y+h,w+.08f,.07f,Color.Lerp(P.TownWall,P.Sun,.3f));
            var win=P.TownWindow;
            ink.ArchShape(x+w*.2f,y+h*.66f,w*.22f,h*.2f,win);ink.ArchShape(x+w*.58f,y+h*.66f,w*.22f,h*.2f,win);
            ink.ArchShape(x+w*.38f,y+h*.3f,w*.22f,h*.13f,win);
            // drum, dome, lantern, cross
            float dy=y+h+.07f;ink.Rect(x+w*.12f,dy,w*.76f,.16f,P.TownShade);ink.Rect(x+w*.12f,dy,w*.45f,.16f,P.TownWall);
            ink.Sector(new Vector2(x+w*.5f,dy+.16f),w*.42f,w*.5f,0,180,P.Dome);
            ink.Rect(x+w*.43f,dy+.16f+w*.48f,w*.14f,.09f,P.Dome);
            ink.Rect(x+w*.485f,dy+.25f+w*.48f,.025f,.2f,P.Dome);ink.Rect(x+w*.43f,dy+.37f+w*.48f,w*.14f,.022f,P.Dome);
        }
        static void TownHouse(Ink ink,Rng r,float x,float y,float w,float h,bool detail)
        {
            int tone=r.Int(0,3);
            Color wall=tone==0?P.TownWall:tone==1?Color.Lerp(P.TownWall,P.NearWallWarm,.45f):Color.Lerp(P.TownWall,P.Sun,.25f);
            ink.Rect(x,y,w,h,wall);
            ink.Rect(x+w*.74f,y,w*.26f,h,Color.Lerp(wall,P.TownShade,.8f));
            // terracotta roof with a visible slope and shaded far side
            float rh=r.Range(.16f,.26f);
            ink.Quad(new Vector2(x-.06f,y+h),new Vector2(x+w+.06f,y+h),new Vector2(x+w*.62f,y+h+rh),new Vector2(x+w*.38f,y+h+rh),P.TownRoof);
            ink.Quad(new Vector2(x+w*.5f,y+h),new Vector2(x+w+.06f,y+h),new Vector2(x+w*.62f,y+h+rh),new Vector2(x+w*.5f,y+h+rh),P.TownRoofShade);
            ink.Rect(x-.04f,y+h-.035f,w+.08f,.035f,P.TownRoofShade);
            if(!detail)return;
            int wins=w>.75f?2:1;
            for(int i=0;i<wins;i++)
            {
                float wx=x+w*(i+.5f)/wins-.05f-w*.06f,wy=y+h*r.Range(.45f,.6f);
                if(r.Chance(.35f)){ink.Rect(wx-.04f,wy,.18f,.16f,Color.Lerp(P.Shutter,P.TownWall,.25f));}
                else ink.Rect(wx,wy,.08f,.14f,P.TownWindow);
            }
        }
        static void HillTown(Ink ink,int k)
        {
            var saved=P;P=P.Hazed(P.RidgeMid,.12f);
            try{HillTownBuild(ink,k);}finally{P=saved;}
        }
        static void HillTownBuild(Ink ink,int k)
        {
            var r=new Rng(k*7331+5+(k<-1||k>0?Variant*100003:0));float x0=k*36;
            if(k!=-1 && !r.Chance(.62f))
            {
                // Open water: a few rocks and a sailing dinghy.
                for(int i=0;i<r.Int(0,3);i++)Rocks(ink,r,x0+r.Range(3,33),.95f,r.Range(.5f,1.1f));
                if(r.Chance(.6f)){float bx=x0+r.Range(4,30),by=1.2f;var sail=Color.Lerp(P.Ferry,P.SeaHorizon,.15f);ink.Quad(new Vector2(bx-.22f,by),new Vector2(bx+.22f,by),new Vector2(bx+.18f,by+.06f),new Vector2(bx-.2f,by+.06f),P.FerryShade);ink.Triangle(new Vector2(bx,by+.07f),new Vector2(bx+.2f,by+.08f),new Vector2(bx,by+.5f),sail);ink.Triangle(new Vector2(bx-.02f,by+.07f),new Vector2(bx-.02f,by+.42f),new Vector2(bx-.18f,by+.08f),sail);}
                return;
            }
            float a=x0+r.Range(4,14),len=r.Range(8,13),b=a+len,peak=r.Range(.35f,.65f);
            if(k==-1){a=-8.5f;len=9;b=a+len;peak=.5f;}
            float rise=r.Range(1.0f,1.5f);
            System.Func<float,float> hill=x=>{float t=Mathf.Clamp01((x-a)/len);float u=t<peak?t/peak:(1-t)/(1-peak);return 2.0f+rise*Mathf.Sin(u*Mathf.PI*.5f);};
            // headland: rock shoulders into the sea, hill mass under the houses
            var land=new List<Vector2>{new Vector2(a-2.0f,.85f),new Vector2(a-1.1f,1.35f),new Vector2(a-.2f,2.0f)};
            for(float x=a;x<=b;x+=.5f)land.Add(new Vector2(x,hill(x)-.3f));
            land.Add(new Vector2(b+.3f,2.0f));land.Add(new Vector2(b+1.4f,1.3f));land.Add(new Vector2(b+2.4f,.85f));
            ink.Polygon(land,Color.Lerp(P.RampartOpening,P.Hill,.3f));
            // cypresses behind the houses
            for(int t=0;t<r.Int(5,9);t++){float tx=r.Range(a,b);Cypress(ink,tx,hill(tx)-.6f,r.Range(1.4f,2.4f),r.Range(.24f,.34f),P.Cypress,P.CypressShade);}
            float towerX=a+len*peak+r.Range(-.8f,.4f);
            // cascade of houses from the crest down to the sea wall, back rows first
            for(int row=3;row>=0;row--)
            {
                float x=a+r.Range(-.2f,.6f);
                while(x<b-.3f)
                {
                    float w=r.Range(.5f,1.0f),h=r.Range(.5f,.85f);
                    float baseY=hill(x+w*.5f)-row*.48f-h*.35f;
                    if(baseY>2.0f-.05f && !(row>=2 && Mathf.Abs(x+w*.5f-towerX-.3f)<.6f))TownHouse(ink,r,x,baseY,w,h,true);
                    x+=w+(r.Chance(.3f)?r.Range(.15f,.6f):r.Range(-.12f,.05f));
                }
                if(row==2)BellTower(ink,towerX,hill(towerX+.3f)-.7f,.6f,2.1f);
                if(row==1)for(int t=0;t<r.Int(2,5);t++){float tx=r.Range(a+.5f,b-.5f);Cypress(ink,tx,hill(tx)-1.2f,r.Range(1.2f,1.9f),r.Range(.22f,.3f),P.Cypress,P.CypressShade);}
                // garden clumps between the rows
                for(int t=0;t<r.Int(2,5);t++){float tx=r.Range(a,b);float ty=hill(tx)-row*.48f-.3f;if(ty>2.0f){ink.Ellipse(new Vector2(tx,ty),.22f,.16f,P.Shrub);ink.Ellipse(new Vector2(tx+.18f,ty-.03f),.17f,.13f,P.ShrubShade);}}
            }
            if(r.Chance(.6f)){float px=r.Range(a+1,b-1);Palm(ink,px,2.05f,r.Range(1.2f,1.6f),P.Palm);}
            // sea wall with an arcade of openings
            float wallTop=2.08f,wallBottom=.8f;
            ink.Rect(a-.4f,wallBottom,len+.8f,wallTop-wallBottom,P.RampartWall);
            ink.Rect(a-.4f,wallTop-.25f,len+.8f,.25f,Color.Lerp(P.RampartWall,P.TownWall,.25f));
            ink.Rect(a-.45f,wallTop,len+.9f,.06f,Color.Lerp(P.RampartWall,P.Sun,.25f));
            float step=r.Range(.85f,1.1f);
            for(float x=a+.3f;x<b-.5f;x+=step)ink.ArchShape(x,wallBottom+.05f,.5f,.72f,P.RampartOpening);
            Rocks(ink,r,a-1.2f,.8f,r.Range(.7f,1.2f));Rocks(ink,r,b+1.1f,.8f,r.Range(.8f,1.4f));
            if(r.Chance(.5f))Rocks(ink,r,b+r.Range(3,6),.85f,r.Range(.4f,.8f));
        }
        static void Rocks(Ink ink,Rng r,float x,float y,float s)
        {
            int n=r.Int(2,4);
            for(int i=0;i<n;i++)
            {
                float cx=x+r.Range(-.6f,.6f)*s,w=r.Range(.35f,.8f)*s,h=r.Range(.25f,.6f)*s;
                var c=r.Chance(.5f)?P.Rock:P.RockShade;
                ink.Polygon(new[]{new Vector2(cx-w,y-.05f),new Vector2(cx+w,y-.05f),new Vector2(cx+w*.55f,y+h*.55f),new Vector2(cx+w*.1f,y+h),new Vector2(cx-w*.45f,y+h*.75f)},c);
            }
        }

        // ---------- near town ----------
        public struct Line{public Vector2 A,B;public int Seed;}
        static readonly Dictionary<int,List<Line>> laundryCache=new Dictionary<int,List<Line>>();
        public static List<Line> LaundryLines(int k)
        {
            int key=k*64+Variant;
            if(laundryCache.TryGetValue(key,out var list))return list;
            list=new List<Line>();var dummy=new Ink();NearTownBuild(dummy,k,list);laundryCache[key]=list;
            if(laundryCache.Count>64)laundryCache.Clear();
            return list;
        }
        static void NearTown(Ink ink,int k){NearTownBuild(ink,k,null);}
        static void Shrub(Ink ink,Rng r,float x,float y,float w,float h)
        {
            int n=Mathf.Clamp(Mathf.CeilToInt(w*4),3,10);float scale=Mathf.Min(1,h*1.6f);
            ink.Rect(x+.1f,y-.4f,w-.2f,.45f,P.ShrubShade);
            for(int i=0;i<n;i++){float t=n==1?.5f:i/(float)(n-1);float cx=x+t*w,cy=y+Mathf.Sin(t*Mathf.PI)*h*.5f;float rr=r.Range(.22f,.32f)*scale;ink.Ellipse(new Vector2(cx,cy),rr,rr*.9f,i%3==0?P.ShrubShade:P.Shrub);}
            // serrated leafy rim, like the reference hedges
            int leaves=Mathf.CeilToInt(w*9);
            for(int i=0;i<leaves;i++){float t=i/(float)Mathf.Max(1,leaves-1);float cx=x+t*w,cy=y+Mathf.Sin(t*Mathf.PI)*h*.5f+.12f*scale;float ang=90+(t-.5f)*140+r.Range(-20,20);ink.Leaf(new Vector2(cx,cy),ang,r.Range(.2f,.34f)*scale,.12f*scale,i%4==0?P.ShrubShade:P.Shrub);}
        }
        static void Window(Ink ink,Rng r,float x,float y,float w,float h,bool arched)
        {
            bool open=r.Chance(.55f);
            if(open)
            {
                if(arched)ink.ArchShape(x,y,w,h,P.Window);else ink.Rect(x,y,w,h,P.Window);
                ink.Rect(x-w*.42f,y,w*.4f,h,P.Shutter);ink.Rect(x+w*1.02f,y,w*.4f,h,P.Shutter);
            }
            else
            {
                ink.Rect(x,y,w,h,P.Shutter);
                var slat=Color.Lerp(P.Shutter,P.NearWall,.18f);for(float sy=y+.06f;sy<y+h-.03f;sy+=.07f)ink.Rect(x+.02f,sy,w-.04f,.012f,slat);
                ink.Rect(x+w*.49f,y,.012f,h,Color.Lerp(P.Shutter,P.Rail,.5f));
            }
            ink.Rect(x-.03f,y-.04f,w+.06f,.04f,Color.Lerp(P.NearWall,P.Sun,.35f));
        }
        static void NearTownBuild(Ink ink,int k,List<Line> laundry)
        {
            var r=new Rng(k*4099+71+(k<-1||k>0?Variant*100003:0));float x0=k*28;
            if(k!=-1 && !r.Chance(.5f))
            {
                // open water: at most a moored skiff by the bridge
                if(r.Chance(.5f)){float bx=x0+r.Range(4,24),by=.05f;ink.Quad(new Vector2(bx-.7f,by+.22f),new Vector2(bx+.8f,by+.22f),new Vector2(bx+.55f,by-.05f),new Vector2(bx-.5f,by-.05f),P.Shutter);ink.Rect(bx-.6f,by+.2f,1.35f,.05f,P.Laundry2);}
                return;
            }
            float x=x0+r.Range(1,8),end=x+r.Range(5,11),ground=-.4f;
            if(k==-1){x=-10.5f;end=-3.2f;}
            float prevTop=0,prevRight=0;int index=0;
            while(x<end)
            {
                float w=r.Range(1.7f,3.3f),h=r.Range(3.2f,6.4f);
                int kind=r.Int(0,3);
                Color wall=kind==0?P.NearWall:kind==1?P.NearWallWarm:P.NearWallPale;
                Color shade=Color.Lerp(wall,P.NearShade,.7f);
                // Laundry strung across a lane between the previous facade and this one.
                float laneY=Mathf.Min(prevTop,h+ground)-r.Range(.9f,1.6f);
                if(laundry!=null && index>0 && x-prevRight>.6f)laundry.Add(new Line{A=new Vector2(prevRight-.05f,laneY),B=new Vector2(x+.05f,laneY+r.Range(-.08f,.08f)),Seed=k*31+index});
                // facade and side plane
                ink.Rect(x,ground,w,h,wall);
                ink.Rect(x+w*.86f,ground,w*.14f,h,shade);
                // roof: overhanging terracotta with eave shadow, or a flat parapet
                if(r.Chance(.7f))
                {
                    float rh=r.Range(.35f,.6f);
                    ink.Rect(x-.1f,ground+h-.12f,w+.2f,.12f,P.NearRoofShade);
                    ink.Quad(new Vector2(x-.22f,ground+h),new Vector2(x+w+.22f,ground+h),new Vector2(x+w*.7f,ground+h+rh),new Vector2(x+w*.3f,ground+h+rh),P.NearRoof);
                    ink.Quad(new Vector2(x+w*.62f,ground+h),new Vector2(x+w+.22f,ground+h),new Vector2(x+w*.7f,ground+h+rh),new Vector2(x+w*.62f,ground+h+rh),Color.Lerp(P.NearRoof,P.NearRoofShade,.4f));
                    if(r.Chance(.35f)){float cx=x+w*r.Range(.2f,.7f);ink.Rect(cx,ground+h+rh*.4f,.16f,rh*.9f,shade);ink.Rect(cx-.03f,ground+h+rh*1.3f,.22f,.05f,P.NearRoofShade);}
                }
                else{ink.Rect(x-.05f,ground+h,w+.1f,.12f,Color.Lerp(wall,P.Sun,.3f));}
                // floors: windows with shutters, some balconies with rails and plants
                float floorH=1.15f;int floors=Mathf.FloorToInt((h-.3f)/floorH);
                bool arched=r.Chance(.3f);int cols=w>2.4f?2:1;
                for(int f=1;f<floors;f++)
                {
                    float fy=ground+f*floorH+.25f;
                    bool balcony=f>=1 && r.Chance(.28f);
                    for(int c=0;c<cols;c++)
                    {
                        float ww=.34f,wx=x+w*(c+.5f)/cols-ww*.5f-w*.05f;
                        Window(ink,r,wx,fy,ww,.62f,arched);
                        if(balcony)
                        {
                            float bx=wx-.45f,bw=ww+.9f;
                            ink.Rect(bx,fy-.08f,bw,.08f,P.Balcony);
                            ink.Rect(bx,fy+.32f,bw,.03f,P.Rail);
                            for(float px=bx+.03f;px<bx+bw;px+=.085f)ink.Rect(px,fy,.018f,.32f,P.Rail);
                            if(r.Chance(.6f)){for(int lf=0;lf<7;lf++)ink.Leaf(new Vector2(bx+.25f,fy+.3f),35+lf*18,r.Range(.22f,.34f),.1f,lf%2==0?P.Shrub:P.ShrubShade);}
                        }
                    }
                }
                // ground floor: arched doorway with depth, or a shop opening
                float dw=Mathf.Min(1.1f,w*.45f),dx=x+w*r.Range(.12f,.45f);
                ink.ArchShape(dx,ground,dw,1.55f+.4f,P.Door);ink.ArchShape(dx+dw*.16f,ground,dw*.68f,1.55f+.2f,P.DoorInner);
                prevTop=ground+h;prevRight=x+w;index++;
                x+=w+(r.Chance(.45f)?r.Range(.9f,1.8f):0);
            }
            // front garden wall and shrubs along part of the frontage
            float gx=x0+r.Range(0,4),gw=r.Range(2,4.5f);
            ink.Rect(gx,-.4f,gw,1.25f,Color.Lerp(P.NearWallPale,P.NearShade,.3f));ink.Rect(gx-.05f,.85f,gw+.1f,.08f,Color.Lerp(P.NearWallPale,P.Sun,.4f));
            Shrub(ink,r,gx+.3f,.9f,gw*.5f,.7f);
            if(r.Chance(.7f))Cypress(ink,end+r.Range(.3f,1.2f),.2f,r.Range(3.2f,4.6f),.5f,P.Cypress,P.CypressShade);
        }
        // Laundry cloths sway on their lines (per frame; frozen by reduced motion).
        public static void Laundry(Ink ink,int k,Vector2 shift,float scale,float time,bool still)
        {
            var lines=LaundryLines(k);
            float saved=ink.Scale;Vector2 savedOffset=ink.Offset;ink.Scale=scale;ink.Offset=shift;
            foreach(var l in lines)
            {
                Vector2 a=l.A,b=l.B;
                int seg=8;Vector2 prev=a;
                System.Func<float,Vector2> at=t=>Vector2.Lerp(a,b,t)-new Vector2(0,Mathf.Sin(t*Mathf.PI)*.18f);
                for(int i=1;i<=seg;i++){var p=at(i/(float)seg);ink.Line(prev,p,.018f,P.Rail);prev=p;}
                int cloths=2+(int)(Hash(l.Seed,3)*3);
                for(int c=0;c<cloths;c++)
                {
                    float t=(c+.7f)/(cloths+.4f);var top=at(t);
                    float w=.26f+Hash(l.Seed,c)*.22f,h=.38f+Hash(l.Seed,c+9)*.3f;
                    float sway=still?0:Mathf.Sin(time*1.7f+c*1.3f+l.Seed)*.06f;
                    Color col=c%3==0?P.Laundry1:c%3==1?P.Laundry3:P.Laundry2;
                    ink.Quad(top+new Vector2(-w*.5f,0),top+new Vector2(w*.5f,0),top+new Vector2(w*.5f+sway,-h),top+new Vector2(-w*.5f+sway*.7f,-h+.02f),col);
                }
            }
            ink.Scale=saved;ink.Offset=savedOffset;
        }

        // ---------- road: stone bridge, parapet, promenade, gates, lamps ----------
        public static float Lower(Course c,float x){float g=c.Ground(c.Endless?x:Mathf.Clamp(x,0,c.Length-.01f));return g<-50?0:g;}
        static bool LowerAt(Course c,float x,out float y){y=c.Ground(c.Endless?x:Mathf.Clamp(x,0,c.Length-.01f));return y>-50;}
        static void Road(Ink ink,int k,Course course)
        {
            float x0=k*16,x1=x0+16;var road=course as PortoRoad;
            // Raised promenades on arcades sit behind the riding plane.
            foreach(var s in course.Surfaces)if(s.Upper && s.End>x0-.5f && s.Start<x1+.5f)Promenade(ink,course,s,x0,x1);
            foreach(float g in course.Arches)if(g>x0-3 && g<x1+1)Gate(ink,course,g);
            // lamps and parapet blocks along the back edge, only where there is road
            for(float lx=Mathf.Ceil((x0-9)/18)*18+9;lx<x1;lx+=18)if(lx>=x0 && !NearGate(course,lx,2.5f) && LowerAt(course,lx,out float ly) && LowerAt(course,lx+.5f,out _) && !NearRail(road,lx))Lamp(ink,lx,ly);
            for(float bx=Mathf.Ceil(x0/8.5f)*8.5f;bx<x1;bx+=8.5f)if(!NearGate(course,bx,2.2f) && LowerAt(course,bx-.3f,out float y) && LowerAt(course,bx+.3f,out _)){ink.RoundRect(bx-.27f,y-.05f,.54f,.4f,.07f,P.Bollard);ink.Rect(bx-.27f,y+.3f,.54f,.04f,Color.Lerp(P.Bollard,P.DeckEdge,.5f));}
            if(!course.Endless && course.Length>=x0 && course.Length<x1+2)Finish(ink,course);
            if(road!=null)
            {
                foreach(var r in road.Rails)if(r.X1>x0-.5f && r.X0<x1+.5f && r.X0>=x0-.001f && r.X0<x1)Bunting(ink,course,r);
                foreach(var pr in road.Props)if(pr.X>=x0 && pr.X<x1)Crates(ink,pr);
            }
            // deck, arches and water openings; broken bridge ends at gaps
            float step=.2f;
            for(float x=x0;x<x1-1e-4f;x+=step)
            {
                float nx=Mathf.Min(x1,x+step);
                bool ha=LowerAt(course,x,out float a),hb=LowerAt(course,nx,out float b);
                if(!ha||!hb)
                {
                    // find the exact edge inside this strip and draw a broken masonry face there
                    if(ha!=hb)
                    {
                        float lo=x,hi=nx;for(int i=0;i<10;i++){float m=(lo+hi)*.5f;bool hm=LowerAt(course,m,out _);if(hm==ha)lo=m;else hi=m;}
                        float edge=ha?lo:hi;LowerAt(course,edge+(ha?-.001f:.001f),out float ey);
                        BrokenEnd(ink,edge,ey,ha?1:-1,k);
                        if(ha){DeckStrip(ink,x,edge,a,ey,-9,-9);}else{DeckStrip(ink,edge,nx,ey,b,-9,-9);}
                    }
                    continue;
                }
                DeckStrip(ink,x,nx,a,b,ArchUnder(course,x),ArchUnder(course,nx));
            }
            // pale lip marks on kickers
            var lip=P.Sun;lip.a=.85f;
            foreach(var s in course.Surfaces)if(s.Lip && s.End>x0 && s.End<=x1+.01f)
            {for(float x=s.End-.7f;x<s.End-.02f;x+=.1f)ink.Line(new Vector2(x,s.Height(x)+.03f),new Vector2(x+.1f,s.Height(x+.1f)+.03f),.05f,lip);}
        }
        static void DeckStrip(Ink ink,float x,float nx,float a,float b,float ua,float ub)
        {
            ink.Quad(new Vector2(x,ua),new Vector2(nx,ub),new Vector2(nx,b-.2f),new Vector2(x,a-.2f),P.DeckShade,P.DeckShade,P.Deck,P.Deck);
            ink.Quad(new Vector2(x,a-.2f),new Vector2(nx,b-.2f),new Vector2(nx,b),new Vector2(x,a),P.DeckTop);
            ink.Quad(new Vector2(x,a-.215f),new Vector2(nx,b-.215f),new Vector2(nx,b-.185f),new Vector2(x,a-.185f),P.DeckEdge);
            ink.Quad(new Vector2(x,a-.012f),new Vector2(nx,b-.012f),new Vector2(nx,b+.018f),new Vector2(x,a+.018f),Color.Lerp(P.DeckTop,P.DeckEdge,.6f));
        }
        // A broken bridge end: jagged masonry face, a few fallen stones at the waterline.
        static void BrokenEnd(Ink ink,float x,float top,int dir,int k)
        {
            var pts=new System.Collections.Generic.List<Vector2>();
            float[] jag={0,.18f,.05f,.26f,.1f,.3f,.12f,.22f};
            pts.Add(new Vector2(x-dir*.35f,top));
            for(int i=0;i<jag.Length;i++)pts.Add(new Vector2(x+dir*jag[i]*.6f,top-i*.55f));
            pts.Add(new Vector2(x-dir*.35f,top-(jag.Length-1)*.55f));
            ink.Polygon(pts,P.DeckShade);
            ink.Quad(new Vector2(x-dir*.35f,top-.2f),new Vector2(x,top-.2f),new Vector2(x+dir*.08f,top),new Vector2(x-dir*.35f,top),P.DeckTop);
            var r=new Rng(k*131+(int)(x*7));
            for(int i=0;i<3;i++){float sx=x+dir*r.Range(.3f,1.6f),sw=r.Range(.2f,.45f);ink.Polygon(new[]{new Vector2(sx-sw,-2.3f),new Vector2(sx+sw,-2.3f),new Vector2(sx+sw*.5f,-2.3f+sw*.7f),new Vector2(sx-sw*.3f,-2.3f+sw*.8f)},i%2==0?P.DeckShade:P.RockShade);}
        }
        static bool NearRail(PortoRoad road,float x){if(road==null)return false;foreach(var r in road.Rails)if(x>r.X0-1.5f && x<r.X1+1.5f)return true;return false;}
        // Festival bunting strung between two posts: the rope is the grind line.
        static void Bunting(Ink ink,Course course,PortoRoad.Rail r)
        {
            LowerAt(course,r.X0,out float g0);LowerAt(course,r.X1,out float g1);
            ink.Rect(r.X0-.06f,g0,.08f,r.Y0-g0+.35f,P.Lamp);ink.Rect(r.X1-.02f,g1,.08f,r.Y1-g1+.35f,P.Lamp);
            ink.Ellipse(new Vector2(r.X0-.02f,r.Y0+.36f),.07f,.07f,P.LampGlass);ink.Ellipse(new Vector2(r.X1+.02f,r.Y1+.36f),.07f,.07f,P.LampGlass);
            ink.Line(new Vector2(r.X0,r.Y0),new Vector2(r.X1,r.Y1),.05f,P.Rail);
            ink.Line(new Vector2(r.X0,r.Y0+.02f),new Vector2(r.X1,r.Y1+.02f),.015f,Color.Lerp(P.Rail,P.Sun,.35f));
            int n=Mathf.FloorToInt((r.X1-r.X0)/.42f);
            for(int i=0;i<n;i++)
            {
                float x=r.X0+.2f+i*.42f,y=r.Y(x)-.03f;
                Color c=i%4==0?P.Laundry1:i%4==1?P.Flower:i%4==2?P.Shutter:P.Lemon;
                ink.Triangle(new Vector2(x-.13f,y),new Vector2(x+.13f,y),new Vector2(x+.01f,y-.3f),c);
            }
        }
        // Wooden lemon crates: the obstacles to hop.
        static void Crates(Ink ink,PortoRoad.Prop p)
        {
            int rows=p.Kind==1?2:1;float ch=p.H/rows;
            for(int i=0;i<rows;i++)
            {
                float y=p.Base+i*ch,x=p.X+(i==1?.06f:0),w=p.W-(i==1?.12f:0);
                ink.Rect(x,y,w,ch,P.Wood);ink.Rect(x+w*.8f,y,w*.2f,ch,P.WoodShade);
                for(int s2=1;s2<3;s2++)ink.Rect(x,y+ch*s2/3f-.02f,w,.035f,P.WoodShade);
                ink.Rect(x-.02f,y+ch-.05f,w+.04f,.05f,Color.Lerp(P.Wood,P.Sun,.3f));
            }
            float top=p.Base+p.H;int lemons=Mathf.Max(3,Mathf.FloorToInt(p.W/.2f));
            for(int i=0;i<lemons;i++){float lx=p.X+.12f+i*(p.W-.24f)/(lemons-1);ink.Ellipse(new Vector2(lx,top+.08f+(i%2)*.05f),.1f,.075f,P.Lemon);}
            ink.Leaf(new Vector2(p.X+p.W*.5f,top+.15f),40,.22f,.09f,P.LemonLeaf);
        }
        // Grind sparks and boost wind streaks around the rider.
        public static void Effects(Ink ink,Motor motor,Vector2 pos,float time,bool still)
        {
            var p=motor.RenderPose;
            if(p.Mode==1)
            {
                for(int i=0;i<9;i++)
                {
                    float t=Mathf.Repeat(time*3.1f+i*.137f,1),a=Hash(i,(int)(time*8))*40+150;
                    Vector2 d=Ink.Rotate(Vector2.right,a)*(.15f+t*.55f);var c=P.Spark;c.a=1-t;
                    ink.Line(pos+new Vector2(.05f,.02f)+d*.6f,pos+new Vector2(.05f,.02f)+d,.03f*(1-t)+.01f,c);
                }
            }
            if(motor is MopedMotor mm && mm.Boost>0 && !still)
            {
                var c=P.Paper;
                for(int i=0;i<6;i++)
                {
                    float y=pos.y+.2f+Hash(i,3)*1.4f,len=.8f+Hash(i,4)*1.4f;float x=pos.x-1.2f-Mathf.Repeat(time*14+i*1.7f,4);
                    c.a=.35f*Mathf.Clamp01(mm.Boost);ink.Rect(x-len,y,len,.025f,c);
                }
            }
        }
        // Lemons to collect (drawn per frame so they can disappear and bob).
        public static void Lemons(Ink ink,PortoRoad road,float camX,float camY,float hw,float time)
        {
            foreach(var l in road.Lemons)
            {
                if(l.Taken||l.X<camX-hw-1||l.X>camX+hw+1)continue;
                Vector2 c=new Vector2(l.X-camX,l.Y-camY+Mathf.Sin(time*3+l.X)*.05f);
                var glow=P.Lemon;glow.a=.25f;ink.Ellipse(c,.24f,.24f,glow);
                ink.Ellipse(c,.15f,.11f,P.Lemon);ink.Ellipse(c+new Vector2(-.03f,.03f),.06f,.035f,Color.Lerp(P.Lemon,P.Sun,.6f));
                ink.Triangle(c+new Vector2(.14f,-.02f),c+new Vector2(.14f,.02f),c+new Vector2(.19f,0),P.Lemon);
                ink.Leaf(c+new Vector2(-.02f,.09f),60,.14f,.07f,P.LemonLeaf);
            }
        }
        const float ArchSpacing=3.9f,ArchRadius=1.25f,ArchRise=1.3f,DeckDepth=1.2f;
        // Underside of the deck: arch intrados over the water, or piers down below the screen.
        static float ArchUnder(Course c,float x)
        {
            float cell=Mathf.Floor(x/ArchSpacing),cx=(cell+.5f)*ArchSpacing,d=(x-cx)/ArchRadius;
            if(Mathf.Abs(d)>=1)return -9;
            if(!LowerAt(c,cx-ArchRadius-.4f,out float l)||!LowerAt(c,cx,out float m)||!LowerAt(c,cx+ArchRadius+.4f,out float r))return -9;
            float crown=Mathf.Min(l,Mathf.Min(m,r))-DeckDepth;
            return crown-ArchRise+ArchRise*Mathf.Sqrt(1-d*d);
        }
        static bool NearGate(Course c,float x,float r){foreach(float g in c.Arches)if(x>g-r && x<g+2.2f+r)return true;return false;}
        static void Promenade(Ink ink,Course course,Surface s,float x0,float x1)
        {
            float a=Mathf.Max(s.Start,x0),b=Mathf.Min(s.End,x1);if(b<=a)return;
            // arcade columns and arches beneath, standing on the lower road
            for(float cx=Mathf.Ceil(s.Start/1.8f)*1.8f+.9f;cx<s.End-.3f;cx+=1.8f)
            {
                if(cx<a-.1f||cx>=b+.1f)continue;
                float top=s.Height(cx)-.34f,ground=Lower(course,cx);if(top-ground<.35f)continue;
                ink.Rect(cx-.13f,ground,.26f,top-ground,P.Promenade);
                ink.Rect(cx-.17f,top-.1f,.34f,.1f,Color.Lerp(P.Promenade,P.PromenadeTop,.3f));
            }
            for(float x=a;x<b-1e-4f;x+=.2f)
            {
                float nx=Mathf.Min(b,x+.2f),ha=s.Height(x),hb=s.Height(nx);
                ink.Quad(new Vector2(x,ha-.34f),new Vector2(nx,hb-.34f),new Vector2(nx,hb),new Vector2(x,ha),P.Promenade);
                ink.Quad(new Vector2(x,ha-.03f),new Vector2(nx,hb-.03f),new Vector2(nx,hb+.02f),new Vector2(x,ha+.02f),P.PromenadeTop);
                // balustrade on the back edge
                ink.Quad(new Vector2(x,ha+.42f),new Vector2(nx,hb+.42f),new Vector2(nx,hb+.47f),new Vector2(x,ha+.47f),P.Promenade);
            }
            for(float px=Mathf.Ceil(a/.16f)*.16f;px<b;px+=.16f){float h=s.Height(px);ink.Rect(px-.025f,h,.05f,.42f,Color.Lerp(P.Promenade,P.Deck,.35f));}
            // planters with flowers at the promenade ends
            if(s.Start>=x0 && s.Start<x1 && s.A>.5f)Planter(ink,s.Start+.3f,s.A);
        }
        static void Planter(Ink ink,float x,float y)
        {
            ink.Rect(x,y,.5f,.28f,P.NearWallWarm);ink.Rect(x-.03f,y+.26f,.56f,.05f,Color.Lerp(P.NearWallWarm,P.Sun,.3f));
            for(int i=0;i<5;i++)ink.Leaf(new Vector2(x+.25f,y+.3f),40+i*25,.32f,.12f,P.Shrub);
            for(int i=0;i<3;i++)ink.Ellipse(new Vector2(x+.12f+i*.13f,y+.5f+(i%2)*.07f),.05f,.05f,P.Flower,8);
        }
        static void Gate(Ink ink,Course course,float g)
        {
            // Town gate across the road in warm stone: the low arch to pass under with both wheels down.
            float y=Lower(course,g+1.1f),left=g-.45f,right=g+2.65f,top=y+2.75f;
            Color stone=Color.Lerp(P.RampartWall,P.NearShade,.35f),stoneLit=Color.Lerp(stone,P.Sun,.18f),ring=Color.Lerp(stone,P.Sun,.32f);
            System.Func<float,float> under=x=>{float d=(x-(g+1.1f))/1.28f;return d*d>=1?y:y+1.2f+.45f*Mathf.Sqrt(1-d*d);};
            for(float x=left;x<right-1e-4f;x+=.1f)
            {float nx=x+.1f;ink.Quad(new Vector2(x,under(x)),new Vector2(nx,under(nx)),new Vector2(nx,top),new Vector2(x,top),x<g+1.1f?stoneLit:stone);}
            // pilasters, cornice and a low parapet
            ink.Rect(left,y,.22f,top-y,Color.Lerp(stone,P.Deck,.12f));ink.Rect(right-.22f,y,.22f,top-y,Color.Lerp(stone,P.Deck,.12f));
            ink.Rect(left-.1f,top,right-left+.2f,.1f,ring);
            ink.Rect(left,top+.1f,right-left,.34f,stone);
            ink.Rect(left-.1f,top+.44f,right-left+.2f,.08f,ring);
            // arch ring and keystone
            for(float t=0;t<1;t+=.05f){float x=Mathf.Lerp(g-.18f,g+2.38f,t),nx=Mathf.Lerp(g-.18f,g+2.38f,t+.05f);ink.Line(new Vector2(x,under(x)+.04f),new Vector2(nx,under(nx)+.04f),.09f,ring);}
            ink.Quad(new Vector2(g+1.0f,y+1.62f),new Vector2(g+1.2f,y+1.62f),new Vector2(g+1.25f,y+1.95f),new Vector2(g+.95f,y+1.95f),ring);
            // small planted urns on the parapet
            for(int i=0;i<2;i++)
            {
                float ux=i==0?left+.25f:right-.55f,uy=top+.52f;
                ink.Quad(new Vector2(ux,uy),new Vector2(ux+.3f,uy),new Vector2(ux+.36f,uy+.22f),new Vector2(ux-.06f,uy+.22f),P.NearWallWarm);
                for(int l=0;l<6;l++)ink.Leaf(new Vector2(ux+.15f,uy+.22f),40+l*20,.32f,.12f,l%2==0?P.Shrub:P.ShrubShade);
                ink.Ellipse(new Vector2(ux+.1f,uy+.42f),.045f,.045f,P.Flower,8);ink.Ellipse(new Vector2(ux+.24f,uy+.38f),.045f,.045f,P.Flower,8);
            }
        }
        static void Lamp(Ink ink,float x,float y)
        {
            ink.Rect(x-.03f,y,.06f,2.05f,P.Lamp);ink.Rect(x-.07f,y,.14f,.22f,P.Lamp);ink.Rect(x-.05f,y+.22f,.1f,.05f,P.Lamp);
            ink.Line(new Vector2(x,y+2.0f),new Vector2(x+.24f,y+2.1f),.03f,P.Lamp);
            ink.Quad(new Vector2(x+.17f,y+1.84f),new Vector2(x+.31f,y+1.84f),new Vector2(x+.34f,y+2.07f),new Vector2(x+.14f,y+2.07f),P.Lamp);
            ink.Rect(x+.185f,y+1.87f,.11f,.16f,P.Evening?P.LampGlass:Color.Lerp(P.LampGlass,P.Lamp,.35f));
            if(P.Evening){var glow=P.LampGlass;glow.a=.18f;ink.Ellipse(new Vector2(x+.24f,y+1.95f),.4f,.4f,glow);}
        }
        static void Finish(Ink ink,Course course)
        {
            // Arrival: bunting strung across the road between two posts.
            float a=course.Length-1.2f,b=course.Length+2.4f,ya=Lower(course,a),yb=Lower(course,b);
            ink.Rect(a-.04f,ya,.08f,2.9f,P.Lamp);ink.Rect(b-.04f,yb,.08f,2.9f,P.Lamp);
            Vector2 p=new Vector2(a,ya+2.8f),q=new Vector2(b,yb+2.8f);
            System.Func<float,Vector2> at=t=>Vector2.Lerp(p,q,t)-new Vector2(0,Mathf.Sin(t*Mathf.PI)*.35f);
            for(int i=0;i<12;i++){var s=at(i/12f);var e=at((i+1)/12f);ink.Line(s,e,.02f,P.Lamp);var m=(s+e)*.5f;ink.Triangle(s,e,m-new Vector2(0,.28f),i%3==0?P.Laundry1:i%3==1?P.Flower:P.Shutter);}
        }

        // ---------- foreground planting ----------
        static void Foreground(Ink ink,int k)
        {
            var r=new Rng(k*5051+29+(k<-1||k>0?Variant*100003:0));float x0=k*20;
            if(k!=0 && !r.Chance(.45f))return;
            float cx=k==0?2.5f:x0+r.Range(3,17),baseY=-2.7f;
            int leaves=r.Int(24,38);
            for(int i=0;i<leaves;i++)
            {
                float rx=cx+r.Range(-1.8f,1.8f),ang=r.Range(55,125)+(rx-cx)*14;
                float len=r.Range(.8f,1.7f)*(1-Mathf.Abs(rx-cx)/3.4f);
                var root=new Vector2(rx,baseY+r.Range(0,.7f));var tip=root+Ink.Rotate(Vector2.right,ang)*len;
                if(tip.y>-.7f)len*=(-.7f-root.y)/Mathf.Max(.1f,tip.y-root.y);
                ink.Leaf(root,ang,len,len*r.Range(.32f,.42f),i%4==0?P.LeafLight:P.Leaf);
            }
            if(k==0 || r.Chance(.55f))
            {
                int flowers=r.Int(3,7);
                for(int f=0;f<flowers;f++)
                {
                    Vector2 c=new Vector2(cx+r.Range(-1.2f,1.2f),baseY+r.Range(.9f,1.7f));
                    if(c.y>-.9f)c.y=-.9f;
                    for(int p=0;p<5;p++)ink.Leaf(c,p*72+r.Range(0,30),.2f,.13f,P.Flower);
                    ink.Ellipse(c,.04f,.04f,P.FlowerLight,8);
                }
            }
        }
    }
}
