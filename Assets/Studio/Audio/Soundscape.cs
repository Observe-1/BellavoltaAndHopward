using UnityEngine;
namespace Fosters.Studio
{
    // Deterministic original synthesis; no samples or third-party recordings.
    public sealed class Soundscape : MonoBehaviour
    {
        StudioApp app;AudioSource music,effects,engine;AudioClip tick,success;float lastPulse;
        public void Initialize(StudioApp owner)
        {
            app=owner;music=gameObject.AddComponent<AudioSource>();effects=gameObject.AddComponent<AudioSource>();engine=gameObject.AddComponent<AudioSource>();
            music.loop=true;music.clip=MakeMusic();music.Play();tick=Tone("Spring contact",145,.13f);success=Tone("Landed line",440,.38f);
            if(app.Module.IsMotorbike){engine.loop=true;engine.clip=MakeEngine();engine.Play();}
        }
        void Update()
        {
            bool playing=app.State==ScreenState.Playing;
            music.volume=app.Progress.Music*.27f;effects.volume=app.Progress.Effects*.4f;
            engine.volume=playing?app.Progress.Effects*.10f:0;
            if(app.Motor==null)return;
            float pulse=app.Motor.ContactPulse;
            if(playing && pulse>lastPulse+.3f)effects.PlayOneShot(tick,.3f);
            lastPulse=pulse;
        }
        public void Cue(bool reward)
        {
            effects.PlayOneShot(reward?success:tick,.5f);
            if(app.Progress.Haptics && Application.isMobilePlatform)Handheld.Vibrate();
        }
        static AudioClip Tone(string name,float hz,float seconds)
        {
            const int rate=24000;var data=new float[Mathf.CeilToInt(seconds*rate)];
            for(int i=0;i<data.Length;i++){float t=i/(float)rate;data[i]=Mathf.Sin(2*Mathf.PI*hz*t)*Mathf.Exp(-t*12)*Mathf.Min(1,t*150)*.24f;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        static AudioClip MakeMusic()
        {
            const int rate=24000;const int seconds=24;var data=new float[rate*seconds];
            float[] notes={220,329.63f,293.66f,196,261.63f,329.63f,246.94f,293.66f};
            for(int n=0;n<notes.Length;n++)for(int i=0;i<rate*7;i++)
            {float t=i/(float)rate;float env=Mathf.Min(1,t*5)*Mathf.Exp(-t*.8f);float f=notes[n];float value=(Mathf.Sin(t*f*2*Mathf.PI)+.25f*Mathf.Sin(t*f*4*Mathf.PI))*.11f*env;data[(n*rate*3+i)%data.Length]+=value;}
            var clip=AudioClip.Create("Quayside original eight-note study",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        static AudioClip MakeEngine()
        {
            const int rate=24000;var data=new float[rate];
            for(int i=0;i<rate;i++){float t=i/(float)rate;data[i]=(.13f*Mathf.Sin(2*Mathf.PI*74*t)+.04f*Mathf.Sin(2*Mathf.PI*148*t))*(.65f+.35f*Mathf.Sin(2*Mathf.PI*13*t));}
            var clip=AudioClip.Create("Rounded moped putter",rate,1,rate,false);clip.SetData(data,0);return clip;
        }
        void OnDestroy(){if(music && music.clip)Destroy(music.clip);if(engine && engine.clip)Destroy(engine.clip);if(tick)Destroy(tick);if(success)Destroy(success);}
    }
}
