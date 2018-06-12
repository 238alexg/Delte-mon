// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

// Modified from Unity GameObject_Inspector

#define NO_PREVIEW
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace ZX
{
    using UnityEditor;
    using System.Reflection;
    using System.IO;

#if !CLOSE_SHOW_DELTDEX
    [CustomEditor(typeof(GameObject))]
    [CanEditMultipleObjects]  
#endif
    class ZX_ShowDeltMonster_Editor : Editor
    {
        static Color backGroundColor = new Color32(123,0,218,255);


        static BindingFlags flagX = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        static MethodInfo TrTextContent = typeof(EditorGUIUtility).GetMethod("TrTextContent", flagX, null, new Type[] { typeof(string), typeof(string), typeof(Texture) }, null);

        SerializedProperty m_Name;
        SerializedProperty m_IsActive;
        SerializedProperty m_Layer;
        SerializedProperty m_Tag;
        SerializedProperty m_StaticEditorFlags;
        SerializedProperty m_Icon;

        class Styles
        {
            public GUIContent goIcon = EditorGUIUtility.IconContent("GameObject Icon");
            public GUIContent typelessIcon = EditorGUIUtility.IconContent("Prefab Icon");
            public GUIContent prefabIcon = EditorGUIUtility.IconContent("PrefabNormal Icon");
            public GUIContent modelIcon = EditorGUIUtility.IconContent("PrefabModel Icon");

            public GUIContent staticContent = TrTextContent.Invoke(null, new object[3] { "Static", "Enable the checkbox to mark this GameObject as static for all systems.\n\nDisable the checkbox to mark this GameObject as not static for all systems.\n\nUse the drop-down menu to mark as this GameObject as static or not static for individual systems.", null }) as GUIContent;
            public GUIContent layerContent = TrTextContent.Invoke(null, new object[3] { "Layer", "The layer that this GameObject is in.\n\nChoose Add Layer... to edit the list of available layers.", null }) as GUIContent;
            public GUIContent tagContent = TrTextContent.Invoke(null, new object[3] { "Tag", "The tag that this GameObject has.\n\nChoose Untagged to remove the current tag.\n\nChoose Add Tag... to edit the list of available tags.", null }) as GUIContent;

            public float tagFieldWidth;
            public float layerFieldWidth;

            public GUIStyle staticDropdown = "StaticDropdown";
            public GUIStyle layerPopup = new GUIStyle(EditorStyles.popup);

            public GUIStyle instanceManagementInfo = new GUIStyle(EditorStyles.helpBox);
            public GUIStyle inspectorBig = new GUIStyle(typeof(EditorStyles).GetProperty("inspectorBig", flagX).GetValue(null, null) as GUIStyle);

            public GUIContent goTypeLabelMultiple = TrTextContent.Invoke(null, new object[3] { "Multiple", null, null }) as GUIContent;

            public GUIContent[] goTypeLabel =
            {
                null,//             None = 0,
                TrTextContent.Invoke(null,new object[3] { "Prefab" ,null,null}) as GUIContent,           // Prefab = 1
                TrTextContent.Invoke(null,new object[3] { "Model",null,null}) as GUIContent,            // ModelPrefab = 2
                TrTextContent.Invoke(null,new object[3] { "Prefab",null,null}) as GUIContent,           // PrefabInstance = 3
                TrTextContent.Invoke(null,new object[3] { "Model",null,null}) as GUIContent,            // ModelPrefabInstance = 4
                TrTextContent.Invoke(null,new object[3] { "Missing", "The source Prefab or Model has been deleted.",null}) as GUIContent,          // MissingPrefabInstance
                TrTextContent.Invoke(null,new object[3] { "Prefab", "You have broken the prefab connection. Changes to the prefab will not be applied to this object before you Apply or Revert.",null}) as GUIContent, // DisconnectedPrefabInstance
                TrTextContent.Invoke(null,new object[3] { "Model", "You have broken the prefab connection. Changes to the model will not be applied to this object before you Revert.",null}) as GUIContent, // DisconnectedModelPrefabInstance
            };

            public Styles()
            {
                tagFieldWidth = EditorStyles.boldLabel.CalcSize(tagContent).x;
                layerFieldWidth = EditorStyles.boldLabel.CalcSize(layerContent).x;
                GUIStyle miniButtonMid = "MiniButtonMid";
                instanceManagementInfo.padding = miniButtonMid.padding;
                instanceManagementInfo.alignment = miniButtonMid.alignment;

                // Seems to be a bug in the way controls with margin internal to layout groups with padding calculate position. We'll work around it here.
                layerPopup.margin.right = 0;

                // match modification in Editor.Styles
                inspectorBig.padding.bottom -= 1;
            }
        }
        static Styles s_Styles;
        const float kIconSize = 24;
#if NO_PREVIEW
#else
        Vector2 previewDir;
#endif
        class PreviewData : IDisposable
        {
            bool m_Disposed;
            GameObject m_GameObject;

            public readonly PreviewRenderUtility renderUtility;
            public GameObject gameObject { get { return m_GameObject; } }

            public PreviewData(UnityObject targetObject)
            {
                renderUtility = new PreviewRenderUtility();
                renderUtility.camera.fieldOfView = 30.0f;
                UpdateGameObject(targetObject);
            }

            public void UpdateGameObject(UnityObject targetObject)
            {
                UnityObject.DestroyImmediate(gameObject);
                m_GameObject = typeof(EditorUtility).GetMethod("InstantiateForAnimatorPreview", flagX, null, new Type[] { typeof(UnityObject) }, null).Invoke(null, new object[] { targetObject }) as GameObject;
                //renderUtility.AddManagedGO(gameObject);
                typeof(PreviewRenderUtility).GetMethod("AddManagedGO", flagX).Invoke(null, new object[] { gameObject });
            }

            public void Dispose()
            {
                if (m_Disposed)
                    return;
                renderUtility.Cleanup();
                UnityObject.DestroyImmediate(gameObject);
                m_GameObject = null;
                m_Disposed = true;
            }
        }

        Dictionary<int, PreviewData> m_PreviewInstances = new Dictionary<int, PreviewData>();

        bool m_HasInstance = false;
        bool m_AllOfSamePrefabType = true;

        public void OnEnable()
        {
#if NO_PREVIEW
#else
            if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D)
                previewDir = new Vector2(0, 0);
            else
                previewDir = new Vector2(120, -20);
#endif
            m_Name = serializedObject.FindProperty("m_Name");
            m_IsActive = serializedObject.FindProperty("m_IsActive");
            m_Layer = serializedObject.FindProperty("m_Layer");
            m_Tag = serializedObject.FindProperty("m_TagString");
            m_StaticEditorFlags = serializedObject.FindProperty("m_StaticEditorFlags");
            m_Icon = serializedObject.FindProperty("m_Icon");

            CalculatePrefabStatus();
        }

        void CalculatePrefabStatus()
        {
            m_HasInstance = false;
            m_AllOfSamePrefabType = true;
            PrefabType firstType = PrefabUtility.GetPrefabType(targets[0] as GameObject);
            foreach (GameObject go in targets)
            {
                PrefabType type = PrefabUtility.GetPrefabType(go);
                if (type != firstType)
                    m_AllOfSamePrefabType = false;
                if (type != PrefabType.None && type != PrefabType.Prefab && type != PrefabType.ModelPrefab)
                    m_HasInstance = true;
            }
        }

        void OnDisable()
        {
            foreach (var previewData in m_PreviewInstances.Values)
                previewData.Dispose();
            m_PreviewInstances.Clear();
        }

        private static bool ShowMixedStaticEditorFlags(StaticEditorFlags mask)
        {
            uint countedBits = 0;
            uint numFlags = 0;
            foreach (var i in Enum.GetValues(typeof(StaticEditorFlags)))
            {
                numFlags++;
                if ((mask & (StaticEditorFlags)i) > 0)
                    countedBits++;
            }

            //If we have more then one selected... but it is not all the flags
            //All indictates 'everything' which means it should be a tick!
            return countedBits > 0 && countedBits != numFlags;
        }

        protected override void OnHeaderGUI()
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            bool enabledTemp = GUI.enabled;
            GUI.enabled = true;
            EditorGUILayout.BeginVertical(s_Styles.inspectorBig);
            GUI.enabled = enabledTemp;
            DrawInspector();
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI() { }

        internal bool DrawInspector()
        {
            serializedObject.Update();

            GameObject go = target as GameObject;

            GUIContent iconContent = null;

            PrefabType prefabType = PrefabType.None;
            // Leave iconContent to be null if multiple objects not the same type.
            if (m_AllOfSamePrefabType)
            {
                prefabType = PrefabUtility.GetPrefabType(go);
                switch (prefabType)
                {
                    case PrefabType.None:
                        iconContent = s_Styles.goIcon;
                        break;
                    case PrefabType.Prefab:
                    case PrefabType.PrefabInstance:
                    case PrefabType.DisconnectedPrefabInstance:
                    case PrefabType.MissingPrefabInstance:
                        iconContent = s_Styles.prefabIcon;
                        break;
                    case PrefabType.ModelPrefab:
                    case PrefabType.ModelPrefabInstance:
                    case PrefabType.DisconnectedModelPrefabInstance:
                        iconContent = s_Styles.modelIcon;
                        break;
                }
            }
            else
                iconContent = s_Styles.typelessIcon;

            EditorGUILayout.BeginHorizontal();
            //EditorGUI.ObjectIconDropDown(GUILayoutUtility.GetRect(kIconSize, kIconSize, GUILayout.ExpandWidth(false)), targets, true, iconContent.image as Texture2D, m_Icon);
            typeof(EditorGUI).GetMethod("ObjectIconDropDown", flagX, null, new Type[5] { typeof(Rect), typeof(UnityObject[]), typeof(bool), typeof(Texture2D), typeof(SerializedProperty) }, null).Invoke(null, new object[5] { GUILayoutUtility.GetRect(kIconSize, kIconSize, GUILayout.ExpandWidth(false)), targets, true, iconContent.image as Texture2D, m_Icon });
            //DrawPostIconContent();
            typeof(Editor).GetMethod("DrawPostIconContent", flagX, null, new Type[] { }, null).Invoke(this, null);

            using (new EditorGUI.DisabledScope(prefabType == PrefabType.ModelPrefab))
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginHorizontal(GUILayout.Width(s_Styles.tagFieldWidth));

                GUILayout.FlexibleSpace();

                // IsActive
                EditorGUI.PropertyField(GUILayoutUtility.GetRect(EditorStyles.toggle.padding.left, EditorGUIUtility.singleLineHeight, EditorStyles.toggle, GUILayout.ExpandWidth(false)), m_IsActive, GUIContent.none);

                EditorGUILayout.EndHorizontal();

                // Name
                EditorGUILayout.DelayedTextField(m_Name, GUIContent.none);

                // Static flags toggle
                DoStaticToggleField(go);

                // Static flags dropdown
                DoStaticFlagsDropDown(go);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                // Tag
                DoTagsField(go);

                // Layer
                DoLayerField(go);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            // Seems to be a bug in margin not being applied consistently as tag/layer line, account for it here
            GUILayout.Space(2f);

            // Prefab Toolbar
            using (new EditorGUI.DisabledScope(prefabType == PrefabType.ModelPrefab))
                DoPrefabButtons(prefabType, go);

            serializedObject.ApplyModifiedProperties();

            return true;
        }

        private void DoPrefabButtons(PrefabType prefabType, GameObject go)
        {
            // @TODO: If/when we support multi-editing of prefab/model instances,
            // handle it here. Only show prefab bar if all are same type?
            if (!m_HasInstance) return;

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUILayout.BeginHorizontal();

                // Prefab information
                GUIContent prefixLabel = targets.Length > 1 ? s_Styles.goTypeLabelMultiple : s_Styles.goTypeLabel[(int)prefabType];

                if (prefixLabel != null)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(kIconSize + s_Styles.tagFieldWidth));
                    GUILayout.FlexibleSpace();
                    if (prefabType == PrefabType.DisconnectedModelPrefabInstance || prefabType == PrefabType.MissingPrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
                    {
                        GUI.contentColor = GUI.skin.GetStyle("CN StatusWarn").normal.textColor;
                        GUILayout.Label(prefixLabel, EditorStyles.whiteLabel, GUILayout.ExpandWidth(false));
                        GUI.contentColor = Color.white;
                    }
                    else
                        GUILayout.Label(prefixLabel, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();
                }

                if (targets.Length > 1)
                    GUILayout.Label("Instance Management Disabled", s_Styles.instanceManagementInfo);
                else
                {
                    // Select prefab
                    if (prefabType != PrefabType.MissingPrefabInstance)
                    {
                        if (GUILayout.Button("Select", "MiniButtonLeft"))
                        {
                            //Selection.activeObject = PrefabUtility.GetCorrespondingObjectFromSource(target);
                            Selection.activeObject = PrefabUtility.GetPrefabParent(target);
                            EditorGUIUtility.PingObject(Selection.activeObject);
                        }
                    }

                    using (new EditorGUI.DisabledScope(AnimationMode.InAnimationMode()))
                    {
                        if (prefabType != PrefabType.MissingPrefabInstance)
                        {
                            // Revert this gameobject and components to prefab
                            if (GUILayout.Button("Revert", "MiniButtonMid"))
                            {
                                //PrefabUtility.RevertPrefabInstanceWithUndo(go);
                                typeof(PrefabUtility).GetMethod("RevertPrefabInstanceWithUndo", flagX, null, new Type[] { typeof(GameObject) }, null).Invoke(null, new object[] { go });
                                // case931300 - The selected gameobject might get destroyed by RevertPrefabInstance
                                if (go != null)
                                {
                                    CalculatePrefabStatus();
                                }

                                // This is necessary because Revert can potentially destroy game objects and components
                                // In that case the Editor classes would be destroyed but still be invoked. (case 837113)
                                GUIUtility.ExitGUI();
                            }

                            // Apply to prefab
                            if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
                            {
                                GameObject rootUploadGameObject = PrefabUtility.FindValidUploadPrefabInstanceRoot(go);

                                GUI.enabled = rootUploadGameObject != null && !AnimationMode.InAnimationMode();

                                if (GUILayout.Button("Apply", "MiniButtonRight"))
                                {
                                    UnityObject prefabParent = PrefabUtility.GetPrefabParent(rootUploadGameObject);
                                    string prefabAssetPath = AssetDatabase.GetAssetPath(prefabParent);

                                    bool editablePrefab = (bool)typeof(Provider).GetMethod("PromptAndCheckoutIfNeeded", flagX).Invoke(null, new object[] {
                                            new string[] { prefabAssetPath },
                                            "The version control requires you to check out the prefab before applying changes." });

                                    if (editablePrefab)
                                    {
                                        //PrefabUtility.ReplacePrefabWithUndo(rootUploadGameObject);
                                        typeof(PrefabUtility).GetMethod("ReplacePrefabWithUndo", flagX).Invoke(null, new object[] { rootUploadGameObject });
                                        CalculatePrefabStatus();

                                        // This is necessary because ReplacePrefab can potentially destroy game objects and components
                                        // In that case the Editor classes would be destroyed but still be invoked. (case 468434)
                                        GUIUtility.ExitGUI();
                                    }
                                }
                            }
                        }
                    }

                    // Edit model prefab
                    if (prefabType == PrefabType.DisconnectedModelPrefabInstance || prefabType == PrefabType.ModelPrefabInstance)
                    {
                        if (GUILayout.Button("Open", "MiniButtonRight"))
                        {
                            //AssetDatabase.OpenAsset(PrefabUtility.GetCorrespondingObjectFromSource(target));
                            AssetDatabase.OpenAsset(typeof(PrefabUtility).GetMethod("GetCorrespondingObjectFromSource", flagX).Invoke(null, new object[] { target }) as UnityObject);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DoLayerField(GameObject go)
        {
            EditorGUIUtility.labelWidth = s_Styles.layerFieldWidth;
            Rect layerRect = GUILayoutUtility.GetRect(GUIContent.none, s_Styles.layerPopup);
            EditorGUI.BeginProperty(layerRect, GUIContent.none, m_Layer);
            EditorGUI.BeginChangeCheck();
            int layer = EditorGUI.LayerField(layerRect, s_Styles.layerContent, go.layer, s_Styles.layerPopup);
            if (EditorGUI.EndChangeCheck())
            {
                //includeChildren = GameObjectUtility.DisplayUpdateChildrenDialogIfNeeded(targets.OfType<GameObject>(),
                //       "Change Layer", string.Format("Do you want to set layer to {0} for all child objects as well?", InternalEditorUtility.GetLayerName(layer)));
                int includeChildren = (int)typeof(GameObjectUtility).GetMethod("DisplayUpdateChildrenDialogIfNeeded", flagX).Invoke(null, new object[] {targets.OfType<GameObject>(),
                       "Change Layer", string.Format("Do you want to set layer to {0} for all child objects as well?", InternalEditorUtility.GetLayerName(layer)) });
                //if (includeChildren != GameObjectUtility.ShouldIncludeChildren.Cancel)
                if (includeChildren != 2)
                {
                    m_Layer.intValue = layer;
                    //SetLayer(layer, includeChildren == GameObjectUtility.ShouldIncludeChildren.IncludeChildren);
                    SetLayer(layer, includeChildren == 0);
                }
                // Displaying the dialog to ask the user whether to update children nukes the gui state
                EditorGUIUtility.ExitGUI();
            }
            EditorGUI.EndProperty();
        }

        private void DoTagsField(GameObject go)
        {
            string tagName = null;
            try
            {
                tagName = go.tag;
            }
            catch (System.Exception)
            {
                tagName = "Undefined";
            }
            EditorGUIUtility.labelWidth = s_Styles.tagFieldWidth;
            Rect tagRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup);
            EditorGUI.BeginProperty(tagRect, GUIContent.none, m_Tag);
            EditorGUI.BeginChangeCheck();
            string tag = EditorGUI.TagField(tagRect, s_Styles.tagContent, tagName);
            if (EditorGUI.EndChangeCheck())
            {
                m_Tag.stringValue = tag;
                //Undo.RecordObjects(targets, "Change Tag of " + targetTitle);
                Undo.RecordObjects(targets, "Change Tag of " + typeof(Editor).GetProperty("targetTitle").GetValue(this, null) as string);
                foreach (UnityObject obj in targets)
                    (obj as GameObject).tag = tag;
            }
            EditorGUI.EndProperty();
        }

        private void DoStaticFlagsDropDown(GameObject go)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = m_StaticEditorFlags.hasMultipleDifferentValues;
            int changedFlags;
            bool changedToValue;
            //EditorGUI.EnumFlagsField(
            //    GUILayoutUtility.GetRect(GUIContent.none, s_Styles.staticDropdown, GUILayout.ExpandWidth(false)),
            //    GUIContent.none,
            //    GameObjectUtility.GetStaticEditorFlags(go),
            //    out changedFlags, out changedToValue,
            //    s_Styles.staticDropdown
            //    );
            var paramsX = new object[6]
            {
                 GUILayoutUtility.GetRect(GUIContent.none, s_Styles.staticDropdown, GUILayout.ExpandWidth(false)),
               GUIContent.none,
               GameObjectUtility.GetStaticEditorFlags(go),
                null,  null,
               s_Styles.staticDropdown
            };
            typeof(EditorGUI).GetMethod("EnumFlagsField", flagX, null, new Type[6] { typeof(Rect), typeof(GUIContent), typeof(Enum), typeof(int).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(GUIStyle) }, null).Invoke(null, paramsX);
            changedFlags = (int)paramsX[3];
            changedToValue = (bool)paramsX[4];

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                SceneModeUtility.SetStaticFlags(targets, changedFlags, changedToValue);
                serializedObject.SetIsDifferentCacheDirty();

                // Displaying the dialog to ask the user whether to update children nukes the gui state (case 962453)
                EditorGUIUtility.ExitGUI();
            }
        }

        private void DoStaticToggleField(GameObject go)
        {
            var staticRect = GUILayoutUtility.GetRect(s_Styles.staticContent, EditorStyles.toggle, GUILayout.ExpandWidth(false));
            EditorGUI.BeginProperty(staticRect, GUIContent.none, m_StaticEditorFlags);
            EditorGUI.BeginChangeCheck();
            var toggleRect = staticRect;
            EditorGUI.showMixedValue |= ShowMixedStaticEditorFlags((StaticEditorFlags)m_StaticEditorFlags.intValue);
            // Ignore mouse clicks that are not with the primary (left) mouse button so those can be grabbed by other things later.
            Event evt = Event.current;
            EventType origType = evt.type;
            bool nonLeftClick = (evt.type == EventType.MouseDown && evt.button != 0);
            if (nonLeftClick)
                evt.type = EventType.Ignore;
            var toggled = EditorGUI.ToggleLeft(toggleRect, s_Styles.staticContent, go.isStatic);
            if (nonLeftClick)
                evt.type = origType;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                SceneModeUtility.SetStaticFlags(targets, ~0, toggled);
                serializedObject.SetIsDifferentCacheDirty();

                // Displaying the dialog to ask the user whether to update children nukes the gui state (case 962453)
                EditorGUIUtility.ExitGUI();
            }
            EditorGUI.EndProperty();
        }

        UnityObject[] GetObjects(bool includeChildren)
        {
            return SceneModeUtility.GetObjects(targets, includeChildren);
        }

        void SetLayer(int layer, bool includeChildren)
        {
            UnityObject[] objects = GetObjects(includeChildren);
            //Undo.RecordObjects(objects, "Change Layer of " + targetTitle);
            Undo.RecordObjects(objects, "Change Layer of " + typeof(Editor).GetProperty("targetTitle").GetValue(this, null) as string);
            foreach (GameObject go in objects)
                go.layer = layer;
        }

        public static bool HasRenderableParts(GameObject go)
        {
            // Do we have a mesh?
            var renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var filter = renderer.gameObject.GetComponent<MeshFilter>();
                if (filter && filter.sharedMesh)
                    return true;
            }

            // Do we have a skinned mesh?
            var skins = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skin in skins)
            {
                if (skin.sharedMesh)
                    return true;
            }

            // Do we have a Sprite?
            var sprites = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                if (sprite.sprite)
                    return true;
            }

            // Nope, we don't have it.
            return false;
        }

        public override bool HasPreviewGUI()
        {
            if (!EditorUtility.IsPersistent(target))
                return false;

            return HasStaticPreview();
        }

        private bool HasStaticPreview()
        {
            if (targets.Length > 1)
                return true;

            if (target == null)
                return false;

            GameObject go = target as GameObject;

            // Is this a camera?
            Camera camera = go.GetComponent(typeof(Camera)) as Camera;
            if (camera)
                return true;

            return HasRenderableParts(go);
        }

        Texture2D cachet2d;
        public override Texture2D RenderStaticPreview(string assetPath, UnityObject[] subAssets, int width, int height)
        {

            var zx = (target as GameObject).GetComponent<DeltDexClass>();
            if (zx && zx.frontImage)
            {
                string path = "Assets/Gizmos/DeltDexCache/xxx" + zx.frontImage.name + "";
                if (cachet2d) return cachet2d;
                
                if(File.Exists(path))
                {
                    var rBytes = File.ReadAllBytes(path);
                    int w = BitConverter.ToInt32(rBytes, rBytes.Length- 8);
                    int h = BitConverter.ToInt32(rBytes, rBytes.Length - 4);
                    cachet2d = new Texture2D(w, h);
                    for (int i = 0; i < cachet2d.width; i++)
                        for (int j = 0; j < cachet2d.height; j++)
                        {
                            float r, g, b, a;
                            r = BitConverter.ToSingle(rBytes, i * cachet2d.width * 16 + j * 16);
                            g = BitConverter.ToSingle(rBytes, i * cachet2d.width * 16 + j * 16+4);
                            b = BitConverter.ToSingle(rBytes, i * cachet2d.width * 16 + j * 16+8);
                            a = BitConverter.ToSingle(rBytes, i * cachet2d.width * 16 + j * 16+12);
                            Color xc = new Color(r, g, b, a);
                            if (xc.a == 0)
                                cachet2d.SetPixel(i, j, backGroundColor);
                            else
                                cachet2d.SetPixel(i, j, xc);
                        }
                    cachet2d.Apply();
                    return cachet2d;

                }


                //Debug.Log("xxxxx....|" + zx.frontImage.name);
                byte[] bytes = new byte[width * height * 16 + 4 + 4];
                var rect = zx.frontImage.rect;
                var pat = AssetDatabase.GetAssetPath(zx.frontImage.texture);
                var ti = (TextureImporter)AssetImporter.GetAtPath(pat);
                bool or = ti.isReadable;


                byte[] wb = System.BitConverter.GetBytes((int)rect.width);
                byte[] wh = BitConverter.GetBytes((int)rect.height);
                for (int i = 0; i < 4; i++)
                {
                    bytes[width * height * 16 + i] = wb[i];
                    bytes[width * height * 16 + 4 + i] = wh[i];
                }
                Texture2D t2d;
                t2d = new Texture2D((int)rect.width, (int)rect.height);
                ti.isReadable = true;
                ti.SaveAndReimport();
                zx.frontImage.texture.Apply();
                for (int i = 0; i < t2d.width; i++)
                    for (int j = 0; j < t2d.height; j++)
                    {

                        Color c = zx.frontImage.texture.GetPixel((int)rect.x + i, (int)rect.y + j);
                        if (c.a == 0)
                            t2d.SetPixel(i, j, backGroundColor);
                        else
                            t2d.SetPixel(i, j, c);
                        var bsR = BitConverter.GetBytes(c.r);
                        var bsG = BitConverter.GetBytes(c.g);
                        var bsB = BitConverter.GetBytes(c.b);
                        var bsA = BitConverter.GetBytes(c.a);
                        for (int x = 0; x < 4; x++)
                        {
                            bytes[i * t2d.width * 16 + j * 16 + 0 + x] = bsR[x];
                            bytes[i * t2d.width * 16 + j * 16 + 4 + x] = bsG[x];
                            bytes[i * t2d.width * 16 + j * 16 + 8 + x] = bsB[x];
                            bytes[i * t2d.width * 16 + j * 16 + 12 + x] = bsA[x];
                        }
                    }
                t2d.Apply();
                ti.isReadable = or;
                ti.SaveAndReimport();
                zx.frontImage.texture.Apply();

                
                File.WriteAllBytes(path, bytes);
                return t2d;
            }

            if (!HasStaticPreview() || !ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                return null;
            }
            return null;
        }



    }
}
