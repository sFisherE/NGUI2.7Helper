using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

class UIAssetBundleBuilder : EditorWindow
{
    [MenuItem("NGUI/UIAssetBundleBuilder")]
    static void Init()
    {
        UIAssetBundleBuilder window =
            (UIAssetBundleBuilder)EditorWindow.GetWindow(typeof(UIAssetBundleBuilder), false, "UIAssetBundleBuilder");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Build Selected", GUILayout.Width(150)))
        {
            if (Selection.activeObject != null)
            {
                Build();
            }
        }
    }
    void Build()
    {
        Debug.Log(UIAssetbundlePath);
        //foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        //{
        //    Debug.Log("Build");
        //}
        //Debug.Log("******* Creating assetbundles for: " + name + " *******");
        if (!Directory.Exists(UIAssetbundlePath))
        {
            Directory.CreateDirectory(UIAssetbundlePath);
            AssetDatabase.Refresh();
        }
        //清空原来的assetbundle
        //string[] existingAssetbundles = Directory.GetFiles(UIAssetbundlePath);
        //foreach (string bundle in existingAssetbundles)
        //{
        //    if (bundle.EndsWith(".assetbundle")/* && bundle.Contains("/assetbundles/" + name)*/)
        //        File.Delete(bundle);
        //}


    }


    public static string UIAssetbundlePath
    {
        get { return "Assets" + Path.DirectorySeparatorChar + "StreamingAssets" + Path.DirectorySeparatorChar+"UI"; }
    }
}

