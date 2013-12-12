using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
///   replace the font with another font
/// </summary>
public class ReplaceAtlas : ScriptableWizard
{
    [MenuItem("NGUIHelper/Replace/Replace Atlas")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<ReplaceAtlas>("Replace Atlas", "Close", "Replace");
    }

    public ReplaceAtlas()
    {
        Load();
    }
    void Load()
    {
        atlasFrom = NGUIHelperSetting.instance.replace_atlasFrom;
        atlasTo = NGUIHelperSetting.instance.replace_atlasTo;
    }
    void Save()
    {
        NGUIHelperSetting.instance.replace_atlasFrom= atlasFrom;
        NGUIHelperSetting.instance.replace_atlasTo=atlasTo;
        EditorUtility.SetDirty(NGUIHelperSetting.instance);
    }
    void OnWizardCreate()
    {
        //Save();
        //UIAtlas temp = atlasFrom;
        //atlasFrom = atlasTo;
        //atlasTo = temp;

        Save();
    }

    public UIAtlas atlasFrom;
    public UIAtlas atlasTo;
    public Object folder;
    public string path;

    public GameObject target;

    void OnWizardUpdate()
    {
        helpString = "you can select a gameobject or select a folder or both!";
        if (folder != null)
            path = AssetDatabase.GetAssetPath(folder);
        else
            path = string.Empty;
    }

    void OnWizardOtherButton()
    {
        if (folder!=null && !string.IsNullOrEmpty(path))
        {
            List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(path);
            foreach (var goPath in paths)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(goPath, typeof(GameObject)) as GameObject;
                Replace(go);
            }
        }
        if (target!=null)
        {
            Replace(target);
        }
        AssetDatabase.Refresh();

        Save();
    }

    void Replace(GameObject go)
    {
        if (go != null)
        {
            UISprite[] labels = go.GetComponentsInChildren<UISprite>(true);
            bool change = false;
            foreach (var v in labels)
            {
                if (atlasFrom!= null && atlasTo!= null)
                {
                    if (v.atlas== atlasFrom)
                    {
                        change = true;
                        v.atlas= atlasTo;
                        v.MakePixelPerfect();
                    }
                }
            }
            if (change)
            {
                EditorUtility.SetDirty(go);
            }
        }
    }
}
