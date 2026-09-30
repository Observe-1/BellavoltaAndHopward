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
            app=a;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            if(position.y>r.yMax-80*scale && position.x>r.xMax-110*scale)return true;
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject(Input.touchCount>0?Input.GetTouch(0).fingerId:-1);
        }
        Text Label(string text,Vector2 anchor,Vector2 pos,Vector2 size,int fontSize,TextAnchor alignment,Color color)
        {
            var r=Rect(text,safe,anchor,anchor,pos,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=fontSize;t.alignment=alignment;t.color=color;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        Button Button(string text,Vector2 anchor,Vector2 pos,Vector2 size,System.Action click,bool filled=true)
        {
            var r=Rect(text,safe,anchor,anchor,pos,size);var image=r.gameObject.AddComponent<Image>();image.color=filled?ink:new Color(ink.r,ink.g,ink.b,.07f);
            var button=r.gameObject.AddComponent<Button>();button.targetGraphic=image;var colors=button.colors;colors.highlightedColor=new Color(.94f,.91f,.85f);colors.pressedColor=new Color(.76f,.75f,.72f);button.colors=colors;
            var tr=Rect("Label",r,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var label=tr.gameObject.AddComponent<Text>();label.font=font;label.text=text;label.fontSize=16;label.alignment=TextAnchor.MiddleCenter;label.color=filled?cream:ink;label.raycastTarget=false;
            if(click!=null)button.onClick.AddListener(()=>click());return button;
        }
        void Control(string text,float x,int action,bool held)
        {
            var b=Button(text,new Vector2(1,0),new Vector2(x,50),new Vector2(145,56),null,false);
            var input=b.gameObject.AddComponent<TouchControl>();input.Down=()=>app.InputReader.Button(action,true,app.Motor.Pose.Grounded);input.Up=()=>{if(held)app.InputReader.Button(action,false,app.Motor.Pose.Grounded);};
        }
        void Toggle(string name,bool value,float y,System.Action<bool> setter)
        {Button(name+"   "+(value?"ON":"OFF"),new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(360,42),()=>{setter(!value);app.Progress.Save();Rebuild();},false);}
        public void Rebuild()
        {
            if(!safe)return;
            foreach(Transform c in safe){c.gameObject.SetActive(false);Destroy(c.gameObject);}distance=feedback=lesson=null;
            var palette=app.Module.Palette(app.Progress.Dusk);ink=palette[6];cream=palette[1];
            if(app.State==ScreenState.Playing)
            {
                distance=Label("",new Vector2(0,1),new Vector2(148,-36),new Vector2(240,45),19,TextAnchor.MiddleLeft,ink);
                Button("II",Vector2.one,new Vector2(-48,-36),new Vector2(56,50),app.Pause,false);
                feedback=Label("",new Vector2(.5f,1),new Vector2(0,-40),new Vector2(440,48),17,TextAnchor.MiddleCenter,ink);
                lesson=Label("",new Vector2(.5f,0),new Vector2(0,38),new Vector2(520,48),16,TextAnchor.MiddleCenter,cream);
                if(app.Progress.Buttons)
                {
                    Control(app.Module.PrimaryControl,-244,app.Module.IsMotorbike?0:1,app.Module.IsMotorbike);
                    Control(app.Module.SecondaryControl,-84,2,false);
                }
                return;
            }
            // Left paper field leaves the town and rider visible; no dashboard tiles.
            var paper=Rect("Warm paper",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);paper.gameObject.AddComponent<Image>().color=new Color(cream.r,cream.g,cream.b,app.State==ScreenState.Home?.90f:.96f);
            Label("F O S T E R S D I G I T A L",new Vector2(0,1),new Vector2(174,-35),new Vector2(290,30),11,TextAnchor.MiddleLeft,ink);
            Label(app.Module.Place.ToUpperInvariant(),new Vector2(1,1),new Vector2(-180,-35),new Vector2(300,30),11,TextAnchor.MiddleRight,ink);
            var mid=new Vector2(.5f,.5f);
            if(app.State==ScreenState.Home)
            {
                Label(app.Module.Title,mid,new Vector2(0,115),new Vector2(700,100),70,TextAnchor.MiddleCenter,ink);
                Label(app.Module.Tagline,mid,new Vector2(0,47),new Vector2(620,44),18,TextAnchor.MiddleCenter,ink);
                Button("Begin a journey  →",mid,new Vector2(0,-21),new Vector2(270,52),()=>app.StartRun(NextOuting(),false));
                Button("Route journal",mid,new Vector2(-140,-88),new Vector2(260,48),()=>app.Change(ScreenState.Journal),false);
                Button("Flow run",mid,new Vector2(140,-88),new Vector2(260,48),()=>app.StartRun(0,true),false);
                Button("Settings",mid,new Vector2(0,-151),new Vector2(180,44),app.Settings,false);
                Label("A little rhythm. A different way through.",new Vector2(.5f,0),new Vector2(0,25),new Vector2(600,28),12,TextAnchor.MiddleCenter,ink);
            }
            else if(app.State==ScreenState.Journal)
            {
                Label("Places, at your own pace",mid,new Vector2(0,158),new Vector2(650,60),34,TextAnchor.MiddleCenter,ink);
                for(int i=0;i<app.Module.Outings.Length;i++)
                {int n=i;bool arrived=(app.Progress.Arrivals&(1<<i))!=0;bool branch=(app.Progress.Branches&(1<<i))!=0;Button((i+1).ToString("00")+"   "+app.Module.Outings[i]+(arrived?"  ·  arrived":"")+(branch?"  ·  upper path":""),mid,new Vector2(0,76-i*63),new Vector2(510,52),()=>app.StartRun(n,false),false);}
                Button("Back",mid,new Vector2(0,-159),new Vector2(180,44),()=>app.Change(ScreenState.Home),false);
            }
            else if(app.State==ScreenState.Settings)
            {
                Label("Make yourself comfortable",mid,new Vector2(0,190),new Vector2(650,46),30,TextAnchor.MiddleCenter,ink);
                Toggle("Separate controls",app.Progress.Buttons,132,v=>app.Progress.Buttons=v);
                Toggle("Steadier scenery",app.Progress.ReducedMotion,84,v=>app.Progress.ReducedMotion=v);
                Toggle("High-contrast path",app.Progress.Contrast,36,v=>app.Progress.Contrast=v);
                Toggle("Evening light",app.Progress.Dusk,-12,v=>app.Progress.Dusk=v);
                Toggle("Haptics",app.Progress.Haptics,-60,v=>app.Progress.Haptics=v);
                Button("Music  "+Mathf.RoundToInt(app.Progress.Music*100)+"%",mid,new Vector2(-94,-111),new Vector2(172,42),()=>{app.Progress.Music=NextVolume(app.Progress.Music);app.Progress.Save();Rebuild();},false);
                Button("Sound  "+Mathf.RoundToInt(app.Progress.Effects*100)+"%",mid,new Vector2(94,-111),new Vector2(172,42),()=>{app.Progress.Effects=NextVolume(app.Progress.Effects);app.Progress.Save();Rebuild();},false);
                Button("Done",mid,new Vector2(0,-170),new Vector2(180,46),app.CloseSettings);
            }
            else
            {
                bool pause=app.State==ScreenState.Paused,arrival=app.State==ScreenState.Arrival;
                Label(pause?"Take a breath":arrival?"You have arrived":"Find your rhythm",mid,new Vector2(0,114),new Vector2(720,80),46,TextAnchor.MiddleCenter,ink);
                Label(pause?app.Module.Outings[app.Outing]:arrival?(app.Flow?"A line worth remembering · "+app.Score.Banked+" points":app.Module.Outings[app.Outing]+" · added to your journal"):app.Feedback,mid,new Vector2(0,48),new Vector2(690,50),18,TextAnchor.MiddleCenter,ink);
                Button(pause?"Continue  →":arrival?"Ride again  →":app.Flow?"Try again  →":"Back to the last landmark  →",mid,new Vector2(0,-19),new Vector2(350,52),()=>{if(pause)app.Change(ScreenState.Playing);else if(arrival)app.StartRun(app.Outing,app.Flow);else app.Recover();});
                Button(pause?"Settings":"Route journal",mid,new Vector2(-138,-84),new Vector2(255,46),()=>{if(pause)app.Settings();else app.Change(ScreenState.Journal);},false);
                Button(pause?"Restart":"Home",mid,new Vector2(138,-84),new Vector2(255,46),()=>{if(pause)app.StartRun(app.Outing,app.Flow);else app.Change(ScreenState.Home);},false);
                if(pause)Button("Return home",mid,new Vector2(0,-148),new Vector2(240,42),()=>app.Change(ScreenState.Home),false);
            }
        }
        int NextOuting(){for(int i=0;i<app.Module.Outings.Length;i++)if((app.Progress.Arrivals&(1<<i))==0)return i;return 0;}
        static float NextVolume(float v){return v<.01f?.25f:v<.3f?.5f:v<.6f?1:0;}
        public void Refresh()
        {
            if(oldSafe!=Screen.safeArea || oldW!=Screen.width || oldH!=Screen.height)Resize();
            if(distance)distance.text=app.Flow?Mathf.FloorToInt(app.Motor.Pose.X)+" m     "+app.Score.Banked+(app.Score.Pending>0?"  +"+app.Score.Pending+" ×"+app.Score.Multiplier:""):app.Module.Outings[app.Outing];
            if(feedback)feedback.text=app.FeedbackTime>0?app.Feedback:"";
            if(lesson)
            {
                int index=Mathf.FloorToInt(app.RunTime/7);
                lesson.text=!app.Progress.Buttons && index<app.Module.Lessons.Length?app.Module.Lessons[index]:"";
            }
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

