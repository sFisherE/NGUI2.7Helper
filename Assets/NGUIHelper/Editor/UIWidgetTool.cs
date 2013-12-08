using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 预添加：
///   
///   
/// </summary>
public class UIWidgetTool : EditorWindow
{
    [MenuItem("NGUIHelper/WidgetTool")]
    static void Init()
    {
        UIWidgetTool window = (UIWidgetTool)EditorWindow.GetWindow(typeof(UIWidgetTool),false,"Widget Tool");
    }

    class Entry
    {
        public UIWidget widget;
        public Texture mainTexture;
        public int depth;
        public float zDepth;
        public bool isEnabled;
    }
    static int Compare(Entry a, Entry b) 
    { 
        if (a.mainTexture==null && b.mainTexture!=null)
        {
            return -1;
        }
        else if (a.mainTexture!=null && b.mainTexture==null)
        {
            return 1;
        }
        else if (a.mainTexture == null && b.mainTexture == null)
        {
            return 0;
        }
        else
        {
            int ret = string.Compare(a.mainTexture.name, b.mainTexture.name);
            if (ret==0)
                if (a.widget is UISprite && b.widget is UISprite)
                {
                    UISprite spriteA=a.widget as UISprite;
                    UISprite spriteB=b.widget as UISprite;
                    return string.Compare(spriteA.spriteName,spriteB.spriteName);
                }
                else if (a.widget is UISprite && b.widget is UILabel)
                {
                    UISprite spriteA=a.widget as UISprite;
                    return string.Compare(spriteA.spriteName,b.widget.name);
                }
                else if (a.widget is UILabel && b.widget is UISprite )
                {
                    UISprite spriteB=b.widget as UISprite;
                    return string.Compare(a.widget.name,spriteB.spriteName);
                }
                else
                    return string.Compare(a.widget.name, b.widget.name);
            else
                return ret;
        }
    }



    void OnSelectionChange() { Repaint(); }

    static List<UIWidget> GetListOfWidgets()
    {
        List<UIWidget> widgets = NGUIEditorTools.FindInScene<UIWidget>();
        return widgets;
    }
    static List<UIWidget> GetListOfWidgets(Transform tf)
    {
        List<UIWidget> retList=new List<UIWidget>();
        if (tf != null)
        {
            UIWidget[] widgets = tf.GetComponentsInChildren<UIWidget>(true);
            foreach (var v in widgets)
                retList.Add(v);
        }
        return retList;
    }

    static List<UIAtlas> GetListOfAtlases()
    {
        List<UIAtlas> atlases = NGUIEditorTools.FindInScene<UIAtlas>();
        return atlases;
    }

    static void SetActiveState(Transform t, bool state)
    {
        for (int i = 0; i < t.childCount; ++i)
        {
            Transform child = t.GetChild(i);
            if (state)
            {
                NGUITools.SetActiveSelf(child.gameObject, true);
                SetActiveState(child, true);
            }
            else
            {
                SetActiveState(child, false);
                NGUITools.SetActiveSelf(child.gameObject, false);
            }
            EditorUtility.SetDirty(child.gameObject);
        }
    }
    static void SetActiveState(UIWidget widget, bool state)
    {
        if (state)
        {
            NGUITools.SetActiveSelf(widget.gameObject, true);
            SetActiveState(widget.transform, true);
        }
        else
        {
            SetActiveState(widget.transform, false);
            NGUITools.SetActiveSelf(widget.gameObject, false);
        }
        EditorUtility.SetDirty(widget.gameObject);
    }

    Vector2 mScroll = Vector2.zero;

    //bool filterAtlas = false;
    //string filterAtlasName=string.Empty;

    bool mFilterTransform = false;
    Transform mSelectedFilterTransform;
    string selectedFilterTransformName=string.Empty;


