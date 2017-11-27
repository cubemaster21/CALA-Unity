#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TexDrawLib;
//using System;


//TO DO: Add Search Feature & Filter by Type
[CustomEditor(typeof(TEXPreference))]
public class TEXPreferenceEditor : Editor
{
	
    static internal class Styles
    {
        public static GUIContent none = GUIContent.none;
        public static GUIContent[] HeaderUpdate = new GUIContent[]
        {
            new GUIContent("Auto Refresh"),
            new GUIContent("Auto Refresh"),
            new GUIContent("Refresh Now")
        };

        public static GUIStyle[] HeaderStyles = new GUIStyle[]
        {
            new GUIStyle(EditorStyles.miniButtonLeft),
            new GUIStyle(EditorStyles.miniButtonMid),
            new GUIStyle(EditorStyles.miniButtonMid),
            new GUIStyle(EditorStyles.miniButtonRight)
        };
        public static GUIStyle ManagerFamily = new GUIStyle(EditorStyles.boldLabel);
		public static GUIStyle ManagerChild = new GUIStyle(EditorStyles.miniButton);
        public static GUIStyle FontPreviewSymbols = new GUIStyle(EditorStyles.objectFieldThumb);
        public static GUIStyle FontPreviewRelation = new GUIStyle(EditorStyles.textArea);
        public static GUIStyle FontPreviewEnabled = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle FontPreviewDisabled = new GUIStyle(EditorStyles.label);

        public static GUIStyle[] ManagerHeader = new GUIStyle[]
        {
            new GUIStyle(EditorStyles.miniButtonLeft),
            new GUIStyle(EditorStyles.miniButtonMid),
            new GUIStyle(EditorStyles.miniButtonRight)
        };
        public static GUIContent[] ManagerHeaderContent = new GUIContent[]
        {
            new GUIContent("Fonts"),
            new GUIContent("Options"),
            new GUIContent("Character")
        };
        public static GUIContent ImporterOptionFontMessage = new GUIContent(
                                                                "So far there's nothing to customize for importing a font.");
        public static GUIStyle ImporterOptionFontStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
        public static GUIStyle ImporterPresetArea = new GUIStyle(EditorStyles.textArea);
        public static GUIStyle[] SetterHeader = new GUIStyle[]
        {
            new GUIStyle(EditorStyles.miniButtonLeft),
            new GUIStyle(EditorStyles.miniButtonRight)
        };
        public static GUIContent[] SetterHeaderContent = new GUIContent[]
        {
            new GUIContent("Properties"),
            new GUIContent("Relations")
        };
        public static GUIStyle SetterPreview = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterNextLarger = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendTop = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendMiddle = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendBottom = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterExtendRepeat = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle SetterTitle = new GUIStyle(EditorStyles.label);
        public static GUIStyle SetterFont = new GUIStyle(EditorStyles.label);
        public static GUIStyle GlueLabelH = new GUIStyle(EditorStyles.label);
        public static GUIStyle GlueLabelV = new GUIStyle(EditorStyles.label);
        public static GUIStyle GlueProgBack;
        public static GUIStyle GlueProgBar;
        public static GUIStyle GlueProgText;

        public static GUIContent[] CharMapContents = new GUIContent[0xffff];

        public static GUIContent GetCharMapContent (char c)
        {
            return CharMapContents[c] ?? (CharMapContents[c] = new GUIContent(new string(c, 1)));
        }

        public static GUIContent[] SetterCharMap = new GUIContent[33];
        public static string[] DefaultTypes = new string[]
        {
            ("Numbers"),
            ("Capitals"),
            ("Small"),
            ("Commands"),
            ("Text"),
            ("Unicode")
        };
        public static GUIContent[] CharTypes = new GUIContent[]
        {
            new GUIContent("Ordinary"),
            new GUIContent("Geometry"),
            new GUIContent("Operator"),
            new GUIContent("Relation"),
            new GUIContent("Arrow"),
            new GUIContent("Open Delimiter"),
            new GUIContent("Close Delimiter"),
            new GUIContent("Big Operator"),
            new GUIContent("Inner"),
       //			new GUIContent ("Accent")
        };
        public static int[] SetterCharMapInt = new int[33];

        public static GUIContent[] HeaderTitles = new GUIContent[3];
        //public static GUIContent[] fontSettings;
        public static GUIStyle Buttons = new GUIStyle(EditorStyles.miniButton);

