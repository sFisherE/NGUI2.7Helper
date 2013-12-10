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

#region ArtFont
    [MenuItem("NGUIHelper/ArtFont/Create ArtFontSettings")]
    public static ArtFontSettings CreateArtFontSettings()
    {
        var path = System.IO.Path.Combine("Assets/NGUIHelper/Resources", "ArtFontSettings.asset");
        var so = AssetDatabase.LoadMainAssetAtPath(path) as ArtFontSettings;
        if (so)
            return so;
        so = ScriptableObject.CreateInstance<ArtFontSettings>();
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/NGUIHelper/Resources");
        if (!di.Exists)
        {
            di.Create();
        }
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }
    /// <summary>
    ///   扫描目标文件夹下所有的psd文档，然后配置到ArtFontSettings中去
    /// </summary>
    [MenuItem("NGUIHelper/ArtFont/Init ArtFontSettings")]
    public static void InitArtFontSettings()
    {
        ArtFontSettings so = CreateArtFontSettings();

        DirectoryInfo dirInfo = new DirectoryInfo(NGUIHelperSetting.CreateNGUIHelperSetting().artFontProtoPath);
        if (dirInfo != null)
        {
            FileSystemInfo[] files = dirInfo.GetFileSystemInfos();
            FileInfo[] psds = dirInfo.GetFiles("*.psd");
            so.artFonts = new List<ArtFont>();
            foreach (FileInfo f in psds)
            {
                //Debug.Log(f.ToString());
                so.artFonts.Add(new ArtFont(f.Name.Substring(0,f.Name.Length-".psd".Length)));
            }
        }
    }





#endregion


    public string rawResourcePath = "Assets/RawResources/UI/Atlases";
    public string prefabPath = "Assets/Resources/UI/Prefabs";

    public string artFontProtoPath = "Assets/RawResources/UI/ArtFonts";

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