    void FlatTransform(Transform tf)
    {
        if (tf.childCount>0)
        {
           Vector3 v= tf.transform.localPosition;
           v.z = 0;
           tf.transform.localPosition = v;
           for (int i = 0; i < tf.childCount;i++ )
           {
               FlatTransform(tf.GetChild(i));
           }
        }
    }
    void RoundToInt_Pos(Transform tf)
    {
        Vector3 v = tf.transform.localPosition;
        v.x = Mathf.RoundToInt(v.x);
        v.y = Mathf.RoundToInt(v.y);
        v.z = Mathf.RoundToInt(v.z);
        tf.transform.localPosition = v;
        for (int i = 0; i < tf.childCount; i++)
        {
            RoundToInt_Pos(tf.GetChild(i));
        }
    }
    void DrawTransformFilter()
    {
        mSelectedFilterTransform = (Transform)EditorGUILayout.ObjectField("Select Transform:", this.mSelectedFilterTransform, typeof(Transform), true, GUILayout.ExpandWidth(true));
        if (mSelectedFilterTransform!=null)
            selectedFilterTransformName = mSelectedFilterTransform.name;

        GUILayout.BeginHorizontal();
        {

            //EditorGUILayout.ObjectField("Select Transfrom:", GameObject,true);
            //EditorGUILayout.ObjectField(mSelectedFilterTransform, Transform, true);
            //if (GUILayout.Button("Select Transfrom", GUILayout.Width(150)))
            //{
            //    if (Selection.activeGameObject!=null)
            //    {
            //        mSelectedFilterTransform = Selection.activeGameObject.transform;
            //        selectedFilterTransformName = mSelectedFilterTransform.name;
            //    }
            //}
            if (GUILayout.Button("Flat Z Depth",GUILayout.Width(150)))
            {
                if (mSelectedFilterTransform!=null)
                {
                    FlatTransform(mSelectedFilterTransform);
                }
            }
            if (GUILayout.Button("Pos RoundToInt", GUILayout.Width(150)))
            {
                if (mSelectedFilterTransform != null)
                {
                    RoundToInt_Pos(mSelectedFilterTransform);
                }
            }
        }
        GUILayout.EndHorizontal();
        //获取某个transform下面的widget
        GUILayout.BeginHorizontal();
        {
            mFilterTransform = EditorGUILayout.Toggle("Filter Transform:", mFilterTransform);
            selectedFilterTransformName = EditorGUILayout.TextField(selectedFilterTransformName);
            if (selectedFilterTransformName != string.Empty)
            {
                GameObject temp = GameObject.Find(selectedFilterTransformName);
                if (temp != null)
                    mSelectedFilterTransform = temp.transform;
                else
                    mSelectedFilterTransform = null;
            }
        }
        GUILayout.EndHorizontal();
    }
    void OnGUI()
    {
        DrawTransformFilter();
        List<UIWidget> widgets = new List<UIWidget>() ;
        if (mFilterTransform)
        {
            if (mSelectedFilterTransform!=null)
            {
                widgets = GetListOfWidgets(mSelectedFilterTransform);
            }
        }
        else
        {
            widgets = GetListOfWidgets();
        }

        NGUIEditorTools.DrawSeparator();
        DrawAtlasChooser(widgets);
        NGUIEditorTools.DrawSeparator();


        List<string> visualAtlases = new List<string>();
        for (int i = 0; i < atlasNames.Count; i++)
        {
            if (mAtlasActiveStates[i] == true)
            {
                visualAtlases.Add(atlasNames[i]);
            }
        }

        if (widgets != null && widgets.Count > 0)
        {
            UIWidget selectWidget = NGUITools.FindInParents<UIWidget>(Selection.activeGameObject);

            Entry selectedEntry = null;
            bool allEnabled = true;

            List<Entry> entries = new List<Entry>();
            foreach (UIWidget widget in widgets)
            {
                Entry ent = new Entry();
                ent.widget = widget;
                if (widget.mainTexture == null)
                    ent.mainTexture = null;
                else
                    ent.mainTexture = widget.mainTexture;

                ent.depth = widget.depth;
                ent.zDepth = widget.transform.localPosition.z;
                ent.isEnabled = widget.enabled && NGUITools.GetActive(widget.gameObject);

                if (!ent.isEnabled)
                    allEnabled = false;

                if (ent.mainTexture != null)
                {
                    if (mAtlasFilter)
                    {
                        if (visualAtlases.Contains(ent.mainTexture.name))
                            entries.Add(ent);
                        else
                            NGUITools.SetActive(ent.widget.gameObject, false);
                    }
                    else
                    {
                        entries.Add(ent);
                    }
                }
                else
                    entries.Add(ent);
            }
            entries.Sort(Compare);

            DrawAtlasDepthEditor(entries);
            NGUIEditorTools.DrawSeparator();

            EditorGUIUtility.LookLikeControls(80f);
            bool showAll = DrawRow(null, null, allEnabled);
            NGUIEditorTools.DrawSeparator();

            mScroll = GUILayout.BeginScrollView(mScroll);
            foreach (Entry ent in entries)
            {
                if (DrawRow(ent, selectWidget, ent.isEnabled))
                {
                    selectedEntry = ent;
                }
            }
            NGUIEditorTools.DrawSeparator();
            DrawCommands(entries);
            NGUIEditorTools.DrawSeparator();
            GUILayout.EndScrollView();

            if (showAll)
            {
                foreach (Entry ent in entries)
                {
                    SetActiveState(ent.widget, !allEnabled);
                }
            }
            else if (selectedEntry != null)
            {
                SetActiveState(selectedEntry.widget, !selectedEntry.isEnabled);
            }
        }
    }

