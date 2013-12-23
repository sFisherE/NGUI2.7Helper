using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;
class AnimationClipSpider : EditorWindow
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

    //UIFont mFont;
    //Object mAnimation;
    AnimationClip mAnimationClip;
    void OnGUI()
    {
        mAnimationClip = EditorGUILayout.ObjectField(mAnimationClip, typeof(AnimationClip),false) as AnimationClip;
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
                bool find = false;
                Animation[] anims = go.GetComponentsInChildren<Animation>(true);
                foreach (var a in anims)
                {
                    foreach ( AnimationState state in a)
                    {
                        if (state.clip == mAnimationClip)
                        {
                            find = true;
                            break;
                        }
                    }
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
                    Animation[] anims = g.GetComponentsInChildren<Animation>(true);
                    foreach (var a in anims)
                    {
                        bool find = false;
                        foreach (AnimationState state in a)
                        {
                            if (state.clip == mAnimationClip)
                            {
                                find = true;
                                break;
                            }
                        }
                        if (find)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30f);
                            string path = NGUIHelperUtility.GetGameObjectPath(a.gameObject);
                            if (Selection.activeGameObject == a.gameObject)
                                GUI.contentColor = Color.blue;
                            else
                                GUI.contentColor = new Color32(70, 70, 70, 255);
                            if (GUILayout.Button(path, EditorStyles.whiteLabel, GUILayout.MinWidth(100f)))
                            {
                                Selection.activeGameObject = a.gameObject;
                            }
                            GUILayout.EndHorizontal();
                        }

                    }
                }
            }
        }
        GUILayout.EndScrollView();
    }
}
