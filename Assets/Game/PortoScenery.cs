using System.Collections.Generic;
using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    // Runtime for Porto Chiaro. Static art is baked once per tile into its own mesh and only moved
    // each frame (parallax); sky, sea, boats, laundry and the rider are rebuilt per frame.
    public sealed class PortoScenery : IScenery
    {
        sealed class TileSet
        {
            public PortoArt.LayerSpec Spec;public readonly Dictionary<int,InkLayer> Live=new Dictionary<int,InkLayer>();
            public readonly Stack<InkLayer> Pool=new Stack<InkLayer>();
        }
        readonly Transform root;readonly GameModule module;readonly List<TileSet> sets=new List<TileSet>();
        readonly InkLayer sky,sea,boats,laundry,rider;
        Course builtFor;bool builtDusk;readonly List<int> scratch=new List<int>();

        public PortoScenery(Transform parent,GameModule m)
        {
            root=parent;module=m;
            foreach(var spec in PortoArt.Layers)sets.Add(new TileSet{Spec=spec});
            sky=InkLayer.Create("Sky and sun",PortoArt.SkyOrder,root);
            sea=InkLayer.Create("Sea",PortoArt.SeaOrder,root);
            boats=InkLayer.Create("Boats",PortoArt.BoatOrder,root);
            laundry=InkLayer.Create("Laundry",PortoArt.LaundryOrder,root);
            rider=InkLayer.Create("Rider",PortoArt.RiderOrder,root);
        }
        void Invalidate()
        {
            foreach(var s in sets){foreach(var t in s.Live.Values){t.Visible=false;s.Pool.Push(t);}s.Live.Clear();}
        }
        public void Draw(Course course,Motor motor,Progress prefs,float camX,float camY,float hw,float hh,float time)
        {
            if(prefs.Dusk!=builtDusk || PortoArt.P==null){PortoArt.P=prefs.Dusk?PortoPalette.Dusk():PortoPalette.Day();builtDusk=prefs.Dusk;Invalidate();}
            if(course!=builtFor){builtFor=course;PortoArt.Variant=course.Surfaces.Count;Invalidate();}
            bool still=prefs.ReducedMotion;
            foreach(var s in sets)
            {
                var spec=s.Spec;float fx=still && spec.Fx<1?0:spec.Fx;
                PortoArt.TileRange(spec,fx,camX,hw,out int k0,out int k1);
                scratch.Clear();foreach(int k in s.Live.Keys)if(k<k0||k>k1)scratch.Add(k);
                foreach(int k in scratch){var t=s.Live[k];t.Visible=false;s.Pool.Push(t);s.Live.Remove(k);}
                var place=PortoArt.Placement(spec,fx,camX,camY);
                for(int k=k0;k<=k1;k++)
                {
                    if(!s.Live.TryGetValue(k,out var tile))
                    {
                        tile=s.Pool.Count>0?s.Pool.Pop():InkLayer.Create(spec.Name,spec.Order,root);
                        tile.Visible=true;tile.Ink.Begin();tile.Ink.Scale=spec.Scale;spec.Draw(tile.Ink,k,course);tile.Ink.Scale=1;tile.Flush();s.Live[k]=tile;
                    }
                    tile.Place(place.x,place.y);
                }
            }
            sky.Ink.Begin();PortoArt.Sky(sky.Ink,hw,hh,camY);sky.Flush();
            sea.Ink.Begin();PortoArt.Sea(sea.Ink,hw,hh,camX,camY,time,still);sea.Flush();
            boats.Ink.Begin();PortoArt.Boats(boats.Ink,hw,still?0:camX,camY,time,still);boats.Flush();
            // Laundry follows the near-town tiles.
            laundry.Ink.Begin();
            var near=sets.Find(s=>s.Spec.Name=="Near town");
            if(near!=null)
            {
                float fx=still?0:near.Spec.Fx;
                var shift=PortoArt.Placement(near.Spec,fx,camX,camY);
                foreach(int k in near.Live.Keys)PortoArt.Laundry(laundry.Ink,k,shift,near.Spec.Scale,time,still);
            }
            laundry.Flush();
            rider.Ink.Begin();
            var r=motor.RenderPose;Vector2 pos=new Vector2(r.X-camX,r.Y-camY);
            float ground=course.Ground(r.X);
            if(ground>-50){var shadow=PortoArt.P.DeckShade;shadow.a=.55f;float lift=Mathf.Clamp01((r.Y-ground)/2.5f);rider.Ink.Ellipse(new Vector2(pos.x+.47f,ground-camY+.02f),.62f*(1-lift*.5f),.045f,shadow);}
            module.DrawRider(rider.Ink,motor,pos,time);
            rider.Flush();
        }
    }
}
