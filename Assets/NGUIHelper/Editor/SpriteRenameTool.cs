using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
/// <summary>
///   rename sprite in atlas,to make management neatly
/// </summary>
class SpriteRenameTool : EditorWindow
{
    UIAtlas mSelectAtlas;
    void DrawSelectAtlas()
    {
        ComponentSelector.Draw<UIAtlas>("Select", mSelectAtlas, obj =>
        {
            UIAtlas atlas = obj as UIAtlas;
            if (atlas != null && mSelectAtlas != atlas)
            {
                mSelectAtlas = atlas;
                stateList.Clear();
                foreach (UIAtlas.Sprite s in mSelectAtlas.spriteList)
                {
                    Entry en = new Entry();
                    en.spriteName = s.name;
                    en.spriteNameTo = string.Empty;
                    stateList.Add(en);
                }
            }

            Repaint();
        });
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

    public class Entry
    {
        public string spriteName;
        public string spriteNameTo;
        public bool changed
        {
            get
            {
                if (!string.IsNullOrEmpty(spriteNameTo))
                {
                    if (spriteName != spriteNameTo)
                        return true;
                }
                return false;
            }
        }
        public bool available = false;
    }
    List<Entry> stateList = new List<Entry>();
    void OnGUI()
    {
        DrawSelectAtlas();
        if (stateList.Count == 0 && mSelectAtlas != null)
        {
            foreach (UIAtlas.Sprite s in mSelectAtlas.spriteList)
            {
                Entry en = new Entry();
                en.spriteName = s.name;
                en.spriteNameTo = string.Empty;
                stateList.Add(en);
            }
        }
        NGUIEditorTools.DrawSeparator();
        DrawSelectPath();
        NGUIEditorTools.DrawSeparator();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Process", GUILayout.Width(200)))
        {
            if (mSelectAtlas != null)
            {
                Process();
            }
        }
        GUILayout.EndHorizontal();
        DrawWidgets();

    }
    void Process()
    {
        //colect all the changed item
        List<Entry> changedList = new List<Entry>();
        foreach (var state in stateList)
        {
            if (state.changed && state.available)
                changedList.Add(state);
        }
        bool change = false;

        //check prefab first
        if (!string.IsNullOrEmpty(mPath))
        {
            List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(mPath);

            foreach (var goPath in paths)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(goPath, typeof(GameObject)) as GameObject;
                if (go != null)
                {
                    UISprite[] sprites = go.GetComponentsInChildren<UISprite>(true);
                    change = false;
                    foreach (var s in sprites)
                    {
                        if (s.atlas == mSelectAtlas)
                        {
                            Entry en = changedList.Find(p => p.spriteName == s.spriteName);
                            if (en != null)
                            {
                                s.spriteName = en.spriteNameTo;
                                s.MarkAsChanged();
                                change = true;
                            }
                        }
                    }
                    if (change)
                        EditorUtility.SetDirty(go);
                }
            }
        }
        //check scene
       List<UISprite> sceneSpites= NGUIEditorTools.FindInScene<UISprite>();
       foreach (var s in sceneSpites)
       {
           if (s.atlas == mSelectAtlas)
           {
               Entry en = changedList.Find(p => p.spriteName == s.spriteName);
               if (en != null)
                   s.spriteName = en.spriteNameTo;
           }
       }
        //change the atlas
        change = false;
        foreach (UIAtlas.Sprite s in mSelectAtlas.spriteList)
        {
            Entry en = changedList.Find(p => p.spriteName == s.name);
            if (en != null)
            {
                s.name = en.spriteNameTo;
                change = true;

            }
        }
        if (change)
            EditorUtility.SetDirty(mSelectAtlas);

        //stateList.Clear();
        foreach (var state in stateList)
        {
            Entry en = changedList.Find(p => p.spriteName == state.spriteName);
            if (en!=null)
                en.spriteNameTo = string.Empty;
        }

        AssetDatabase.Refresh();
    }
    Vector2 mScroll = Vector2.zero;
    void DrawWidgets()
    {
        if (mSelectAtlas != null)
        {
            mScroll = GUILayout.BeginScrollView(mScroll);
            {
                GUILayout.BeginVertical();
                {
                    foreach (var state in stateList)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (state.changed)
                                GUI.backgroundColor = NGUIHelperSettings.Green;
                            Entry sameNameEn = stateList.Find(p => p.spriteName == state.spriteNameTo);
                            if (sameNameEn == null)
                                state.available = true;
                            else
                                state.available = false;

                            if (!state.available)
                                GUI.backgroundColor = NGUIHelperSettings.Red;

                            GUILayout.Label(state.spriteName, "HelpBox", GUILayout.Width(150), GUILayout.Height(18f));
                            if (GUILayout.Button("Sprite", "DropDownButton", GUILayout.Width(76f)))
                            {
                                SpriteSelector.Show(mSelectAtlas, state.spriteName, null);
                            }
                            state.spriteNameTo = GUILayout.TextField(state.spriteNameTo);
                            //GUILayout.BeginHorizontal();
                            if (!string.IsNullOrEmpty(state.spriteNameTo))
                            {
                                //GUI.contentColor = available ? Color.green : Color.red;
                                GUILayout.Label(state.available ? "☑" : "☒");
                            }
                            //GUILayout.EndHorizontal();
                            //state.spriteNameTo = GUILayout.TextField(state.spriteNameTo);
                            GUI.backgroundColor = Color.white;

                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
    }
}
