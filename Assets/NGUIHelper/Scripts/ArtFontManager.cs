using System;
using System.Collections.Generic;
using UnityEngine;


class ArtFontManager:MonoBehaviour
{
    static ArtFontManager mInstance;
    public static ArtFontManager instance
    {
        get
        {
            if (!mInstance)
            {
                mInstance = FindObjectOfType(typeof(ArtFontManager)) as ArtFontManager;
                if (!mInstance)
                {
                    var obj = new GameObject("ArtFontManager");
                    mInstance = obj.AddComponent<ArtFontManager>();
                    DontDestroyOnLoad(obj);
                    mInstance.settings = Resources.Load("ArtFontSettings") as ArtFontSettings;
                }
            }
            return mInstance;
        }
    }


    public ArtFontSettings settings;
    //public ArtFont[] artFonts;

    void Awake()
    {
        //settings = Resources.Load("ArtFontSettings") as ArtFontSettings;

        //artFonts = settings.artFonts;
    }


}
