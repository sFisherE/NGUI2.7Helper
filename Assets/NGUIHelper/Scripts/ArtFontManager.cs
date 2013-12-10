using System;
using System.Collections.Generic;
using UnityEngine;


class ArtFontManager:MonoBehaviour
{
    public ArtFontSettings settings;
    //public ArtFont[] artFonts;

    void Awake()
    {
        settings = Resources.Load("ArtFontSettings") as ArtFontSettings;

        //artFonts = settings.artFonts;
    }


}
