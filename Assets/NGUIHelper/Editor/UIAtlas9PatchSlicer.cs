using UnityEngine;
using UnityEditor;
using System.IO;
/// <summary>
///  两种情况：
///  1.9宫格拉伸部分没有渐变
///  2.9宫格拉伸部分有渐变，这种9宫格主要是为了减少图片体积，这种图的图集不能压缩
/// 现在只处理了情况1
/// 
/// roadmap:
/// 如果原来的sprite没有border信息，需要将所有用到这张sprite的界面元素的类型改成sliced
/// </summary>
public class UIAtlas9PatchSlicer : EditorWindow
{
    //[MenuItem("NGUIHelper/9 Patch/Atlas 9 Patch Slicer")]
    //static public void openAtlas9PatchSlicer()
    //{
    //    EditorWindow.GetWindow<UIAtlas9PatchSlicer>(false, "Atlas 9 Patch Slicer", true);
    //}

    Texture2D mTex;
    Texture2D mPreviewTex;

    NGUIEditorTools.IntVector mBorderA;
    NGUIEditorTools.IntVector mBorderB;
    //string mPath;
    string mOutputPath;
    string mRawPath;

    UIAtlas mAtlas;
    string mSpriteName;

    AtlasUtility.SpriteEntry mSpriteEntry;
    void OnGUI()
    {
        ComponentSelector.Draw<UIAtlas>(mAtlas, obj =>
            {
                mAtlas = obj as UIAtlas;

            });

        if (mAtlas == null) return;
        NGUIEditorTools.AdvancedSpriteField(mAtlas, mSpriteName,
            spriteName =>
            {
                if (mAtlas != null)
                {
                    NGUIEditorTools.ImportTexture(mAtlas.texture, true, false);
                }

                //string path = NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlas.name + "/" + spriteName + ".png";
                mRawPath = NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlas.name;
                mOutputPath = NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlas.name + "/" + spriteName + ".png";
                //加载的是本地备份的图片
                mTex = AssetDatabase.LoadAssetAtPath(mOutputPath, typeof(Texture2D)) as Texture2D;
                mSpriteEntry = AtlasUtility.ExtractSprite(mAtlas, spriteName);

                mBorderA.x = (int)mSpriteEntry.border.x;
                mBorderA.y =(int)mSpriteEntry.border.z;
                mBorderB.x = (int)mSpriteEntry.border.y;
                mBorderB.y =(int)mSpriteEntry.border.w;

                if (mTex != null)
                {
                    mSpriteName = spriteName;
                }
                else
                {
                    if (EditorUtility.DisplayDialog("警告", "本地数据不存在，是否使用AtlasSpliter进行同步？", "ok", "cancle"))
                    {
                        UIAtlasSpliter spliter = EditorWindow.GetWindow<UIAtlasSpliter>(false, "AtlasSpliter", true);
                        spliter.atlas = mAtlas;
                    }
                }

            }, false);

        NGUIEditorTools.DrawSeparator();

        if (mTex == null)
        {
            return;
        }
        //mPath = AssetDatabase.GetAssetPath(mTex);
        //mPath = mPath.Substring(0, mPath.Length - mTex.name.Length - ".png".Length - "/".Length);

        EditorGUILayout.Separator();
        GUI.backgroundColor = NGUIHelperSetting.Blue;
        mBorderA = NGUIEditorTools.IntPair("Border", "Left", "Right", mBorderA.x, mBorderA.y);
        mBorderB = NGUIEditorTools.IntPair(null, "Top", "Bottom", mBorderB.x, mBorderB.y);

        //按钮
        DrawSmartPart();
        GUILayout.BeginHorizontal();
        EditorGUILayout.Separator();

        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();


        const int EditMinWidth = 200;
        mCoff = 1;
        if (mTex.width < 100)
        {
            mCoff = EditMinWidth / mTex.width;
        }
        mRect = new Rect(AnchorX, AnchorY, mTex.width * mCoff, mTex.height * mCoff);

        GUI.DrawTextureWithTexCoords(mRect, mTex, new Rect(0, 0, 1, 1));
        if (mPreviewTex != null)
        {
            Rect rect = new Rect(AnchorX + mTex.width * mCoff + 100, AnchorY, mPreviewTex.width * mCoff, mPreviewTex.height * mCoff);
            GUI.DrawTexture(rect, mPreviewTex);
        }

        DrawBorder();
    }
    float mSmartCoff = 0.01f;//允许像素有一定的偏差
    float mAbnormalCoff = 0.02f;//允许有几个像素超出偏差
    float mShrinkRatio = 0;

