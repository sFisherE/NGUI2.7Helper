//using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
///   侦测某个atlas的使用情况，被那个prefab所使用
///   侦测某个atlas里面哪个元素没有被使用过，这样便于清理
///   
/// roadmap:
/// 统计每个sprite的使用频次，有多少个界面元素用到了
/// </summary>
public class UIAtlasSpider : EditorWindow
{
    [MenuItem("NGUIHelper/Atlas Spider")]
    static void Init()
    {
        UIAtlasSpider window = (UIAtlasSpider)EditorWindow.GetWindow(typeof(UIAtlasSpider), false, "Atlas Spider");
    }

    UIAtlas mSelectAtlas;

    void OnSelectAtlas(Object obj)
    {
        UIAtlas atlas = obj as UIAtlas;

        if (mSelectAtlas != atlas)
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
        mSelectAtlas = atlas;
        Repaint();
    }
    void DrawSelectAtlas()
    {
        ComponentSelector.Draw<UIAtlas>("Select", mSelectAtlas, OnSelectAtlas);
        //ComponentSelector.Draw<UIAtlas>("Select", mSelectAtlas, OnSelectAtlas, true);
        GUILayout.BeginHorizontal();
        {
            //编辑器刷新之后可能刷掉数据了，所以当没有数据的时候强制再去获取一次
            if (spriteUseStates.Count==0)
            {
                if (mSelectAtlas != null)
                {
                    //mSelectAtlasName = mSelectAtlas.spriteMaterial.mainTexture.name;
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
    bool mShowPrefabs = false;
     Object mFolder;
    void OnSelectFolder(Object obj)
     {
         mFolder = obj;
         if (mFolder!=null)
        {
            mPath = AssetDatabase.GetAssetPath(mFolder);

            mPrefabNames.Clear();
            mPrefabNames = NGUIHelperUtility.GetAllPrefabs(mPath);
        }
     }
    void DrawSelectPath()
    {
        GUILayout.BeginHorizontal();
        {
            //ComponentSelector.Draw<Object>("Select", mFolder, OnSelectFolder);
            mFolder = EditorGUILayout.ObjectField("Select Path:", mFolder, typeof(Object), false) as Object;
            //mFolder = EditorGUILayout.ObjectField(mFolder, typeof(Object), false) as Object;
            if (mFolder != null)
            {
                mPath = AssetDatabase.GetAssetPath(mFolder);

                mPrefabNames.Clear();
                mPrefabNames = NGUIHelperUtility.GetAllPrefabs(mPath);
            }
            GUILayout.Label(mPath);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Instantiate All the Prefabs", GUILayout.Width(200)))
            {
                if (!string.IsNullOrEmpty(mPath))
                {
                    GameObject panel = UICreateNewUIWizard.CreateNewUI();
                    GameObject parent = EditorUtility.CreateGameObjectWithHideFlags(RootName, HideFlags.DontSave);
                    parent.transform.parent = panel.transform;
                    parent.transform.localPosition = Vector3.zero;
                    foreach (string item in mPrefabNames)
                    {
                        GameObject go = NGUIHelperUtility.InstantiatePrefab(item);
                        if (go != null)
                            go.transform.parent = parent.transform;
                    }
                }
            }
        }
        GUILayout.EndHorizontal();

        mShowPrefabs = EditorGUILayout.Foldout(mShowPrefabs, "Show Prefabs");
        if (mShowPrefabs)
        {
            //Debug.Log(mPath);
            if (!string.IsNullOrEmpty(mPath))
            {
                mScroll2 = GUILayout.BeginScrollView(mScroll2, GUILayout.MinHeight(200));
                {
                    //mPrefabNames.Clear();
                    //mPrefabNames = NGUIEditorToolsExt.GetAllPrefabs(mPath);
                    foreach (string name in mPrefabNames)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(name);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
            }
        }
    }
    List<string> mPrefabNames = new List<string>();

    Vector2 mScroll2 = Vector2.zero;
    Vector2 mScroll = Vector2.zero;
    GUIStyle mStyle = new GUIStyle();

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

                                            if (NGUITools.GetActive(go))
                                            {
                                                //GUI.backgroundColor = Color.black;
                                                GUI.contentColor = Color.black;
                                            }
                                            else
                                            {
                                                //GUI.backgroundColor = Color.gray;
                                                GUI.contentColor = Color.gray;
                                            }

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
        DrawSelectPath();
        DrawSelectAtlas();
        NGUIEditorTools.DrawSeparator();
        //DrawTestButton();
        //SetSpriteUseState(NGUIEditorTools.FindAll<UISprite>());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Detect", GUILayout.Width(200)))
        {
            GetSpriteUseState();
        }
        mShowRelatedSprites = EditorGUILayout.Toggle("Show Related Sprite", mShowRelatedSprites);
        GUILayout.EndHorizontal();
        DrawWidgets();
    }
    const string RootName = "SpiderRoot";
    //void DrawTestButton()
    //{
    //    GUILayout.BeginHorizontal();
    //    {
    //        if (GUILayout.Button("Start Detect", GUILayout.Width(150)))
    //        {
    //            SetSpriteUseState(NGUIEditorTools.FindAll<UISprite>());
    //        }
    //        GUILayout.Space(50);
    //        mShowRelatedSprites = EditorGUILayout.Toggle("Show Related Sprite", mShowRelatedSprites);
    //    }
    //    GUILayout.EndHorizontal();
    //}

    void GetSpriteUseState()
    {
        mPrefabNames = NGUIHelperUtility.GetAllPrefabs(mPath);
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
