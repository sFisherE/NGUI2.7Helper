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

            if (mInstance.localizeTxt != null && mInstance.mLocalizeDictionary.Count == 0)
            {
                mInstance.InitLocalization();
            }
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
    public string atlasPath;
    public string assetAtlasPath
    {
        get { return CreateAssetPath(atlasPath); }
    }
    //when user change the txt outside the editor,how to detect it and reinitialize the dictionary???
    [SerializeField]
    public TextAsset localizeTxt;
    Dictionary<string, string> mLocalizeDictionary = new Dictionary<string, string>();
    public string GetLocalizeValue(string key)
    {
        string val;
        if (mLocalizeDictionary.TryGetValue(key, out val)) 
            return val;
        return string.Empty;
    }
    public string TryGetLocalizeKey(string value)
    {
        foreach (var item in mLocalizeDictionary )
        {
            if (item.Value == value)
                return item.Key;
        }
        return string.Empty;
    }
    public void AddNewLocalize(string key, string value)
    {
        if (!mLocalizeDictionary.ContainsKey(key))
        {
            mLocalizeDictionary.Add(key, value);
            //add new record
            StreamWriter sw = new StreamWriter(AssetDatabase.GetAssetPath(localizeTxt), true);
            sw.WriteLine(string.Format("{0}={1}", key, value));
            sw.Close();
            AssetDatabase.Refresh();
        }
    }
    public void DeleteLocalize(string key)
    {
        mLocalizeDictionary.Remove(key);
        FileStream fs = new FileStream(AssetDatabase.GetAssetPath(localizeTxt), FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        foreach (var item in mLocalizeDictionary)
        {
            sw.WriteLine(string.Format("{0}={1}", item.Key, item.Value));
        }
        sw.Close();
        fs.Close();
        AssetDatabase.Refresh();
    }

    bool mLocalizeInited=false;
    public void InitLocalization()
    {
        if (mLocalizeInited)
            return;
        if (localizeTxt!=null)
        {
            ByteReader reader = new ByteReader(localizeTxt);
            mLocalizeDictionary = reader.ReadDictionary();
        }
    }



    public static readonly Color Blue = new Color(0f, 0.7f, 1f, 1f);
    public static readonly Color Green = new Color(0.4f, 1f, 0f, 1f);
    public static readonly Color Red = new Color32(255, 146, 146, 255);
}