    bool mEnableXSlice = true;
    bool mEnableYSlice = true;
    void DrawSmartPart()
    {
        GUI.backgroundColor = Color.white;
        GUILayout.BeginHorizontal();
        {
            mEnableXSlice = GUILayout.Toggle(mEnableXSlice, "enable x slice");

            EditorGUILayout.Separator();
            GUILayout.Label("SmartCoff:from 0", GUILayout.Width(140));
            mSmartCoff = GUILayout.HorizontalSlider(mSmartCoff, 0, 0.3f, GUILayout.Width(150));
            GUILayout.Label("to 0.3", GUILayout.Width(50));
            mSmartCoff = float.Parse(GUILayout.TextField(mSmartCoff.ToString(), GUILayout.Width(50)));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            mEnableYSlice = GUILayout.Toggle(mEnableYSlice, "enable y slice");
            EditorGUILayout.Separator();

            GUILayout.Label("AbnormalCoff:from 0", GUILayout.Width(140));
            mAbnormalCoff = GUILayout.HorizontalSlider(mAbnormalCoff, 0, 0.3f, GUILayout.Width(150));
            GUILayout.Label("to 0.3", GUILayout.Width(50));
            mAbnormalCoff = float.Parse(GUILayout.TextField(mAbnormalCoff.ToString(), GUILayout.Width(50)));
        }
        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();
        EditorGUILayout.Separator();
        GUILayout.Label(string.Format("shrink ratio:{0:p}", mShrinkRatio));
        if (GUILayout.Button("Smart Slice", GUILayout.Width(76f)))
        {
            //Debug.Log("split...");
            //Slice();
            SmartSlice();
        }
        if (GUILayout.Button("Preview", GUILayout.Width(76f)))
        {
            mPreviewTex = Slice();
            mShrinkRatio = 1 - (float)mPreviewTex.width * mPreviewTex.height / (mTex.width * mTex.height);
        }
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Slice", GUILayout.Width(76f)))
        {
            Texture2D output = Slice();
            if (output != null)
            {
                SaveTexture(output, mTex.name);
                mTex = AssetDatabase.LoadAssetAtPath(mOutputPath, typeof(Texture2D)) as Texture2D;
                AtlasUtility.AddOrUpdate(mAtlas, mTex);

                //更新border
                mAtlas.coordinates = UIAtlas.Coordinates.Pixels;
                foreach (var s in mAtlas.spriteList)
                {
                    //s.inner
                    if (s.name == mTex.name)
                    {
                        Rect outer = s.outer;
                        Rect inner = new Rect();
                        inner.xMin = mBorderA.x + outer.xMin;
                        inner.yMin = mBorderB.y + outer.yMin;
                        inner.xMax = outer.xMax - mBorderA.y;
                        inner.yMax = outer.yMax - mBorderB.x;
                        s.inner = inner;
                        Debug.Log(s.inner.ToString() + " " + s.outer.ToString());

                        //所有使用过该张sprite的界面元素都需要设置为sliced模式
                    }
                }
            }
            AssetDatabase.Refresh();
            Resources.UnloadUnusedAssets();
        }
        GUILayout.EndHorizontal();
        NGUIEditorTools.DrawSeparator();
    }

    void Preview()
    {

    }

