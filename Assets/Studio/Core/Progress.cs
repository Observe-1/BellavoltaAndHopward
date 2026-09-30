using UnityEngine;
namespace Fosters.Studio
{
    [System.Serializable]
    public sealed class Progress
    {
        public int Arrivals, Branches, Best;
        public bool Buttons, ReducedMotion, Contrast, Dusk, Haptics;
        public float Effects=.45f, Music=.35f;
        string key;
        public static Progress Load(string slug)
        {
            string k="fostersdigital."+slug+".v1";Progress p=null;
            try{p=JsonUtility.FromJson<Progress>(PlayerPrefs.GetString(k,""));}catch(System.ArgumentException){}
            if(p==null)p=new Progress();p.key=k;return p;
        }
        public void Save(){PlayerPrefs.SetString(key,JsonUtility.ToJson(this));PlayerPrefs.Save();}
    }
    public sealed class LineScore
    {
        public int Banked, Pending, Multiplier=1;
        readonly int cap;
        public LineScore(int multiplierCap=3){cap=Mathf.Max(1,multiplierCap);}
        float quiet; readonly System.Collections.Generic.Dictionary<string,int> moves=new System.Collections.Generic.Dictionary<string,int>();
        public void Add(string name,int points)
        {
            moves.TryGetValue(name,out int count);moves[name]=count+1;
            Pending+=Mathf.RoundToInt(points/(1+count*.55f));Multiplier=Mathf.Min(cap,moves.Count);quiet=0;
        }
        public void Tick(float dt){if(Pending>0 && (quiet+=dt)>2.5f)Bank();}
        public void Bank(){Banked+=Pending*Multiplier;Clear();}
        public void Clear(){Pending=0;Multiplier=1;quiet=0;moves.Clear();}
    }
}
