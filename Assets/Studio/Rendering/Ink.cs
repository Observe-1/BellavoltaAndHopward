using System.Collections.Generic;
using UnityEngine;
namespace Fosters.Studio
{
    // Original vector artwork builder. Pure geometry: triangles with per-vertex colour, in world units.
    // InkLayer turns a builder into a Unity mesh; previews and tests can use the builder directly.
    public sealed class Ink
    {
        public readonly List<Vector3> Vertices=new List<Vector3>(8192);
        public readonly List<Color32> Colors=new List<Color32>(8192);
        public readonly List<int> Indices=new List<int>(16384);
        public Color Dark, Cream, Coral, Teal;
        // Translation applied to every vertex; used by baked tiles and the preview compositor.
        public Vector2 Offset;
        // Uniform scale applied before Offset; baked parallax tiles use it to set their depth scale.
        public float Scale=1;
        // Screen density used for curve level of detail (pixels per world unit at rest).
        public static float PixelsPerUnit=75;
        public int Count=>Indices.Count/3;
        public void Begin(){Vertices.Clear();Colors.Clear();Indices.Clear();}
        public void End(){}
        int V(Vector2 p,Color c){Vertices.Add(new Vector3(p.x*Scale+Offset.x,p.y*Scale+Offset.y,0));Colors.Add(c);return Vertices.Count-1;}
        public static int Segments(float radius,int min=6,int max=72)
        {
            float px=Mathf.Max(.5f,radius*PixelsPerUnit);
            return Mathf.Clamp(Mathf.CeilToInt(Mathf.PI/Mathf.Sqrt(1f/px)),min,max);
        }
        public void Triangle(Vector2 a,Vector2 b,Vector2 c,Color color){int n=V(a,color);V(b,color);V(c,color);Indices.Add(n);Indices.Add(n+1);Indices.Add(n+2);}
        public void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color){Quad(a,b,c,d,color,color,color,color);}
        public void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color ca,Color cb,Color cc,Color cd)
        {int n=V(a,ca);V(b,cb);V(c,cc);V(d,cd);Indices.Add(n);Indices.Add(n+1);Indices.Add(n+2);Indices.Add(n);Indices.Add(n+2);Indices.Add(n+3);}
        public void Rect(float x,float y,float w,float h,Color color){Quad(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),color);}
        // Vertical gradient rectangle: bottom colour to top colour.
        public void Gradient(float x,float y,float w,float h,Color bottom,Color top)
        {Quad(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),bottom,bottom,top,top);}
        public void Ellipse(Vector2 center,float rx,float ry,Color color,int sides=-1)
        {
            if(sides<3)sides=Segments(Mathf.Max(rx,ry)*Scale);
            int c=V(center,color),first=V(center+new Vector2(rx,0),color),last=first;
            for(int i=1;i<=sides;i++)
            {
                int next=i==sides?first:V(center+new Vector2(Mathf.Cos(i*Mathf.PI*2/sides)*rx,Mathf.Sin(i*Mathf.PI*2/sides)*ry),color);
                Indices.Add(c);Indices.Add(last);Indices.Add(next);last=next;
            }
        }
        // Filled elliptical sector from angle a0 to a1 (degrees, counter-clockwise).
        public void Sector(Vector2 center,float rx,float ry,float a0,float a1,Color color,int sides=-1)
        {
            if(sides<2)sides=Mathf.Max(2,Mathf.CeilToInt(Segments(Mathf.Max(rx,ry)*Scale)*Mathf.Abs(a1-a0)/360f));
            int c=V(center,color),last=V(center+new Vector2(Mathf.Cos(a0*Mathf.Deg2Rad)*rx,Mathf.Sin(a0*Mathf.Deg2Rad)*ry),color);
            for(int i=1;i<=sides;i++)
            {float a=Mathf.Lerp(a0,a1,i/(float)sides)*Mathf.Deg2Rad;int next=V(center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),color);Indices.Add(c);Indices.Add(last);Indices.Add(next);last=next;}
        }
        // Rectangle topped with a half ellipse: windows, doors, arcade openings.
        public void ArchShape(float x,float y,float w,float h,Color color)
        {float r=w*.5f;if(h>r)Rect(x,y,w,h-r,color);Sector(new Vector2(x+r,y+Mathf.Max(0,h-r)),r,Mathf.Min(r,h),0,180,color);}
        public void RoundRect(float x,float y,float w,float h,float r,Color color)
        {
            r=Mathf.Min(r,Mathf.Min(w,h)*.5f);
            Rect(x+r,y,w-2*r,h,color);Rect(x,y+r,r,h-2*r,color);Rect(x+w-r,y+r,r,h-2*r,color);
            Sector(new Vector2(x+r,y+r),r,r,180,270,color);Sector(new Vector2(x+w-r,y+r),r,r,270,360,color);
            Sector(new Vector2(x+w-r,y+h-r),r,r,0,90,color);Sector(new Vector2(x+r,y+h-r),r,r,90,180,color);
        }
        // Pointed leaf (vesica) from base along an angle.
        public void Leaf(Vector2 root,float degrees,float length,float width,Color color)
        {
            Vector2 d=Rotate(Vector2.right,degrees),n=new Vector2(-d.y,d.x);int steps=Mathf.Clamp(Mathf.CeilToInt(length*PixelsPerUnit/6),3,10);
            int c=V(root+d*length*.5f,color),first=V(root,color),last=first;
            for(int side=0;side<2;side++)for(int i=1;i<=steps;i++)
            {
                float t=i/(float)steps;if(side==1)t=1-t;
                float bulge=Mathf.Sin(t*Mathf.PI)*width*.5f*(side==0?1:-1);
                int next=(side==1 && i==steps)?first:V(root+d*length*t+n*bulge,color);
                Indices.Add(c);Indices.Add(last);Indices.Add(next);last=next;
            }
        }
        public void Line(Vector2 a,Vector2 b,float width,Color color)
        {Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;Quad(a+n,a-n,b-n,b+n,color);}
        public void Capsule(Vector2 a,Vector2 b,float w,Color color)
        {Line(a,b,w,color);int s=Segments(w*.5f,6,20);Ellipse(a,w/2,w/2,color,s);Ellipse(b,w/2,w/2,color,s);}
        public void Limb(Vector2 root,Vector2 end,float length,float bend,float width,Color color)
        {
            Vector2 d=end-root;float dist=d.magnitude;
            Vector2 middle=(root+end)*.5f+new Vector2(-d.y,d.x).normalized*Mathf.Sqrt(Mathf.Max(.001f,length*length-dist*dist*.25f))*bend;
            Capsule(root,middle,width,color);Capsule(middle,end,width,color);
        }
        // Height-field silhouette between x0 and x1: top(x) down to bottom, optional vertical gradient.
        public void Ridge(float x0,float x1,float step,System.Func<float,float> top,float bottom,Color color){Ridge(x0,x1,step,top,bottom,color,color);}
        public void Ridge(float x0,float x1,float step,System.Func<float,float> top,float bottom,Color topColor,Color bottomColor)
        {
            float x=x0,a=top(x0);
            while(x<x1-1e-4f)
            {
                float nx=Mathf.Min(x1,x+step),b=top(nx);
                Quad(new Vector2(x,bottom),new Vector2(nx,bottom),new Vector2(nx,b),new Vector2(x,a),bottomColor,bottomColor,topColor,topColor);
                x=nx;a=b;
            }
        }
        // Simple polygon (any winding, no holes) by ear clipping. Intended for baked, not per-frame, art.
        public void Polygon(IList<Vector2> points,Color color)
        {
            int n=points.Count;if(n<3)return;
            var idx=new List<int>(n);float area=0;
            for(int i=0;i<n;i++){idx.Add(i);var p=points[i];var q=points[(i+1)%n];area+=p.x*q.y-q.x*p.y;}
            if(area<0)idx.Reverse();
            int start=Vertices.Count;foreach(var p in points)V(p,color);
            int guard=0;
            while(idx.Count>3 && guard++<n*n)
            {
                bool clipped=false;
                for(int i=0;i<idx.Count;i++)
                {
                    int ia=idx[(i+idx.Count-1)%idx.Count],ib=idx[i],ic=idx[(i+1)%idx.Count];
                    Vector2 a=points[ia],b=points[ib],c=points[ic];
                    if((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x)<=1e-7f)continue;
                    bool inside=false;
                    foreach(int j in idx){if(j==ia||j==ib||j==ic)continue;if(Inside(points[j],a,b,c)){inside=true;break;}}
                    if(inside)continue;
                    Indices.Add(start+ia);Indices.Add(start+ib);Indices.Add(start+ic);idx.RemoveAt(i);clipped=true;break;
                }
                if(!clipped)break;
            }
            if(idx.Count==3){Indices.Add(start+idx[0]);Indices.Add(start+idx[1]);Indices.Add(start+idx[2]);}
        }
        static bool Inside(Vector2 p,Vector2 a,Vector2 b,Vector2 c)
        {
            float d1=(p.x-b.x)*(a.y-b.y)-(a.x-b.x)*(p.y-b.y),d2=(p.x-c.x)*(b.y-c.y)-(b.x-c.x)*(p.y-c.y),d3=(p.x-a.x)*(c.y-a.y)-(c.x-a.x)*(p.y-a.y);
            bool neg=d1<0||d2<0||d3<0,pos=d1>0||d2>0||d3>0;return !(neg&&pos);
        }
        // Copies another builder's triangles, translated.
        public void Append(Ink other,Vector2 shift)
        {
            int n=Vertices.Count;
            for(int i=0;i<other.Vertices.Count;i++){var v=other.Vertices[i];Vertices.Add(new Vector3(v.x+shift.x,v.y+shift.y,0));Colors.Add(other.Colors[i]);}
            foreach(int i in other.Indices)Indices.Add(n+i);
        }
        public static Vector2 Rotate(Vector2 v,float degrees)
        {float a=degrees*Mathf.Deg2Rad;return new Vector2(v.x*Mathf.Cos(a)-v.y*Mathf.Sin(a),v.x*Mathf.Sin(a)+v.y*Mathf.Cos(a));}
        public static Color Hex(string hex)
        {int v=System.Convert.ToInt32(hex.TrimStart('#'),16);return new Color(((v>>16)&255)/255f,((v>>8)&255)/255f,(v&255)/255f,1);}
        public static Color Mix(Color a,Color b,float t)=>Color.Lerp(a,b,t);
    }
}
