using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIArtText))]
public class UIArtTextInspector : Editor
{
    protected UIArtText mArtText;
    int mFontIndex;

    string[] mFontOptions;
    bool mInitialize=false;
    void Init()
    {
        mInitialize = true;
        ArtFontSettings settings = ArtFontUtility.CreateArtFontSettings();
        mFontOptions = new string[settings.artFonts.Count];
        for (int i = 0; i < settings.artFonts.Count; i++)
        {
            mFontOptions[i] = settings.artFonts[i].name;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!mInitialize)
            Init();

        EditorGUIUtility.LookLikeControls(80f);
        mArtText = target as UIArtText;

        ComponentSelector.Draw<UIAtlas>(mArtText.atlas, obj =>
            {
                if (mArtText != null)
                {
                    bool resize = (mArtText.atlas == null);
                    mArtText.atlas = obj as UIAtlas;

                    Init();
                    mArtText.font = mFontOptions[0];
                    EditorUtility.SetDirty(mArtText.gameObject);
                    mArtText.Generate();
                }
            });
        if (mArtText.atlas == null) return;

        int index = EditorGUILayout.Popup("Select Font:", mFontIndex, mFontOptions);
        if (index != mFontIndex)
        {
            mFontIndex = index;
            mArtText.font = mFontOptions[mFontIndex];
            //re generate
            mArtText.Generate();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Pivot", GUILayout.Width(76f));
        Toggle("◄", "ButtonLeft", UIArtText.Pivot.Left);
        Toggle("▬", "ButtonMid", UIArtText.Pivot.Center);
        Toggle("►", "ButtonRight", UIArtText.Pivot.Right);
        GUILayout.EndHorizontal();


        string text = string.IsNullOrEmpty(mArtText.text) ? "" : mArtText.text;
        text = EditorGUILayout.TextArea(mArtText.text, GUI.skin.textArea, GUILayout.Height(100f));
        if (!text.Equals(mArtText.text)) 
        { 
            mArtText.text = text;
            //mArtText.Generate();
        }


        int space = EditorGUILayout.IntField("Space:",mArtText.space);
        if (space!=mArtText.space)
        {
            mArtText.space = space;
            mArtText.Reposition();
        }
    }

    void Toggle(string text, string style, UIArtText.Pivot pivot)
    {
        bool isActive = false;

        switch (pivot)
        {
            case UIArtText.Pivot.Left:
                isActive = mArtText.pivot==UIArtText.Pivot.Left;
                break;

            case UIArtText.Pivot.Right:
                isActive = mArtText.pivot == UIArtText.Pivot.Right;
                break;

            case UIArtText.Pivot.Center:
                isActive = mArtText.pivot == UIArtText.Pivot.Center;
                break;
        }
        if (GUILayout.Toggle(isActive, text, style) != isActive)
        {
            if (mArtText.pivot != pivot)
            {
                mArtText.pivot = pivot;
                mArtText.Reposition();
            }
        }
    }

}
