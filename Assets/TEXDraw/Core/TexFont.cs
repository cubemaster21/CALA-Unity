using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if TEXDRAW_TMP
using TMPro;
#endif

namespace TexDrawLib
{
    /// <summary>
    /// a class contain each font data, and saved in the preference file.
    /// NOTE: all members are public for convenience, you shouldn't modify it at all
    /// </summary>
    public class TexFont : ScriptableObject
    {

        // core data 
        public TexFontType type;
        public string id;
        public int index;

        public string importCatalog;
        public string assetPath;
        public char[] parsedCatalogs;

        // font data
        [FormerlySerializedAs("font")]
        public Font Font_Asset;
        public float font_lineHeight;

        // sprite data, also used for SDF Texture
        [FormerlySerializedAs("sprite")]
        public Texture2D Sprite_Asset;
        public int sprite_xLength;
        public int sprite_yLength;
        public bool sprite_detectAlpha;
        public float sprite_scale = 1;
        public float sprite_lineOffset;
        public bool sprite_alphaOnly;

        public TexChar[] chars;

        public TexFont()
        {
        }

        public TexCharTranslatorDictionary charCatalogs;

        public TexChar GetCharacterData(char c)
        {
            #if UNITY_EDITOR
            try
            {
                return chars[charCatalogs[c]];

            }
            catch (System.Exception ex)
            {
                Debug.LogWarningFormat("Character doesn't found: {0} in {1}. Please rebuild the font data", c, this.name);
                throw ex;
            }
            #else
            return chars[charCatalogs[c]];
            #endif
        }

        public TexCharMetric CreateCharacterDataOnTheFly(char c, float scale, out CharacterInfo info)
        {
            if (type == TexFontType.Font)
            {
                CharacterInfo f;
                Font_Asset.RequestCharactersInTexture(new string(c, 1), TexUtility.RenderTextureSize, TexUtility.RenderFontStyle);
                Font_Asset.GetCharacterInfo(c, out f, TexUtility.RenderTextureSize, TexUtility.RenderFontStyle);
                info = f;
                var factor = 1f / (info.size == 0 ? Font_Asset.fontSize : info.size);
                return TexCharMetric.Get(null, info.maxY * factor, -info.minY * factor, -info.minX * factor, info.maxX * factor, info.advance * factor, scale);
            }
            else
            {
                // Nothing we can do for sprites ...
                info = new CharacterInfo();
                return TexCharMetric.Get(null, 0, 0, 0, 0, 0, 0);
            }
        }
        
        public bool supported {
            get {
                return (type == TexFontType.Font) ? Font_Asset != null : Sprite_Asset != null;
            }
        }

#if UNITY_EDITOR

        public void PopulateCatalogs()
        {
            charCatalogs = new TexCharTranslatorDictionary();
            for (int i = 0; i < chars.Length; i++)
            {
                charCatalogs[chars[i].characterIndex] = chars[i].index;
            }
        }

        /*public TexFont(string ID, int Index, TexFontType FontType, string AssetPath, string ImportCatalog = null)
        {
            type = FontType;
            id = ID;
            index = Index;
            importCatalog = ImportCatalog;
            assetPath = AssetPath;


            if (importCatalog != null)
                Populate(false);
        }*/
        
        public void ClearAssetData ()
        {
            Font_Asset = null;
            Sprite_Asset = null;
#if TEXDRAW_TMP
            SDF_Asset = null;
#endif           
        }
        
        public void GrabAssetData(string path)
        {
            assetPath = path;
            GrabAssetData();
        }
        
        public void GrabAssetData()
        {
            switch (type)
            {
                case TexFontType.Font:
                    Font_Asset = AssetDatabase.LoadAssetAtPath<Font>(assetPath);
                    break;
                case TexFontType.Sprite:
                    Sprite_Asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    break;
#if TEXDRAW_TMP
                case TexFontType.Font_SDF:
                    SDF_Asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                    break;
#endif
            }
        }