    bool[] mAtlasActiveStates;
    List<string> atlasNames = new List<string>();
    void GetAtlasNames(List<UIWidget> widgets)
    {
        //获取所有的
        atlasNames.Clear();
        foreach (var item in widgets)
        {
            if (item.mainTexture!=null)
            {
                if (!atlasNames.Contains(item.mainTexture.name))
                {
                    atlasNames.Add(item.mainTexture.name);
                }
            }
            else
            {
                if (!atlasNames.Contains("null"))
                {
                    atlasNames.Add("null");
                }
            }
        }
        mAtlasActiveStates = new bool[atlasNames.Count];
        for (int i = 0; i < mAtlasActiveStates.Length;i++ )
        {
            mAtlasActiveStates[i] = true;
        }
    }


    void DrawCommands(List<Entry> entries)
    {
        //执行命令
        GUILayout.BeginHorizontal();
        {
            GUILayout.Button("Command:", EditorStyles.label, GUILayout.MinWidth(100f));

            GUILayout.Label("Depth:", GUILayout.Width(50f));
            mDepth = EditorGUILayout.IntField(mDepth, GUILayout.Width(50f));
            if (GUILayout.Button("Apply", GUILayout.Width(50f)))
            {
                foreach (Entry ent in entries)
                    ent.widget.depth = mDepth;
            }

            GUILayout.Space(20);

            GUILayout.Label("Z Depth:", GUILayout.Width(60f));
            mZDepth = EditorGUILayout.FloatField(mZDepth, GUILayout.Width(50f));
            if (GUILayout.Button("Apply", GUILayout.Width(50f)))
            {
                foreach (Entry ent in entries)
                {
                    Vector3 v = ent.widget.transform.localPosition;
                    v.z = mZDepth;
                    ent.widget.transform.localPosition = v;
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    public bool mAtlasFilter;
    void DrawAtlasChooser(List<UIWidget> widgets)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Atlas Filter:",GUILayout.Width(70));
            mAtlasFilter = EditorGUILayout.Toggle(mAtlasFilter);

            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                GetAtlasNames(widgets);
            }
            if (GUILayout.Button("Select All", GUILayout.Width(100)))
            {
                for (int i = 0; i < atlasNames.Count; i++)
                {
                    mAtlasActiveStates[i] = true;
                }
            }
            if (GUILayout.Button("Unselect All", GUILayout.Width(100)))
            {
                for (int i = 0; i < atlasNames.Count; i++)
                {
                    mAtlasActiveStates[i] = false;
                }
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical();
        {
            for (int i = 0; i < atlasNames.Count / 4;i++ )
            {
                GUILayout.BeginHorizontal();
                {
                    for (int j = 0; j < 4;j++ )
                    {
                        mAtlasActiveStates[i * 4 + j] = GUILayout.Toggle(mAtlasActiveStates[i * 4 + j], 
                                                                        atlasNames[i * 4 + j], 
                                                                        GUILayout.Width(150));
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            for (int i = atlasNames.Count / 4 * 4; i < atlasNames.Count;i++ )
            {
                mAtlasActiveStates[i] = GUILayout.Toggle(mAtlasActiveStates[i], atlasNames[i], GUILayout.Width(150));
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }


    int mDepth;
    float mZDepth;
    public bool showAtlasDepthEditor = true;

    class AtlasEntry
    {
        public Texture texture;
        public float depthSum;
        public int num;
        public float meanDepth
        {
            get
            {
                return depthSum / num;
            }
        }
        public List<Entry> entries = new List<Entry>();
        public AtlasEntry(Texture t, float s, int n,Entry e)
        {
            texture = t;
            depthSum = s;
            num = n;
            entries.Add(e);
        }
        
    }
    void DrawAtlasDepthEditor(List<Entry> entries)
    {
        showAtlasDepthEditor = EditorGUILayout.Foldout(showAtlasDepthEditor, "AtlasDepthEditor");
        if (showAtlasDepthEditor)
        {
            List<AtlasEntry> atlases = new List<AtlasEntry>();
            foreach (Entry e in entries)
            {
                if (e.mainTexture!=null)
                {
                    AtlasEntry at = atlases.Find((AtlasEntry ent) => { return ent.texture == e.mainTexture; });
                    if (at != null)
                    {
                        at.depthSum += e.widget.transform.localPosition.z;
                        at.num++;
                        at.entries.Add(e);
                    }
                    else
                    {
                        atlases.Add(new AtlasEntry(e.mainTexture,e.widget.transform.localPosition.z,1,e));
                    }
                }
            }

            atlases.Sort((AtlasEntry a, AtlasEntry b) =>
            {
                if (a.meanDepth > b.meanDepth)
                    return 1;
                else if (a.meanDepth < b.meanDepth)
                    return -1;
                else
                    return 0;
            });

            mScroll2 = GUILayout.BeginScrollView(mScroll2,GUILayout.MinHeight(200));

            GUILayout.BeginVertical();
            {
                foreach (AtlasEntry e in atlases)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(e.texture.name, GUILayout.Width(150));
                        GUILayout.Label(string.Format("Num:{0}", e.num), GUILayout.Width(50));
                        GUILayout.Space(10);
                        GUILayout.Label(string.Format("Mean Depth:{0}", e.meanDepth), GUILayout.Width(150));
                        GUILayout.Space(10);
                        mTempDepth = EditorGUILayout.FloatField(mTempDepth, GUILayout.Width(50));
                        if (GUILayout.Button("Apply", GUILayout.Width(50)))
                        {
                            foreach (Entry item in e.entries)
                            {
                                Vector3 v = item.widget.transform.localPosition;
                                v.z = mTempDepth;
                                item.zDepth = mTempDepth;
                                item.widget.transform.localPosition = v;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
    Vector3 mScroll2;
    float mTempDepth;
    bool DrawRow(Entry ent, UIWidget selected, bool isChecked)
    {
        bool retVal = false;
        string name, textureName,depth,zDepth;
        if (ent != null)
        {
            if (ent.widget is UISprite)
            {
                UISprite sprite=ent.widget as UISprite;
                name = sprite.spriteName;
            }
            else/* if (ent.widget is UILabel)*/
            {
                name = ent.widget.name;
            }
            //name = ent.widget.name;
            if (ent.mainTexture != null)
                textureName = ent.mainTexture.name;
            else
                textureName = "null";
            depth=ent.depth.ToString();
            zDepth =string.Format("{0:2}",ent.zDepth.ToString());
        }
        else
        {
            name = "Name";
            textureName = "Texture";
            depth="Depth";
            zDepth = "zDepth";
        }

        //if (ent != null)
        //    NGUIEditorTools.HighlightLine(ent.isEnabled ? new Color(0.6f, 0.6f, 0.6f) : Color.black);

        GUILayout.BeginHorizontal();
        {
            GUI.color = Color.white;
            if (isChecked != EditorGUILayout.Toggle(isChecked, GUILayout.Width(20f))) 
                retVal = true;

            if (ent == null)
            {
                GUI.contentColor = Color.white;
            }
            else if (ent.isEnabled)
            {
                GUI.contentColor = (ent.widget == selected) ? new Color(0f, 0.8f, 1f) : Color.white;
            }
            else
            {
                GUI.contentColor = (ent.widget == selected) ? new Color(0f, 0.5f, 0.8f) : Color.grey;
            }

            if (GUILayout.Button(name, EditorStyles.label, GUILayout.MinWidth(100f)))
            {
                if (ent != null)
                {
                    Selection.activeGameObject = ent.widget.gameObject;
                    EditorUtility.SetDirty(ent.widget.gameObject);
                }
            }
            if (ent != null)
            {
                ent.widget.depth = EditorGUILayout.IntField(ent.depth, GUILayout.Width(50f));
                GUILayout.Space(10);
                Vector3 v = ent.widget.transform.localPosition;
                v.z= EditorGUILayout.FloatField(ent.zDepth, GUILayout.Width(50f));
                ent.widget.transform.localPosition = v;
                GUILayout.Space(10);
            }
            else
            {
                GUILayout.Label("Depth", GUILayout.Width(50f));
                GUILayout.Space(10);

                GUILayout.Label("Z Depth", GUILayout.Width(50f));
                GUILayout.Space(10);
            }
            GUILayout.Label(textureName, GUILayout.Width(150f));
        }
        GUILayout.EndHorizontal();
        return retVal;
    }
}