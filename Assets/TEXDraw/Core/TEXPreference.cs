#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TexDrawLib
{
    public partial class TEXPreference : ScriptableObject
    {
        static TEXPreference m_main;
        
#if UNITY_EDITOR
        //PAY ATTENTION TO THIS: IF YOU RUN INTO ERROR WHILE MOVING TEXDRAW
        //FOLDER, THEN REPLACE THIS IN DEBUG INSPECTOR (NOT HERE)
        public string MainFolderPath = "Assets/TEXDraw";
        const string DefaultTexFolder = "Assets/TEXDraw";

        public bool IncludeMathSlot = true;

        /*public TextAsset XMLFontDefinitions;
        public TextAsset XMLSymbolDefinitions;
	    public TextAsset XMLMathDefinitions;
	    public TextAsset XMLConfiguration;*/
        public int header_mathCount;
        public int header_userCount;
        /// Check if we are on importing process.
        /// This solve issues where TEXDraw component 
        /// tries to render in the middle of importing process..
        public bool editorReloading = false;

        //Main & Shared access to TEXDraw Preference
        static public TEXPreference main
        {
            get
            {
                if (!m_main)
                {
                    //Get the Preference
                    string[] targetData = AssetDatabase.FindAssets("t:TEXPreference");
                    if (targetData.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(targetData[0]);
                        m_main = AssetDatabase.LoadAssetAtPath<TEXPreference>(path);
                        m_main.MainFolderPath = System.IO.Path.GetDirectoryName(path);
                        // TEXDraw preference now put into resources files after 3.0
                        if (m_main.MainFolderPath.Contains("Resources"))
                            m_main.MainFolderPath = System.IO.Path.GetDirectoryName(m_main.MainFolderPath);
                        if (targetData.Length > 1)
                            Debug.LogWarning("You have more than one TEXDraw preference, ensure that only one Preference exist in your Project");
                    }
                    else
                    {
                        //Create New One
                        m_main = ScriptableObject.CreateInstance<TEXPreference>();
                        if (AssetDatabase.IsValidFolder(DefaultTexFolder))
                        {
                            AssetDatabase.CreateAsset(m_main, DefaultTexFolder + "/Resources/TEXDrawPreference.asset");
                            m_main.FirstInitialize(DefaultTexFolder);
                        }
                        else
                        {
                            //Find alternative path to the TEXPreference, that's it: Parent path of TEXPreference script.
                            string AlternativePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_main));
                            AlternativePath = System.IO.Directory.GetParent(AlternativePath).Parent.FullName;
                            AssetDatabase.CreateAsset(m_main, AlternativePath + "/Resources/TEXDrawPreference.asset");
                            m_main.FirstInitialize(AlternativePath);
                        }
                    }
                }
                return m_main;
            }
            set
            {
                //Leaving this empty,
                //TEXDraw components will assign this manually
                //when on real build.
            }
        }
#else
        static public TEXPreference main
        {
            get
            {
                if (!m_main) {
                    // The only thing that we can found is in the Resource folder
                    m_main = (TEXPreference)Resources.Load("TEXDrawPreference");
                }
                return m_main;
            }
            set
            {
                m_main = value;
            }
        }
#endif

#region Runtime Utilities
        //Default Material
        public Material defaultMaterial;
        //The most important reference to Central font data
        public TexFont[] fontData;
        //Dictionaries for references, etc.
        public TexSymbolDictionary symbolData;
        public TexCharMapDictionary charMapData;
        public TEXConfiguration preferences;
        //public TexTypeFaceDictionary defaultTypefaces;
        //Rule Table: Left * 10 + Right
        public int[] glueTable = new int[100];

        public TexFont GetFontByID(string id)
	    {
		    for (int i = 0; i < fontData.Length; i++) {
			    if(fontData[i].id == id)
				    return fontData[i];
		    }
		    return null;
        }

        public int GetFontIndexByID(string id)
        {
        	if (string.IsNullOrEmpty(id))
        		return -1;
	        for (int i = 0; i < fontData.Length; i++) {
		        if(fontData[i].id == id)
			        return fontData[i].index;
	        }
	        return -1;
        }

        public TexChar GetChar(int font, int ch)
        {
            return fontData[font].chars[ch];
        }

        public TexChar GetChar(int hash)
        {
            if (hash < 0)
                return null;
            return fontData[hash >> 8].chars[hash % 256];
        }

        public TexChar GetChar(string symbol)
        {
            return GetChar(symbolData[symbol, -1]);
        }

        public TexCharMetric GetCharMetric(string symbol, float size)
        {
            return GetChar(symbol).GetMetric(size);
        }

        public TexCharMetric GetCharMetric(string symbol, TexStyle style)
        {
            return GetChar(symbol).GetMetric(TexUtility.SizeFactor(style));
        }

        public TexCharMetric GetCharMetric(TexChar Char, TexStyle style)
        {
            if (Char != null)
                return Char.GetMetric(TexUtility.SizeFactor(style));
            else
                return null;
        }

        public TexCharMetric GetCharMetric(char ch, TexStyle style)
        {
            TexFont font;
        #if UNITY_EDITOR
            if (!TEXConfiguration.main)
                Debug.LogError("No tex configuraion exists!"); // Never gonna happen
        #endif
            if (ch >= 'A' && ch <= 'Z') // char.IsUpper(ch)
                font = fontData[preferences.Typeface_Capitals];
            else if (ch >= 'a' && ch <= 'z') // char.IsLower(ch)
                font = fontData[preferences.Typeface_Small];
            else if (ch >= '0' && ch <= '9') // char.IsDigit(ch)
                font = fontData[preferences.Typeface_Number];
            else
                font = fontData[preferences.Typeface_Unicode];
                
            return font.GetCharacterData(ch).GetMetric(TexUtility.SizeFactor(style));
        }

        public TexCharMetric GetCharMetric(int font, char ch, TexStyle style)
        {
            return fontData[font].GetCharacterData(ch).GetMetric(TexUtility.SizeFactor(style));           
        }
        
        public bool IsCharAvailable(int font, char ch)
        {
            return fontData[font].charCatalogs.ContainsKey(ch);           
        }

        public int GetGlue(CharType leftType, CharType rightType)
        {
            return glueTable[(int)leftType * 10 + (int)rightType]; 
        }

        static public int CharToHash(TexChar ch)
        {
            return ch.index | ch.font.index << 8;
        }

        static public int CharToHash(int font, int ch)
        {
            return ch | font << 8;
        }

        [System.Obsolete("No more been used. Use the Dictionary instead")]
        static public int TranslateChar(int charIdx)
        {
            //An Integer Conversion from TEX-Character-Space (0-7F) to Actual-Character-Map (ASCII Latin-1)
            if (charIdx >= 0x0 && charIdx <= 0xf)
                return charIdx + 0xc0;
            if (charIdx == 0x10)
                return 0xb0;
            if (charIdx >= 0x11 && charIdx <= 0x16)
                return charIdx + (0xd1 - 0x11);
            if (charIdx == 0x17)
                return 0xb7;
            if (charIdx >= 0x18 && charIdx <= 0x1c)
                return charIdx + (0xd8 - 0x18);
            if (charIdx >= 0x1d && charIdx <= 0x1e)
                return charIdx + (0xb5 - 0x1d);
            if (charIdx == 0x1f)
                return 0xdf;
            if (charIdx == 0x20)
                return 0xef;
            if (charIdx >= 0x21 && charIdx <= 0x7e)
                return charIdx;
            if (charIdx == 0x7f)
                return 0xff;
            return 0;
        }
		
#endregion
    }
}