using UnityEngine;
using System.Collections;

/// <summary>
///   艺术字控件
/// </summary>
public class UIArtText : MonoBehaviour
{
    public enum Pivot
    {
        Left,
        Center,
        Right,
    }
    public Pivot pivot;

    public string text;
    public string font;
    public UIAtlas atlas;

    public int space;//间隔

    //根据text生成图片
    public void Generate()
    {
        //情况子节点
        for (int i = transform.childCount; i > 0; i--)
        {
            GameObject go = transform.GetChild(i - 1).gameObject;
            go.transform.parent = null;
#if UNITY_EDITOR
            GameObject.DestroyImmediate(go);
#else
            GameObject.Destroy(go);
#endif
        }

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            UISprite sprite = NGUITools.AddChild<UISprite>(transform.gameObject);
            sprite.gameObject.layer = NGUIHelperControlCenter.BaseLayer;
            sprite.gameObject.name = i.ToString();
            sprite.atlas = atlas;
            sprite.spriteName = font + "_" + ((int)c).ToString();
            sprite.pivot = UISprite.Pivot.BottomLeft;
            sprite.MakePixelPerfect();
        }
        Reposition();
    }

    public void Reposition()
    {
        if (transform.childCount > 0)
        {
            transform.GetChild(0).localPosition = Vector3.zero;
            for (int i = 1; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                GameObject pre = transform.GetChild(i - 1).gameObject;
                Vector3 v = pre.transform.localPosition;
                v.x += pre.transform.localScale.x + space;
                go.transform.localPosition = v;
            }
            Transform last = transform.GetChild(transform.childCount - 1);
            float length = last.localPosition.x + last.localScale.x;

            Vector3 newV = transform.localPosition;
            switch (pivot)
            {
                case Pivot.Left:
                    break;
                case Pivot.Center:
                    for (int i = 0; i < transform.childCount;i++ )
                    {
                        Transform child = transform.GetChild(i);
                        Vector3 v = child.localPosition;
                        v.x -= length / 2;
                        child.localPosition = v;
                    }
                    break;
                case Pivot.Right:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        Vector3 v = child.localPosition;
                        v.x -= length;
                        child.localPosition = v;
                    }
                    break;
            }
            transform.localPosition = newV;
        }
    }

}
