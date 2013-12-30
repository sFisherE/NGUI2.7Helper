//using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

using Object = UnityEngine.Object;
public class ComponentSpider : EditorWindow
{
    [MenuItem("NGUIHelper/Find/Component Spider")]
    static void Init()
    {
        ComponentSpider window = (ComponentSpider)EditorWindow.GetWindow(typeof(ComponentSpider), false, "Component Spider");
    }
    #region ForType
    public ComponentSpider()
    {
        mTypes = NGUIHelperUtility.GetTypeList();
    }
    private List<Type> mTypes = new List<Type>();
    Type GetSelectedType()
    {
        foreach (Type t in mTypes)
        {
            if (t.Name == mComponentName)
                return t;
        }
        return null;
    }
    #endregion

    #region  WorkingPath
    string mPath;
    Object mFolder;
    void DrawSelectPath()
    {
       Object folder = EditorGUILayout.ObjectField("Select Path:", mFolder, typeof(Object), false) as Object;
        if (folder!=mFolder)
        {
            mFolder = folder;
            mRelatedGameObjects.Clear();
        }
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
    #endregion

    List<string> mPrefabNames = new List<string>();

    Vector2 mScroll = Vector2.zero;
    GUIStyle mStyle = new GUIStyle();
    bool mShowSpecific = false;
    string mComponentName;
    bool mFindComponent;
    void OnGUI()
    {
       string componentName = EditorGUILayout.TextField("Component Name: ", mComponentName);
        if (componentName!=mComponentName)
        {
            mRelatedGameObjects.Clear();
            mComponentName = componentName;
        }
        Type t = GetSelectedType();
        if (mTypes.Count == 0)
            EditorGUILayout.HelpBox("Typelist is empty.", MessageType.Warning);

        if (t == null)
        {
            EditorGUILayout.HelpBox("Type doesnt exist.", MessageType.Warning);
            mFindComponent = false;
        }
        else
            mFindComponent = true;


        NGUIEditorTools.DrawSeparator();
        DrawSelectPath();
        NGUIEditorTools.DrawSeparator();
        if (mFindComponent)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Detect", GUILayout.Width(200)))
            {
                mRelatedGameObjects.Clear();

                List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(mPath);
                foreach (var p in paths)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath(p, typeof(GameObject)) as GameObject;
                    Component[] cs = go.GetComponentsInChildren(GetSelectedType(), true);
                    if (cs.Length>0)
                    {
                        mRelatedGameObjects.Add(go);
                    }
                }
            }
            mShowSpecific = EditorGUILayout.Toggle("Show Specific", mShowSpecific);
            GUILayout.EndHorizontal();
        }
        DrawRelatedGameObject();
    }
    List<GameObject> mRelatedGameObjects=new List<GameObject>();
    void DrawRelatedGameObject()
    {
        mScroll = GUILayout.BeginScrollView(mScroll);
        {
            foreach (var g in mRelatedGameObjects)
            {
                //GUILayout.Label(AssetDatabase.GetAssetPath(g));
                if (g==null)
                    continue;

                if (Selection.activeGameObject ==g)
                    GUI.contentColor = Color.blue;
                else
                    GUI.contentColor = Color.black;
                if (GUILayout.Button(AssetDatabase.GetAssetPath(g), EditorStyles.whiteLabel, GUILayout.MinWidth(100f)))
                {
                    Selection.activeGameObject = g;
                }
                if (mShowSpecific)
                {
                    Component[] cs = g.GetComponentsInChildren(GetSelectedType(), true);
                    foreach (var c in cs)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30f);
                        string path = NGUIHelperUtility.GetGameObjectPath(c.gameObject);
                        if (Selection.activeGameObject ==c.gameObject)
                            GUI.contentColor = Color.blue;
                        else
                            GUI.contentColor = new Color32(70, 70, 70, 255);
                        if (GUILayout.Button(path, EditorStyles.whiteLabel, GUILayout.MinWidth(100f)))
                        {
                            Selection.activeGameObject = c.gameObject;
                        }
                        GUILayout.EndHorizontal();
                    }
                }

            }
        }
        GUILayout.EndScrollView();
    }


    /// <summary>
    ///   实时更新
    /// </summary>
    //void OnInspectorUpdate()
    //{
    //    Repaint();
    //} 
}
