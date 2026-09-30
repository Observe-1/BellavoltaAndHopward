using UnityEngine;
using Fosters.Studio;
namespace Fosters.Bellavolta
{
    // Porto Chiaro colour roles, matched to the approved late-afternoon reference, plus an evening preset.
    public sealed class PortoPalette
    {
        public Color SkyTop, SkyLow, Sun, SunGlow;
        public Color RidgeFar, RidgeMid, Hill, HillShade;
        public Color VillageWall, VillageRoof;
        public Color Sea, SeaHorizon, SeaStreak, Rock, RockShade;
        public Color TownWall, TownShade, TownRoof, TownRoofShade, TownWindow, RampartWall, RampartOpening, Dome;
        public Color Cypress, CypressShade, Palm;
        public Color NearWall, NearWallWarm, NearWallPale, NearShade, NearRoof, NearRoofShade, Shutter, Window, Door, DoorInner, Rail, Balcony, Shrub, ShrubShade;
        public Color Laundry1, Laundry2, Laundry3;
        public Color Deck, DeckTop, DeckEdge, DeckShade, Bollard, Promenade, PromenadeTop, Gate, GateCap, Lamp, LampGlass;
        public Color Leaf, LeafLight, Flower, FlowerLight;
        public Color Ferry, FerryShade, Funnel, Wake;
        public Color Jacket, JacketShade, Helmet, Hair, Skin, Trousers, TrousersShade, Shoe, Moped, MopedShade, MopedLight, Tyre, Rim, Metal;
        public Color Ink, Paper;
        public Color Wood, WoodShade, Lemon, LemonLeaf, Spark;
        public bool Evening;

        static Color H(string s)=>Studio.Ink.Hex(s);

        // Atmospheric copy: every role pulled toward a haze colour (used for distant set pieces).
        public PortoPalette Hazed(Color haze,float t)
        {
            var copy=(PortoPalette)MemberwiseClone();
            foreach(var f in typeof(PortoPalette).GetFields())
                if(f.FieldType==typeof(Color))f.SetValue(copy,Color.Lerp((Color)f.GetValue(this),haze,t));
            return copy;
        }

        public static PortoPalette Day()=>new PortoPalette
        {
            SkyTop=H("F9B089"),SkyLow=H("F8B892"),Sun=H("FBDFB4"),SunGlow=H("FAC99C"),
            RidgeFar=H("A4A4C9"),RidgeMid=H("8F92BE"),Hill=H("7780A3"),HillShade=H("6C7597"),
            VillageWall=H("E7A68A"),VillageRoof=H("CE6E60"),
            Sea=H("5F95AA"),SeaHorizon=H("6FA2B2"),SeaStreak=H("8AB5B8"),Rock=H("7A7088"),RockShade=H("635A74"),
            TownWall=H("F1B08D"),TownShade=H("DB947C"),TownRoof=H("CF6357"),TownRoofShade=H("A9504F"),TownWindow=H("9A6D70"),
            RampartWall=H("B08A8D"),RampartOpening=H("887689"),Dome=H("4F5768"),
            Cypress=H("4D5A6A"),CypressShade=H("414C5C"),Palm=H("525B6D"),
            NearWall=H("EE9E78"),NearWallWarm=H("E68B6C"),NearWallPale=H("F4BC98"),NearShade=H("D27D66"),NearRoof=H("C86659"),NearRoofShade=H("97484A"),
            Shutter=H("3B585E"),Window=H("8E5A5C"),Door=H("955A5A"),DoorInner=H("B96C63"),Rail=H("3E3148"),Balcony=H("C7775F"),Shrub=H("535168"),ShrubShade=H("454259"),
            Laundry1=H("FBDDB0"),Laundry2=H("F3E4CF"),Laundry3=H("6077A2"),
            Deck=H("47374C"),DeckTop=H("554359"),DeckEdge=H("6B5166"),DeckShade=H("3D2F43"),Bollard=H("4A3A50"),
            Promenade=H("5E4A61"),PromenadeTop=H("8C6E80"),Gate=H("6B5670"),GateCap=H("7E6780"),Lamp=H("3E3046"),LampGlass=H("F6D9A8"),
            Leaf=H("35283E"),LeafLight=H("43334B"),Flower=H("CE6671"),FlowerLight=H("E58A86"),
            Ferry=H("F2EDF0"),FerryShade=H("C9C1CB"),Funnel=H("D0503F"),Wake=H("A4C8C8"),
            Jacket=H("DD5E43"),JacketShade=H("B84836"),Helmet=H("F7DDB3"),Hair=H("32293E"),Skin=H("A8705E"),Trousers=H("2F4953"),TrousersShade=H("263B44"),Shoe=H("F5D5A8"),
            Moped=H("3F6A66"),MopedShade=H("2E4D4E"),MopedLight=H("5B8580"),Tyre=H("2A2331"),Rim=H("5A5261"),Metal=H("8C8595"),
            Ink=H("4A3A4E"),Paper=H("FBE6C6"),
            Wood=H("B87A5C"),WoodShade=H("8F5A4A"),Lemon=H("F4CF55"),LemonLeaf=H("5E8A62"),Spark=H("FFE7A8")
        };

        public static PortoPalette Dusk()
        {
            var p=Day();p.Evening=true;
            p.SkyTop=H("B98BA6");p.SkyLow=H("F1A58A");p.Sun=H("FBD29C");p.SunGlow=H("F4B08E");
            p.RidgeFar=H("9A8FB5");p.RidgeMid=H("8580AA");p.Hill=H("6A6D92");p.HillShade=H("5E6286");
            p.VillageWall=H("D59384");p.VillageRoof=H("B45F5D");
            p.Sea=H("4E7F98");p.SeaHorizon=H("7C98AE");p.SeaStreak=H("E6B39A");p.Rock=H("655C77");p.RockShade=H("534B66");
            p.TownWall=H("DE9C86");p.TownShade=H("C3837A");p.TownRoof=H("B8574F");p.TownRoofShade=H("924548");p.TownWindow=H("F7C98A");
            p.RampartWall=H("9C7C88");p.RampartOpening=H("736579");p.Dome=H("454B60");
            p.Cypress=H("414C60");p.CypressShade=H("363F52");p.Palm=H("454D62");
            p.NearWall=H("DA8F76");p.NearWallWarm=H("CF7F6B");p.NearWallPale=H("E3A88E");p.NearShade=H("BA7066");p.NearRoof=H("AE5856");p.NearRoofShade=H("823F45");
            p.Shutter=H("35505A");p.Window=H("F6C07E");p.Door=H("7F4C54");p.DoorInner=H("A05D5C");p.Rail=H("362B40");p.Balcony=H("B06A5E");p.Shrub=H("4A4861");p.ShrubShade=H("3D3A52");
            p.Deck=H("3F3146");p.DeckTop=H("4B3B51");p.DeckEdge=H("61485E");p.DeckShade=H("35293C");p.Bollard=H("413347");
            p.Promenade=H("54425A");p.PromenadeTop=H("7E6276");p.Gate=H("5E4B65");p.GateCap=H("705B75");p.Lamp=H("372A3F");p.LampGlass=H("FFE2A6");
            p.Leaf=H("2E2336");p.LeafLight=H("3A2D43");
            p.Ferry=H("E6DDE3");p.FerryShade=H("B9AFBE");p.Wake=H("C9B2AE");
            p.Ink=H("3F3145");p.Paper=H("F6DDC4");
            p.Wood=H("A06A57");p.WoodShade=H("7C4E46");p.Lemon=H("F0C353");p.LemonLeaf=H("4F775A");
            return p;
        }
    }
}
