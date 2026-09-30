using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Fosters.Studio.Editor
{
    [Serializable] public sealed class Product { public string title="Studio Foundation",slug="studiofoundation"; }
    [InitializeOnLoad]
    public static class StudioBuild
    {
        const string ScenePath="Assets/Scenes/Journey.unity";
        static StudioBuild()
        {
            EditorApplication.delayCall+=EnsureProject;
            EditorApplication.playModeStateChanged+=Entered;
        }
        public static Product Current => JsonUtility.FromJson<Product>(File.ReadAllText("Assets/Resources/Product.json"));
        [MenuItem("fostersdigital/Prepare project")]
        public static void EnsureProject()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)return;
            Configure();
            if(!File.Exists(ScenePath))
            {var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(scene,ScenePath);}
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();
        }
        public static void Configure()
        {
            var p=Current;PlayerSettings.companyName="fostersdigital";PlayerSettings.productName=p.title;PlayerSettings.bundleVersion="0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.fostersdigital."+p.slug);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.fostersdigital."+p.slug);
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
            PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.defaultScreenWidth=1560;PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.runInBackground=false;PlayerSettings.colorSpace=ColorSpace.Gamma;
            PlayerSettings.iOS.targetOSVersionString="18.0";PlayerSettings.iOS.appleEnableAutomaticSigning=false;
            PlayerSettings.iOS.appleDeveloperTeamID="";PlayerSettings.iOS.buildNumber="1";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode=1;
            PlayerSettings.Android.useCustomKeystore=false;
            PlayerSettings.stripEngineCode=true;
        }
        [MenuItem("fostersdigital/Build/Unsigned iOS Xcode project")]
        public static void IOS(){Build(BuildTarget.iOS,"Builds/iOS/"+Current.title);}
        [MenuItem("fostersdigital/Build/Android App Bundle")]
        public static void Android(){EditorUserBuildSettings.buildAppBundle=true;Build(BuildTarget.Android,"Builds/Android/"+Current.slug+".aab");}
        [MenuItem("fostersdigital/Build/Desktop preview")]
        public static void Desktop(){Build(BuildTarget.StandaloneWindows64,"Builds/Preview/"+Current.title+".exe");}
        static void Build(BuildTarget target,string path)
        {
            EnsureProject();
            if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target),target))throw new InvalidOperationException("Install "+target+" build support through Unity Hub first.");
            if(Current.slug=="studiofoundation")throw new InvalidOperationException("Select a product branch to build a game.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=path,target=target,options=BuildOptions.None});
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+result.summary.result);
        }
        // One bounded editor smoke check, invoked after code and deterministic model checks.
        public static void Smoke()
        {
            EnsureProject();EditorSceneManager.OpenScene(ScenePath);SessionState.SetBool("StudioSmoke",true);
            EditorApplication.isPlaying=true;
        }
        static double started;static int phase;
        static void Entered(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("StudioSmoke",false))return;
            started=EditorApplication.timeSinceStartup;phase=0;EditorApplication.update+=SmokeUpdate;
        }
        static void SmokeUpdate()
        {
            if(EditorApplication.timeSinceStartup-started<1)return;
            var app=UnityEngine.Object.FindAnyObjectByType<StudioApp>();
            if(!app){Debug.LogError("SMOKE: no product app");EditorApplication.Exit(2);return;}
            if(phase==0){app.StartRun(0,false);phase=1;}
            double elapsed=EditorApplication.timeSinceStartup-started;
            if(elapsed>4 && phase==1){app.Pause();if(app.State!=ScreenState.Paused)throw new Exception("Pause failed");app.Change(ScreenState.Playing);phase=2;}
            if(elapsed>7 && phase==2)
            {
                Directory.CreateDirectory("Artifacts");
                var cam=Camera.main; if(!cam)cam=UnityEngine.Object.FindAnyObjectByType<Camera>();
                var rt=new RenderTexture(1560,720,24){antiAliasing=4};var tex=new Texture2D(1560,720,TextureFormat.RGB24,false);
                cam.targetTexture=rt;cam.aspect=1560f/720f;
                foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;}
                app.RenderFrame();Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1560,720),0,0);tex.Apply();File.WriteAllBytes("Artifacts/"+Current.slug+"-scene.png",tex.EncodeToPNG());
                cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);
                Debug.Log("SMOKE PASS: "+Current.title+" boot, movement, pause/resume. x="+app.Motor.Pose.X+" state="+app.State);
                SessionState.SetBool("StudioSmoke",false);EditorApplication.update-=SmokeUpdate;EditorApplication.Exit(0);
            }
        }
    }
}

