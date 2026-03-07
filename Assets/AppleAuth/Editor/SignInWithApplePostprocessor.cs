
# if UNITY_IOS
using AppleAuth.Editor;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
#endif
public static class SignInWithApplePostprocessor
{
    # if UNITY_IOS
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS)
            return;

        var projectPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromString(System.IO.File.ReadAllText(projectPath));
        var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
        manager.AddSignInWithApple();
        manager.WriteToFile();
    }
    #endif
}