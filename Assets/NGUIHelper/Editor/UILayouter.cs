using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class UILayouter : EditorWindow
{

    private Object[] mSelectedObjects = null;
    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        UpdateSelection();
        this.Repaint();
    }
    private void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }
    private void UpdateSelection()
    {
        mSelectedObjects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
    }
    Transform mRoot;
    Texture mReferenceTexture;
    void OnGUI()
    {
        mRoot = (Transform)EditorGUILayout.ObjectField("Select Root:", this.mRoot, typeof(Transform), true, GUILayout.ExpandWidth(true));
        //place reference background
        //GUILayout.BeginHorizontal();
        //mReferenceTexture = (Texture)EditorGUILayout.ObjectField("Select Reference:", this.mReferenceTexture, typeof(Texture), true, GUILayout.ExpandWidth(true));
        //if (GUILayout.Button("Create"))
        //{
        //    GameObject go = new GameObject("Reference_" + mReferenceTexture.name);
        //    go.transform.parent = mRoot;
        //    UITexture com = go.AddComponent<UITexture>();
        //    com.depth = -100;
        //    com.mainTexture = mReferenceTexture;
        //    com.MakePixelPerfect();
        //}
        //GUILayout.EndHorizontal();
        NGUIEditorTools.DrawSeparator();

        //create node


        //flip horizontal or vertical
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Init"))
        {
            GameObject selectGo = Selection.activeGameObject;
            if (selectGo != null && selectGo.activeInHierarchy)
                selectGo.transform.localEulerAngles = Vector3.zero;
        }
        if (GUILayout.Button("Flip Horizontal"))
        {
            GameObject selectGo = Selection.activeGameObject;
            if (selectGo != null && selectGo.activeInHierarchy)
            {
                Vector3 eulerAngles = selectGo.transform.localEulerAngles;
                eulerAngles.y += 180;
                selectGo.transform.localEulerAngles = eulerAngles;
            }
        }
        if (GUILayout.Button("Flip Vertical"))
        {
            GameObject selectGo = Selection.activeGameObject;
            if (selectGo != null && selectGo.activeInHierarchy)
            {
                Vector3 eulerAngles = selectGo.transform.localEulerAngles;
                eulerAngles.x += 180;
                selectGo.transform.localEulerAngles = eulerAngles;
            }
        }
        GUILayout.EndHorizontal();
        //view control
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("100%"))
        {
            
        }
        if (GUILayout.Button("50%"))
        {
        }
        if (GUILayout.Button("200%"))
        {
        }
        GUILayout.EndHorizontal();


    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            List<Texture2D> draggedTextures = new List<Texture2D>();
            foreach (Object rObject in DragAndDrop.objectReferences)
            {
                if (rObject is Texture2D)
                {
                    draggedTextures.Add(rObject as Texture2D);
                }
            }

            if (draggedTextures.Count > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    // Drop
                    // Update editor selection
                    Selection.objects = this.CreateSpritesFromDragAndDrop(draggedTextures, ComputeDropPositionWorld(sceneView), Event.current.alt, Event.current.shift);
                }

                Event.current.Use();
            }
        }
    }
    // Compute the drop position world
    private Vector3 ComputeDropPositionWorld(SceneView sceneView)
    {
        // compute mouse position on the world y=0 plane
        float fOffsetY = 30.0f;
        Ray mouseRay = sceneView.camera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - fOffsetY, 0.0f));

        float t = -mouseRay.origin.z / mouseRay.direction.z;
        Vector3 mouseWorldPos = mouseRay.origin + t * mouseRay.direction;
        mouseWorldPos.z = 0.0f;

        return mouseWorldPos;
    }
    private GameObject[] CreateSpritesFromDragAndDrop(List<Texture2D> texturesList, Vector3 position, bool alt, bool shift)
    {
        GameObject[] generatedSprites;

        int textureCount = texturesList.Count;
        generatedSprites = new GameObject[textureCount];

        for (int index = 0; index < textureCount; ++index)
        {
            generatedSprites[index] = this.CreateSprite(texturesList[index], position, shift);
        }

        return generatedSprites;
    }
    // spirte;sliced sprite;texture
    private GameObject CreateSprite(Texture2D texture, bool shift)
    {
        if (shift)
        {
            Debug.Log("create texture");
            GameObject go = new GameObject("Texture_" + texture.name);
            go.transform.parent = mRoot;
            UITexture com = go.AddComponent<UITexture>();
            com.mainTexture = texture;
            com.MakePixelPerfect();
            return go;
        }
        else
        {
            Debug.Log("create sprite");
            string path = AssetDatabase.GetAssetPath(texture);
            string atlasName = Path.GetFileName(Path.GetDirectoryName(path));
            UIAtlas atlas = UIAtlasCollection.instance.atlases.Find(p => p.name == atlasName);
            if (atlas != null)
            {
                GameObject go = new GameObject("Sprite_" + texture.name);
                go.transform.parent = mRoot;
                UISprite com = go.AddComponent<UISprite>();
                com.atlas = atlas;
                com.spriteName = texture.name;
                string spriteName = texture.name;
                UIAtlas.Sprite spriteData = atlas.spriteList.Find(p => p.name == spriteName);
                if (spriteData.inner.Equals(spriteData.outer))
                {
                    //Debug.Log("simple sprite");
                    com.MakePixelPerfect();
                }
                else
                {
                    //Debug.Log("sliced sprite");
                    com.type = UISprite.Type.Sliced;
                    if (com.transform.localScale.x < 100 && com.transform.localScale.y < 50)
                        com.transform.localScale = new Vector3(100, 50, 1);
                }
                return go;
            }
            else
            {
                Debug.LogError("can't find related atlas");
            }
        }
        return null;
    }

    // NEW
    private GameObject CreateSprite(Texture2D texture, Vector3 creationPosition, bool shift)
    {
        //string path = AssetDatabase.GetAssetPath(texture);
        //Debug.Log(path);
        ////check the filename is atlas
        //Debug.Log(Path.GetFileName(Path.GetDirectoryName(path)));


        // Create sprite game object
        GameObject spriteGameObject = CreateSprite(texture, shift);
        // Set creation position
        if (spriteGameObject != null)
        {
            spriteGameObject.transform.position = creationPosition;
        }

        return spriteGameObject;
    }
}
