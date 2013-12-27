using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILocalize))]
public class UILocalizeInspector : Editor
{
    UILocalize mLocatize;

    string mTempKey;
    string mTempValue;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUIUtility.LookLikeControls(80f);
        mLocatize = target as UILocalize;

        mTempKey = EditorGUILayout.TextField("Key:", mLocatize.key);
        if (mTempKey != mLocatize.key)
        {
            Debug.Log("change");
            mLocatize.key = mTempKey;
        }
        if (!string.IsNullOrEmpty(mTempKey))
        {
            string text = NGUIHelperSettings.instance.GetLocalizeValue(mTempKey);
            if (!string.IsNullOrEmpty(text))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Value:");
                EditorGUILayout.LabelField(text);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Delete this Key"))
                {
                    NGUIHelperSettings.instance.DeleteLocalize(mTempKey);
                }
            }
            else
            {
                EditorGUILayout.LabelField("can't find the target Value");
            }

        }

        mTempValue = EditorGUILayout.TextField("Value:", mTempValue);
        string newKey = NGUIHelperSettings.instance.TryGetLocalizeKey(mTempValue);
        if (!string.IsNullOrEmpty(newKey))
        {
            EditorGUILayout.LabelField("find the target key:" + newKey);
            if (GUILayout.Button("Set Key"))
            {
                mLocatize.key = newKey;
            }
        }
        else
        {
            EditorGUILayout.LabelField("can't find the related Key");
            if (GUILayout.Button("Create New Key"))
            {
                NGUIHelperSettings.instance.AddNewLocalize(mTempKey, mTempValue);
            }
        }
    }

}
