using UnityEngine;
namespace Fosters.Studio
{
    public sealed class Landscape
    {
        readonly Ink ink;readonly GameModule module;
        public Landscape(Ink i,GameModule m){ink=i;module=m;}
        static float Hash(int n){return Mathf.Repeat(Mathf.Sin(n*127.1f+311.7f)*43758.5453f,1);}
        public void Draw(Course course,Motor motor,Progress prefs,float cameraX,float cameraY,float halfWidth,float time)
        {
            var p=module.Palette(prefs.Dusk);
            ink.Dark=p[6];ink.Cream=p[1];ink.Coral=p[7];ink.Teal=p[4];ink.Begin();
            float left=-halfWidth-2,right=halfWidth+2;
            ink.Rect(left,-12,right-left,24,p[0]);
            ink.Ellipse(new Vector2(halfWidth*.49f,5.25f),.53f,.53f,p[1],48);
            float motion=prefs.ReducedMotion?0:1;
            Hills(left,right,-.1f,cameraX*.07f*motion,1.45f,p[2],.15f);
            Hills(left,right,-.8f,cameraX*.14f*motion,1.05f,Color.Lerp(p[2],p[3],.55f),1.7f);
            Town(left,right,-1.3f,cameraX*.21f*motion,p[3],p[2],false,time,prefs.ReducedMotion);
            ink.Rect(left,-5,right-left,3.45f,p[4]);
            // Broad quiet water, only a few moving reflected strips.
            Color waterLight=Color.Lerp(p[4],p[1],.22f);
            for(int j=0;j<8;j++){float x=Mathf.Repeat(j*7.31f-cameraX*.12f,halfWidth*2+4)-halfWidth-2;ink.Rect(x,-2.05f-j*.31f,.8f+Hash(j)*2,.025f,waterLight);}
            float boatX=Mathf.Repeat(8-cameraX*.18f+(prefs.ReducedMotion?0:time*.09f),halfWidth*2+10)-halfWidth-5;
            Boat(boatX,-2.3f,p[6],p[1]);
            Town(left,right,-4.25f,cameraX*.43f*motion,p[5],p[6],true,time,prefs.ReducedMotion);
            // A background canal bridge is deliberately lighter and well behind the active course.
            float bx=Mathf.Repeat(18-cameraX*.35f,54)-20;
            Bridge(bx,-3.6f,4.2f,p[3],p[4]);
            float offset=-cameraX, floor=-11;
            foreach(var s in course.Surfaces)
            {
                if(s.End+offset<left || s.Start+offset>right)continue;
                float start=Mathf.Max(s.Start,left-offset),end=Mathf.Min(s.End,right-offset);
                Color body=s.Upper?Color.Lerp(p[6],p[5],.25f):p[6];
                for(float x=start;x<end;x+=.35f)
                {
                    float nx=Mathf.Min(end,x+.35f),a=s.Height(x)-cameraY,b=s.Height(nx)-cameraY;
                    float bottom=s.Upper?Mathf.Min(a,b)-.22f:floor;
                    ink.Quad(new Vector2(x+offset,bottom),new Vector2(nx+offset,bottom),new Vector2(nx+offset,b),new Vector2(x+offset,a),body);
                    ink.Line(new Vector2(x+offset,a),new Vector2(nx+offset,b),prefs.Contrast?.095f:.045f,prefs.Contrast?p[1]:Color.Lerp(p[5],p[1],.45f));
                }
                if(s.Upper)
                {
                    for(float x=s.Start+1.5f;x<s.End;x+=4)if(x+offset>left && x+offset<right)
                    {float top=s.Height(x)-cameraY;ink.Rect(x+offset,-10,.13f,top+10,p[6]);}
                    if(s.Start+offset>left && s.Start+offset<right)Planter(s.Start+offset+.3f,s.A-cameraY,p);
                }
                if(s.Lip && s.End+offset>left && s.End+offset<right)
                {float h=s.B-cameraY;ink.Line(new Vector2(s.End+offset-.6f,h+.07f),new Vector2(s.End+offset,h+.07f),.065f,p[1]);}
            }
            foreach(float x in course.Arches)
            {
                if(x+offset<left-2 || x+offset>right+2)continue;
                float y=course.Ground(x)-cameraY;
                // Posts behind the travel plane, with readable low clearance in the active silhouette.
                ink.Rect(x+offset-.18f,y+1.65f,2.4f,.55f,p[6]);
                ink.Rect(x+offset-.18f,y-1.2f,.18f,3.4f,Color.Lerp(p[5],p[6],.6f));
                ink.Rect(x+offset+2.08f,y-1.2f,.18f,3.4f,Color.Lerp(p[5],p[6],.6f));
                ink.Line(new Vector2(x+offset,y+1.62f),new Vector2(x+offset+2.2f,y+1.62f),.05f,p[1]);
            }
            // Sparse roadside motifs, never across the character contact or a landing edge.
            for(int j=Mathf.FloorToInt((cameraX-halfWidth)/22);j<(cameraX+halfWidth)/22+1;j++)
            {
                float wx=j*22+14, y=course.Ground(wx);
                if(y>-50)Planter(wx+offset,y-cameraY-.55f,p);
            }
            float finish=course.Length-cameraX;
            if(finish<right+4){float y=course.Ground(course.Length-.1f)-cameraY;ink.Rect(finish,y,.09f,2.4f,p[1]);ink.Triangle(new Vector2(finish,y+2.4f),new Vector2(finish+.9f,y+2.13f),new Vector2(finish,y+1.87f),p[7]);}
            var r=motor.RenderPose;Vector2 pos=new Vector2(r.X-cameraX,r.Y-cameraY);
            float ground=course.Ground(r.X);
            if(ground>-50)ink.Ellipse(new Vector2(pos.x,ground-cameraY+.02f),.32f,.025f,Color.Lerp(p[6],p[1],.2f));
            module.DrawRider(ink,motor,pos,time);
            ink.End();
        }
        void Hills(float left,float right,float baseline,float offset,float height,Color c,float phase)
        {
            for(float x=left;x<right;x+=.35f)
            {float a=baseline+height*(.6f+.32f*Mathf.Sin((x+offset)*.27f+phase)+.22f*Mathf.Sin((x+offset)*.59f));float nx=x+.35f;float b=baseline+height*(.6f+.32f*Mathf.Sin((nx+offset)*.27f+phase)+.22f*Mathf.Sin((nx+offset)*.59f));ink.Quad(new Vector2(x,-6),new Vector2(nx,-6),new Vector2(nx,b),new Vector2(x,a),c);}
        }
        void Town(float left,float right,float baseline,float offset,Color plaster,Color accent,bool near,float time,bool still)
        {
            int first=Mathf.FloorToInt((left+offset)/3.2f)-1;
            for(int n=first;n<(right+offset)/3.2f+1;n++)
            {
                if(Hash(n+13)>.73f)continue;
                float x=n*3.2f-offset,w=1.1f+Hash(n)*1.5f,h=(near?1.4f:.5f)+Hash(n+2)*(near?1.5f:1.1f);
                ink.Rect(x,baseline,w,h,plaster);
                ink.Triangle(new Vector2(x-.1f,baseline+h),new Vector2(x+w+.1f,baseline+h),new Vector2(x+w*.43f,baseline+h+.22f),Color.Lerp(plaster,accent,.3f));
                if(!near)continue;
                Color shade=Color.Lerp(plaster,accent,.42f);
                ink.Rect(x+w*.5f,baseline,.26f,.68f,shade);
                for(int j=0;j<2;j++)ink.Rect(x+.23f+j*w*.5f,baseline+h-.6f,.24f,.36f,shade);
                if(n%3==0)
                {
                    ink.Line(new Vector2(x+.2f,baseline+h-.12f),new Vector2(x+w-.1f,baseline+h-.16f),.018f,shade);
                    float sway=still?0:Mathf.Sin(time*1.1f+n)*.045f;
                    ink.Quad(new Vector2(x+.5f,baseline+h-.12f),new Vector2(x+.88f,baseline+h-.13f),new Vector2(x+.88f+sway,baseline+h-.45f),new Vector2(x+.5f+sway,baseline+h-.44f),Color.Lerp(plaster,ink.Cream,.65f));
                }
                if(n%5==0)
                {float px=x+w+.28f;ink.Ellipse(new Vector2(px,baseline+.57f),.075f,.085f,shade);ink.Capsule(new Vector2(px,baseline+.46f),new Vector2(px+.04f,baseline+.2f),.10f,shade);ink.Line(new Vector2(px,baseline+.4f),new Vector2(px+.22f,baseline+.38f+(still?0:Mathf.Sin(time*.65f)*.07f)),.04f,shade);}
            }
        }
        void Boat(float x,float y,Color dark,Color cream)
        {ink.Quad(new Vector2(x-.6f,y+.15f),new Vector2(x+.7f,y+.15f),new Vector2(x+.45f,y),new Vector2(x-.45f,y),Color.Lerp(dark,cream,.3f));ink.Rect(x-.35f,y+.15f,.63f,.22f,cream);ink.Rect(x-.1f,y+.37f,.04f,.28f,dark);}
        void Bridge(float x,float y,float width,Color c,Color water)
        {ink.Rect(x,y,width,1.1f,c);ink.Ellipse(new Vector2(x+width/2,y),width*.34f,.83f,water,32);ink.Rect(x-.1f,y+1.1f,width+.2f,.13f,c);}
        void Planter(float x,float y,Color[] p)
        {ink.Rect(x,y-.18f,.42f,.18f,p[5]);ink.Ellipse(new Vector2(x+.12f,y+.12f),.18f,.21f,Color.Lerp(p[4],p[6],.2f),12);ink.Ellipse(new Vector2(x+.33f,y+.09f),.17f,.16f,Color.Lerp(p[4],p[6],.2f),12);}
    }
}
