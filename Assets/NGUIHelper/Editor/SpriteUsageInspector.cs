using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteUsage))]
class SpriteUsageInspector : Editor
{
    SpriteUsage mSpriteUsage;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        mSpriteUsage = target as SpriteUsage;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SetDirty"))
            EditorUtility.SetDirty(mSpriteUsage);
        if (GUILayout.Button("Clear"))
            mSpriteUsage.data.Clear();
        GUILayout.EndHorizontal();
        foreach (var v in mSpriteUsage.data)
        {
            GUILayout.Label(v.atlasName);
            foreach (var item in v.spriteNames)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.Label(item);
                GUILayout.EndHorizontal();
            }
        }

    }
}
