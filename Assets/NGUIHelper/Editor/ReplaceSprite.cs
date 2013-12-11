using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
///   replace a sprite in atlas A with another sprite in atlas B
/// </summary>
public class ReplaceSprite : ScriptableWizard
{
    [MenuItem("NGUIHelper/Replace/Replace Sprite")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<ReplaceSprite>("Replace Sprite", "Close", "Replace");
    }

    public UIAtlas atlasFrom;
    public string spriteFrom;
    public UIAtlas atlasTo;
    public string spriteTo;
    public Object folder;
    public string path;

    public ReplaceSprite()
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
        NGUIHelperSetting.instance.replace_atlasFrom = atlasFrom;
        NGUIHelperSetting.instance.replace_atlasTo = atlasTo;
    }

    void OnWizardCreate()
    {
        Save();
    }

    void OnWizardUpdate()
    {
        string p = string.Empty;
        if (folder != null)
            p = AssetDatabase.GetAssetPath(folder);

        if (NGUIHelperUtility.IsDirectory(p))
        {
            path = p;
        }
        else
        {
            path = string.Empty;
        }

    }
    void OnWizardOtherButton()
    {
        if (path != null)
        {
            List<string> paths = NGUIHelperUtility.GetAllPrefabs(path);

            foreach (var goPath in paths)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(goPath, typeof(GameObject)) as GameObject;
                if (go != null)
                {
                    UISprite[] sprites = go.GetComponentsInChildren<UISprite>(true);
                    foreach (var s in sprites)
                    {
                        if (s.atlas == atlasFrom)
                        {
                            if (s.spriteName==spriteFrom)
                            {
                                s.atlas = atlasTo;
                                s.spriteName = spriteTo;
                                s.MakePixelPerfect();

                            }
                        }
                    }
                }
            }
        }
        AssetDatabase.Refresh();

        Save();
    }
}
