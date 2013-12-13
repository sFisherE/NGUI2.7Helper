using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
///   move a sprite from atlasA to atlasB，meanwhile change all the UISprite components in prefab.
///   
/// roadmap:
/// @1.
/// </summary>
class UIAtlasExchanger : EditorWindow
{
    [MenuItem("NGUIHelper/Atlas Exchanger")]
    static public void openAtlasExchanger()
    {
        EditorWindow.GetWindow<UIAtlasExchanger>(false, "Atlas Exchanger", true);
    }

    UIAtlas mAtlasA;
    UIAtlas mAtlasB;
    string mPathA;
    string mPathB;


    //同名比较难处理，直接不处理，由其他工具进行处理
    /// <summary>
    ///   从A中删除一张卡时，先看一下，是不是在removeFromB中，如果在，那就从removeFromB中删掉
    ///   给A添加一张卡时，先看一下，是不是在removeFromA中，如果在，从removeFromA中删掉
    ///   根据addToA去atlasB中获取相应的sprite的数据，然后添加在addToA中
    ///   根据removeFromA中的数据，
    /// </summary>
    List<string> mRemoveFromA = new List<string>();
    List<string> mRemoveFromB = new List<string>();


    bool getTextureSuccess = false;
    public List<string> GetTextures(string path, bool left)
    {
        List<string> paths = new List<string>();
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists)
        {
            getTextureSuccess = true;
            FileInfo[] textures = dirInfo.GetFiles("*.png");
            foreach (FileInfo f in textures)
            {
                string fullPath = f.ToString();
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("Assets");
                System.Text.RegularExpressions.Match match = regex.Match(fullPath);
                paths.Add(fullPath.Substring(match.Index));
            }
        }
        else
        {
            getTextureSuccess = false;
            if (EditorUtility.DisplayDialog("警告", "本地数据不存在，是否使用AtlasSpliter进行同步？", "ok", "cancle"))
            {
                UIAtlasSpliter spliter = EditorWindow.GetWindow<UIAtlasSpliter>(false, "AtlasSpliter", true);
                spliter.atlas = left ? mAtlasA : mAtlasB;
                if (left)
                {
                    mAtlasA = null;
                    mPathA = string.Empty;
                }
                else
                {
                    mAtlasB = null;
                    mPathB = null;
                }
            }
        }
        return paths;
    }
    List<Texture2D> mTexturesListA = new List<Texture2D>();
    List<Texture2D> mTexturesListB = new List<Texture2D>();

    void OnSelectAtlasA(Object obj)
    {
        mAtlasA = obj as UIAtlas;
        mPathA = NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlasA.name;

        mTexturesListA.Clear();
        List<string> paths = GetTextures(mPathA, true);
        if (!getTextureSuccess)
        {
            return;
        }
        foreach (var p in paths)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath(p, typeof(Texture2D)) as Texture2D;
            mTexturesListA.Add(tex);
        }
        //检测atlas中的数据和文件夹中的数据是否同步，不同则弹窗询问是否同步
        bool same = true;
        List<UIAtlas.Sprite> sprites = mAtlasA.spriteList;
        if (mTexturesListA.Count == sprites.Count)
        {
            foreach (var s in sprites)
            {
                int index = mTexturesListA.FindIndex(p => p.name == s.name);
                if (index < 0)
                {
                    same = false;
                    break;
                }
            }
        }
        else
            same = false;

        if (!same)
        {
            if (EditorUtility.DisplayDialog("警告", "atlas与本地数据不同步，是否使用AtlasSpliter进行同步？", "ok", "cancle"))
            {
                UIAtlasSpliter spliter = EditorWindow.GetWindow<UIAtlasSpliter>(false, "AtlasSpliter", true);
                spliter.atlas = mAtlasA;
                mAtlasA = null;
                mPathA = null;
            }
        }
        Repaint();
    }
    void OnSelectAtlasB(Object obj)
    {
        mAtlasB = obj as UIAtlas;
        mPathB = NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlasB.name;
        mTexturesListB.Clear();
        List<string> paths = GetTextures(mPathB, false);
        foreach (var p in paths)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath(p, typeof(Texture2D)) as Texture2D;
            mTexturesListB.Add(tex);
        }
        #region 检查是否相同
        //检测atlas中的数据和文件夹中的数据是否同步，不同则弹窗询问是否同步
        bool same = true;
        List<UIAtlas.Sprite> sprites = mAtlasB.spriteList;
        if (mTexturesListB.Count == sprites.Count)
        {
            foreach (var s in sprites)
            {
                int index = mTexturesListB.FindIndex(p => p.name == s.name);
                if (index < 0)
                {
                    same = false;
                    break;
                }
            }
        }
        else
            same = false;
        if (!same)
        {
            if (EditorUtility.DisplayDialog("警告", "atlas与本地数据不同步，是否使用AtlasSpliter进行同步？", "ok", "cancle"))
            {
                UIAtlasSpliter spliter = EditorWindow.GetWindow<UIAtlasSpliter>(false, "AtlasSpliter", true);
                spliter.atlas = mAtlasB;
                mAtlasB = null;
                mPathB = string.Empty;
            }
        }
        #endregion
        Repaint();
    }

    void GetTexturesInPath(string path)
    {
        AssetDatabase.GetAllAssetPaths();
    }


    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        //DrawA();
        DrawAtlasSprites(true);
        //GUILayout.Space(20f);
        DrawVerticalSeparator();
        DrawAtlasSprites(false);
        //DrawB();
        GUILayout.EndHorizontal();

        NGUIEditorTools.DrawSeparator();

        if (GUILayout.Button("Process"))
        {
            if (mAtlasA != null && mAtlasB != null && (mRemoveFromA.Count > 0 || mRemoveFromB.Count > 0))
            {
                List<AtlasUtility.SpriteEntry> spritesFromA = new List<AtlasUtility.SpriteEntry>();
                AtlasUtility.ExtractSprites(mAtlasA, spritesFromA);
                //保存border信息
                mAtlasA.coordinates = UIAtlas.Coordinates.Pixels;
                //List<UIAtlas.Sprite> spriteListA = new List<UIAtlas.Sprite>();
                List<AtlasUtility.BorderEntry> borderListA = new List<AtlasUtility.BorderEntry>();
                foreach (var s in mAtlasA.spriteList)
                {
                    if (spritesFromA.Find(p=>p.tex.name==s.name)!=null)
                    {
                        var ns = new AtlasUtility.BorderEntry();
                        ns.name = s.name;
                        //Rect outer = s.outer;
                        //Rect inner = s.inner;
                        //ns.border = new Vector4(inner.xMin - outer.xMin, 
                        //    inner.yMin - outer.yMin, 
                        //    outer.xMax - inner.xMax, 
                        //    outer.yMax - inner.yMax);
                        ns.border = AtlasUtility.GetBorder(s);
                        borderListA.Add(ns);
                    }
                }
                List<AtlasUtility.SpriteEntry> spritesFromB = new List<AtlasUtility.SpriteEntry>();
                AtlasUtility.ExtractSprites(mAtlasB, spritesFromB);
                List<AtlasUtility.BorderEntry> borderListB = new List<AtlasUtility.BorderEntry>();
                foreach (var s in mAtlasB.spriteList)
                {
                    if (spritesFromB.Find(p => p.tex.name == s.name) != null)
                    {
                        var ns = new AtlasUtility.BorderEntry();
                        ns.name = s.name;
                        //Rect outer = s.outer;
                        //Rect inner = s.inner;
                        //ns.border = new Vector4(inner.xMin - outer.xMin, 
                        //    inner.yMin - outer.yMin, 
                        //    outer.xMax - inner.xMax, 
                        //    outer.yMax - inner.yMax);
                        ns.border = AtlasUtility.GetBorder(s);
                        borderListB.Add(ns);
                    }
                }

                List<AtlasUtility.SpriteEntry> tempSprites = new List<AtlasUtility.SpriteEntry>();
                tempSprites.AddRange(spritesFromA);
                tempSprites.AddRange(spritesFromB);
                foreach (var name in mRemoveFromA)
                {
                    AtlasUtility.SpriteEntry data = spritesFromA.Find(p => p.tex.name == name);
                    if (data != null)
                    {
                        spritesFromB.Add(data);
                        spritesFromA.Remove(data);
                    }
                }
                foreach (var name in mRemoveFromB)
                {
                    AtlasUtility.SpriteEntry data = spritesFromB.Find(p => p.tex.name == name);
                    if (data != null)
                    {
                        spritesFromA.Add(data);
                        spritesFromB.Remove(data);
                    }
                }

                AtlasUtility.UpdateAtlas(mAtlasA, spritesFromA);
                AtlasUtility.UpdateAtlas(mAtlasB, spritesFromB);
                //更新border信息
                foreach (var b in borderListA)
                {
                    UIAtlas.Sprite sp = mAtlasB.spriteList.Find(p => p.name == b.name);
                    if (sp!=null)
                    {
                        AtlasUtility.UpdateBorder(sp, b.border);
                    }
                }
                foreach (var b in borderListB)
                {
                    UIAtlas.Sprite sp = mAtlasA.spriteList.Find(p => p.name == b.name);
                    if (sp != null)
                    {
                        AtlasUtility.UpdateBorder(sp, b.border);
                    }
                }

                //将相应的使用的sprite的spritename换掉
                List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(NGUIHelperSetting.GetPrefabPath());
                foreach (var path in paths)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (go != null)
                    {
                        UISprite[] sprites = go.GetComponentsInChildren<UISprite>(true);
                        //匹配
                        bool changed = false;
                        foreach (var s in sprites)
                        {
                            if (s.atlas == mAtlasA)
                            {
                                int index = mRemoveFromA.FindIndex(p => p == s.spriteName);
                                if (index >= 0)
                                {
                                    s.atlas = mAtlasB;
                                    changed = true;
                                }
                            }
                            else if (s.atlas == mAtlasB)
                            {
                                int index = mRemoveFromB.FindIndex(p => p == s.spriteName);
                                if (index >= 0)
                                {
                                    s.atlas = mAtlasA;
                                    changed = true;
                                }
                            }
                        }
                        if (changed)
                        {
                            EditorUtility.SetDirty(go);
                        }
                    }
                }
                mRemoveFromA.Clear();
                mRemoveFromB.Clear();
                AssetDatabase.Refresh();
                Resources.UnloadUnusedAssets();
                //重新刷新一遍数据
                OnSelectAtlasA(mAtlasA);
                OnSelectAtlasB(mAtlasB);
            }
        }
        GUILayout.EndVertical();
    }
    void DeleteSprite(UIAtlas atlas, List<string> removedList)
    {
        if (atlas != null && removedList != null && removedList.Count > 0)
        {
            Texture2D tex = mAtlasA.texture as Texture2D;
            UIAtlasSpliter.MakeReadable(ref tex);

            List<AtlasUtility.SpriteEntry> sprites = new List<AtlasUtility.SpriteEntry>();
            AtlasUtility.ExtractSprites(atlas, sprites);
            for (int i = sprites.Count; i > 0; )
            {
                AtlasUtility.SpriteEntry ent = sprites[--i];
                if (removedList.Contains(ent.tex.name))
                    sprites.RemoveAt(i);
            }
            AtlasUtility.UpdateAtlas(atlas, sprites);
        }
    }

    int leftWidth;
    int rightWidth
    {
        get { return Screen.width - leftWidth - 18; }
    }
    public void DrawVerticalSeparator()
    {
        const int Space = 6;
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = NGUIEditorTools.blankTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            leftWidth = (int)rect.xMax;
            //Debug.Log(rect.ToString());
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(rect.xMax + Space, 0, 4, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax + Space, 0, 1, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax + 3 + Space, 0, 1, rect.height), tex);
            GUI.color = Color.white;
        }
        GUILayout.Space(18f);
    }


    Vector2 mScrollPosA;
    Vector2 mScrollPosB;
    void DrawAtlasSprites(bool left)
    {
        if (left)
            mScrollPosA = GUILayout.BeginScrollView(mScrollPosA);
        else
            mScrollPosB = GUILayout.BeginScrollView(mScrollPosB);
        if (left)
        {
            ComponentSelector.Draw<UIAtlas>("Select", mAtlasA, OnSelectAtlasA);
            GUILayout.BeginHorizontal();
            GUILayout.Label(mPathA);
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                mAtlasA = null;
                mTexturesListA.Clear();
                mPathA = string.Empty;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            ComponentSelector.Draw<UIAtlas>("Select", mAtlasB, OnSelectAtlasB);
            GUILayout.BeginHorizontal();
            GUILayout.Label(mPathB);
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                mAtlasB = null;
                mTexturesListB.Clear();
                mPathB = string.Empty;
            }
            GUILayout.EndHorizontal();
        }
        if ((left && mAtlasA != null) || (!left && mAtlasB != null))
        {
            DrawTextures(left);
        }
        GUILayout.EndScrollView();
    }


    Vector2 mPos;
    Vector2 mPos2;
    Texture2D mSelectTexture;
    void DrawTextures(bool left)
    {
        List<Texture2D> textures = left ? mTexturesListA : mTexturesListB;
        EditorGUIUtility.labelWidth = 80f;
        bool close = false;
        GUILayout.Label(left ? mAtlasA.name : mAtlasB.name + " Sprites", "LODLevelNotifyText");
        NGUIEditorTools.DrawSeparator();

        GUILayout.BeginHorizontal();
        GUILayout.Space(84f);

        GUILayout.EndHorizontal();

        float size = 80f;
        float padded = size + 10f;

        int columns = Mathf.FloorToInt((left ? leftWidth : rightWidth) / padded);
        if (columns < 1) columns = 1;

        int offset = 0;
        Rect rect = new Rect(10f, 0, size, size);

        GUILayout.Space(10f);
        if (left)
        {
            mPos = GUILayout.BeginScrollView(mPos/*, GUILayout.Height(200)*/);
        }
        else
        {
            mPos2 = GUILayout.BeginScrollView(mPos2/*, GUILayout.Height(200)*/);
        }

        while (offset < textures.Count)
        {
            GUILayout.BeginHorizontal();
            {
                int col = 0;
                rect.x = 10f;
                for (; offset < textures.Count; ++offset)
                {
                    Texture2D tex = textures[offset];

                    if (tex == null) continue;
                    // Button comes first
                    if (GUI.Button(rect, ""))
                    {
                        //Debug.Log(tex.name + " clicked");
                        Selection.activeInstanceID = tex.GetInstanceID();

                        if (tex != mSelectTexture)
                        {
                            mSelectTexture = tex;
                        }
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        // On top of the button we have a checkboard grid
                        //NGUIEditorTools.DrawTiledTexture(rect, NGUIEditorTools.backdropTexture);
                        // Calculate the texture's scale that's needed to display the sprite in the clipped area
                        float scaleX = rect.width;// / uv.width;
                        float scaleY = rect.height;// / uv.height;

                        // Stretch the sprite so that it will appear proper
                        float aspect = (scaleY / scaleX) / ((float)tex.height / tex.width);
                        Rect clipRect = rect;

                        if (aspect != 1f)
                        {
                            if (aspect < 1f)
                            {
                                // The sprite is taller than it is wider
                                float padding = size * (1f - aspect) * 0.5f;
                                clipRect.xMin += padding;
                                clipRect.xMax -= padding;
                            }
                            else
                            {
                                // The sprite is wider than it is taller
                                float padding = size * (1f - 1f / aspect) * 0.5f;
                                clipRect.yMin += padding;
                                clipRect.yMax -= padding;
                            }
                        }
                        GUI.DrawTexture(clipRect, tex);
                        // Draw the selection
                        if (tex == mSelectTexture)
                        {
                            NGUIEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
                        }
                    }

                    GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                    GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                    if (tex == mSelectTexture)
                    {
                        if (GUI.Button(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), left ? "\u25BA" : "\u25C4"))
                        {
                            Debug.Log("move to " + (left ? "left" : "right"));
                            string leftPath = mPathA + "/" + tex.name + ".png";
                            string rightPath = mPathB + "/" + tex.name + ".png";
                            if (mAtlasA != null && mAtlasB != null)
                            {
                                if (left)
                                {
                                    //删掉目标文件夹下同名的文件
                                    Texture2D t = AssetDatabase.LoadAssetAtPath(rightPath, typeof(Texture2D)) as Texture2D;
                                    if (t != null)
                                    {
                                        Debug.LogWarning("不能移动同名文件，可以使用SpriteReplace进行处理");
                                        //mTexturesB.Remove(t);
                                        //AssetDatabase.DeleteAsset(rightPath);
                                    }
                                    else
                                    {
                                        //移动文件
                                        AssetDatabase.MoveAsset(leftPath, rightPath);
                                        mTexturesListA.Remove(tex);
                                        mTexturesListB.Add(tex);

                                        if (mRemoveFromB.Contains(tex.name))
                                            mRemoveFromB.Remove(tex.name);
                                        else
                                            mRemoveFromA.Add(tex.name);
                                    }
                                }
                                else
                                {
                                    Texture2D t = AssetDatabase.LoadAssetAtPath(leftPath, typeof(Texture2D)) as Texture2D;
                                    if (t != null)
                                    {
                                        Debug.LogWarning("不能移动同名文件，可以使用SpriteReplace进行处理");
                                        //mTexturesA.Remove(t);
                                        //AssetDatabase.DeleteAsset(leftPath);
                                    }
                                    else
                                    {
                                        AssetDatabase.MoveAsset(rightPath, leftPath);
                                        mTexturesListB.Remove(tex);
                                        mTexturesListA.Add(tex);
                                        if (mRemoveFromA.Contains(tex.name))
                                            mRemoveFromA.Remove(tex.name);
                                        else
                                            mRemoveFromB.Add(tex.name);
                                    }
                                }
                            }
                        }
                    }
                    else
                        GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), tex.name, "ProgressBarBack");

                    GUI.contentColor = Color.white;
                    GUI.backgroundColor = Color.white;

                    if (++col >= columns)
                    {
                        ++offset;
                        break;
                    }
                    rect.x += padded;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(padded + 26);
            rect.y += padded + 26;
        }
        //GUILayout.Space(padded*2.5f);
        GUILayout.EndScrollView();
        if (close) Close();
    }

}
