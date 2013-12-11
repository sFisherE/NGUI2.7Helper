using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
///   NGUIHelper setting
/// </summary>
public class NGUIHelperSetting : ScriptableObject
{
    static NGUIHelperSetting mInstance;

    public static NGUIHelperSetting instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = CreateNGUIHelperSetting();
            }
            return mInstance;
        }
    }


    [MenuItem("NGUIHelper/Create NGUIHelperSetting")]
    public static NGUIHelperSetting CreateNGUIHelperSetting()
    {
        var path = System.IO.Path.Combine("Assets/NGUIHelper", "NGUIHelperSettings.asset");
        var so = AssetDatabase.LoadMainAssetAtPath(path) as NGUIHelperSetting;
        if (so)
            return so;
        so = ScriptableObject.CreateInstance<NGUIHelperSetting>();
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/NGUIHelper");
        if (!di.Exists)
        {
            di.Create();
        }
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }

    public string rawResourcePath = "Assets/RawResources/UI/Atlases";
    public string prefabPath = "Assets/Resources/UI/Prefabs";

    public string artFontProtoPath = "Assets/RawResources/UI/ArtFonts";
    public string artFontOutputPath = "";

    public UIFont replace_fontFrom;
    public UIFont replace_fontTo;
    public Vector3 replace_fontscaleCoeff = new Vector3(0.9f, 0.9f, 1);

    public UIAtlas replace_atlasFrom;
    public UIAtlas replace_atlasTo;



    public static readonly Color Blue = new Color(0f, 0.7f, 1f, 1f);
    public static readonly Color Green = new Color(0.4f, 1f, 0f, 1f);
    public static readonly Color Red = new Color32(255, 146, 146, 255);

    //[MenuItem("NGUIHelper/GetRawResourcePath")]
    public static string GetRawResourcePath()
    {
        var so = CreateNGUIHelperSetting();
        DirectoryInfo di = new DirectoryInfo(so.rawResourcePath);
        if (!di.Exists)
        {
            di.Create();
            AssetDatabase.Refresh();
        }
        return so.rawResourcePath;
    }

    //[MenuItem("NGUIHelper/GetPrefabPath")]
    public static string GetPrefabPath()
    {
        var so = CreateNGUIHelperSetting();
        DirectoryInfo di = new DirectoryInfo(so.prefabPath);
        if (!di.Exists)
        {

            Debug.LogError("ui prefab path not exist!");
            return null;
        }
        return so.prefabPath;
    }


}
