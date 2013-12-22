using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Text;

/// <summary>
///   @1:生成艺术字
///   @2：使用Atlas Maker生成艺术字的atlas，并设置到ArtFontSetting上
///   @3：在游戏中使用UIArtText拼界面
///   
/// ##需要设置jsx的默认打开方式为photoshop，否则无法正常执行
/// </summary>
public class ArtFontUtility
{
    [MenuItem("NGUIHelper/ArtFont/Create ArtFontSettings")]
    public static ArtFontSettings CreateArtFontSettings()
    {
        var path = System.IO.Path.Combine("Assets/NGUIHelper/Resources", "ArtFontSettings.asset");
        var so = AssetDatabase.LoadMainAssetAtPath(path) as ArtFontSettings;
        if (so)
            return so;
        so = ScriptableObject.CreateInstance<ArtFontSettings>();
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/NGUIHelper/Resources");
        if (!di.Exists)
        {
            di.Create();
        }
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }
    /// <summary>
    ///   扫描目标文件夹下所有的psd文档，然后配置到ArtFontSettings中去
    /// </summary>
    [MenuItem("NGUIHelper/ArtFont/Init ArtFontSettings")]
    public static void InitArtFontSettings()
    {
        ArtFontSettings so = CreateArtFontSettings();

        DirectoryInfo dirInfo = new DirectoryInfo(NGUIHelperSettings.instance.assetArtFontProtoPath);
        if (dirInfo != null)
        {
            //FileSystemInfo[] files = dirInfo.GetFileSystemInfos();
            FileInfo[] psds = dirInfo.GetFiles("*.psd");
            so.artFonts = new List<ArtFont>();
            foreach (FileInfo f in psds)
            {
                //Debug.Log(f.ToString());
                ArtFont af = new ArtFont();
                so.artFonts.Add(af);
                af.name = f.Name.Substring(0, f.Name.Length - ".psd".Length);
                //so.artFonts.Add(new ArtFont(f.Name.Substring(0, f.Name.Length - ".psd".Length)));
                string txtPath = f.FullName.Substring(0, f.FullName.Length - ".psd".Length) + ".txt";
                Debug.Log(txtPath.ToString());
                af.content = ReadArtFontTxt(txtPath);
            }
            EditorUtility.SetDirty(so);
        }
    }
    /// <summary>
    ///   需要去除重复的字
    /// </summary>
    static string ReadArtFontTxt(string path)
    {
        FileInfo fi = new FileInfo(path);
        if (!fi.Exists)
        {
            Debug.LogError("lack font content file");
            return null;
        }

        StreamReader reader = new StreamReader(path);
        StringBuilder sb = new StringBuilder();
        if (reader != null)
        {
            string sLine = "";
            while (sLine != null)
            {
                sLine = reader.ReadLine();
                if (sLine != null && !sLine.Equals(""))
                    sb.Append(sLine);
            }
            reader.Close();
        }
        string r = sb.ToString();
        List<Char> cs = new List<Char>();
        foreach (var c in r.ToCharArray())
        {
            if (!cs.Contains(c))
                cs.Add(c);
        }
        sb = new StringBuilder();
        foreach ( var c  in cs)
        {
            sb.Append(c);
        }
        //Debug.Log(sb.ToString());
        return sb.ToString();
    }


    [MenuItem("NGUIHelper/ArtFont/Generate ArtFont")]
    public static void GenerateArtFont()
    {
        InitArtFontSettings();
        ArtFontSettings afs = CreateArtFontSettings();

        DirectoryInfo dirInfo = new DirectoryInfo(NGUIHelperSettings.instance.assetArtFontProtoPath);
        //写入文件
        Dictionary<string, string> charDic = new Dictionary<string, string>();
        List<string> fontTypes = new List<string>();
        foreach (var v in afs.artFonts)
        {
            fontTypes.Add(v.name);
            //charDic.Clear();
            foreach (var c in v.content.ToCharArray())
            {
                //先把文件中相应的图片字删掉
                if (!charDic.ContainsKey(c.ToString()))
                    charDic.Add(c.ToString(), ((int)c).ToString());
            }
        }

        DirectoryInfo outputDir = new DirectoryInfo(NGUIHelperSettings.instance.assetArtFontOutputPath);
        FileInfo[] fs = outputDir.GetFiles("*.png");
        int clipLength=".png".Length;
        foreach (var  f in fs)
        {
            //看#之前的字符串是不是已经有的图片字体
           string[] strs= f.Name.Split(ArtFontSettings.SeparatorChar);
            if (strs.Length>0)
            {
                string name = strs[0];
                if (fontTypes.Contains(name))
                {
                    Debug.Log("delete file");
                    f.Delete();
                }
            }

        }
        AssetDatabase.Refresh();


        StringBuilder sb = new StringBuilder();
        string fullname = dirInfo.FullName;
        //路径分隔符替换，脚本中必须使用'/'
        fullname = fullname.Replace('\\', '/');
        sb.AppendLine(string.Format("var langPath=\"{0}/\"", fullname));
        sb.AppendLine("var fontChars = {");
        foreach (var v in afs.artFonts)
        {
            sb.AppendLine(string.Format("{0}:\"{1}\",", v.name, v.content));
        }
        sb.AppendLine("}");

        sb.AppendLine("var charHexs = {");
        foreach (var v in charDic)
        {
            sb.AppendLine(string.Format("\"{0}\":\"{1}\",", v.Key, v.Value));
        }
        sb.AppendLine("}");

        string s= @"
    main();
    function main(){
	    for(var font in fontChars){
		    runQueue(font);
	    }
	    var file = new File(langPath + ""buildArtFont.jsx"");
	    file.remove();
    }
    function runQueue(font){
	    var chars=fontChars[font];
	    app.open(new File(langPath+font+"".psd""));
	    var doc=app.activeDocument;
	    var arr=[];
	    var otherChars="""";
	    for(var i=0;i<doc.layers.length;i++){
		    var layer=doc.layers[i];
		    if (layer.name.indexOf("":"")>-1) {
			    otherChars += layer.name.slice(layer.name.indexOf("":"")+1);
		    }
		    arr.push(layer);
	    }
	    for(i=0;i<chars.length;i++){
		    var s=chars.charAt(i);
		    for(var j=0;j<arr.length;j++){
			    var layer=arr[j];
			    if (otherChars.indexOf(s) > -1) {
				    var n = layer.name.indexOf("":"");
				    layer.visible=n>-1&&layer.name.indexOf(s,n+1)>-1;
			    }
			    else {
				    layer.visible = layer.name.indexOf("":"")==-1;
			    }
			    if(layer.kind==LayerKind.TEXT){
				    var textItem=layer.textItem;
				    textItem.contents = s;
			    }
		    }
		    var doc2 = doc.duplicate(""buildTemp"");
		    doc2.trim(TrimType.TRANSPARENT,false);
		    var pngFile=File(langPath+""/Output/""+font+""#""+charHexs[s]+"".png"");
		    doc2.saveAs(pngFile, new PNGSaveOptions(), true, Extension.LOWERCASE);
		    doc2.close(SaveOptions.DONOTSAVECHANGES);
	    }
	    doc.close(SaveOptions.DONOTSAVECHANGES);
    }";
        sb.Append(s);

        string path = dirInfo.FullName + "/buildArtFont.jsx";
        StreamWriter sw = File.CreateText(path);
        sw.Write(sb);
        sw.Close();
        //打开photoshop
        System.Diagnostics.Process.Start(path);
    }


}
