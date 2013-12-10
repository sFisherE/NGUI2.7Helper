using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArtFontSettings : ScriptableObject
{
    public List<ArtFont> artFonts;



}

[System.Serializable]
public class ArtFont
{
    public string name;
    public Texture2D texture;
    public ArtFont(string name)
    {
        this.name = name;
    }
}