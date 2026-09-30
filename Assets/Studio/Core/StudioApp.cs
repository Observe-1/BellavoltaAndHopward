using UnityEngine;
namespace Fosters.Studio
{
    public enum ScreenState { Home, Journal, Playing, Paused, Settings, Arrival, Recovery }
    public sealed class StudioApp : MonoBehaviour
    {
        public GameModule Module {get;private set;}
        public Motor Motor {get;private set;}
        public Course Course {get;private set;}
        public Progress Progress {get;private set;}
        public ScreenState State {get;private set;}
        public readonly GestureInput InputReader=new GestureInput();
        public LineScore Score {get;private set;}=new LineScore();
        public bool Flow;public int Outing;public string Feedback="";public float FeedbackTime;
        public float CameraX,CameraY=3.2f;public float RunTime;
        StudioUI ui;IScenery scenery;Camera view;Soundscape sound;
        float accumulator,camVelocity;const float Step=1f/120;ScreenState settingsReturn;
        public void Initialize(GameModule module)
        {
            Module=module;Progress=Progress.Load(module.Slug);
            Application.targetFrameRate=60;QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=4;
            UnityEngine.Screen.sleepTimeout=SleepTimeout.SystemSetting;
            view=new GameObject("Landscape camera").AddComponent<Camera>();view.orthographic=true;view.orthographicSize=module.CameraSize;view.transform.position=new Vector3(0,0,-10);view.clearFlags=CameraClearFlags.SolidColor;
            view.gameObject.AddComponent<AudioListener>();
            Ink.PixelsPerUnit=UnityEngine.Screen.height/(2f*module.CameraSize);scenery=module.CreateScenery(new GameObject("Scenery").transform);
            sound=gameObject.AddComponent<Soundscape>();sound.Initialize(this);
            ui=gameObject.AddComponent<StudioUI>();ui.Initialize(this);
            LoadOuting(0);Change(ScreenState.Home);
        }
        void LoadOuting(int index)
        {Outing=index;Course=Module.CreateCourse(index);Course.Validate();Motor=Module.CreateMotor(Course,Award,Stumble);Motor.Wobbled=Wobble;Motor.Reset(1);Motor.Interpolate(1);Score=new LineScore(Module.MultiplierCap);RunTime=0;CameraX=Motor.Pose.X+view.orthographicSize*view.aspect*.36f;CameraY=Course.Ground(Motor.Pose.X)+Module.CameraLift;camVelocity=0;accumulator=0;}
        public void StartRun(int outing,bool flow)
        {Flow=flow;LoadOuting(outing);Feedback="";FeedbackTime=0;Change(ScreenState.Playing);}
        public void Change(ScreenState next)
        {
            InputReader.Cancel();Motor?.CancelInput();accumulator=0;State=next;
            UnityEngine.Screen.sleepTimeout=next==ScreenState.Playing?SleepTimeout.NeverSleep:SleepTimeout.SystemSetting;
            ui.Rebuild();
        }
        public void Pause(){if(State==ScreenState.Playing)Change(ScreenState.Paused);}
        public void Settings(){settingsReturn=State;Change(ScreenState.Settings);}
        public void CloseSettings(){Progress.Save();Change(settingsReturn);}
        public void Recover()
        {
            if(Flow){StartRun(Outing,true);return;}
            Motor.Reset(Course.Checkpoint(Motor.Pose.X));Motor.Interpolate(1);camVelocity=0;
            CameraX=Motor.Pose.X+view.orthographicSize*view.aspect*.36f;CameraY=Course.Ground(Motor.Pose.X)+Module.CameraLift;Change(ScreenState.Playing);
        }
        void Award(string name,int points)
        {Score.Add(name,points);Feedback=Flow?name.ToUpperInvariant()+"  +"+points:name;FeedbackTime=1.8f;sound.Reward();}
        void Wobble(string cause){Score.Clear();Feedback=cause;FeedbackTime=2.4f;}
        void Stumble(string cause)
        {Score.Clear();Feedback=cause;FeedbackTime=4;Progress.Save();Change(ScreenState.Recovery);}
        public void Hint(string text){Feedback=text;FeedbackTime=1.2f;}
        void Update()
        {
            if(Module==null)return;
            if(UnityEngine.Input.GetKeyDown(KeyCode.Escape)){if(State==ScreenState.Playing)Pause();else if(State==ScreenState.Paused)Change(ScreenState.Playing);}
            if(State==ScreenState.Playing)
            {
                InputReader.Poll(Motor.Pose.Grounded,ui.Blocks,Progress.Buttons);
                float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);accumulator+=dt;RunTime+=dt;
                while(accumulator>=Step && State==ScreenState.Playing)
                {
                    bool preparedBefore=Motor.Pose.Prepared;
                    Motor.Previous=Motor.Pose;Motor.Step(Step,InputReader.Consume());accumulator-=Step;
                    if(!preparedBefore && Motor.Pose.Prepared){if(Module.PreparedHint!=null)Hint(Module.PreparedHint);sound.Accept();}
                    Score.Tick(Step);
                    if(Motor.UpperDiscovered && (Progress.Branches&(1<<Outing))==0){Progress.Branches|=1<<Outing;Progress.Save();Hint("A different way through");}
                    if(Motor.Pose.X>=Course.Length)
                    {
                        Score.Bank();Progress.Arrivals|=1<<Outing;Progress.Best=Mathf.Max(Progress.Best,Score.Banked);Progress.Save();Change(ScreenState.Arrival);
                    }
                }
                Motor.Interpolate(Mathf.Clamp01(accumulator/Step));
                float target=Motor.RenderPose.X+view.orthographicSize*view.aspect*.36f;
                CameraX=Mathf.SmoothDamp(CameraX,target,ref camVelocity,Progress.ReducedMotion?.08f:.18f,Mathf.Infinity,dt);
                float routeY=Course.Ground(Motor.Pose.X);if(routeY<-50)routeY=0;
                CameraY=Mathf.Lerp(CameraY,routeY+Module.CameraLift,1-Mathf.Exp(-dt*(Progress.ReducedMotion?.25f:.65f)));
                FeedbackTime=Mathf.Max(0,FeedbackTime-dt);
            }
            ui.Refresh();
        }
        void LateUpdate(){RenderFrame();}
        public void RenderFrame()
        {if(Module!=null)scenery.Draw(Course,Motor,Progress,CameraX,CameraY,view.orthographicSize*view.aspect,view.orthographicSize,Progress.ReducedMotion?0:RunTime);}
        void OnApplicationPause(bool paused){if(paused){Pause();Progress?.Save();}}
        void OnApplicationFocus(bool focused){if(!focused){InputReader.Cancel();Motor?.CancelInput();Pause();}}
        void OnApplicationQuit(){Progress?.Save();}
    }
}

