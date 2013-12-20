using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

/// <summary>
///   find the gameobject which is missing behaviour
/// </summary>
using Object = UnityEngine.Object;
public class UIMissingMonoBehaviourSpider : EditorWindow
{
    #region  WorkingPath
    string mPath;
    Object mFolder;
    void DrawSelectPath()
    {
        Object folder = EditorGUILayout.ObjectField("Select Path:", mFolder, typeof(Object), false) as Object;
        if (folder != mFolder)
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
    bool mShowSpecific = false;

    void OnGUI()
    {
        DrawSelectPath();
        NGUIEditorTools.DrawSeparator();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Detect", GUILayout.Width(200)))
        {
            mRelatedGameObjects.Clear();

            List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(mPath);
            foreach (var p in paths)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(p, typeof(GameObject)) as GameObject;

                MonoBehaviour[] coms = go.GetComponentsInChildren<MonoBehaviour>(true);
                bool find = false;
                foreach (var v in coms)
                {
                    if (v == null)
                        find = true;
                }
                if (find)
                    mRelatedGameObjects.Add(go);
            }
        }
        mShowSpecific = EditorGUILayout.Toggle("Show Specific", mShowSpecific);
        GUILayout.EndHorizontal();
        DrawRelatedGameObject();
    }
    List<GameObject> mRelatedGameObjects = new List<GameObject>();
    void DrawRelatedGameObject()
    {
        mScroll = GUILayout.BeginScrollView(mScroll);
        {
            foreach (var g in mRelatedGameObjects)
            {
                if (Selection.activeGameObject == g)
                    GUI.contentColor = Color.blue;
                else
                    GUI.contentColor = Color.black;
                if (GUILayout.Button(AssetDatabase.GetAssetPath(g), EditorStyles.whiteLabel, GUILayout.MinWidth(100f)))
                {
                    Selection.activeGameObject = g;
                }
                if (mShowSpecific)
                {
                    List<Transform> children = GetChildren(g.transform);
                    List<GameObject> findGos = new List<GameObject>();
                    foreach (var c in children)
                    {
                        MonoBehaviour[] coms = c.GetComponents<MonoBehaviour>();
                        bool find = false;
                        foreach (var v in coms)
                        {
                            if (v == null)
                                find = true;
                        }
                        if (find)
                            findGos.Add(c.gameObject);
                    }
                    foreach (var go in findGos)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30f);
                        string path = NGUIHelperUtility.GetGameObjectPath(go);
                        if (Selection.activeGameObject == go)
                            GUI.contentColor = Color.blue;
                        else
                            GUI.contentColor = new Color32(70, 70, 70, 255);
                        if (GUILayout.Button(path, EditorStyles.whiteLabel, GUILayout.MinWidth(100f)))
                        {
                            Selection.activeGameObject = go;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        GUILayout.EndScrollView();
    }

    List<Transform> GetChildren(Transform tf)
    {
        List<Transform> list = new List<Transform>();
        GetChildren(tf, list);
        return list;
    }
    void GetChildren(Transform tf, List<Transform> list)
    {
        for (int i = 0; i < tf.childCount; i++)
        {
            Transform child = tf.GetChild(i);
            list.Add(child);
            GetChildren(child, list);
        }
    }
}
