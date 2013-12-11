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

    [MenuItem("NGUIHelper/Destroy Component")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Destroy Component", typeof(DestroyComponent), "Close", "Destroy");
    }

#region TypeInit
    public DestroyComponent()
    {
        FillTypeList();
    }
    private List<Type> mTypes = new List<Type>();
    void FillTypeList()
    {
        AppDomain domain = AppDomain.CurrentDomain;
        Type ComponentType = typeof(Component);
        mTypes.Clear();
        foreach (Assembly asm in domain.GetAssemblies())
        {
            Assembly currentAssembly = null;
            //	add UnityEngine.dll component types
            if (asm.FullName == "UnityEngine")
                currentAssembly = asm;
            //	check only for temporary assemblies (i.e. d6a5e78fb39c28ds27a1ec4f9g1 )
            if (ContainsNumbers(asm.FullName))
                currentAssembly = asm;
            if (currentAssembly != null)
            {
                foreach (Type t in currentAssembly.GetExportedTypes())
                {
                    if (ComponentType.IsAssignableFrom(t))
                    {
                        mTypes.Add(t);
                    }
                }
            }
        }
    }
    bool ContainsNumbers(String text)
    {
        int i = 0;
        foreach (char c in text)
        {
            if (int.TryParse(c.ToString(), out i))
                return true;
        }
        return false;
    }
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