        static Styles()
        {  
            ManagerFamily.alignment = TextAnchor.MiddleCenter; 
            
			ManagerChild.fontSize = 10;
            ManagerChild.fixedHeight = 20;
            foreach (var gui in ManagerHeader)
            {
                gui.fontSize = 10;
            }
            ImporterPresetArea.wordWrap = true;
            ImporterOptionFontStyle.alignment = TextAnchor.MiddleCenter;
            FontPreviewEnabled.alignment = TextAnchor.MiddleCenter;
            FontPreviewSymbols.alignment = TextAnchor.MiddleCenter;
            FontPreviewRelation.alignment = TextAnchor.MiddleCenter;
            FontPreviewDisabled.alignment = TextAnchor.MiddleCenter;
            FontPreviewRelation.fixedHeight = 0; 
            FontPreviewRelation.onActive = FontPreviewEnabled.onActive; 
            FontPreviewRelation.onNormal = FontPreviewRelation.focused;
            FontPreviewRelation.focused = FontPreviewEnabled.focused;

            SetterTitle.fontStyle = FontStyle.Bold;
            SetterTitle.fontSize = 16;
            SetterTitle.fixedHeight = 25;
            SetterFont.richText = true;
            SetterPreview.fontSize = 34;
            SetterPreview.alignment = TextAnchor.MiddleCenter;
            SetterNextLarger.fontSize = 24;
            SetterNextLarger.alignment = TextAnchor.MiddleCenter;

            SetterExtendTop.fontSize = 24;
            SetterExtendTop.alignment = TextAnchor.MiddleCenter;
            SetterExtendMiddle.fontSize = 24;
            SetterExtendMiddle.alignment = TextAnchor.MiddleCenter;
            SetterExtendBottom.fontSize = 24;
            SetterExtendBottom.alignment = TextAnchor.MiddleCenter;
            SetterExtendRepeat.fontSize = 24;
            SetterExtendRepeat.alignment = TextAnchor.MiddleCenter;
           
            for (int i = 0; i < 33; i++) {
                SetterCharMap[i] = new GUIContent(new string(TexChar.possibleCharMaps[i], 1));
                SetterCharMapInt[i] = i;
            }
            SetterCharMap[0].text = "(Unassigned)"; //Yeah, just space isn't funny
            SetterCharMap[4].text = "\\\\"; //It can't be rendered correctly using actual character
            SetterCharMap[27].text = "&&"; //The ampersand character need to be done like this
            HeaderTitles = new GUIContent[4]
            {
                new GUIContent("Characters"),
                new GUIContent("Configurations"),
                new GUIContent("Materials"),
                new GUIContent("Glue Matrix")
            };
            for (int i = 0; i < 4; i++) {
                HeaderStyles[i].fontSize = 12;
                HeaderStyles[i].fixedHeight = 24;
            }

            GlueLabelH.alignment = TextAnchor.MiddleRight;
            GlueLabelV.alignment = TextAnchor.MiddleLeft;

            GlueProgBack = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ProgressBarBack");
            GlueProgBar = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ProgressBarBar");
            GlueProgText = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ProgressBarText");
            GlueProgText.alignment = TextAnchor.MiddleCenter;
            Buttons.alignment = TextAnchor.MiddleCenter;
            Buttons.fontSize = 11;
        }
    }

#region Base of all GUI Renderings

    static TEXPreference targetPreference;
    
    //0 = Auto Update; 1 = Manual, No Change Applied; 2 = Manual, Pending Change
    [SerializeField] int changeState = 0;
    [SerializeField] int managerState = 0;

    void OnEnable()
    { 
        Undo.undoRedoPerformed += RecordRedrawCallback;
	    matProp = serializedObject.FindProperty("watchedMaterial");
        configEditor = Editor.CreateEditor(TEXConfiguration.main);
   }

    void OnDisable()
    {
        if (targetPreference) {
            targetPreference.PushToDictionaries();
        }
        DestroyImmediate(configEditor);
        Undo.undoRedoPerformed -= RecordRedrawCallback;
    }
 
    protected override void OnHeaderGUI()
    {
        base.OnHeaderGUI();
        Rect r = new Rect(46, 24, 146, 16);
        if (headerActive > 0) {
            if (GUI.Toggle(r, changeState == 0, Styles.HeaderUpdate[changeState], Styles.Buttons)) { 
                if (changeState == 1) {
                    RecordRedraw();
                    changeState = 0;
                } else if (changeState == 2) {
                    targetPreference.PushToDictionaries();
                    targetPreference.CallRedraw();
                    changeState = 1;
                }
            } else
                changeState = changeState == 0 ? 1 : changeState;
        } else {
            if (GUI.Button(r, Styles.HeaderUpdate[2], Styles.Buttons)) {
                targetPreference.PushToDictionaries();
                targetPreference.CallRedraw();
                EditorUtility.SetDirty(targetPreference);
            }
        }
    }
    
