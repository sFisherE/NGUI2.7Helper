using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
///   replace the font with another font
/// </summary>
public class ReplaceFont : ScriptableWizard
{
    //[MenuItem("NGUIHelper/Replace/Replace Font")]
    //static void CreateWizard()
    //{
    //    ScriptableWizard.DisplayWizard<ReplaceFont>("Replace Font", "Close", "Replace");
    //}

    public ReplaceFont()
    {
        Load();
    }
    void Load()
    {
        fontFrom = NGUIHelperSetting.instance.replace_fontFrom;
        fontTo = NGUIHelperSetting.instance.replace_fontTo;
        scaleCoeff = NGUIHelperSetting.instance.replace_fontscaleCoeff;
    }
    void Save()
    {
        NGUIHelperSetting.instance.replace_fontFrom = fontFrom;
        NGUIHelperSetting.instance.replace_fontTo = fontTo;
        NGUIHelperSetting.instance.replace_fontscaleCoeff = scaleCoeff;
        EditorUtility.SetDirty(NGUIHelperSetting.instance);
    }
    void OnWizardCreate()
    {
        Save();
    }


    public UIFont fontFrom;
    public UIFont fontTo;
    public Object folder;
    public string path;

    public GameObject target;
    //dynamic font label is always bigger than the bmp font label
    public Vector3 scaleCoeff =Vector3.one;

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
            Debug.Log("path:"+path);
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
            bool change = false;
            UILabel[] labels = go.GetComponentsInChildren<UILabel>(true);
            foreach (var v in labels)
            {
                if (fontFrom != null && fontTo != null)
                {
                    if (v.font == fontFrom)
                    {
                        v.font = fontTo;
                        Vector3 from = v.transform.localScale;
                        v.transform.localScale = new Vector3(from.x * scaleCoeff.x, from.y * scaleCoeff.y, 1);
                        change = true;
                    }
                }
            }
            if (change)
                EditorUtility.SetDirty(go);
        }
    }
}
