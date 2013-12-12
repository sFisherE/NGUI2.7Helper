using System;
using System.Collections.Generic;
using UnityEngine;
public enum DragEventFilterMode
{
    None,
    X,
    Y
}

/// <summary>
///   控制中心，存储一些全局变量
/// </summary>
public class NGUIHelperControlCenter
{
    public static DragEventFilterMode dragEventFilteMode;
    //public static int BaseLayer = LayerMask.NameToLayer("UI");


}
