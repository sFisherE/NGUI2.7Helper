using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


/// <summary>
///   extracted from UIAtlasMaker mainly 
/// </summary>
internal class AtlasUtility
{
    internal class SpriteEntry
    {
        public Texture2D tex;	// Sprite texture -- original texture or a temporary texture
        public Rect rect;		// Sprite's outer rectangle within the generated texture atlas
        public int minX = 0;	// Padding, if any (set if the sprite is trimmed)
        public int maxX = 0;
        public int minY = 0;
        public int maxY = 0;
        public bool temporaryTexture = false;	// Whether the texture is temporary and should be deleted

        public Vector4 border;
    }

    public class BorderEntry
    {
        public string name;
        public Vector4 border;
    }

    /// <summary>
    ///   更新border信息,确保atlas的coordinates = UIAtlas.Coordinates.Pixels;
    /// </summary>
    public static void UpdateBorder(UIAtlas.Sprite sprite, Vector4 border)
    {
        Rect outer = sprite.outer;
        Rect inner = new Rect();
        inner.xMin = border.x + outer.xMin;
        inner.yMin = border.y + outer.yMin;
        inner.xMax = outer.xMax - border.z;
        inner.yMax = outer.yMax - border.w;
        sprite.inner = inner;
    }
    /// <summary>
    ///   从sprite中获取border信息
    /// </summary>
    public static Vector4 GetBorder(UIAtlas.Sprite sprite)
    {
        Rect outer = sprite.outer;
        Rect inner = sprite.inner;
        Vector4 border = new Vector4(inner.xMin - outer.xMin,
            inner.yMin - outer.yMin,
            outer.xMax - inner.xMax,
            outer.yMax - inner.yMax);
        return border;
    }


    /// <summary>
    /// Extract sprites from the atlas, adding them to the list.
    /// </summary>

    internal static void ExtractSprites(UIAtlas atlas, List<SpriteEntry> sprites)
    {
        // Make the atlas texture readable
        Texture2D atlasTex = NGUIEditorTools.ImportTexture(atlas.texture, true, false);

        if (atlasTex != null)
        {
            atlas.coordinates = UIAtlas.Coordinates.Pixels;

            Color32[] oldPixels = null;
            int oldWidth = atlasTex.width;
            int oldHeight = atlasTex.height;
            List<UIAtlas.Sprite> list = atlas.spriteList;

            foreach (UIAtlas.Sprite asp in list)
            {
                bool found = false;

                foreach (SpriteEntry se in sprites)
                {
                    if (asp.name == se.tex.name)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // Read the atlas
                    if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

                    Rect rect = asp.outer;
                    rect.xMin = Mathf.Clamp(rect.xMin, 0f, oldWidth);
                    rect.yMin = Mathf.Clamp(rect.yMin, 0f, oldHeight);
                    rect.xMax = Mathf.Clamp(rect.xMax, 0f, oldWidth);
                    rect.yMax = Mathf.Clamp(rect.yMax, 0f, oldHeight);

                    int newWidth = Mathf.RoundToInt(rect.width);
                    int newHeight = Mathf.RoundToInt(rect.height);
                    if (newWidth == 0 || newHeight == 0) continue;

                    Color32[] newPixels = new Color32[newWidth * newHeight];
                    int xmin = Mathf.RoundToInt(rect.x);
                    int ymin = Mathf.RoundToInt(oldHeight - rect.yMax);

                    for (int y = 0; y < newHeight; ++y)
                    {
                        for (int x = 0; x < newWidth; ++x)
                        {
                            int newIndex = y * newWidth + x;
                            int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                            newPixels[newIndex] = oldPixels[oldIndex];
                        }
                    }

                    // Create a new sprite
                    SpriteEntry sprite = new SpriteEntry();
                    sprite.temporaryTexture = true;
                    sprite.tex = new Texture2D(newWidth, newHeight);
                    sprite.tex.name = asp.name;
                    sprite.rect = new Rect(0f, 0f, newWidth, newHeight);
                    sprite.tex.SetPixels32(newPixels);
                    sprite.tex.Apply();

                    // Min/max coordinates are in pixels
                    sprite.minX = Mathf.RoundToInt(asp.paddingLeft * newWidth);
                    sprite.maxX = Mathf.RoundToInt(asp.paddingRight * newWidth);
                    sprite.minY = Mathf.RoundToInt(asp.paddingBottom * newHeight);
                    sprite.maxY = Mathf.RoundToInt(asp.paddingTop * newHeight);

                    sprites.Add(sprite);
                }
            }
        }

        // The atlas no longer needs to be readable
        NGUIEditorTools.ImportTexture(atlas.texture, false, false);
    }