    void SetPreviewFont (Font font)
    {
        if (!font)
            return;
        Styles.FontPreviewEnabled.font = font;
        Styles.FontPreviewSymbols.font = font;
        Styles.FontPreviewRelation.font = font;
        Styles.SetterPreview.font = font;
    }

    // Root of all GUI instruction begin here.
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (!targetPreference) {
            targetPreference = TEXPreference.main;
            if (selectedFont != null)
                SetPreviewFont(selectedFont.Font_Asset);
        }
        RecordUndo();
        DrawHeaderOption();
       if (headerActive == 0) {
            Rect v = EditorGUILayout.GetControlRect(GUILayout.Height(5));
            //Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.labelWidth / 3));
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.labelWidth));
                
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < 3; i++)
                {
                    if (GUILayout.Toggle(i == managerState, Styles.ManagerHeaderContent[i], Styles.ManagerHeader[i]))
                        managerState = i;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth /= 2;
                if (managerState == 0)
                    DrawManager();
                else if (managerState == 1)
                    DrawImporter();
                else if (managerState == 2)
                    DrawSetter();
                EditorGUIUtility.labelWidth *= 2;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel = 0;
            EditorGUILayout.Separator();
            if (selectedFont != null)
            {
                v.xMin += EditorGUIUtility.labelWidth + 4;
                v.height = Screen.height - ViewerHeight;
                CheckEvent(false);
                if (selectedFont.type == TexFontType.Font)
                    DrawViewerFont(v);
                else
                    DrawViewerSprite(v);

            }
        }
        else if (headerActive == 1)
            DrawConfiguration();
        else if (headerActive == 2)
            DrawMaterial();
        else if (headerActive == 3)
            DrawGlue();
        serializedObject.ApplyModifiedProperties();
    }



    [SerializeField]
    int headerActive = 0;

    void DrawHeaderOption()
    {
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 4; i++) {
            if(GUILayout.Toggle(i == headerActive, Styles.HeaderTitles[i], Styles.HeaderStyles[i]))
                headerActive = i;
        }
        EditorGUILayout.EndHorizontal();
        /*Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(24));
        r.width /= 4f;
        for (int i = 0; i < 3; i++) {
            if (GUI.Toggle(r, i == headerActive, Styles.HeaderTitles[i], Styles.HeaderStyles[i]))
                headerActive = i;
            r.x += r.width;
        }*/
    }

#endregion

