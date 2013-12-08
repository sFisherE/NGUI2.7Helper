using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class UIAtlasSpliter : EditorWindow
{
    [MenuItem("NGUIHelper/AtlasSpliter")]
    static public void OpenAtlasSplitTool()
    {
        EditorWindow.GetWindow<UIAtlasSpliter>(false, "AtlasSpliter", true);
    }

  public  UIAtlas atlas;
    Object source;
    void OnSelectAtlas(Object obj)
    {
        atlas = obj as UIAtlas;
        Repaint();
    }
    void OnGUI()
    {
        //EditorGUILayout.HelpBox("The atlas prefab Coordinates must be \"Pixels\"", MessageType.None);
        EditorGUILayout.BeginHorizontal();
        //控制label宽度
        EditorGUIUtility.labelWidth = 80f;
        ComponentSelector.Draw<UIAtlas>("Select", atlas, OnSelectAtlas);

        EditorGUILayout.EndHorizontal();
        if (atlas != null)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Target Path:   " + NGUIHelperSetting.GetRawResourcePath() + "/" + atlas.name);
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Split", GUILayout.Width(76f)))
            {
                SaveTextures();
            }
            GUI.backgroundColor = Color.white;
            DrawSprites();
        }
    }

    void SaveTextures()
    {
        AssetDatabase.DeleteAsset(NGUIHelperSetting.GetRawResourcePath() + "/" + atlas.name);//清除之前的数据
        AssetDatabase.CreateFolder(NGUIHelperSetting.GetRawResourcePath(), atlas.name);//新建一个文件夹
        AssetDatabase.Refresh();

        Texture2D tex = atlas.texture as Texture2D;
        MakeReadable(ref tex);
        //使其可读
        atlas.coordinates = UIAtlas.Coordinates.Pixels;

        foreach (var sprite in atlas.spriteList)
        {
            SaveTexture(ExtractTexture(sprite, atlas), sprite.name);
        }
        MakeTextureAnAtlas(ref tex);
        AssetDatabase.Refresh();
        //统一刷新一遍设置
        List<string> paths = AtlasUtility.GetTextures(NGUIHelperSetting.GetRawResourcePath() + "/" + atlas.name);
        foreach ( var p in paths)
        {
            AtlasUtility.MakeTextureTrue(p);
        }
        //AtlasUtility.MakeTextureTrue(path);
    }

    public static void MakeReadable(ref Texture2D tex)
    {
        //使其可读
        string path = (tex != null) ? AssetDatabase.GetAssetPath(tex.GetInstanceID()) : "";
        tex = NGUIEditorTools.ImportTexture(path, true, false);
    }

    public static void MakeTextureAnAtlas(ref Texture2D tex)
    {
        string path = (tex != null) ? AssetDatabase.GetAssetPath(tex.GetInstanceID()) : "";
        tex = NGUIEditorTools.ImportTexture(path, false, false);
    }

    public static Texture2D ExtractTexture(UIAtlas.Sprite sprite, UIAtlas atlas)
    {
        //需要将border信息存下来

        Texture2D tex = atlas.texture as Texture2D;
        Rect uv = NGUIMath.ConvertToTexCoords(sprite.outer, tex.width, tex.height);
        Debug.Log(uv.ToString());

        // left to right, bottom to top
        int x = (int)sprite.outer.x;
        int y = (int)(tex.height - sprite.outer.height - sprite.outer.y);
        y = Mathf.Max(y, 0);//有可能-1

        Color[] pix = tex.GetPixels(x, y, (int)sprite.outer.width, (int)sprite.outer.height);//从左下角开始截取
        Debug.Log(sprite.outer.ToString());
        Texture2D destTex = new Texture2D((int)sprite.outer.width, (int)sprite.outer.height);
        destTex.SetPixels(pix);
        destTex.Apply();
        return destTex;
    }


    void SaveTexture(Texture2D tex, string name)
    {
        byte[] bytes = tex.EncodeToPNG();
        string path = NGUIHelperSetting.GetRawResourcePath() + "/" + atlas.name + "/" + name + ".png";
        System.IO.File.WriteAllBytes(path, bytes);

        //AssetDatabase.Refresh();

        //AtlasUtility.MakeTextureTrue(path);
        //NGUIEditorTools.ImportTexture(path, false, true);
        //这里设置总是不太对
        //TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        //if (ti == null) return;
        //TextureImporterSettings settings = new TextureImporterSettings();
        //ti.ReadTextureSettings(settings);
        //settings.npotScale = TextureImporterNPOTScale.None;
        //settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        //settings.filterMode = FilterMode.Trilinear;
        //settings.ApplyTextureType(TextureImporterType.GUI, true);
        //ti.SetTextureSettings(settings);
    }

    Vector2 mPos;
    void DrawSprites()
    {
        EditorGUIUtility.labelWidth = 80f;
        if (atlas == null)
        {
            GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
        }
        else
        {
            bool close = false;
            GUILayout.Label(atlas.name + " Sprites", "LODLevelNotifyText");
            NGUIEditorTools.DrawSeparator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(84f);

            GUILayout.EndHorizontal();

            Texture2D tex = atlas.texture as Texture2D;

            if (tex == null)
            {
                GUILayout.Label("The atlas doesn't have a texture to work with");
                return;
            }

            BetterList<string> sprites = atlas.GetListOfSprites(string.Empty);

            float size = 80f;
            float padded = size + 10f;
            int columns = Mathf.FloorToInt(Screen.width / padded);
            if (columns < 1) columns = 1;

            int offset = 0;
            Rect rect = new Rect(10f, 0, size, size);

            GUILayout.Space(10f);
            mPos = GUILayout.BeginScrollView(mPos/*, GUILayout.Height(200)*/);

            while (offset < sprites.size)
            {
                GUILayout.BeginHorizontal();
                {
                    int col = 0;
                    rect.x = 10f;

                    for (; offset < sprites.size; ++offset)
                    {
                        UIAtlas.Sprite sprite = atlas.GetSprite(sprites[offset]);
                        if (sprite == null) continue;

                        if (Event.current.type == EventType.Repaint)
                        {
                            // On top of the button we have a checkboard grid
                            NGUIEditorTools.DrawTiledTexture(rect, NGUIEditorTools.backdropTexture);

                            Rect uv = sprite.outer;
                            if (atlas.coordinates == UIAtlas.Coordinates.Pixels)
                                uv = NGUIMath.ConvertToTexCoords(uv, tex.width, tex.height);

                            // Calculate the texture's scale that's needed to display the sprite in the clipped area
                            float scaleX = rect.width / uv.width;
                            float scaleY = rect.height / uv.height;

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

                            GUI.DrawTextureWithTexCoords(clipRect, tex, uv);
                        }

                        GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                        GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                        GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), sprite.name, "ProgressBarBack");
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
            //GUILayout.Space(padded*2);
            GUILayout.EndScrollView();
            if (close) Close();
        }
    }
}