    Texture2D Slice()
    {
        Texture2D destTex = null;
        //使其可读
        string path = (mTex != null) ? AssetDatabase.GetAssetPath(mTex.GetInstanceID()) : "";
        //mTex = NGUIEditorTools.ImportTexture(path, true, false, true);
        mTex = NGUIEditorTools.ImportTexture(path, true, false);
        //[  ]---[  ]---[  ]---[  ]
        // |       |    |
        //[a2]---[  ]---[a3]---[  ]
        // |       |    |
        //[  ]---[  ]---[  ]---[  ]
        // |       |    |
        //[a0]---[  ]---[a1]---[  ]
        int a0_x = 0, a0_y = 0;
        int a1_x = mTex.width - mBorderA.y, a1_y = 0;
        int a2_x = 0, a2_y = mTex.height - mBorderB.x;
        int a3_x = a1_x, a3_y = a2_y;

        if (mBorderA.x > mTex.width - mBorderA.y || mBorderB.x > mTex.height - mBorderB.y)
        {
            Debug.LogWarning("");
            return null;
        }

        //9宫格
        if (mBorderA.x > 0 && mBorderA.y > 0 && mBorderB.x > 0 && mBorderB.y > 0)
        {
            Color[] pixTopLeft = mTex.GetPixels(a2_x, a2_y, mBorderA.x, mBorderB.x);
            Color[] pixTopRight = mTex.GetPixels(a3_x, a3_y, mBorderA.y, mBorderB.x);
            Color[] pixBottomLeft = mTex.GetPixels(a0_x, a0_y, mBorderA.x, mBorderB.y);
            Color[] pixBottomRight = mTex.GetPixels(a1_x, a1_y, mBorderA.y, mBorderB.y);

            destTex = new Texture2D(mBorderA.x + mBorderA.y, mBorderB.x + mBorderB.y);
            //左下角
            destTex.SetPixels(0, 0, mBorderA.x, mBorderB.y, pixBottomLeft);
            //右下角
            destTex.SetPixels(mBorderA.x, 0, mBorderA.y, mBorderB.y, pixBottomRight);
            //左上角
            destTex.SetPixels(0, mBorderB.y, mBorderA.x, mBorderB.x, pixTopLeft);
            //右上角
            destTex.SetPixels(mBorderA.x, mBorderB.y, mBorderA.y, mBorderB.x, pixTopRight);
            destTex.Apply();
            //SaveTexture(destTex, mTex.name);
            return destTex;
        }
        //横向3宫格
        else if (mBorderA.x > 0 && mBorderA.y > 0 && mBorderB.x <= 0 && mBorderB.y <= 0)
        {
            Color[] pixLeft = mTex.GetPixels(a0_x, a0_y, mBorderA.x, mTex.height);
            Color[] pixRight = mTex.GetPixels(a1_x, a1_y, mBorderA.y, mTex.height);
            destTex = new Texture2D(mBorderA.x + mBorderA.y, mTex.height);
            destTex.SetPixels(0, 0, mBorderA.x, mTex.height, pixLeft);
            destTex.SetPixels(mBorderA.x, 0, mBorderA.y, mTex.height, pixRight);
            destTex.Apply();
            //SaveTexture(destTex, mTex.name);
            return destTex;
        }
        //纵向3宫格
        else if (mBorderA.x <= 0 && mBorderA.y <= 0 && mBorderB.x > 0 && mBorderB.y > 0)
        {
            Color[] pixTop = mTex.GetPixels(a2_x, a2_y, mTex.width, mBorderB.x);
            Color[] pixBottom = mTex.GetPixels(a0_x, a0_y, mTex.width, mBorderB.y);
            destTex = new Texture2D(mTex.width, mBorderB.x + mBorderB.y);
            destTex.SetPixels(0, 0, mTex.width, mBorderB.y, pixBottom);
            destTex.SetPixels(0, mBorderB.y, mTex.width, mBorderB.x, pixTop);
            destTex.Apply();
            //SaveTexture(destTex, mTex.name);
            return destTex;
        }
        else
        {
            Debug.LogWarning("can't slice");
            return null;
        }


    }
    void SaveTexture(Texture2D tex, string name)
    {
        //备份原图
        Object o = AssetDatabase.LoadAssetAtPath(NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlas.name + "/bak", typeof(Object));
        if (o == null)
            AssetDatabase.CreateFolder(mRawPath, "bak");
        //如果原来已经有一张同名的图了，作为备份的图应该还是原来的那张图，不需要覆盖
        Object t = AssetDatabase.LoadAssetAtPath(NGUIHelperSetting.GetRawResourcePath() + "/" + mAtlas.name + "/bak/" + name + ".png", typeof(Texture2D));
        if (t == null)
            AssetDatabase.MoveAsset(mOutputPath, mRawPath + "/bak/" + name + ".png");
        else
            AssetDatabase.DeleteAsset(mOutputPath);

        //Debug.Log("new path:" + mRawPath + "/bak/" + name + ".png");
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(mOutputPath, bytes);
        AssetDatabase.ImportAsset(mOutputPath);
        AtlasUtility.MakeTextureTrue(mOutputPath);
    }