#region Character Management

    const float ViewerHeight = 120f;
    [SerializeField]    Vector2 ViewerScroll;
    [SerializeField]    Vector2 ManagerScroll;
    [SerializeField]    Vector2 SetterScroll;
    [SerializeField]    int selectedFontIdx;
    [SerializeField]    int selectedCharIdx;

    TexFont selectedFont
    {
        get { 
            if (selectedFontIdx >= targetPreference.fontData.Length) selectedFontIdx = 0;
            return targetPreference.fontData[selectedFontIdx]; }
        set { selectedFontIdx = value.index; SetPreviewFont(selectedFont.Font_Asset); }
    }

    TexChar selectedChar
    {
        get { 
             if (selectedCharIdx >= selectedFont.chars.Length) selectedCharIdx = selectedFont.chars.Length - 1;
             return selectedFont.chars[selectedCharIdx]; }
        set {
            selectedCharIdx = value.index;
            selectedFontIdx = value.fontIndex;
        }
    }

    bool lastCharChanged = false;
    int setterState = 0;

    void DrawManager()
    {
        ManagerScroll = EditorGUILayout.BeginScrollView(ManagerScroll, false, false, GUILayout.ExpandHeight(true));
        //EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.labelWidth - 90));
        int Total = targetPreference.fontData.Length;
        //EditorGUI.indentLevel = 0;
        for (int i = 0; i < Total; i++)
        {
            //Draw Headers First, if needed
            if (i == 0)
                GUILayout.Label("Math Fonts", Styles.ManagerFamily);
            else if (i == targetPreference.header_mathCount)
                GUILayout.Label("User Fonts", Styles.ManagerFamily);
            else if (i == targetPreference.header_userCount)
                GUILayout.Label("Sprites", Styles.ManagerFamily);
            //Draw the font
            TexFont d = targetPreference.fontData[i];

            if ((selectedFontIdx == i) != GUILayout.Toggle(selectedFontIdx == i, d.id, Styles.ManagerChild))
            {
                selectedFontIdx = i;
                selectedCharIdx = Mathf.Clamp(selectedCharIdx, 0, d.chars.Length - 1);
                SetPreviewFont(d.Font_Asset);
            }
        }
        //EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    GUIStyle SubDetermineStyle(TexChar c)
    {
        if (c.supported) {	
            if (!string.IsNullOrEmpty(c.symbolName))
                return Styles.FontPreviewSymbols;
            else if (c.nextLargerExist || c.extensionExist)
                return Styles.FontPreviewRelation;
            else
                return Styles.FontPreviewEnabled;
        } else
            return Styles.FontPreviewDisabled;
    }

    void DrawViewerFont(Rect drawRect)
    {
        if (!selectedFont.Font_Asset)
        {
            // Something wrong?

           EditorGUI.LabelField(drawRect, "The Font Asset is NULL. you should reimport again.", Styles.ImporterOptionFontStyle);
            return;
        }
        //Rect r;
        Vector2 childSize = new Vector2(drawRect.width / 8f - 4, selectedFont.Font_Asset.lineHeight * (drawRect.width / 250) + 15);
        ViewerScroll = GUI.BeginScrollView(drawRect, ViewerScroll, new Rect(Vector2.zero, new Vector2((childSize.x + 2) * 8 - 2, (childSize.y + 2) * 16)));
        Styles.FontPreviewEnabled.fontSize = (int)childSize.x / 2;
        Styles.FontPreviewSymbols.fontSize = (int)childSize.x / 2;
        Styles.FontPreviewRelation.fontSize = (int)childSize.x / 2;
        var chars = selectedFont.chars;
        for (int i = 0; i < chars.Length; i++)
        {
            int x = i % 8, y = i / 8, l = selectedCharIdx;
            var r = new Rect(new Vector2((childSize.x + 2) * x, (childSize.y + 2) * y), childSize);
            var ch = chars[i];
            if (CustomToggle(r, selectedCharIdx == i, ch.supported, Styles.GetCharMapContent(ch.characterIndex), SubDetermineStyle(ch))) {
                    int newS = i + (selectedCharIdx - l);
                    if (newS != selectedCharIdx && lastCharChanged) {
                        RecordDirty();
                        lastCharChanged = false;
                    }
                    selectedCharIdx = newS;
                }
        }
        GUI.EndScrollView();
    }

    void DrawViewerSprite(Rect drawRect)
    {
        if (!selectedFont.Sprite_Asset)
        {
            // Something wrong?

            EditorGUI.LabelField(drawRect, "The Sprite Asset is NULL. you should reimport again.", Styles.ImporterOptionFontStyle);
            return;
        }
        int tileX = selectedFont.sprite_xLength, tileY = selectedFont.sprite_yLength, columnTile = 0;
        bool horizonFirst = tileX >= tileY;
        columnTile = horizonFirst ? tileY : tileX;
        Vector2 childSize = new Vector2((drawRect.width - 24) / columnTile, selectedFont.font_lineHeight * (drawRect.width - 24) / columnTile);
        ViewerScroll = GUI.BeginScrollView(drawRect, ViewerScroll, new Rect(Vector2.zero, new Vector2((childSize.x + 2) * columnTile - 2, (childSize.y + 2) * (horizonFirst ? tileX : tileY))));

        var chars = selectedFont.chars;
        for (int i = 0; i < chars.Length; i++)
        {
            int x = i % columnTile, y = i / columnTile, l = selectedCharIdx;
            var r = new Rect(new Vector2((childSize.x + 2) * x, (childSize.y + 2) * y), childSize);
            var ch = chars[i];
            if (CustomToggle(r, selectedCharIdx == i, ch.supported, Styles.none, SubDetermineStyle(ch))) {
                    int newS = i + (selectedCharIdx - l);
                    if (newS != selectedCharIdx && lastCharChanged) {
                        RecordDirty();
                        lastCharChanged = false;
                    }
                    selectedCharIdx = newS;
                }
                if (ch.supported) {
                    
    #if TEXDRAW_TMP
                    // Additional measurements for accurate display in TMP
                    var r2 = r;
                    var ratio = Mathf.Min(1, (ch.height + ch.depth) / selectedFont.font_lineHeight);
                    r.height *= ratio;
                    r.width = (ch.bearing + ch.italic) / (ch.height + ch.depth) * r.height;
                    r.y += (r2.height - r.height) / 2f;
                    r.x += (r2.width - r.width) / 2f;
    #endif
                    GUI.DrawTextureWithTexCoords(r, selectedFont.Sprite_Asset, ch.sprite_uv);
                }
        }
        GUI.EndScrollView();
    }

    void DrawImporter()
    {
        //GUILayoutOption max = GUILayout.MaxWidth(EditorGUIUtility.labelWidth);
        
        GUILayout.Label(selectedFont.id, Styles.SetterTitle);
            
        // The options for sprite assets
        if (selectedFont.type == TexFontType.Sprite) {
            EditorGUI.BeginChangeCheck();
            int x = EditorGUILayout.IntField("Column Count", selectedFont.sprite_xLength);
            int y = EditorGUILayout.IntField("Row Count", selectedFont.sprite_yLength);
            float z = EditorGUILayout.FloatField("Overall Scale", selectedFont.sprite_scale);
            float w = EditorGUILayout.FloatField("Line Offset", selectedFont.sprite_lineOffset);
            bool v = EditorGUILayout.Toggle("Alpha Only", selectedFont.sprite_alphaOnly);
            if (EditorGUI.EndChangeCheck()) {
                RecordDirty();
                selectedFont.sprite_xLength = Mathf.Max(x, 1);
                selectedFont.sprite_yLength = Mathf.Max(y, 1);
                selectedFont.sprite_scale = Mathf.Max(z, 0.01f);
                selectedFont.sprite_lineOffset = w;
                selectedFont.sprite_alphaOnly = v;
            }
            if (GUILayout.Button("Apply")) {
                selectedFont.PopulateSprite();
                targetPreference.CallRedraw();
            }
        }
      
        // The options for import presets
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        var ctgImport = (ImportCharPresetsType)EditorGUILayout.EnumPopup("Import Preset", TexCharPresets.guessEnumPresets(selectedFont.importCatalog));
        if (EditorGUI.EndChangeCheck()) {
            RecordUndo();
            selectedFont.importCatalog = TexCharPresets.charsFromEnum(ctgImport);
        }
        selectedFont.importCatalog = EditorGUILayout.TextArea(selectedFont.importCatalog, Styles.ImporterPresetArea, GUILayout.Height(60));
        if (GUILayout.Button("Reimport"))
        {
            selectedFont.Populate();
        }
        
        EditorGUILayout.Space();
        #if !TEXDRAW_TMP
        if (ctgImport == ImportCharPresetsType.Custom)  {
            GUILayout.Label("Preview:");
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
                GUILayout.Label( string.Join(", ", System.Array.ConvertAll(
                TexCharPresets.CharsFromString( selectedFont.importCatalog ), x =>  x.ToString())), Styles.ImporterOptionFontStyle, GUILayout.ExpandHeight(true));
            else
                GUILayout.Label("X");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please note that characters outside from the list is still available. Only type on characters that need to be turn into symbols. Max allowed symbol count is 256 per font", Styles.ImporterOptionFontStyle);
        }
        #endif
#if TEXDRAW_TMP
        if (selectedFont.type != TexFontType.Sprite)
            TexTMPImporter.SetupGUI(selectedFont);
#endif 
    }

    void DrawSetter()
    {
        GUILayout.Label(selectedFont.id, Styles.SetterTitle, GUILayout.MaxHeight(25));
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("ID");
            GUILayout.Label(string.Format("<b>{0}</b> (#{1:X})", selectedFont.id, selectedFont.index), Styles.SetterFont);
        }
        EditorGUILayout.EndHorizontal();
        TexChar c = selectedChar;
        if (c.supported)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 2; i++)
            {
                if (GUILayout.Toggle(i == setterState, Styles.SetterHeaderContent[i], Styles.SetterHeader[i]))
                    setterState = i;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            if (setterState == 0)
            {
                // Create an thumbnail
                if (selectedFont.type == TexFontType.Font)
                    EditorGUILayout.LabelField(Styles.GetCharMapContent(selectedFont.chars[selectedCharIdx].characterIndex), 
                        Styles.SetterPreview, GUILayout.Height(selectedFont.Font_Asset.lineHeight * 2.2f));
                else
                {
                    Rect r2 = EditorGUILayout.GetControlRect(GUILayout.Height(selectedFont.font_lineHeight * EditorGUIUtility.labelWidth));
                    EditorGUI.LabelField(r2, GUIContent.none, Styles.SetterPreview);
                    r2.width *= .5f;
                    r2.x += r2.width * .5f;
                    GUI.DrawTextureWithTexCoords(r2, selectedFont.Sprite_Asset, selectedChar.sprite_uv);
                }

                // Basic info stuff
                EditorGUILayout.LabelField("Index", string.Format("<b>{0}</b> (#{0:X2})", selectedCharIdx), Styles.SetterFont);
                EditorGUILayout.LabelField("Character Index", "<b>" +
                    selectedChar.characterIndex.ToString() + "</b> (#" + ((int)selectedChar.characterIndex).ToString("X2")
                    + ")", Styles.SetterFont);

                EditorGUILayout.LabelField("Symbol Definition");
                EditorGUILayout.BeginHorizontal();
                {
                    c.symbolAlt = EditorGUILayout.TextField(c.symbolAlt); //Secondary
                    c.symbolName = EditorGUILayout.TextField(c.symbolName); //Primary
                }
                EditorGUILayout.EndHorizontal();

                c.type = (CharType)EditorGUILayout.EnumPopup("Symbol Type", c.type);
                EditorGUILayout.LabelField("In math, this mapped as:");
                c.characterMap = EditorGUILayout.IntPopup(c.characterMap, Styles.SetterCharMap, Styles.SetterCharMapInt);
            }
            else
            {

                SetterScroll = EditorGUILayout.BeginScrollView(SetterScroll, GUILayout.ExpandHeight(true));
                {
                    EditorGUILayout.LabelField(string.Format("Hash \t : <b>{0}</b> (#{1:X1}{2:X2})", selectedFontIdx * 256 + selectedCharIdx, selectedFontIdx, selectedCharIdx), Styles.SetterFont);
                    c.nextLargerHash = SubDrawThumbnail(c.nextLargerHash, "Is Larger Character Exist?", Styles.SetterNextLarger);
                    EditorGUILayout.Space();
                    if (EditorGUILayout.ToggleLeft("Is Part of Extension?", c.extensionExist))
                    {
                        EditorGUI.indentLevel++;
                        c.extensionExist = true;
                        c.extensionHorizontal = EditorGUILayout.ToggleLeft("Is This Horizontal?", c.extensionHorizontal);

                        c.extentTopHash = SubDrawThumbnail(c.extentTopHash, c.extensionHorizontal ? "Has Left Extension?" : "Has Top Extension?", Styles.SetterExtendTop);
                        c.extentMiddleHash = SubDrawThumbnail(c.extentMiddleHash, "Has Middle Extension?", Styles.SetterExtendMiddle);
                        c.extentBottomHash = SubDrawThumbnail(c.extentBottomHash, c.extensionHorizontal ? "Has Right Extension?" : "Has Bottom Extension?", Styles.SetterExtendBottom);
                        c.extentRepeatHash = SubDrawThumbnail(c.extentRepeatHash, "Has Tiled Extension?", Styles.SetterExtendRepeat);

                        EditorGUI.indentLevel--;
                    }
                    else
                        c.extensionExist = false;
                }
                EditorGUILayout.EndScrollView();
            }
            if (EditorGUI.EndChangeCheck())
            {
                RecordDirty();
                lastCharChanged = true;
            }
        }
        else
        {
            EditorGUILayout.LabelField("<i>Select a Supported Character</i>", Styles.SetterFont);
        }
    }

    int SubDrawThumbnail(int hash, string confirmTxt, GUIStyle style)
    {
        if (EditorGUILayout.ToggleLeft(confirmTxt, hash != -1))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel++;

            if (hash < 0)
                hash = selectedChar.ToHash() + 1;
            int font = hash >> 8, ch = hash % 128;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            font = Mathf.Clamp(EditorGUILayout.IntField(font), 0, targetPreference.fontData.Length);
            ch = Mathf.Clamp(EditorGUILayout.IntField(ch), 0, 127);
            var targetFont = targetPreference.fontData[font];
            style.font = targetFont.Font_Asset;
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            if (targetFont.type == TexFontType.Font)
            {
                //r.height = targetFont.Font_Asset.lineHeight * 1.7f;
                EditorGUILayout.LabelField(Styles.GetCharMapContent(targetFont.chars[ch].characterIndex), style);
            }
            else
            {
                //r.height = targetFont.font_lineHeight * r.width;
                EditorGUILayout.LabelField(GUIContent.none);
                var r = EditorGUILayout.GetControlRect(GUILayout.Height(35));
                GUI.DrawTextureWithTexCoords(r, targetFont.Sprite_Asset, targetFont.chars[ch].sprite_uv);
            }
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.GetControlRect(GUILayout.Height(Mathf.Max(r.height - 18, 36)));
            if (EditorGUI.EndChangeCheck())
                return TEXPreference.CharToHash(font, ch);
            else
                return hash;
        }
        else
            return -1;
    }

    void CheckEvent(bool noCmd)
    {
        Event e = Event.current;
        if (headerActive == 0 && selectedFont != null && selectedChar.supported) {
            if (e.isKey & e.type != EventType.KeyUp) {
                if (e.control | noCmd) {
                    doInput:
                    int verticalJump = selectedFont.type == TexFontType.Font ? 8 : Mathf.Min(selectedFont.sprite_xLength, selectedFont.sprite_yLength);
                    if (e.keyCode == KeyCode.UpArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx - verticalJump, 128);
                    else if (e.keyCode == KeyCode.DownArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx + verticalJump, 128);
                    else if (e.keyCode == KeyCode.LeftArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx - 1, 128);
                    else if (e.keyCode == KeyCode.RightArrow)
                        selectedCharIdx = (int)Mathf.Repeat(selectedCharIdx + 1, 128);
                    else if (e.keyCode == KeyCode.Home)
                        selectedFont.chars[selectedCharIdx].type = (CharType)(int)Mathf.Repeat((int)selectedFont.chars[selectedCharIdx].type - 1, 9);
                    else if (e.keyCode == KeyCode.End)
                        selectedFont.chars[selectedCharIdx].type = (CharType)(int)Mathf.Repeat((int)selectedFont.chars[selectedCharIdx].type + 1, 9);
                    else
                        goto skipUse;
                    if (!selectedFont.chars[selectedCharIdx].supported)
                        goto doInput;
                    float ratio;
                    if (selectedFont.type == TexFontType.Font)
                        ratio = selectedFont.Font_Asset.lineHeight * ((Screen.width - EditorGUIUtility.labelWidth - 60) / 250) + 10;
                    else
                        ratio = selectedFont.font_lineHeight * (Screen.width - EditorGUIUtility.labelWidth - 60) / Mathf.Min(selectedFont.sprite_xLength, selectedFont.sprite_yLength) - 8;
                    //This is just estimation... maybe?
                    ViewerScroll.y = Mathf.Clamp(ViewerScroll.y, (selectedCharIdx / verticalJump - 3) * ratio, (selectedCharIdx / verticalJump - 1) * ratio);
                    e.Use();
                    skipUse:
                    return;
                }
            }
        }
    }

