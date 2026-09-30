using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Fosters.Studio
{
    // Original vector artwork, batched into one reusable mesh. Coordinates are world units.
    public sealed class Ink : MonoBehaviour
    {
        readonly List<Vector3> vertices=new List<Vector3>(32768);
        readonly List<Color32> colors=new List<Color32>(32768);
        readonly List<int> indices=new List<int>(65536);
        Mesh mesh; Material material;
        public Color Dark, Cream, Coral, Teal;
        public void Initialize()
        {
            mesh=new Mesh{name="Living landscape"};mesh.indexFormat=IndexFormat.UInt32;mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            material=new Material(Resources.Load<Shader>("FlatInk"));
            gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        public void Begin(){vertices.Clear();colors.Clear();indices.Clear();}
        public void End(){mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();}
        public void Triangle(Vector2 a,Vector2 b,Vector2 c,Color color)
        {int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);indices.Add(n);indices.Add(n+1);indices.Add(n+2);}
        public void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color)
        {int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);for(int i=0;i<4;i++)colors.Add(color);indices.Add(n);indices.Add(n+1);indices.Add(n+2);indices.Add(n);indices.Add(n+2);indices.Add(n+3);}
        public void Rect(float x,float y,float w,float h,Color color){Quad(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),color);}
        public void Ellipse(Vector2 center,float rx,float ry,Color color,int sides=24)
        {Vector2 last=center+new Vector2(rx,0);for(int i=1;i<=sides;i++){float a=i*Mathf.PI*2/sides;Vector2 next=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);Triangle(center,last,next,color);last=next;}}
        public void Line(Vector2 a,Vector2 b,float width,Color color)
        {Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;Quad(a+n,a-n,b-n,b+n,color);}
        public void Capsule(Vector2 a,Vector2 b,float w,Color color)
        {Line(a,b,w,color);Ellipse(a,w/2,w/2,color,10);Ellipse(b,w/2,w/2,color,10);}
        public void Limb(Vector2 root,Vector2 end,float length,float bend,float width,Color color)
        {
            Vector2 d=end-root;float dist=d.magnitude;
            Vector2 middle=(root+end)*.5f+new Vector2(-d.y,d.x).normalized*Mathf.Sqrt(Mathf.Max(.001f,length*length-dist*dist*.25f))*bend;
            Capsule(root,middle,width,color);Capsule(middle,end,width,color);
        }
        public static Vector2 Rotate(Vector2 v,float degrees)
        {float a=degrees*Mathf.Deg2Rad;return new Vector2(v.x*Mathf.Cos(a)-v.y*Mathf.Sin(a),v.x*Mathf.Sin(a)+v.y*Mathf.Cos(a));}
        void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
    }
}
