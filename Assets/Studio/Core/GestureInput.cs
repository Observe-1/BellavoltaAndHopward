using UnityEngine;
namespace Fosters.Studio
{
    public struct Gesture
    { public bool Held, Tap, Up, Down, BeganGrounded, Pressed, Released; public float HeldTime; }
    public sealed class GestureInput
    {
        bool active, grounded; Vector2 start; float began; int finger=-1; Gesture pending;
        public void Cancel(){active=false;finger=-1;pending=default;}
        public void Button(int action,bool on,bool isGrounded)
        {
            if(action==0){ if(on){active=true;grounded=isGrounded;began=Time.unscaledTime;pending.Pressed=true;}else{active=false;pending.Released=true;} }
            if(action==1 && on) pending.Tap=true;
            if(action==2 && on){pending.Up=true;pending.BeganGrounded=isGrounded;}
            if(action==3 && on){pending.Down=true;pending.BeganGrounded=isGrounded;}
        }
        public void Poll(bool isGrounded,System.Func<Vector2,bool> blocked,bool buttons)
        {
            if(buttons)return;
            if(Input.touchCount>0)
            {
                for(int i=0;i<Input.touchCount;i++)
                {
                    var t=Input.GetTouch(i);
                    if(t.phase==TouchPhase.Began && !active && !blocked(t.position))Begin(t.position,isGrounded,t.fingerId);
                    if(t.fingerId!=finger)continue;
                    if(t.phase==TouchPhase.Canceled)Cancel();
                    else if(t.phase==TouchPhase.Ended)End(t.position);
                }
            }
            else
            {
                if(Input.GetMouseButtonDown(0) && !blocked(Input.mousePosition))Begin(Input.mousePosition,isGrounded,-1);
                if(active && Input.GetMouseButtonUp(0))End(Input.mousePosition);
            }
            if(Input.GetKeyDown(KeyCode.Space))Begin(Vector2.zero,isGrounded,-2);
            if(Input.GetKeyUp(KeyCode.Space) && finger==-2)End(Vector2.zero);
            if(Input.GetKeyDown(KeyCode.UpArrow)){pending.Up=true;pending.BeganGrounded=isGrounded;active=false;}
            if(Input.GetKeyDown(KeyCode.DownArrow)){pending.Down=true;pending.BeganGrounded=isGrounded;active=false;}
        }
        void Begin(Vector2 p,bool g,int id){active=true;start=p;grounded=g;finger=id;began=Time.unscaledTime;pending.Pressed=true;}
        void End(Vector2 p)
        {
            pending.Released=true;
            float elapsed=Time.unscaledTime-began;
            float delta=(p.y-start.y)*540f/Screen.height;
            pending.BeganGrounded=grounded;
            if(elapsed<=.24f && Mathf.Abs(delta)>=28){pending.Up=delta>0;pending.Down=delta<0;}
            else if(elapsed<.3f && Vector2.Distance(p,start)*540f/Screen.height<28)pending.Tap=true;
            active=false;finger=-1;
        }
        public Gesture Consume()
        {
            var g=pending;g.Held=active;g.HeldTime=active?Time.unscaledTime-began:0;
            if(active)g.BeganGrounded=grounded;pending=default;return g;
        }
    }
}
