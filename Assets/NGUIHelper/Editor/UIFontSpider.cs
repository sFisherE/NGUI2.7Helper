using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

/// <summary>
///   查找某个字体的使用情况
/// </summary>
using Object = UnityEngine.Object;
public class UIFontSpider : EditorWindow
{
    [MenuItem("NGUIHelper/Find/Font Spider")]
    static void Init()
    {
        UIFontSpider window = (UIFontSpider)EditorWindow.GetWindow(typeof(UIFontSpider), false, "Font Spider");
    }

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

    UIFont mFont;
    void OnGUI()
    {
        ComponentSelector.Draw<UIFont>("Select", mFont, obj =>
            {
                UIFont font = obj as UIFont;
                if (font != mFont)
                    mFont = font;
            });


        NGUIEditorTools.DrawSeparator();
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
                UILabel[] labels = go.GetComponentsInChildren<UILabel>(true);
                bool find = false;
                foreach (var l in labels)
                {
                    if (l.font == mFont)
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                {
                    mRelatedGameObjects.Add(go);
                }
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
                    UILabel[] labels = g.GetComponentsInChildren<UILabel>(true);
                    labels = Array.FindAll(labels, p => p.font == mFont);

                    foreach (var label in labels)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30f);
                        string path = NGUIHelperUtility.GetGameObjectPath(label.gameObject);
                        if (Selection.activeGameObject == label.gameObject)
                            GUI.contentColor = Color.blue;
                        else
                            GUI.contentColor = new Color32(70, 70, 70, 255);
                        if (GUILayout.Button(path, EditorStyles.whiteLabel, GUILayout.MinWidth(100f)))
                        {
                            Selection.activeGameObject = label.gameObject;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        GUILayout.EndScrollView();
    }
}