        //const int maxAllowedCharsIfBlankPermitted = 255;
        // Rebuild character lists........ 
        public void Populate(bool alsoUpdateMaterials = true)
        {
            parsedCatalogs = TexCharPresets.CharsFromString(importCatalog);

            switch (type)
            {
                case TexFontType.Font:
                    PopulateCharacter();
                    break;
                case TexFontType.Sprite:
                    PopulateSprite();
                    break;
#if TEXDRAW_TMP
                case TexFontType.Font_SDF:
                    PopulateSDFFontAsset();
                    break;
#endif
            }
            PopulateCatalogs();
            if (alsoUpdateMaterials) {
                TEXPreference.main.RebuildMaterial();
                TEXPreference.main.PushToDictionaries();
            }
            EditorUtility.SetDirty(this);
        }
        

        public void PopulateCharacter()
        {
            //A GLITCH: Unity's Font.HasCharacter doesn't work properly on dynamic mode, we need to change it to Unicode first
            TrueTypeFontImporter fontData = (TrueTypeFontImporter)AssetImporter.GetAtPath(assetPath);
            if (!fontData)
            {
                assetPath = AssetDatabase.GetAssetPath(Font_Asset);
                fontData = (TrueTypeFontImporter)AssetImporter.GetAtPath(assetPath);
            }
            fontData.customCharacters = parsedCatalogs == null || parsedCatalogs.Length == 0 ? null : new System.String(parsedCatalogs);
            fontData.fontTextureCase = string.IsNullOrEmpty(fontData.customCharacters) ? FontTextureCase.Unicode : FontTextureCase.CustomSet;
            fontData.SaveAndReimport();

            chars = chars ?? new TexChar[0];

            TexChar ch;
            // Import all or on what parsedCatalogs says
            var availableChars = Font_Asset.characterInfo;
            var newChars = new TexChar[parsedCatalogs.Length == 0 ? availableChars.Length : parsedCatalogs.Length];

            // Try to keep the character data not destroyed at all.
            for (int i = 0; i < newChars.Length; i++)
            {
                if (parsedCatalogs.Length == 0)
                    ch = newChars[i] = chars.FirstOrDefault(x => x.characterIndex == availableChars[i].index) ?? new TexChar(this, i, (char)availableChars[i].index, true, 1);
                else
                    ch = newChars[i] = chars.FirstOrDefault(x => x.characterIndex == parsedCatalogs[i]) ?? new TexChar(this, i, parsedCatalogs[i], Font_Asset.HasCharacter(parsedCatalogs[i]), 1);

                if (!ch.supported)
                    continue;
                CharacterInfo c;
                Font_Asset.GetCharacterInfo(ch.characterIndex, out c, 0, FontStyle.Normal);
                float factor = c.size == 0 ? Font_Asset.fontSize : c.size;
                ch.depth = -c.minY / factor;
                ch.height = c.maxY / factor;
                ch.bearing = -c.minX / factor;
                ch.italic = c.maxX / factor;
                ch.width = c.advance / factor;
                ch.fontIndex = index;
                ch.index = i;
            }
            chars = newChars;
            font_lineHeight = Mathf.Max(Font_Asset.lineHeight / Font_Asset.fontSize, 0.2f);

            // Try to keep the character data not destroyed at all.
            for (int i = 0; i < newChars.Length; i++)
                chars = newChars;


            fontData.fontTextureCase = FontTextureCase.Dynamic;
            fontData.SaveAndReimport();
        }

