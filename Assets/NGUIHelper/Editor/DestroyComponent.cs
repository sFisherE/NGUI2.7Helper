using System;
using System.Collections.Generic;

using UnityEngine;
using System.Reflection;
using UnityEditor;

/// <summary>
///   清除指定文件夹下所有的gameobject上挂接的component
/// </summary>
public class DestroyComponent : ScriptableWizard
{



#region TypeInit
    public DestroyComponent()
    {
        mTypes = NGUIHelperUtility.GetTypeList();
    }
    private List<Type> mTypes = new List<Type>();
#endregion

    public string componentName;
    public UnityEngine.Object folder;
    public string path;
    void OnWizardUpdate()
    {
        if (folder != null)
            path = AssetDatabase.GetAssetPath(folder);
        else
            path = string.Empty;

        Type t = GetSelectedType();
        if (t == null)
            errorString = "Type doesnt exist.";
        else
        {
            errorString = string.Empty;
        }
        if (mTypes.Count == 0)
            errorString = "Typelist is empty.";

    }
    void OnWizardOtherButton()
    {
        if (folder != null && !string.IsNullOrEmpty(path))
        {
            List<string> paths = NGUIHelperUtility.GetPrefabsRecursive(path);
            foreach (var goPath in paths)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(goPath, typeof(GameObject)) as GameObject;
                Component[] cs = go.GetComponentsInChildren(GetSelectedType(), true);
                foreach (var c in cs)
                {
                    UnityEngine.Object.DestroyImmediate(c, true);
                }
            }
        }
    }

    Type GetSelectedType()
    {
        foreach (Type t in mTypes)
        {
            if (t.Name == componentName)
            {
                return t;
            }
        }
        return null;
    }

}
