#define RUNTIME_RECORD
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

/// <summary>
///   this class is design to record all the data need at runtime
/// </summary>
public class RuntimeRecorder : MonoBehaviour
{
    static RuntimeRecorder mInstance;
    public static RuntimeRecorder instance
    {
        get
        {
            if (!mInstance)
            {
                mInstance = FindObjectOfType(typeof(RuntimeRecorder)) as RuntimeRecorder;
                if (!mInstance)
                {
                    var obj = new GameObject("RuntimeRecorder");
                    mInstance = obj.AddComponent<RuntimeRecorder>();
                    DontDestroyOnLoad(obj);
                }
            }
            return mInstance;
        }
    }


    void Awake()
    {
        spriteUsage = Resources.Load("SpriteUsage") as SpriteUsage;
    }

    public SpriteUsage spriteUsage;
    [Conditional("RUNTIME_RECORD")]
    public void RecordSpriteUsage(UISprite sprite, string spriteName)
    {
        if (spriteUsage != null)
        {
            spriteUsage.ChangeSpriteName(sprite, spriteName);
        }
    }
}