    Vector2 mPos;
    float mCoff = 1;
    Rect mRect;
    const int AnchorX = 100;
    const int AnchorY = 200;
    void DrawBorder()
    {
        // Draw the border indicator lines
        GUI.BeginGroup(mRect);
        {
            Texture2D tex = NGUIEditorTools.contrastTexture;
            GUI.color = Color.white;

            if (mBorderA.x > 0)
            {
                float x0 = mBorderA.x * mCoff;// (float)mBorderA.x / mRect.width/* * outerRect.width*/ - 1;
                NGUIEditorTools.DrawTiledTexture(new Rect(x0, 0f, 1f, mRect.height), tex);
            }

            if (mBorderA.y > 0)
            {
                float x1 = mRect.width - mBorderA.y * mCoff;// (float)(sprite.width - sprite.borderRight) / sprite.width * outerRect.width - 1;
                NGUIEditorTools.DrawTiledTexture(new Rect(x1, 0f, 1f, mRect.height), tex);
            }

            if (mBorderB.x > 0)
            {
                float y0 = mBorderB.x * mCoff;// (float)(sprite.height - sprite.borderBottom) / sprite.height * outerRect.height - 1;
                NGUIEditorTools.DrawTiledTexture(new Rect(0f, y0, mRect.width, 1f), tex);
            }

            if (mBorderB.y > 0)
            {
                float y1 = mRect.height - mBorderB.y * mCoff;//(float)sprite.borderTop / sprite.height * outerRect.height - 1;
                NGUIEditorTools.DrawTiledTexture(new Rect(0f, y1, mRect.width, 1f), tex);
            }
        }
        GUI.EndGroup();
    }


    void SmartSlice()
    {
        //使其可读
        string path = (mTex != null) ? AssetDatabase.GetAssetPath(mTex.GetInstanceID()) : "";
        mTex = NGUIEditorTools.ImportTexture(path, true, false);

        const int ReservePixNum = 1;//至少保留多少个像素
        int middle = mTex.width / 2;
        int left = middle;
        int right = middle;
        Color[] pixRow = mTex.GetPixels(middle, 0, 1, mTex.height);
        if (mEnableXSlice)
        {
            while (left > ReservePixNum)
            {
                Color[] pixLeftRow = mTex.GetPixels(left, 0, 1, mTex.height);

                int abnormalTime = 0;
                for (int i = 0; i < pixRow.Length; i++)
                {
                    if (!ComparePixel(pixRow[i], pixLeftRow[i]))
                        abnormalTime++;
                }
                if ((float)abnormalTime / mTex.height > mAbnormalCoff)
                    break;
                left--;
            }

            while (right < mTex.width - ReservePixNum)
            {
                Color[] pixRightRow = mTex.GetPixels(right, 0, 1, mTex.height);
                int abnormalTime = 0;
                for (int i = 0; i < pixRow.Length; i++)
                {
                    if (!ComparePixel(pixRow[i], pixRightRow[i]))
                        abnormalTime++;
                }
                if ((float)abnormalTime / mTex.height > mAbnormalCoff)
                    break;
                right++;
            }
        }

        if (right != left)
        {
            mBorderA.x = left;
            mBorderA.y = mTex.width - right;
        }
        else
        {
            mBorderA.x = 0;
            mBorderA.y = 0;
        }

        //////////////////////////////////////////////////////////////////////////
        middle = mTex.height / 2;
        pixRow = mTex.GetPixels(0, middle, mTex.width, 1);
        int bottom = middle;
        int top = middle;
        if (mEnableYSlice)
        {
            while (bottom > ReservePixNum)
            {
                Color[] pixBottomRow = mTex.GetPixels(0, bottom, mTex.width, 1);
                int abnormalTime = 0;
                for (int i = 0; i < pixRow.Length; i++)
                {
                    if (!ComparePixel(pixRow[i], pixBottomRow[i]))
                        abnormalTime++;
                }
                if ((float)abnormalTime / mTex.height > mAbnormalCoff)
                    break;
                bottom--;
            }

            while (top < mTex.height - ReservePixNum)
            {
                Color[] pixTopRow = mTex.GetPixels(0, top, mTex.width, 1);
                int abnormalTime = 0;
                for (int i = 0; i < pixRow.Length; i++)
                {
                    if (!ComparePixel(pixRow[i], pixTopRow[i]))
                        abnormalTime++;
                }
                if ((float)abnormalTime / mTex.height > mAbnormalCoff)
                    break;
                top++;
            }
        }
        if (top != bottom)
        {
            mBorderB.x = bottom;
            mBorderB.y = mTex.height - top;
        }
        else
        {
            mBorderB.x = 0;
            mBorderB.y = 0;
        }
        //}

    }




    bool ComparePixel(Color a, Color b)
    {
        if (Mathf.Abs((a.r - b.r)) < mSmartCoff
            && Mathf.Abs((a.g - b.g)) < mSmartCoff
            && Mathf.Abs((a.b - b.b)) < mSmartCoff
            && Mathf.Abs((a.a - b.a)) < mSmartCoff)
        {
            return true;
        }
        return false;
    }
}
