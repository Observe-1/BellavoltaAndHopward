using UnityEngine;
using UnityEngine.Rendering;
namespace Fosters.Studio
{
    // One mesh on screen: a dynamic per-frame layer or a baked parallax tile. Sorting order sets depth.
    public sealed class InkLayer : MonoBehaviour
    {
        public readonly Ink Ink=new Ink();
        Mesh mesh;static Material shared;
        public static InkLayer Create(string name,int order,Transform parent=null)
        {
            var go=new GameObject(name);if(parent)go.transform.SetParent(parent,false);
            var layer=go.AddComponent<InkLayer>();layer.mesh=new Mesh{name=name};layer.mesh.indexFormat=IndexFormat.UInt32;layer.mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh=layer.mesh;
            if(!shared)shared=new Material(Resources.Load<Shader>("FlatInk"));
            var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=shared;r.sortingOrder=order;
            r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            return layer;
        }
        public void Flush()
        {mesh.Clear();mesh.SetVertices(Ink.Vertices);mesh.SetColors(Ink.Colors);mesh.SetTriangles(Ink.Indices,0,false);mesh.bounds=new Bounds(Vector3.zero,new Vector3(1e5f,1e5f,1));}
        public void Place(float x,float y){transform.localPosition=new Vector3(x,y,0);}
        public bool Visible{set{if(gameObject.activeSelf!=value)gameObject.SetActive(value);}}
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
}
