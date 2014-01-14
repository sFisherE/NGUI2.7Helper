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
    [MenuItem("NGUIHelper/Test #&z")]
    public static void Test()
    {
        //var path = System.IO.Path.Combine(Application.dataPath, "Assets/NGUIHelperSettings.asset");
        //Debug.Log(path);
    }
    [MenuItem("NGUIHelper/WidgetTool")]
    static void ShowWidgetTool()
    {
        UIWidgetTool window = (UIWidgetTool)EditorWindow.GetWindow(typeof(UIWidgetTool), false, "Widget Tool");
    }
    [MenuItem("NGUIHelper/WidgetTool2")]
    static void ShowWidgetTool2()
    {
        UIWidgetTool2 window = (UIWidgetTool2)EditorWindow.GetWindow(typeof(UIWidgetTool2), false, "Widget Tool2");
    }

    [MenuItem("NGUIHelper/Settings/Init Folders")]
    public static void InitFolders()
    {
        //DirectoryInfo di = new DirectoryInfo(Application.dataPath +"/"+ NGUIHelperSettings.instance.artFontOutputPath);
        //if (!di.Exists)
        //{
        //    di.Create();
        //    Debug.Log(di.ToString());
        //    AssetDatabase.Refresh();
        //}
    }
    [MenuItem("NGUIHelper/Settings/Create NGUIHelperSetting")]
    public static void CreateSettingsAsset()
    {
        NGUIHelperSettings.CreateNGUIHelperSetting();
    }

    [MenuItem("NGUIHelper/Output The Selection Path #&g")]
    public static void OutputTheSelectionPath()
    {
        if (Selection.activeGameObject != null)
        {
            Debug.Log(NGUIHelperUtility.GetGameObjectPath(Selection.activeGameObject));
        }
        else if(Selection.activeObject!=null)
        {
            //folder path
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

#region  Find
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
    [MenuItem("NGUIHelper/Find/Missing MonoBehaviour Spider")]
    static void CreateMissingMonoBehaviourSpiderWizard()
    {
        UIMissingMonoBehaviourSpider window = (UIMissingMonoBehaviourSpider)EditorWindow.GetWindow(typeof(UIMissingMonoBehaviourSpider), false, "Missing MonoBehaviour Spider");
    }
    [MenuItem("NGUIHelper/Find/AnimationClip Spider")]
    static void CreateAnimationClipSpider()
    {
        AnimationClipSpider window = (AnimationClipSpider)EditorWindow.GetWindow(typeof(AnimationClipSpider), false, "Animation Spider");
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
#endregion

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


#region  Destroy
    [MenuItem("NGUIHelper/Destroy/Destroy Component")]
    static void DestroyComponent()
    {
        ScriptableWizard.DisplayWizard("Destroy Component", typeof(DestroyComponent), "Close", "Destroy");
    }

    [MenuItem("NGUIHelper/Destroy/Destroy All The Unused AnimationClip")]
    static void DestroyAllTheUnusedAnimationClip()
    {
        string workingPath = NGUIHelperSettings.instance.assetAnimationClipPath;
        if (string.IsNullOrEmpty(workingPath))
        {
            Debug.LogError("set the animationclip folder first");
            return;
        }
       List<string> paths= NGUIHelperUtility.GetAnimationClipsRecursive(NGUIHelperSettings.instance.assetAnimationClipPath);
       List<AnimationClip> clips = new List<AnimationClip>();
        foreach ( var path in paths)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
            if (clip!=null)
                clips.Add(clip);
        }
        bool[] clipStates = new bool[clips.Count];
        paths = NGUIHelperUtility.GetPrefabsRecursive(NGUIHelperSettings.instance.assetPrefabPath);
        foreach (var path in paths)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            Animation[] anims = go.GetComponentsInChildren<Animation>(true);
            foreach ( var anim in anims)
            {
                foreach (AnimationState state in anim)
                {
                    int index = clips.FindIndex(p => (p == state.clip));
                    if (index != -1)
                        clipStates[index] = true;
                }
            }
        }
        bool refresh = false;
        for (int i = 0; i < clipStates.Length;i++ )
        {
            if (clipStates[i]==false)
            {
                refresh = true;
                Debug.Log("delete " + clips[i].name);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(clips[i].GetInstanceID()));
            }
        }
        if (refresh)
            AssetDatabase.Refresh();
    }
#endregion


#region Layout
    [MenuItem("NGUIHelper/Layout/UILayouter")]
    public static void CreateEditorWindow()
    {
        UILayouter window = EditorWindow.GetWindow<UILayouter>("UILayouter");
    }
    [MenuItem("NGUIHelper/Layout/Create AtlasCollection")]
    public static void CreateAtlasCollection()
    {
        UIAtlasCollection.CreateAtlasCollection();
    }
    [MenuItem("NGUIHelper/Layout/Init AtlasCollection")]
    public static void InitAtlasCollection()
    {
        string workingPath = NGUIHelperSettings.instance.assetAtlasPath;
        if (string.IsNullOrEmpty(workingPath))
        {
            Debug.LogError("set the atlas folder first");
            return;
        }
        List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(workingPath);
        foreach (var path in paths)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (go != null)
            {
                UIAtlas atlas = go.GetComponent<UIAtlas>();
                if (atlas!=null)
                {
                    List<UIAtlas> atlases = UIAtlasCollection.instance.atlases;
                    if (atlases == null)
                        atlases = new List<UIAtlas>();
                    if (!atlases.Contains(atlas))
                    {
                        atlases.Add(atlas);
                    }
                }
            }
        }
    }
#endregion
}
