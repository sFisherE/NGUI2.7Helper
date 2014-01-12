using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class UIAtlasCollection : ScriptableObject
{
    static UIAtlasCollection mInstance;
    public static UIAtlasCollection instance
    {
        get
        {
            if (mInstance == null)
                mInstance = CreateAtlasCollection();
            return mInstance;
        }
    }
    //[MenuItem("NGUIHelper/Create AtlasCollection")]
    public static UIAtlasCollection CreateAtlasCollection()
    {
        var path = System.IO.Path.Combine("Assets/NGUIHelper", "AtlasCollection.asset");
        var so = AssetDatabase.LoadMainAssetAtPath(path) as UIAtlasCollection;
        if (so)
            return so;
        so = ScriptableObject.CreateInstance<UIAtlasCollection>();
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }


    [SerializeField]
    public List<UIAtlas> atlases;

}
