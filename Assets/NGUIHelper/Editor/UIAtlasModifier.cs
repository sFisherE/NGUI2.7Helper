using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


class UIAtlasModifier : EditorWindow
{

    [MenuItem("NGUIHelper/Atlas Modifier")]
    static public void openAtlasExchanger()
    {
        EditorWindow.GetWindow<UIAtlasModifier>(false, "Atlas Modifier", true);
    }

    UIAtlas mAtlasA;
    UIAtlas mAtlasB;
    void OnSelectAtlasA(Object obj)
    {
        mAtlasA = obj as UIAtlas;
        Repaint();
    }
    void OnSelectAtlasB(Object obj)
    {
        mAtlasB = obj as UIAtlas;
        Repaint();
    }
    Vector2 mScrollPosA;
    Vector2 mScrollPosB;



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




        List<AtlasUtility.SpriteEntry> mSpritesFromAtlasA = new List<AtlasUtility.SpriteEntry>();
        List<AtlasUtility.SpriteEntry> mSpritesFromAtlasB = new List<AtlasUtility.SpriteEntry>();

        if (GUILayout.Button("test"))
        {
            //将A中要移除的sprite抽出来，然后将texture保存下来
            if (mAtlasA != null && mRemovedSpriteA.Count > 0)
            {
                Texture2D tex = mAtlasA.texture as Texture2D;
                UIAtlasSpliter.MakeReadable(ref tex);
                foreach (var v in mAtlasA.spriteList)
                {
                    if (mRemovedSpriteA.Contains(v.name))
                    {
                        tex = UIAtlasSpliter.ExtractTexture(v, mAtlasA);
                        AtlasUtility.SpriteEntry en = new AtlasUtility.SpriteEntry();
                        en.tex = tex;
                        mSpritesFromAtlasA.Add(en);
                    }
                }

                List<AtlasUtility.SpriteEntry> sprites = new List<AtlasUtility.SpriteEntry>();
                AtlasUtility.ExtractSprites(mAtlasA, sprites);

                //AssetDatabase.DeleteAsset(UNeedSetting.rawPath + "/" + mAtlasA.name);
                //AssetDatabase.CreateFolder(UNeedSetting.rawPath, mAtlasA.name);
                //foreach (var v in sprites)
                //{
                //    byte[] bytes = v.tex.EncodeToPNG();
                //    string path = UNeedSetting.rawPath + "/" + mAtlasA.name + "/" + v.name + ".png";
                //    System.IO.File.WriteAllBytes(path, bytes);
                //}
                //AssetDatabase.Refresh();

                //for (int i = sprites.Count; i > 0; )
                //{
                //    AtlasUtility.SpriteEntry ent = sprites[--i];
                //    if (mRemovedSpriteA.Contains(ent.name))
                //        sprites.RemoveAt(i);
                //}
                //AtlasUtility.UpdateAtlas(mAtlasA, sprites);
                //AssetDatabase.Refresh();
            }
            //将B中要移除的sprite抽出来，保存

            //将B中抽出来的sprite存到A中

            //将A中抽出来的sprite存到B中

        }
        GUILayout.EndVertical();
    }



    static public void DrawVerticalSeparator()
    {
        //GUILayout.Space(6f);
        const int Space = 6;
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = NGUIEditorTools.blankTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            //Debug.Log(rect.ToString());
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(rect.xMax + Space, 0, 4, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax + Space, 0, 1, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax + 3 + Space, 0, 1, rect.height), tex);
            GUI.color = Color.white;
        }
        GUILayout.Space(18f);
    }

    List<string> mRemovedSpriteA = new List<string>();
    List<string> mRemovedSpriteB = new List<string>();

    void DrawAtlasSprites(bool left)
    {
        if (left)
            mScrollPosA = GUILayout.BeginScrollView(mScrollPosA);
        else
            mScrollPosB = GUILayout.BeginScrollView(mScrollPosA);
        if (left)
            ComponentSelector.Draw<UIAtlas>("Select", mAtlasA, OnSelectAtlasA);
        else
            ComponentSelector.Draw<UIAtlas>("Select", mAtlasB, OnSelectAtlasB);

        if ((left && mAtlasA != null) || (!left && mAtlasB != null))
        {
            //NGUIEditorTools.DrawHeader("Sprites", true);
            {
                List<UIAtlas.Sprite> sprites = left ? mAtlasA.spriteList : mAtlasB.spriteList;
                List<string> removedSprites = left ? mRemovedSpriteA : mRemovedSpriteB;
                int index = 0;
                foreach (UIAtlas.Sprite sprite in sprites)
                {
                    ++index;
                    GUILayout.Space(-1f);

                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                    GUI.backgroundColor = Color.white;
                    GUILayout.Label(index.ToString(), GUILayout.Width(24f));
                    GUILayout.Label(sprite.name);

                    string arrowString = left ? "\u25BA" : "\u25C4";
                    if (removedSprites.Contains(sprite.name))
                    {
                        GUI.backgroundColor = Color.red;
                        if (!GUILayout.Toggle(true, arrowString, "ButtonRight", GUILayout.Width(40)))
                        {
                            removedSprites.Remove(sprite.name);
                        }
                    }
                    else
                    {
                        if (GUILayout.Toggle(false, arrowString, "ButtonRight", GUILayout.Width(40)))
                        {
                            removedSprites.Add(sprite.name);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }
        }
        GUILayout.EndScrollView();
    }
}
