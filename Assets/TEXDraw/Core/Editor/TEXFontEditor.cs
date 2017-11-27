
using TexDrawLib;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TexFont))]
public class TEXFontEditor : Editor
{
    static internal class Styles
    {
        public static GUIStyle FontPreviewSymbols = new GUIStyle(EditorStyles.objectFieldThumb);
        public static GUIStyle FontPreviewRelation = new GUIStyle(EditorStyles.textArea);
        public static GUIStyle FontPreviewEnabled = new GUIStyle(EditorStyles.helpBox);
        public static GUIStyle FontPreviewDisabled = new GUIStyle(EditorStyles.label);
        public static GUIStyle SetterPreview = new GUIStyle(EditorStyles.helpBox);

        public static GUIContent[] CharMapContents = new GUIContent[0xffff];

        public static GUIStyle ImporterOptionFontStyle = new GUIStyle(EditorStyles.wordWrappedLabel);

        public static GUIContent GetCharMapContent (char c)
        {
            return CharMapContents[c] ?? (CharMapContents[c] = new GUIContent(new string(c, 1)));
        }
        
        static Styles () {
            ImporterOptionFontStyle.alignment = TextAnchor.MiddleCenter;
            FontPreviewEnabled.alignment = TextAnchor.MiddleCenter;
            FontPreviewSymbols.alignment = TextAnchor.MiddleCenter;
            FontPreviewRelation.alignment = TextAnchor.MiddleCenter;
            FontPreviewDisabled.alignment = TextAnchor.MiddleCenter;
            FontPreviewRelation.fixedHeight = 0; 
            FontPreviewRelation.onActive = FontPreviewEnabled.onActive; 
            FontPreviewRelation.onNormal = FontPreviewRelation.focused;
            FontPreviewRelation.focused = FontPreviewEnabled.focused;
            SetterPreview.fontSize = 34;
            SetterPreview.alignment = TextAnchor.MiddleCenter;
        }
    }
    void OnEnable ()
    {
        isFirstGUI = true;
    }

    public override void OnInspectorGUI()
    {
        if (isFirstGUI)
        {
            Styles.FontPreviewEnabled.font = selectedFont.Font_Asset;
            Styles.FontPreviewSymbols.font = selectedFont.Font_Asset;
            Styles.FontPreviewRelation.font = selectedFont.Font_Asset;
            Styles.SetterPreview.font = selectedFont.Font_Asset;
            isFirstGUI = false;
        }

        var f = (TexFont)target;
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        {
            var r = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUIUtility.labelWidth /= 2f;
            DrawOptionsPanel(f);
            EditorGUIUtility.labelWidth *= 2f;
            EditorGUILayout.EndVertical();
            r = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            r.height = Screen.height - 100;
            if (f.type == TexFontType.Font)
                DrawViewerFont(r, f);
            else
                DrawViewerSprite(r, f);
        }
        EditorGUILayout.EndHorizontal();
    }

    bool isFirstGUI;    
    Vector2 ViewerScroll;
    TexFont selectedFont { get { return (TexFont)target; } }
    int selectedCharIdx;
    FontStyle selectedStyle;