    /// <summary>
    /// Update the sprite atlas, keeping only the sprites that are on the specified list.
    /// </summary>

    internal static void UpdateAtlas(UIAtlas atlas, List<SpriteEntry> sprites)
    {
        if (sprites.Count > 0)
        {
            // Combine all sprites into a single texture and save it
            if (UpdateTexture(atlas, sprites))
            {
                // Replace the sprites within the atlas
                ReplaceSprites(atlas, sprites);

                // Release the temporary textures
                ReleaseSprites(sprites);
            }
            else return;
        }
        else
        {
            atlas.spriteList.Clear();
            string path = NGUIEditorTools.GetSaveableTexturePath(atlas);
            atlas.spriteMaterial.mainTexture = null;
            if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
        }
        atlas.MarkAsDirty();

        Debug.Log("The atlas has been updated. Don't forget to save the scene to write the changes!");
    }
    /// <summary>
    /// Combine all sprites into a single texture and save it to disk.
    /// </summary>

    static bool UpdateTexture(UIAtlas atlas, List<SpriteEntry> sprites)
    {
        // Get the texture for the atlas
        Texture2D tex = atlas.texture as Texture2D;
        string oldPath = (tex != null) ? AssetDatabase.GetAssetPath(tex.GetInstanceID()) : "";
        string newPath = NGUIEditorTools.GetSaveableTexturePath(atlas);

        // Clear the read-only flag in texture file attributes
        if (System.IO.File.Exists(newPath))
        {
            System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(newPath);
            newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
            System.IO.File.SetAttributes(newPath, newPathAttrs);
        }

        bool newTexture = (tex == null || oldPath != newPath);

        if (newTexture)
        {
            // Create a new texture for the atlas
            tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        }
        else
        {
            // Make the atlas readable so we can save it
            tex = NGUIEditorTools.ImportTexture(oldPath, true, false);
        }

        // Pack the sprites into this texture
        if (PackTextures(tex, sprites))
        {
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(newPath, bytes);
            bytes = null;

            // Load the texture we just saved as a Texture2D
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            tex = NGUIEditorTools.ImportTexture(newPath, false, true);

            // Update the atlas texture
            if (newTexture)
            {
                if (tex == null) Debug.LogError("Failed to load the created atlas saved as " + newPath);
                else atlas.spriteMaterial.mainTexture = tex;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return true;
        }
        else
        {
            if (!newTexture) NGUIEditorTools.ImportTexture(oldPath, false, true);

            //Debug.LogError("Operation canceled: The selected sprites can't fit into the atlas.\n" +
            //	"Keep large sprites outside the atlas (use UITexture), and/or use multiple atlases instead.");

            EditorUtility.DisplayDialog("Operation Canceled", "The selected sprites can't fit into the atlas.\n" +
                    "Keep large sprites outside the atlas (use UITexture), and/or use multiple atlases instead", "OK");
            return false;
        }
    }

    /// <summary>
    /// Pack all of the specified sprites into a single texture, updating the outer and inner rects of the sprites as needed.
    /// </summary>

    static bool PackTextures(Texture2D tex, List<SpriteEntry> sprites)
    {
        Texture2D[] textures = new Texture2D[sprites.Count];
        Rect[] rects;

#if UNITY_3_5
		int maxSize = 4096;
#else
        int maxSize = SystemInfo.maxTextureSize;
#endif

#if UNITY_ANDROID || UNITY_IPHONE
#if !UNITY_3_5
        if (PlayerSettings.targetGlesGraphics == TargetGlesGraphics.OpenGLES_1_x)
        {
            maxSize = Mathf.Min(maxSize, 1024);
        }
        else
#endif
        {
            maxSize = Mathf.Min(maxSize, NGUISettings.allow4096 ? 4096 : 2048);
        }
#endif

        if (NGUISettings.unityPacking)
        {
            for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
            rects = tex.PackTextures(textures, NGUISettings.atlasPadding, maxSize);
        }
        else
        {
            sprites.Sort(Compare);
            for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
            rects = UITexturePacker.PackTextures(tex, textures, 4, 4, NGUISettings.atlasPadding, maxSize);
        }

        for (int i = 0; i < sprites.Count; ++i)
        {
            Rect rect = NGUIMath.ConvertToPixels(rects[i], tex.width, tex.height, true);

            // Make sure that we don't shrink the textures
            if (Mathf.RoundToInt(rect.width) != textures[i].width) return false;

            sprites[i].rect = rect;
            //BleedTexture(tex, sprites[i].rect);
        }
        return true;
    }


    /// <summary>
    /// Replace the sprites within the atlas.
    /// </summary>

    static void ReplaceSprites(UIAtlas atlas, List<SpriteEntry> sprites)
    {
        // Get the list of sprites we'll be updating
        List<UIAtlas.Sprite> spriteList = atlas.spriteList;
        List<UIAtlas.Sprite> kept = new List<UIAtlas.Sprite>();

        // The atlas must be in pixels
        atlas.coordinates = UIAtlas.Coordinates.Pixels;

        // Run through all the textures we added and add them as sprites to the atlas
        for (int i = 0; i < sprites.Count; ++i)
        {
            SpriteEntry se = sprites[i];
            UIAtlas.Sprite sprite = AddSprite(spriteList, se);
            kept.Add(sprite);
        }

        // Remove unused sprites
        for (int i = spriteList.Count; i > 0; )
        {
            UIAtlas.Sprite sp = spriteList[--i];
            if (!kept.Contains(sp)) spriteList.RemoveAt(i);
        }
        atlas.MarkAsDirty();
    }
    /// <summary>
    /// Add the specified texture to the atlas, or update an existing one.
    /// </summary>

    static public void AddOrUpdate(UIAtlas atlas, Texture2D tex)
    {
        if (atlas != null && tex != null)
        {
            List<Texture> textures = new List<Texture>();
            textures.Add(tex);
            List<SpriteEntry> sprites = CreateSprites(textures);
            ExtractSprites(atlas, sprites);
            UpdateAtlas(atlas, sprites);
        }
    }
    /// <summary>
    /// Create a list of sprites using the specified list of textures.
    /// </summary>

    static List<SpriteEntry> CreateSprites(List<Texture> textures)
    {
        List<SpriteEntry> list = new List<SpriteEntry>();

        foreach (Texture tex in textures)
        {
            Texture2D oldTex = NGUIEditorTools.ImportTexture(tex, true, false);
            if (oldTex == null) continue;

            // If we aren't doing trimming, just use the texture as-is
            if (!NGUISettings.atlasTrimming)
            {
                SpriteEntry sprite = new SpriteEntry();
                sprite.rect = new Rect(0f, 0f, oldTex.width, oldTex.height);
                sprite.tex = oldTex;
                sprite.temporaryTexture = false;
                list.Add(sprite);
                continue;
            }

            // If we want to trim transparent pixels, there is more work to be done
            Color32[] pixels = oldTex.GetPixels32();

            int xmin = oldTex.width;
            int xmax = 0;
            int ymin = oldTex.height;
            int ymax = 0;
            int oldWidth = oldTex.width;
            int oldHeight = oldTex.height;

            // Find solid pixels
            for (int y = 0, yw = oldHeight; y < yw; ++y)
            {
                for (int x = 0, xw = oldWidth; x < xw; ++x)
                {
                    Color32 c = pixels[y * xw + x];

                    if (c.a != 0)
                    {
                        if (y < ymin) ymin = y;
                        if (y > ymax) ymax = y;
                        if (x < xmin) xmin = x;
                        if (x > xmax) xmax = x;
                    }
                }
            }

            int newWidth = (xmax - xmin) + 1;
            int newHeight = (ymax - ymin) + 1;

            // If the sprite is empty, don't do anything with it
            if (newWidth > 0 && newHeight > 0)
            {
                SpriteEntry sprite = new SpriteEntry();
                sprite.rect = new Rect(0f, 0f, oldTex.width, oldTex.height);

                // If the dimensions match, then nothing was actually trimmed
                if (newWidth == oldWidth && newHeight == oldHeight)
                {
                    sprite.tex = oldTex;
                    sprite.temporaryTexture = false;
                }
                else
                {
                    // Copy the non-trimmed texture data into a temporary buffer
                    Color32[] newPixels = new Color32[newWidth * newHeight];

                    for (int y = 0; y < newHeight; ++y)
                    {
                        for (int x = 0; x < newWidth; ++x)
                        {
                            int newIndex = y * newWidth + x;
                            int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                            newPixels[newIndex] = pixels[oldIndex];
                        }
                    }

                    // Create a new texture
                    sprite.temporaryTexture = true;
                    sprite.tex = new Texture2D(newWidth, newHeight);
                    sprite.tex.name = oldTex.name;
                    sprite.tex.SetPixels32(newPixels);
                    sprite.tex.Apply();

                    // Remember the padding offset
                    sprite.minX = xmin;
                    sprite.maxX = oldWidth - newWidth - xmin;
                    sprite.minY = ymin;
                    sprite.maxY = oldHeight - newHeight - ymin;
                }
                list.Add(sprite);
            }
        }
        return list;
    }

    /// <summary>
    /// Add a new sprite to the atlas, given the texture it's coming from and the packed rect within the atlas.
    /// </summary>

    static UIAtlas.Sprite AddSprite(List<UIAtlas.Sprite> sprites, SpriteEntry se)
    {
        UIAtlas.Sprite sprite = null;

        // See if this sprite already exists
        foreach (UIAtlas.Sprite sp in sprites)
        {
            if (sp.name == se.tex.name)
            {
                sprite = sp;
                break;
            }
        }

        if (sprite != null)
        {
            float x0 = sprite.inner.xMin - sprite.outer.xMin;
            float y0 = sprite.inner.yMin - sprite.outer.yMin;
            float x1 = sprite.outer.xMax - sprite.inner.xMax;
            float y1 = sprite.outer.yMax - sprite.inner.yMax;

            sprite.outer = se.rect;
            sprite.inner = se.rect;

            sprite.inner.xMin = Mathf.Max(sprite.inner.xMin + x0, sprite.outer.xMin);
            sprite.inner.yMin = Mathf.Max(sprite.inner.yMin + y0, sprite.outer.yMin);
            sprite.inner.xMax = Mathf.Min(sprite.inner.xMax - x1, sprite.outer.xMax);
            sprite.inner.yMax = Mathf.Min(sprite.inner.yMax - y1, sprite.outer.yMax);
        }
        else
        {
            sprite = new UIAtlas.Sprite();
            sprite.name = se.tex.name;
            sprite.outer = se.rect;
            sprite.inner = se.rect;
            sprites.Add(sprite);
        }

        float width = Mathf.Max(1f, sprite.outer.width);
        float height = Mathf.Max(1f, sprite.outer.height);

        // Sprite's padding values are relative to width and height
        sprite.paddingLeft = se.minX / width;
        sprite.paddingRight = se.maxX / width;
        sprite.paddingTop = se.maxY / height;
        sprite.paddingBottom = se.minY / height;
        return sprite;
    }

    /// <summary>
    /// Release all temporary textures created for the sprites.
    /// </summary>

    static void ReleaseSprites(List<SpriteEntry> sprites)
    {
        foreach (SpriteEntry se in sprites)
        {
            if (se.temporaryTexture)
            {
                NGUITools.Destroy(se.tex);
                se.tex = null;
            }
        }
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// Used to sort the sprites by pixels used
    /// </summary>

    static int Compare(SpriteEntry a, SpriteEntry b)
    {
        // A is null b is not b is greater so put it at the front of the list
        if (a == null && b != null) return 1;

        // A is not null b is null a is greater so put it at the front of the list
        if (a == null && b != null) return -1;

        // Get the total pixels used for each sprite
        int aPixels = (int)(a.rect.height * a.rect.width);
        int bPixels = (int)(b.rect.height * b.rect.width);

        if (aPixels > bPixels) return -1;
        else if (aPixels < bPixels) return 1;
        return 0;
    }

    /// <summary>
    ///   保存为最高配置
    /// </summary>
  public  static void MakeTextureTrue(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        //if (ti == null) return false;

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        //if (settings.readable ||
        //    settings.maxTextureSize < 4096 ||
        //    settings.wrapMode != TextureWrapMode.Clamp ||
        //    settings.npotScale != TextureImporterNPOTScale.ToNearest)
        //{
            //settings.mipmapEnabled = true;
            settings.mipmapEnabled = false;
            settings.readable = false;
            settings.maxTextureSize = 4096;
            settings.textureFormat = TextureImporterFormat.RGBA32;
            settings.filterMode = FilterMode.Trilinear;
            settings.aniso = 4;
            settings.wrapMode = TextureWrapMode.Clamp;
            settings.npotScale = TextureImporterNPOTScale.None;

            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        //}
        //return true;
    }

  public static List<string> GetTextures(string path)
  {
      List<string> paths = new List<string>();
      DirectoryInfo dirInfo = new DirectoryInfo(path);
      FileInfo[] textures = dirInfo.GetFiles("*.png");
      foreach (FileInfo f in textures)
      {
          string fullPath = f.ToString();
          System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("Assets");
          System.Text.RegularExpressions.Match match = regex.Match(fullPath);
          paths.Add(fullPath.Substring(match.Index));
      }
      return paths;
  }

  /// <summary>
  ///   替换sprite
  /// </summary>
  public static void ReplaceSprite(GameObject go, UIAtlas from, UIAtlas to, string spriteName)
  {
      if (go != null && from != null && to != null && !string.IsNullOrEmpty(spriteName))
      {
          UISprite[] sprites = go.GetComponentsInChildren<UISprite>(true);
          foreach (var s in sprites)
          {
              if (s.atlas == from)
              {
                  if (s.spriteName == spriteName)
                  {
                      s.atlas = to;
                      s.spriteName = spriteName;
                      s.MakePixelPerfect();
                      //如果是sliced或者normal，是不是应该有不同的处理手法
                  }
              }
          }
      }
  }


    //public class SpriteEntry2
    //{
    //    int texInstanceID;
    //    Rect inner;
    //}


  //  /// <summary>
  //  ///   根据spriteNames获取atlas中的Sprite数据，必须要含有9宫格信息，暂时没有
  //  /// </summary>
  //static List<SpriteEntry> GetSpriteData(UIAtlas atlas, List<string> spriteNames)
  //{
  //    // Make the atlas texture readable
  //    Texture2D atlasTex = NGUIEditorTools.ImportTexture(atlas.texture, true, false);
  //    List<SpriteEntry> retList = new List<SpriteEntry>();

  //    if (atlasTex != null)
  //    {
  //        atlas.coordinates = UIAtlas.Coordinates.Pixels;

  //        Color32[] oldPixels = null;
  //        int oldWidth = atlasTex.width;
  //        int oldHeight = atlasTex.height;
  //        List<UIAtlas.Sprite> list = atlas.spriteList;

  //        foreach (var spriteName in spriteNames)
  //        {
  //            //bool found = false;
  //            UIAtlas.Sprite foundSprite = null;
  //            foreach (UIAtlas.Sprite se in list)
  //            {
  //                if (spriteName == se.name)
  //                {
  //                    foundSprite = se;
  //                    break;
  //                }
  //            }
  //            if (foundSprite!=null)
  //            {
  //                // Read the atlas
  //                if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

  //                Rect rect = foundSprite.outer;
  //                rect.xMin = Mathf.Clamp(rect.xMin, 0f, oldWidth);
  //                rect.yMin = Mathf.Clamp(rect.yMin, 0f, oldHeight);
  //                rect.xMax = Mathf.Clamp(rect.xMax, 0f, oldWidth);
  //                rect.yMax = Mathf.Clamp(rect.yMax, 0f, oldHeight);

  //                int newWidth = Mathf.RoundToInt(rect.width);
  //                int newHeight = Mathf.RoundToInt(rect.height);
  //                if (newWidth == 0 || newHeight == 0) continue;

  //                Color32[] newPixels = new Color32[newWidth * newHeight];
  //                int xmin = Mathf.RoundToInt(rect.x);
  //                int ymin = Mathf.RoundToInt(oldHeight - rect.yMax);

  //                for (int y = 0; y < newHeight; ++y)
  //                {
  //                    for (int x = 0; x < newWidth; ++x)
  //                    {
  //                        int newIndex = y * newWidth + x;
  //                        int oldIndex = (ymin + y) * oldWidth + (xmin + x);
  //                        newPixels[newIndex] = oldPixels[oldIndex];
  //                    }
  //                }

  //                // Create a new sprite
  //                SpriteEntry sprite = new SpriteEntry();
  //                sprite.temporaryTexture = true;
  //                sprite.tex = new Texture2D(newWidth, newHeight);
  //                sprite.tex.name = foundSprite.name;
  //                sprite.rect = new Rect(0f, 0f, newWidth, newHeight);
  //                sprite.tex.SetPixels32(newPixels);
  //                sprite.tex.Apply();

  //                // Min/max coordinates are in pixels
  //                sprite.minX = Mathf.RoundToInt(foundSprite.paddingLeft * newWidth);
  //                sprite.maxX = Mathf.RoundToInt(foundSprite.paddingRight * newWidth);
  //                sprite.minY = Mathf.RoundToInt(foundSprite.paddingBottom * newHeight);
  //                sprite.maxY = Mathf.RoundToInt(foundSprite.paddingTop * newHeight);

  //                retList.Add(sprite);
  //            }
  //        }
  //    }

  //    // The atlas no longer needs to be readable
  //    NGUIEditorTools.ImportTexture(atlas.texture, false, false);
  //    return retList;
  //}



  public static SpriteEntry ExtractSprite(UIAtlas atlas, string spriteName)
  {
      Texture2D atlasTex = atlas.texture as Texture2D;// NGUIEditorTools.ImportTexture(atlas.texture, true, false);
      SpriteEntry ret = null;
      if (atlasTex != null)
      {
          atlas.coordinates = UIAtlas.Coordinates.Pixels;

          Color32[] oldPixels = null;
          int oldWidth = atlasTex.width;
          int oldHeight = atlasTex.height;
          List<UIAtlas.Sprite> list = atlas.spriteList;

          foreach (UIAtlas.Sprite asp in list)
          {
              if (asp.name==spriteName)
              {
                  // Read the atlas
                  if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

                  Rect rect = asp.outer;
                  rect.xMin = Mathf.Clamp(rect.xMin, 0f, oldWidth);
                  rect.yMin = Mathf.Clamp(rect.yMin, 0f, oldHeight);
                  rect.xMax = Mathf.Clamp(rect.xMax, 0f, oldWidth);
                  rect.yMax = Mathf.Clamp(rect.yMax, 0f, oldHeight);

                  int newWidth = Mathf.RoundToInt(rect.width);
                  int newHeight = Mathf.RoundToInt(rect.height);
                  if (newWidth == 0 || newHeight == 0) continue;

                  Color32[] newPixels = new Color32[newWidth * newHeight];
                  int xmin = Mathf.RoundToInt(rect.x);
                  int ymin = Mathf.RoundToInt(oldHeight - rect.yMax);

                  for (int y = 0; y < newHeight; ++y)
                  {
                      for (int x = 0; x < newWidth; ++x)
                      {
                          int newIndex = y * newWidth + x;
                          int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                          newPixels[newIndex] = oldPixels[oldIndex];
                      }
                  }

                  // Create a new sprite
                  SpriteEntry sprite = new SpriteEntry();
                  sprite.temporaryTexture = true;
                  sprite.tex = new Texture2D(newWidth, newHeight);
                  sprite.tex.name = asp.name;
                  sprite.rect = new Rect(0f, 0f, newWidth, newHeight);
                  sprite.tex.SetPixels32(newPixels);
                  sprite.tex.Apply();

                  Rect outer = asp.outer;
                  Rect inner = asp.inner;
                  sprite.border = new Vector4(inner.xMin - outer.xMin, 
                      inner.yMin - outer.yMin, 
                      outer.xMax - inner.xMax, 
                      outer.yMax - inner.yMax);
                  
                  // Min/max coordinates are in pixels
                  sprite.minX = Mathf.RoundToInt(asp.paddingLeft * newWidth);
                  sprite.maxX = Mathf.RoundToInt(asp.paddingRight * newWidth);
                  sprite.minY = Mathf.RoundToInt(asp.paddingBottom * newHeight);
                  sprite.maxY = Mathf.RoundToInt(asp.paddingTop * newHeight);

                  ret= sprite;
              }
          }
      }

      // The atlas no longer needs to be readable
      //NGUIEditorTools.ImportTexture(atlas.texture, false, false);
      return ret;
  }


}
