using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Fosters.Studio
{
    public sealed class StudioUI : MonoBehaviour
    {
        StudioApp app;RectTransform root,safe;Font font;Text distance,feedback,lesson;Rect oldSafe;int oldW,oldH;Color ink,cream;
        public void Initialize(StudioApp a)
        {
            app=a;font=Resources.Load<Font>("Fonts/Jost-Regular");if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=new GameObject("Safe-area interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(960,540);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=1;
            root=canvas.GetComponent<RectTransform>();safe=Rect("Safe area",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            if(!FindAnyObjectByType<EventSystem>())new GameObject("Touch and keyboard events",typeof(EventSystem),typeof(StandaloneInputModule));
            Resize();
        }
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 pos,Vector2 size)
        {var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
        void Resize()
        {var s=Screen.safeArea;safe.anchorMin=new Vector2(s.x/Screen.width,s.y/Screen.height);safe.anchorMax=new Vector2(s.xMax/Screen.width,s.yMax/Screen.height);safe.offsetMin=Vector2.zero;safe.offsetMax=Vector2.zero;oldSafe=s;oldW=Screen.width;oldH=Screen.height;}
        public bool Blocks(Vector2 position)
        {
            var r=Screen.safeArea;if(!r.Contains(position))return true;
            float scale=Screen.height/540f;
            if(position.y>r.yMax-90*scale && position.x>r.xMax-110*scale)return true;
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject(Input.touchCount>0?Input.GetTouch(0).fingerId:-1);
        }
        Text Label(string text,Vector2 anchor,Vector2 pos,Vector2 size,int fontSize,TextAnchor alignment,Color color)
        {
            var r=Rect(text,safe,anchor,anchor,pos,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=fontSize;t.alignment=alignment;t.color=color;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        // Letter spacing as a fraction of the font size.
        static Text Track(Text t,float em){var tr=t.gameObject.AddComponent<Tracking>();tr.Em=em;return t;}
        Button Button(string text,Vector2 anchor,Vector2 pos,Vector2 size,System.Action click,bool filled=true)
        {
            var r=Rect(text,safe,anchor,anchor,pos,size);var image=r.gameObject.AddComponent<Image>();image.color=filled?ink:new Color(cream.r,cream.g,cream.b,.88f);
            var button=r.gameObject.AddComponent<Button>();button.targetGraphic=image;var colors=button.colors;colors.highlightedColor=new Color(.94f,.91f,.85f);colors.pressedColor=new Color(.76f,.75f,.72f);button.colors=colors;
            var tr=Rect("Label",r,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var label=tr.gameObject.AddComponent<Text>();label.font=font;label.text=text;label.fontSize=16;label.alignment=TextAnchor.MiddleCenter;label.color=filled?cream:ink;label.raycastTarget=false;
            if(click!=null)button.onClick.AddListener(()=>click());return button;
        }
        void Control(string text,float x,int action,bool held)
        {
            var b=Button(text,new Vector2(1,0),new Vector2(x,62),new Vector2(170,82),null,false);
            var input=b.gameObject.AddComponent<TouchControl>();input.Down=()=>app.InputReader.Button(action,true,app.Motor.Pose.Grounded);input.Up=()=>{if(held)app.InputReader.Button(action,false,app.Motor.Pose.Grounded);};
        }
        void Toggle(string name,bool value,float x,float y,System.Action<bool> setter)
        {Button(name+"   "+(value?"ON":"OFF"),new Vector2(.5f,.5f),new Vector2(x,y),new Vector2(330,62),()=>{setter(!value);app.Progress.Save();Rebuild();},false);}
        public void Rebuild()
        {
            if(!safe)return;
            foreach(Transform c in safe){c.gameObject.SetActive(false);Destroy(c.gameObject);}distance=feedback=lesson=null;
            var palette=app.Module.Palette(app.Progress.Dusk);ink=palette[6];cream=palette[1];
            if(app.State==ScreenState.Playing)
            {
                distance=Label("",new Vector2(0,1),new Vector2(160,-40),new Vector2(280,45),20,TextAnchor.MiddleLeft,ink);Track(distance,.10f);
                var pauseButton=Button("",Vector2.one,new Vector2(-50,-42),new Vector2(66,66),app.Pause,false);
                pauseButton.targetGraphic.color=new Color(ink.r,ink.g,ink.b,0);
                for(int i=-1;i<=1;i+=2){var bar=Rect("Pause bar",pauseButton.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(i*7,0),new Vector2(7,26));var img=bar.gameObject.AddComponent<Image>();img.color=ink;img.raycastTarget=false;}
                feedback=Label("",new Vector2(.5f,1),new Vector2(0,-40),new Vector2(520,48),17,TextAnchor.MiddleCenter,ink);Track(feedback,.22f);
                lesson=Label("",new Vector2(.5f,0),new Vector2(0,38),new Vector2(520,48),16,TextAnchor.MiddleCenter,cream);
                if(app.Progress.Buttons)
                {
                    Control(app.Module.PrimaryControl,-290,app.Module.IsMotorbike?0:1,app.Module.IsMotorbike);
                    Control(app.Module.SecondaryControl,-100,2,false);
                }
                return;
            }
            // Left paper field leaves the town and rider visible; no dashboard tiles.
            // Home lets the illustrated scene through; other menus sit on warm paper.
            var paper=Rect("Warm paper",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);paper.gameObject.AddComponent<Image>().color=new Color(cream.r,cream.g,cream.b,app.State==ScreenState.Home?.45f:.92f);
            Track(Label("FOSTERSDIGITAL",new Vector2(0,1),new Vector2(174,-35),new Vector2(290,30),12,TextAnchor.MiddleLeft,ink),.45f);
            Track(Label(app.Module.Place.ToUpperInvariant(),new Vector2(1,1),new Vector2(-180,-35),new Vector2(300,30),12,TextAnchor.MiddleRight,ink),.3f);
            var mid=new Vector2(.5f,.5f);
            var inv=System.Globalization.CultureInfo.InvariantCulture;
            if(app.State==ScreenState.Home && app.Module.Endless)
            {
                Label(app.Module.Title,mid,new Vector2(0,125),new Vector2(700,100),70,TextAnchor.MiddleCenter,ink);
                Label(app.Module.Tagline,mid,new Vector2(0,55),new Vector2(620,44),18,TextAnchor.MiddleCenter,ink);
                Button("Ride  ›",mid,new Vector2(0,-24),new Vector2(300,64),()=>app.StartRun(0,true));
                Button("Settings",mid,new Vector2(0,-100),new Vector2(220,62),app.Settings,false);
                if(app.Progress.BestDistance>0)Track(Label("BEST  "+app.Progress.BestDistance.ToString("N0",inv)+" m  ·  "+app.Progress.Best.ToString("N0",inv)+" POINTS",new Vector2(.5f,0),new Vector2(0,40),new Vector2(640,30),13,TextAnchor.MiddleCenter,ink),.2f);
            }
            else if(app.State==ScreenState.Home)
            {
                Label(app.Module.Title,mid,new Vector2(0,125),new Vector2(700,100),70,TextAnchor.MiddleCenter,ink);
                Label(app.Module.Tagline,mid,new Vector2(0,55),new Vector2(620,44),18,TextAnchor.MiddleCenter,ink);
                Button("Begin a journey  ›",mid,new Vector2(0,-24),new Vector2(300,64),()=>app.StartRun(NextOuting(),false));
                Button("Route journal",mid,new Vector2(-140,-100),new Vector2(260,62),()=>app.Change(ScreenState.Journal),false);
                Button("Flow run",mid,new Vector2(140,-100),new Vector2(260,62),()=>app.StartRun(0,true),false);
                Button("Settings",mid,new Vector2(0,-174),new Vector2(200,62),app.Settings,false);
                Label("A little rhythm. A different way through.",new Vector2(.5f,0),new Vector2(0,25),new Vector2(600,28),12,TextAnchor.MiddleCenter,ink);
            }
            else if(app.State==ScreenState.Journal)
            {
                Label("Places, at your own pace",mid,new Vector2(0,170),new Vector2(650,60),34,TextAnchor.MiddleCenter,ink);
                for(int i=0;i<app.Module.Outings.Length;i++)
                {int n=i;bool arrived=(app.Progress.Arrivals&(1<<i))!=0;bool branch=(app.Progress.Branches&(1<<i))!=0;Button((i+1).ToString("00")+"   "+app.Module.Outings[i]+(arrived?"  ·  arrived":"")+(branch?"  ·  upper path":""),mid,new Vector2(0,88-i*74),new Vector2(540,62),()=>app.StartRun(n,false),false);}
                Button("Back",mid,new Vector2(0,-168),new Vector2(200,62),()=>app.Change(ScreenState.Home),false);
            }
            else if(app.State==ScreenState.Settings)
            {
                Label("Make yourself comfortable",mid,new Vector2(0,200),new Vector2(650,46),30,TextAnchor.MiddleCenter,ink);
                Toggle("Separate controls",app.Progress.Buttons,-175,126,v=>app.Progress.Buttons=v);
                Toggle("Steadier scenery",app.Progress.ReducedMotion,-175,54,v=>app.Progress.ReducedMotion=v);
                Toggle("High-contrast path",app.Progress.Contrast,-175,-18,v=>app.Progress.Contrast=v);
                Toggle("Evening light",app.Progress.Dusk,-175,-90,v=>app.Progress.Dusk=v);
                Toggle("Haptics",app.Progress.Haptics,175,126,v=>app.Progress.Haptics=v);
                Button("Music  "+Mathf.RoundToInt(app.Progress.Music*100)+"%",mid,new Vector2(175,54),new Vector2(330,62),()=>{app.Progress.Music=NextVolume(app.Progress.Music);app.Progress.Save();Rebuild();},false);
                Button("Sound  "+Mathf.RoundToInt(app.Progress.Effects*100)+"%",mid,new Vector2(175,-18),new Vector2(330,62),()=>{app.Progress.Effects=NextVolume(app.Progress.Effects);app.Progress.Save();Rebuild();},false);
                Button("Done",mid,new Vector2(175,-90),new Vector2(330,62),app.CloseSettings);
            }
            else if(app.Module.Endless && (app.State==ScreenState.Recovery||app.State==ScreenState.Paused))
            {
                bool pause=app.State==ScreenState.Paused;
                int metres=Mathf.FloorToInt(app.Motor.Pose.X);
                Label(metres.ToString("N0",inv)+" m",mid,new Vector2(0,140),new Vector2(720,80),52,TextAnchor.MiddleCenter,ink);
                string line=app.Score.Banked.ToString("N0",inv)+" points  ·  "+app.Motor.Pickups+" lemons";
                Label(pause?line:app.Feedback,mid,new Vector2(0,78),new Vector2(690,40),18,TextAnchor.MiddleCenter,ink);
                if(!pause)Track(Label(app.NewBest?"NEW BEST  ·  "+line.ToUpperInvariant():line.ToUpperInvariant(),mid,new Vector2(0,44),new Vector2(690,30),13,TextAnchor.MiddleCenter,ink),.18f);
                Button(pause?"Continue  ›":"Ride again  ›",mid,new Vector2(0,-24),new Vector2(360,64),()=>{if(pause)app.Change(ScreenState.Playing);else app.StartRun(0,true);});
                Button(pause?"Settings":"Home",mid,new Vector2(-138,-104),new Vector2(260,62),()=>{if(pause)app.Settings();else app.Change(ScreenState.Home);},false);
                Button(pause?"Restart":"Settings",mid,new Vector2(138,-104),new Vector2(260,62),()=>{if(pause)app.StartRun(0,true);else app.Settings();},false);
                if(pause)Button("Return home",mid,new Vector2(0,-178),new Vector2(260,62),()=>app.Change(ScreenState.Home),false);
            }
            else
            {
                bool pause=app.State==ScreenState.Paused,arrival=app.State==ScreenState.Arrival;
                Label(pause?"Take a breath":arrival?"You have arrived":"Find your rhythm",mid,new Vector2(0,132),new Vector2(720,80),46,TextAnchor.MiddleCenter,ink);
                Label(pause?app.Module.Outings[app.Outing]:arrival?(app.Flow?"A line worth remembering · "+app.Score.Banked+" points":app.Module.Outings[app.Outing]+" · added to your journal"):app.Feedback,mid,new Vector2(0,66),new Vector2(690,50),18,TextAnchor.MiddleCenter,ink);
                Button(pause?"Continue  ›":arrival?"Ride again  ›":app.Flow?"Try again  ›":"Back to the last landmark  ›",mid,new Vector2(0,-10),new Vector2(360,64),()=>{if(pause)app.Change(ScreenState.Playing);else if(arrival)app.StartRun(app.Outing,app.Flow);else app.Recover();});
                Button(pause?"Settings":"Route journal",mid,new Vector2(-138,-90),new Vector2(260,62),()=>{if(pause)app.Settings();else app.Change(ScreenState.Journal);},false);
                Button(pause?"Restart":"Home",mid,new Vector2(138,-90),new Vector2(260,62),()=>{if(pause)app.StartRun(app.Outing,app.Flow);else app.Change(ScreenState.Home);},false);
                if(pause)Button("Return home",mid,new Vector2(0,-164),new Vector2(260,62),()=>app.Change(ScreenState.Home),false);
            }
        }
        int NextOuting(){for(int i=0;i<app.Module.Outings.Length;i++)if((app.Progress.Arrivals&(1<<i))==0)return i;return 0;}
        static float NextVolume(float v){return v<.01f?.25f:v<.3f?.5f:v<.6f?1:0;}
        public void Refresh()
        {
            if(oldSafe!=Screen.safeArea || oldW!=Screen.width || oldH!=Screen.height)Resize();
            if(distance)distance.text=app.Flow?Mathf.FloorToInt(app.Motor.Pose.X).ToString("N0",System.Globalization.CultureInfo.InvariantCulture)+" m     "+app.Score.Banked+(app.Score.Pending>0?"  +"+app.Score.Pending+" ×"+app.Score.Multiplier:""):app.Module.Outings[app.Outing];
            if(feedback)feedback.text=app.FeedbackTime>0?app.Feedback:"";
            if(lesson)
            {
                int index=Mathf.FloorToInt(app.RunTime/7);
                var lessons=app.Module.LessonsFor(app.Outing);
                bool teach=!app.Module.Endless || app.Progress.Runs<3;
                lesson.text=teach && !app.Progress.Buttons && lessons!=null && index<lessons.Length?lessons[index]:"";
            }
        }
    }
    // Letter spacing for single-line legacy Text: shifts each glyph quad and re-centres by alignment.
    public sealed class Tracking : BaseMeshEffect
    {
        public float Em=.15f;
        readonly System.Collections.Generic.List<UIVertex> stream=new System.Collections.Generic.List<UIVertex>();
        public override void ModifyMesh(VertexHelper vh)
        {
            if(!IsActive())return;
            var text=GetComponent<Text>();if(!text)return;
            stream.Clear();vh.GetUIVertexStream(stream);int quads=stream.Count/6;if(quads<2)return;
            float spacing=text.fontSize*Em,total=spacing*(quads-1);
            var a=text.alignment;
            float start=(a==TextAnchor.UpperCenter||a==TextAnchor.MiddleCenter||a==TextAnchor.LowerCenter)?-total*.5f:(a==TextAnchor.UpperRight||a==TextAnchor.MiddleRight||a==TextAnchor.LowerRight)?-total:0;
            for(int q=0;q<quads;q++)for(int k=0;k<6;k++){var v=stream[q*6+k];v.position.x+=start+q*spacing;stream[q*6+k]=v;}
            vh.Clear();vh.AddUIVertexTriangleStream(stream);
        }
    }
    public sealed class TouchControl : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        public System.Action Down,Up;
        public void OnPointerDown(PointerEventData e){Down?.Invoke();}
        public void OnPointerUp(PointerEventData e){Up?.Invoke();}
        public void OnPointerExit(PointerEventData e){Up?.Invoke();}
        void OnDisable(){Up?.Invoke();}
    }
}

