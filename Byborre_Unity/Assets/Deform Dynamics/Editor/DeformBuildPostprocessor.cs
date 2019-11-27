using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class DeformBuildPostprocessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        char[] slashes = { '/', '\\' };

        string buildPath = pathToBuiltProject.Substring(0, pathToBuiltProject.LastIndexOfAny(slashes));
        string pluginsFolder = "/" + PlayerSettings.productName + "_Data/Plugins/";
        
        FileUtil.CopyFileOrDirectory("Assets/Deform Dynamics/Native/Plugins/deform_config.xml", buildPath + pluginsFolder + "deform_config.xml");
    }
}