    void DrawViewerFont(Rect drawRect, TexFont selectedFont)
    {
        if (!selectedFont.Font_Asset)
        {
            // Something wrong?

            EditorGUI.LabelField(drawRect, "The Font Asset is NULL. This Asset might currently unused.", Styles.ImporterOptionFontStyle);
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
                    selectedCharIdx = newS;
                }
        }
        GUI.EndScrollView();
    }

    void DrawViewerSprite(Rect drawRect, TexFont selectedFont)
    {
        if (!selectedFont.Sprite_Asset)
        {
            // Something wrong?

            EditorGUI.LabelField(drawRect, "The Sprite Asset is NULL. This Asset might currently unused.", Styles.ImporterOptionFontStyle);
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
            if (CustomToggle(r, selectedCharIdx == i, ch.supported, GUIContent.none, SubDetermineStyle(ch))) {
                    int newS = i + (selectedCharIdx - l);
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
    
    const int customToggleHash = 0x05f8;
    
    // Toggles for viewer
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
    
    void CheckEvent(bool noCmd)
    {
        Event e = Event.current;
        if (selectedFont.chars[selectedCharIdx].supported) {
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
    
    void DrawOptionsPanel (TexFont selectedFont) {
        // Draw the header
        EditorGUILayout.LabelField(selectedFont.id, EditorStyles.largeLabel);
        if (!selectedFont.supported)
        {
            return; 
        }
        var sel = (FontStyle)EditorGUILayout.EnumPopup("Preview", selectedStyle);
        if (sel != selectedStyle) {
            selectedStyle = sel;
            Styles.FontPreviewEnabled.fontStyle = sel;
            Styles.FontPreviewSymbols.fontStyle = sel;
            Styles.FontPreviewRelation.fontStyle = sel;
            Styles.SetterPreview.fontStyle = sel;
        }
        EditorGUILayout.Space();
        // Preview the font
        var ch = selectedFont.chars[selectedCharIdx];
        if (!ch.supported)
        {
            EditorGUILayout.HelpBox("Character isn't available", MessageType.Info);
            return;
        }
        if (selectedFont.type == TexFontType.Font)
                    EditorGUILayout.LabelField(Styles.GetCharMapContent(ch.characterIndex), Styles.SetterPreview, GUILayout.Height(selectedFont.Font_Asset.lineHeight * 2.2f)/*..*/);
        else {
            Rect r2 = EditorGUILayout.GetControlRect(GUILayout.Height(selectedFont.font_lineHeight * EditorGUIUtility.labelWidth)/*..*/);
            EditorGUI.LabelField(r2, GUIContent.none, Styles.SetterPreview);
            GUI.DrawTextureWithTexCoords(r2, selectedFont.Sprite_Asset, ch.sprite_uv);
        }
        EditorGUILayout.Space();
        // Draw stats
        EditorGUILayout.LabelField("Character", string.Format("{0} U+{1}", ch.characterIndex, ((int)ch.characterIndex).ToString("X")));
        EditorGUILayout.LabelField("Index", string.Format("{0} #{1}", ch.index, (ch.ToHash()).ToString("X3")));
        if (!string.IsNullOrEmpty(ch.symbolName)) {
            EditorGUILayout.Space();
            if (string.IsNullOrEmpty(ch.symbolAlt))
                EditorGUILayout.LabelField("Symbol", "\\" + ch.symbolName);
            else
                EditorGUILayout.LabelField("Symbol", string.Format("\\{0} / \\{1}", ch.symbolName, ch.symbolAlt));    
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Metric Size", string.Format("{0} x {1}", (ch.bearing + ch.italic).ToString("0.00"), (ch.depth + ch.height).ToString("0.00")));
        EditorGUILayout.LabelField("Height", ch.height.ToString("0.000"));
        EditorGUILayout.LabelField("Depth", ch.depth.ToString("0.000"));
        EditorGUILayout.LabelField("Bearing", ch.bearing.ToString("0.000"));
        EditorGUILayout.LabelField("Italic", ch.italic.ToString("0.000"));
        EditorGUILayout.LabelField("Advance", ch.width.ToString("0.000"));
        EditorGUILayout.Space();
        DrawRelationButton("Larger Char", ch.nextLargerHash);
        if (ch.extensionExist) {
            EditorGUILayout.LabelField("Part of extension");
            if (ch.extensionHorizontal) {
                DrawRelationButton("Left Extent", ch.extentTopHash);
                DrawRelationButton("Middle Extent", ch.extentMiddleHash);
                DrawRelationButton("Right Extent", ch.extentBottomHash);
                DrawRelationButton("Tiled Extent", ch.extentRepeatHash);
            } else {
                DrawRelationButton("Top Extent", ch.extentTopHash);
                DrawRelationButton("Middle Extent", ch.extentMiddleHash);
                DrawRelationButton("Bottom Extent", ch.extentBottomHash);
                DrawRelationButton("Tiled Extent", ch.extentRepeatHash);
            }
        }
            
    }
    
    void DrawRelationButton (string label, int chHash) {
        if (chHash == -1)
            return;
        var r = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent(label));
        if(GUI.Button(r, "#" + chHash.ToString("X3"))) {
            var font = chHash >> 8;
            var ch = chHash % 256;
            Selection.activeObject = TEXPreference.main.fontData[font];
            selectedCharIdx = ch;
        }
    }
    
}