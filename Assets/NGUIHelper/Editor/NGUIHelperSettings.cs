using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
///   NGUIHelper setting
/// </summary>
public class NGUIHelperSettings : ScriptableObject
{
    static NGUIHelperSettings mInstance;
    public static NGUIHelperSettings instance
    {
        get
        {
            if (mInstance == null)
                mInstance = CreateNGUIHelperSetting();
            return mInstance;
        }
    }

    //[MenuItem("NGUIHelper/Create NGUIHelperSetting")]
    public static NGUIHelperSettings CreateNGUIHelperSetting()
    {
        var path = System.IO.Path.Combine("Assets/NGUIHelper", "NGUIHelperSettings.asset");
        var so = AssetDatabase.LoadMainAssetAtPath(path) as NGUIHelperSettings;
        if (so)
            return so;
        so = ScriptableObject.CreateInstance<NGUIHelperSettings>();
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }

    string CreateAssetPath(string path)
    {
        string assetPath = "Assets" + Path.DirectorySeparatorChar + path;
        DirectoryInfo di = new DirectoryInfo(assetPath);
        if (!di.Exists)
        {
            di.Create();
            AssetDatabase.Refresh();
        }
        return assetPath;
    }
    string CreateFullPath(string path)
    {
        return Application.dataPath + Path.DirectorySeparatorChar + path;
    }
    public string rawResourcePath = "RawResources/UI/Atlases";
    public string assetRawResourcePath
    {
        get { return CreateAssetPath(rawResourcePath); }
    }
    public string prefabPath = "Resources/UI/Prefabs";
    //public string fullPrefabPath
    //{
    //    get { return CreateFullPath(prefabPath); }
    //}
    public string assetPrefabPath
    {
        get 
        {
            return CreateAssetPath(prefabPath);
        }
    }

    public string artFontProtoPath = "RawResources/UI/ArtFonts";
    public string assetArtFontProtoPath
    {
        get { return CreateAssetPath(artFontProtoPath); }
    }
    public string artFontOutputPath = "RawResources/UI/ArtFonts/Output";
    public string assetArtFontOutputPath
    {
        get { return CreateAssetPath(artFontOutputPath); }
    }

    public UIFont replace_fontFrom;
    public UIFont replace_fontTo;
    public Vector3 replace_fontscaleCoeff = new Vector3(0.9f, 0.9f, 1);

    public UIAtlas replace_atlasFrom;
    public UIAtlas replace_atlasTo;


    public string animationClipPath;
    public string assetAnimationClipPath
    {
        get { return CreateAssetPath(animationClipPath); }
    }
    public static readonly Color Blue = new Color(0f, 0.7f, 1f, 1f);
    public static readonly Color Green = new Color(0.4f, 1f, 0f, 1f);
    public static readonly Color Red = new Color32(255, 146, 146, 255);
}
