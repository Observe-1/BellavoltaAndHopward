using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
namespace Fosters.Studio.Editor
{
    // Headless iOS export for the Mac mini TestFlight pipeline (Tools/testflight/).
    //
    //   Unity -batchmode -buildTarget iOS -projectPath . -executeMethod Fosters.Studio.Editor.TestFlightBuild.IOS
    //         -studioBuildPath <fresh folder> -quit
    //
    // Identity comes from the environment so nothing account-specific is committed:
    // STUDIO_BUILD_NUMBER (required, unique per upload), STUDIO_TEAM_ID (required)
    // and STUDIO_BUNDLE_ID (optional, defaults to com.fostersdigital.<product slug>).
    public static class TestFlightBuild
    {
        public static void IOS()
        {
            string path=Argument("-studioBuildPath");
            if(string.IsNullOrEmpty(path))throw new ArgumentException("Pass -studioBuildPath <fresh Xcode export folder>.");
            if(Directory.Exists(path))throw new InvalidOperationException("Export folder already exists: "+path);
            string build=Environment.GetEnvironmentVariable("STUDIO_BUILD_NUMBER");
            if(string.IsNullOrEmpty(build)||!Regex.IsMatch(build,"^[1-9][0-9]{0,17}$"))
                throw new ArgumentException("Set STUDIO_BUILD_NUMBER to a positive integer build number.");
            string team=Environment.GetEnvironmentVariable("STUDIO_TEAM_ID");
            if(string.IsNullOrEmpty(team))throw new ArgumentException("Set STUDIO_TEAM_ID to the Apple Developer Team ID.");
            var product=StudioBuild.Current;
            if(product.slug=="studiofoundation")throw new InvalidOperationException("Select a product branch to build a game.");

            // EnsureProject applies the tracked player settings (and blanks signing), so CI overrides come after it.
            StudioBuild.EnsureProject();
            PlayerSettings.iOS.buildNumber=build;
            PlayerSettings.iOS.appleDeveloperTeamID=team;
            PlayerSettings.iOS.appleEnableAutomaticSigning=true;
            string bundle=Environment.GetEnvironmentVariable("STUDIO_BUNDLE_ID");
            if(!string.IsNullOrEmpty(bundle))PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,bundle);

            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{"Assets/Scenes/Journey.unity"},locationPathName=path,target=BuildTarget.iOS,options=BuildOptions.None
            });
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("TestFlight iOS export failed: "+report.summary.result);
            StopEmbeddingUnityRuntime(path);
            Debug.Log(product.title+" TestFlight export "+PlayerSettings.bundleVersion+" ("+build+") "+
                PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS)+": "+report.summary.totalSize+" bytes, "+
                report.summary.totalTime.TotalSeconds.ToString("F0")+"s -> "+path);
        }

        // Unity 6 exports embed Frameworks/UnityRuntime.framework, whose binary is a static archive that
        // UnityFramework already links. App Store Connect rejects the embedded copy (ITMS-90208), so only
        // the app target's embed is removed.
        static void StopEmbeddingUnityRuntime(string path)
        {
#if UNITY_IOS
            string projectPath=PBXProject.GetPBXProjectPath(path);
            var project=new PBXProject();project.ReadFromFile(projectPath);
            string runtime=project.FindFileGuidByProjectPath("Frameworks/UnityRuntime.framework");
            if(runtime==null){Debug.Log("UnityRuntime.framework not in the Xcode project; nothing to unembed.");return;}
            project.RemoveFileFromBuild(project.GetUnityMainTargetGuid(),runtime);
            project.WriteToFile(projectPath);
            Debug.Log("UnityRuntime.framework is no longer embedded in the app; UnityFramework still links it.");
#else
            throw new InvalidOperationException("Run the TestFlight export with -buildTarget iOS.");
#endif
        }

        static string Argument(string name)
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);
            return i>=0&&i+1<args.Length?args[i+1]:null;
        }
    }
}