#endregion

#region Configuration

    const float configHeight = 355;
    Vector2 configScroll;
    Vector2 configTypeScroll;
    SerializedProperty matProp;
    Editor configEditor;
    
    void DrawConfiguration()
    {
        EditorGUI.BeginChangeCheck();

        configEditor.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) {
            RecordDirty();
        }
    }
    
    private void DrawMaterial()
    {
        EditorGUILayout.HelpBox("This panel contains list of known TexDraw material all over the project. The management is almost happen automatically so you can skip this tab and focus on your work instead. More options will be given later.", MessageType.Info);
        EditorGUILayout.Space();
        targetPreference.defaultMaterial = (Material)EditorGUILayout.ObjectField("Default Material", targetPreference.defaultMaterial, typeof(Material), false);
        EditorGUILayout.PropertyField(matProp, true);
    }
    
#endregion

#region Glue Management

    void DrawGlue()
    {
        labelMatrixHeight = (Screen.width - 150) / 9f;
        glueSimmetry = GUILayout.Toggle(glueSimmetry, "Edit Symmetrically", Styles.Buttons, GUILayout.Height(22));
        SubDrawMatrix();
    }

    int GlueGet(int l, int r)
    {
        return targetPreference.glueTable[l * 10 + r];
    }

    void GlueSet(int l, int r, int v)
    {
        targetPreference.glueTable[l * 10 + r] = v;
        RecordDirty();
    }

    const float labelMatrixWidth = 110;
    float labelMatrixHeight = 38;
    [SerializeField]
    bool glueSimmetry = true;
    Vector2 scrollGlue;

    void SubDrawMatrix()
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(labelMatrixWidth));
        GUI.matrix = Matrix4x4.TRS(new Vector2(labelMatrixWidth - r.x, r.y + labelMatrixWidth), Quaternion.Euler(0, 0, -90), Vector3.one);
        r.position = Vector2.zero;
        r.y += labelMatrixHeight / 2 + 4;
        for (int i = 0; i < 9; i++) {
            EditorGUI.LabelField(r, Styles.CharTypes[i], Styles.GlueLabelV);
            r.y += labelMatrixHeight;
        }
        GUI.matrix = Matrix4x4.identity;
        EditorGUILayout.GetControlRect(GUILayout.Height(labelMatrixWidth - 36));
        scrollGlue = EditorGUILayout.BeginScrollView(scrollGlue, GUILayout.Height(Screen.height - 245));
        r = EditorGUILayout.GetControlRect(GUILayout.Width(labelMatrixWidth));
        r.height = labelMatrixHeight;
        float xx = r.x;
        int cur, now;
        for (int i = 0; i < 9; i++) {
            GUI.Label(r, Styles.CharTypes[i], Styles.GlueLabelH);
            r.x += labelMatrixWidth;
            r.width = labelMatrixHeight;
            for (int j = 0; j < 9; j++) {
                if (glueSimmetry) {
                    cur = GlueGet(i, j) != GlueGet(j, i) ? -10 : GlueGet(i, j);
                    now = CustomTuner(r, cur);
                    if (cur != now) {
                        GlueSet(i, j, now);
                        GlueSet(j, i, now);
                    }
                } else {
                    cur = GlueGet(i, j);
                    now = CustomTuner(r, cur);
                    if (cur != now)
                        GlueSet(i, j, now);
                }
                r.x += labelMatrixHeight;
                if (glueSimmetry && ((j) >= (i)))
                    break;
            }
            r.x = xx;
            r.y += labelMatrixHeight;
            r.width = labelMatrixWidth;
        }
        EditorGUILayout.GetControlRect(GUILayout.Height(labelMatrixHeight * 9));
        EditorGUILayout.EndScrollView();
    }

