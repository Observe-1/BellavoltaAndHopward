$unityData = 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Data'
$refs = @(Get-ChildItem "$unityData\NetStandard\ref\2.1.0" -Filter *.dll) + @(Get-ChildItem "$unityData\Managed\UnityEngine" -Filter *.dll)
$ui = "$unityData\Resources\PackageManager\ProjectTemplates\libcache\com.unity.template.2d-cross-platform-2d-7.0.0\ScriptAssemblies\UnityEngine.UI.dll"
$argsList = @('-nologo','-target:library','-langversion:latest','-nostdlib+','-out:work/StudioCheck.dll')
$argsList += $refs | ForEach-Object { '-r:"' + $_.FullName + '"' }
$argsList += '-r:"' + $ui + '"'
$argsList += Get-ChildItem Assets -Recurse -Filter *.cs | ForEach-Object { '"' + $_.FullName + '"' }
$argsList | Set-Content work/compile.rsp
& "$unityData\DotNetSdk\dotnet.exe" "$unityData\DotNetSdk\sdk\8.0.318\Roslyn\bincore\csc.dll" '@work/compile.rsp'
exit $LASTEXITCODE
