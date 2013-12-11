//using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
///   侦测某个atlas的使用情况，被那个prefab所使用
///   侦测某个atlas里面哪个元素没有被使用过，这样便于清理
/// </summary>
public class UIAtlasSpider : EditorWindow
{
    [MenuItem("NGUIHelper/Find/Atlas Spider")]
    static void Init()
    {
        UIAtlasSpider window = (UIAtlasSpider)EditorWindow.GetWindow(typeof(UIAtlasSpider), false, "Atlas Spider");
    }

    UIAtlas mSelectAtlas;

    void DrawSelectAtlas()
    {
        ComponentSelector.Draw<UIAtlas>("Select", mSelectAtlas, obj =>
            {
                UIAtlas atlas = obj as UIAtlas;
                if (mSelectAtlas != atlas)
                {
                    mSelectAtlas = atlas;
                    spriteUseStates.Clear();
                    if (mSelectAtlas != null)
                    {
                        foreach (UIAtlas.Sprite s in mSelectAtlas.spriteList)
                        {
                            spriteUseStates.Add(s.name, new SpriteEntry(s.name, false));
                        }
                    }
                }
                Repaint();
            });
        GUILayout.BeginHorizontal();
        {
            //编辑器刷新之后可能刷掉数据了，所以当没有数据的时候强制再去获取一次
            if (spriteUseStates.Count == 0)
            {
                if (mSelectAtlas != null)
                {
                    spriteUseStates.Clear();
                    foreach (UIAtlas.Sprite s in mSelectAtlas.spriteList)
                    {
                        spriteUseStates.Add(s.name, new SpriteEntry(s.name, false));
                    }
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    string mPath;
     Object mFolder;
     void DrawSelectPath()
     {
         mFolder = EditorGUILayout.ObjectField("Select Path:", mFolder, typeof(Object), false) as Object;
         if (mFolder != null)
         {
             string path = AssetDatabase.GetAssetPath(mFolder);
             if (NGUIHelperUtility.IsDirectory(path))
             {
                 mPath = path;
                 GUILayout.Label("you select a path:    " + mPath);
             }
             else
             {
                 mPath = string.Empty;
                 EditorGUILayout.HelpBox("please select a folder", MessageType.Warning);
             }
         }
     }


    List<string> mPrefabNames = new List<string>();

    Vector2 mScroll = Vector2.zero;
    GUIStyle mStyle = new GUIStyle();
    bool mShowPrefabs = false;

    class SpriteEntry
    {
        public string name;
        public bool useState;
        public int useTimes;
        public List<GameObject> relatedGos = new List<GameObject>();
        public SpriteEntry(string name, bool state)
        {
            this.name = name;
            useState = state;
        }
    }
    Dictionary<string, SpriteEntry> spriteUseStates = new Dictionary<string, SpriteEntry>();
    bool mShowRelatedSprites = false;
    void DrawWidgets()
    {
        if (mSelectAtlas != null)
        {
            mScroll = GUILayout.BeginScrollView(mScroll);
            {
                GUILayout.BeginVertical();
                {
                    foreach (UIAtlas.Sprite s in mSelectAtlas.spriteList)
                    {
                        if (spriteUseStates.ContainsKey(s.name))
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUI.backgroundColor = spriteUseStates[s.name].useState ? NGUIHelperSetting.Green : NGUIHelperSetting.Red;
                                GUILayout.Label(s.name, "HelpBox", GUILayout.Width(150), GUILayout.Height(18f));
                                GUI.backgroundColor = Color.white;
                                //GUILayout.Label(spriteUseStates[s.name].ToString(), GUILayout.Width(150));
                                if (GUILayout.Button("Sprite", "DropDownButton", GUILayout.Width(76f)))
                                {
                                    SpriteSelector.Show(mSelectAtlas, s.name, null);
                                }
                                GUILayout.Label(spriteUseStates[s.name].useTimes.ToString());
                            }
                            GUILayout.EndHorizontal();
                            if (mShowRelatedSprites)
                            {
                                if (spriteUseStates[s.name].useState)
                                {
                                    foreach (GameObject go in spriteUseStates[s.name].relatedGos)
                                    {
                                        if (go != null)
                                        {
                                            GUILayout.BeginHorizontal();
                                            GUILayout.Space(30f);
                                            string path = NGUIHelperUtility.GetGameObjectPath(go);
                                            //path = path.Substring(("/" + RootName).Length);


                                                GUI.contentColor = Color.black;
                  

                                            GUIStyle style = EditorStyles.whiteLabel;
                                            if (Selection.activeGameObject == go)
                                            {
                                                GUI.contentColor = Color.blue;
                                            }
                                            if (GUILayout.Button(path, style, GUILayout.MinWidth(100f)))
                                            {
                                                Selection.activeGameObject = go;
                                            }
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                }
                            }
                            GUILayout.Space(5);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
    }
    void OnGUI()
    {
        DrawSelectAtlas();
        NGUIEditorTools.DrawSeparator();
        DrawSelectPath();
        NGUIEditorTools.DrawSeparator();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Detect", GUILayout.Width(200)))
        {
            GetSpriteUseState();
        }
        mShowRelatedSprites = EditorGUILayout.Toggle("Show Related Sprite", mShowRelatedSprites);
        GUILayout.EndHorizontal();
        DrawWidgets();
    }

    void GetSpriteUseState()
    {
        mPrefabNames = NGUIHelperUtility.GetPrefabsRecursive(mPath);
        foreach (var path in mPrefabNames)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (go != null)
            {

                UISprite[] sprites = go.GetComponentsInChildren<UISprite>(true);
                foreach (var s in sprites)
                {
                    if (s.atlas==mSelectAtlas)
                    {
                        string key = s.spriteName;
                        spriteUseStates[key].useState = true;
                        List<GameObject> gos = spriteUseStates[key].relatedGos;
                        if (!gos.Contains(s.gameObject))
                        {
                            spriteUseStates[key].useTimes++;//使用次数+1
                            gos.Add(s.gameObject);
                        }
                    }
                }
            }
        }
    }


    void SetSpriteUseState(List<UISprite> sprites)
    {
        foreach (UISprite s in sprites)
        {
            if (s.atlas == mSelectAtlas)
            {
                string key = s.spriteName;
                spriteUseStates[key].useState = true;
                List<GameObject> gos = spriteUseStates[key].relatedGos;
                if (!gos.Contains(s.gameObject))
                {
                    spriteUseStates[key].useTimes++;//使用次数+1
                    gos.Add(s.gameObject);
                }
            }
        }
    }
    /// <summary>
    ///   实时更新
    /// </summary>
    //void OnInspectorUpdate()
    //{
    //    Repaint();
    //} 
}