#endregion

#region Undo & Functionality

    void RecordDirty()
    {
        if (headerActive == 0 && selectedFont) {
            EditorUtility.SetDirty(selectedFont);
        }
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(targetPreference);
        switch (changeState) {
            case 0:
                RecordRedraw();
                break;
            case 1:
                changeState = 2;
                break;
        }
    }

    void RecordRedrawCallback()
    {
        switch (changeState) {
            case 0:
                RecordRedraw();
                break;
            case 1:
                changeState = 2;
                break;
        }
    }

    void RecordRedraw()
    {
        if (headerActive == 1)
            targetPreference.PushToDictionaries();
        if (headerActive > 0)
            targetPreference.CallRedraw();
    }

    void RecordUndo()
    {
        //   Undo.IncrementCurrentGroup();
        Undo.RecordObjects(new Object[]{ targetPreference, this }, "Changes to TEXDraw Preference");
    }

#endregion

#region Custom GUI Controls

    const int customToggleHash = 0x05f8;

    bool CustomToggle(Rect r, bool value, bool selectable, GUIContent content, GUIStyle style)
    {
        //TO DO: Add functionality for Tab & Page Up/Down
        int controlID = GUIUtility.GetControlID(customToggleHash, selectable ? FocusType.Passive : FocusType.Passive);
        bool result = GUI.Toggle(r, value, content, style);
        if (value != result)
            GUIUtility.keyboardControl = controlID;
        if (GUIUtility.keyboardControl == controlID)
            CheckEvent(true);
        return result;
    }

    const int customTunerHash = 0x08e3;

    int CustomTuner(Rect r, int value)
    {
        int controlID = GUIUtility.GetControlID(customTunerHash, FocusType.Passive, r);
        Event current = Event.current;
        EventType typeForControl = current.GetTypeForControl(controlID);
        if (typeForControl == EventType.Repaint) {
            Styles.GlueProgBack.Draw(r, false, false, false, false);
            Rect r2 = new Rect(r);
            r2.yMin = Mathf.Lerp(r2.yMax, r2.yMin, value == 10 ? 1 : (value * 0.06f + 0.2f));
            if (value > 0)
                Styles.GlueProgBar.Draw(r2, false, false, false, false);
            Styles.GlueProgText.Draw(r, value == -10 ? "--" : value.ToString(), false, false, false, false);
        } else if (typeForControl == EventType.MouseDrag) {
            Vector2 mousePos = (current.mousePosition);
            if (!r.Contains(mousePos))
                return value;
            float normValue = Mathf.InverseLerp(r.yMin, r.yMax, mousePos.y);
            value = Mathf.Clamp(Mathf.FloorToInt((Mathf.Sqrt((1 - normValue)) / 0.6f - 0.4f) * 10f), -1, 10);
        } else if (typeForControl == EventType.MouseDown) {
            if (!r.Contains(current.mousePosition))
                return value;
            return (int)Mathf.Repeat(value + (current.shift ? -1 : 1) + 1, 12) - 1;
        }
        return value;
    }

#endregion

#if TEXDRAW_TMP

    public override bool RequiresConstantRepaint()
    {
        return TexTMPImporter.onRendering;
    }

#endif
}
#endif