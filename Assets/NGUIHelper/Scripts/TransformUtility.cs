using UnityEngine;
using System.Collections;

public class TransformUtility
{

    /// <summary>
    ///   get relative mouse position under parent node
    ///   the camera must be orthometric
    /// </summary>
    static public Vector3 GetRelativeMousePosition(Camera camera, Transform parent)
    {
        Vector3 v = camera.ScreenToWorldPoint(Input.mousePosition);//screen position to world position
        if (parent!=null)
            v = parent.InverseTransformPoint(v);//world position to local position

        v.z = 0;//flat z 
        return v;
    }



}
