using System;
using System.Collections.Generic;
using UnityEngine;

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
    public void RecordSpriteUsage(UISprite sprite, string spriteName)
    {
        if (spriteUsage != null)
        {
            spriteUsage.ChangeSpriteName(sprite, spriteName);
        }
    }
}