        public void PopulateSprite()
        {
            if (sprite_xLength < 1 || sprite_yLength < 1 || sprite_scale <= 0.0e-5f)
                SuggestTileSize();

            int maxCount = Mathf.Min(sprite_xLength * sprite_yLength, parsedCatalogs.Length == 0 ? int.MaxValue : parsedCatalogs.Length);
            var newChars = new TexChar[maxCount];
            Vector2 size = new Vector2(1 / (float)sprite_xLength, 1 / (float)sprite_yLength);
            // Just for a placeholder (at least no null!)
            chars = chars ?? new TexChar[0];

            for (int i = 0; i < maxCount; i++)
            {
                int x = i % sprite_xLength, y = i / sprite_xLength;

                TexChar ch;
                if (parsedCatalogs.Length == 0)
                    ch = newChars[i] = i < chars.Length ? chars[i] : new TexChar(this, i, (char)i, true, sprite_scale);
                else
                    ch = newChars[i] = chars.FirstOrDefault(j => j.characterIndex == parsedCatalogs[i]) ?? new TexChar(this, i, parsedCatalogs[i], true, sprite_scale);

                ch.depth = -sprite_lineOffset;
                ch.height = sprite_scale + sprite_lineOffset;
                ch.bearing = 0;
                ch.italic = sprite_scale;
                ch.width = sprite_scale;

                ch.sprite_uv = new Rect(Vector2.Scale(new Vector2(x, sprite_yLength - y - 1), size), size);
                ch.supported = i < maxCount;
				ch.fontIndex = index;
				ch.index = i;
            }
            chars = newChars;
            font_lineHeight = Mathf.Max(0.2f, sprite_scale * (Sprite_Asset.height / (float)sprite_yLength) / (Sprite_Asset.width / (float)sprite_xLength));
        }

        void SuggestTileSize()
        {
            sprite_xLength = 8;
            sprite_yLength = 4;
            sprite_scale = 1;
        }

#if TEXDRAW_TMP

        public TMP_FontAsset SDF_Asset;
        System.Exception Exception;

        public void PopulateSDFFontAsset()
        {
            // Char still have to follow what parsed Catalog says.
            var availableChars = TMP_FontAsset.GetCharactersArray(SDF_Asset);
            var newChars = new TexChar[parsedCatalogs.Length == 0 ? availableChars.Length : parsedCatalogs.Length];
            var info = SDF_Asset.fontInfo;
            var padding = info.Padding;
            TexChar ch;
            chars = chars ?? new TexChar[0];
            for (int i = 0; i < newChars.Length; i++)
            {

                if (parsedCatalogs.Length == 0)
                    ch = newChars[i] = chars.FirstOrDefault(j => j.characterIndex == availableChars[i]) ?? new TexChar(this, i, parsedCatalogs[i], true, 1);
                else
                    ch = newChars[i] = chars.FirstOrDefault(j => j.characterIndex == parsedCatalogs[i]) ?? new TexChar(this, i, parsedCatalogs[i], true, 1);

                if (!(ch.supported = parsedCatalogs.Length == 0 || ArrayUtility.Contains(availableChars, parsedCatalogs[i])))
                    continue;

                TMP_Glyph c = SDF_Asset.characterDictionary[parsedCatalogs[i]];

                var factor = c.scale / info.PointSize;

                ch.depth = (c.height - c.yOffset + padding) * factor;
                ch.height = (c.yOffset + padding) * factor;
                ch.bearing = (-c.xOffset + padding) * factor;
                ch.italic = (c.width + c.xOffset + padding) * factor;
                ch.width = c.xAdvance * factor;

                var uv = new Rect();
                uv.x = (c.x - padding) / info.AtlasWidth;
                uv.y = 1 - (c.y + c.height + padding) / info.AtlasHeight;
                uv.width = (c.width + 2 * padding) / info.AtlasWidth;
                uv.height = (c.height + 2 * padding) / info.AtlasHeight;
				
                //var uv = new Rect(c.x / info.AtlasWidth, c.y / info.AtlasHeight, 
                //    c.width / info.AtlasWidth, c.height / info.AtlasHeight);
                ch.sprite_uv = uv; 
				ch.fontIndex = index;
				ch.index = i;
            }
            chars = newChars;
            font_lineHeight = Mathf.Max(0.2f, info.LineHeight / info.PointSize);
            sprite_xLength = 8;
            sprite_yLength = chars.Length / 8 + 1;
            sprite_alphaOnly = true;
            Sprite_Asset = SDF_Asset.atlas;
        }

#endif
#endif
    }
}
