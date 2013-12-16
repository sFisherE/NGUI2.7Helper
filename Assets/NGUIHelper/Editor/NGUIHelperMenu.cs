using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
///   
/// todo:
/// 1.how to apply prefab in runtime,because i always adjust value in runtime to avoid write to much editor code.
/// 2.how to record data in runtime,
/// </summary>
static public class NGUIHelperMenu
{
    //便捷的测试函数
    [MenuItem("NGUIHelper/Test #&t")]
    public static void Test()
    {
        //string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        //IsScript(path);
    }

    [MenuItem("NGUIHelper/Output The Selection Path #&g")]
    public static void OutputTheSelectionPath()
    {
        if (Selection.activeGameObject != null)
        {
            Debug.Log(NGUIHelperUtility.GetGameObjectPath(Selection.activeGameObject));
        }
    }
    //[MenuItem("UIEditTool/Destory All The Missing MonoBehaviour")]
    public static void DestoryAllTheMissMonoBehaviour()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

        if (!NGUIHelperUtility.IsDirectory(path))
        {
            EditorUtility.DisplayDialog("warning", "you must select a directory!", "ok");
        }
        else
        {
            Debug.Log(path + " " + NGUIHelperUtility.IsDirectory(path));

            List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(path);

            //Object[] os = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            //Debug.Log(os.Length);
            //foreach (var g in os)
            //{
            //    Debug.Log(g.name);
            //}
        }
    }



    [MenuItem("NGUIHelper/Replace/Replace Atlas")]
    static void CreateReplaceAtlasWizard()
    {
        ScriptableWizard.DisplayWizard<ReplaceAtlas>("Replace Atlas", "Close", "Replace");
    }
    [MenuItem("NGUIHelper/Replace/Replace Font")]
    static void CreateReplaceFontWizard()
    {
        ScriptableWizard.DisplayWizard<ReplaceFont>("Replace Font", "Close", "Replace");
    }
    [MenuItem("NGUIHelper/Replace/Replace Sprite")]
    static void CreateReplaceSpriteWizard()
    {
        ScriptableWizard.DisplayWizard<ReplaceSprite>("Replace Sprite", "Close", "Replace");
    }


    [MenuItem("NGUIHelper/Find/Atlas Spider")]
    static void CreateAtlasSpiderWizard()
    {
        UIAtlasSpider window = (UIAtlasSpider)EditorWindow.GetWindow(typeof(UIAtlasSpider), false, "Atlas Spider");
    }
    [MenuItem("NGUIHelper/Find/Font Spider")]
    static void CreateFontSpiderWizard()
    {
        UIFontSpider window = (UIFontSpider)EditorWindow.GetWindow(typeof(UIFontSpider), false, "Font Spider");
    }

    [MenuItem("NGUIHelper/Atlas Exchanger")]
    static public void openAtlasExchanger()
    {
        EditorWindow.GetWindow<UIAtlasExchanger>(false, "Atlas Exchanger", true);
    }

    [MenuItem("NGUIHelper/AtlasSpliter")]
    static public void OpenAtlasSplitTool()
    {
        EditorWindow.GetWindow<UIAtlasSpliter>(false, "AtlasSpliter", true);
    }

    [MenuItem("NGUIHelper/9 Patch/Atlas 9 Patch Slicer")]
    static public void openAtlas9PatchSlicer()
    {
        EditorWindow.GetWindow<UIAtlas9PatchSlicer>(false, "Atlas 9 Patch Slicer", true);
    }
    [MenuItem("NGUIHelper/9 Patch/Texture 9 Patch Slicer")]
    static public void openTexture9PatchSlicer()
    {
        EditorWindow.GetWindow<UITexture9PatchSlicer>(false, "Texture 9 Patch Slicer", true);
    }

    [MenuItem("NGUIHelper/Find/Create SpriteUsage")]
    public static SpriteUsage CreateSpriteUsage()
    {
        var path = System.IO.Path.Combine("Assets/NGUIHelper/Resources", "SpriteUsage.asset");
        var so = AssetDatabase.LoadMainAssetAtPath(path) as SpriteUsage;
        if (so)
            return so;
        so = ScriptableObject.CreateInstance<SpriteUsage>();
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/NGUIHelper/Resources");
        if (!di.Exists)
        {
            di.Create();
        }
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }
}
