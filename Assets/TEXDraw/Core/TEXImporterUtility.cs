#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace TexDrawLib
{
    /// Contains some utility codes to manage the automatic importing processes.
    public class TexImporterUtility
    {
        
        
        public static void ReadFromResources(TEXPreference pref)
        {
            var fontList = new List<TexFont>();
            if (!AssetDatabase.IsValidFolder(pref.MainFolderPath + "/Resources/TexFontMetaData"))
                AssetDatabase.CreateFolder(pref.MainFolderPath + "/Resources", "TexFontMetaData");
            LoadPrimaryDefinitionSubset(pref, fontList, pref.MainFolderPath + "/Fonts/Math", "t:Font", TexFontType.Font, 0);
            pref.header_mathCount = fontList.Count;
            LoadPrimaryDefinitionSubset(pref, fontList, pref.MainFolderPath + "/Fonts/User", "t:Font", TexFontType.Font, 1);
            pref.header_userCount = fontList.Count;
            LoadPrimaryDefinitionSubset(pref, fontList, pref.MainFolderPath + "/Fonts/Sprites", "t:Sprite", TexFontType.Sprite, 2);
            EditorUtility.DisplayProgressBar("Reloading", "Preparing Stuff...", .93f);
            
            pref.fontData = fontList.ToArray();
            
        }
        
        const string resourceFontMetaPath = "/Resources/TexFontMetaData/";
        
        static void LoadPrimaryDefinitionSubset(TEXPreference pref, List<TexFont> fontList, string folderPath, string typeStr, TexFontType typeEnum, int mode)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                return;
            string[] customF = AssetDatabase.FindAssets(typeStr, new string[] { folderPath });
            for (int i = 0; i < customF.Length; i++)
            {
                if (fontList.Count >= 31)
                {
                    Debug.LogWarning("Font/Sprite database count are beyond 31, ignoring any assets after " + fontList[fontList.Count - 1].id);
                    break;
                }
                string realPath = AssetDatabase.GUIDToAssetPath(customF[i]);
                string id = Path.GetFileNameWithoutExtension(realPath).ToLower();
                UpdateProgress(mode, id, i, customF.Length);
                if (!isNameValid(id)) {
                    // We show this for information purpose, since *-Regular or *_ is very common mistake
                    // We are not showing this for frequent update, since this behavior is 'intended' for giving an alternative styling
                    if (id.Contains("-Regular") || id.Substring(id.Length - 1) == "_")
                        Debug.LogWarning("File " + id + " is ignored since it has invalid character in it's name");
                    continue;                    
                }
                var metaPath = pref.MainFolderPath + resourceFontMetaPath + id + ".asset";
                var metadata = AssetDatabase.LoadAssetAtPath<TexFont>(metaPath);
                if (!metadata) {
                    metadata = ScriptableObject.CreateInstance<TexFont>();
                    AssetDatabase.CreateAsset(metadata, metaPath);
                    metadata.importCatalog = TexCharPresets.legacyChars;
                }
                metadata.id = id;
                metadata.ClearAssetData();
                metadata.index = fontList.Count;
                metadata.type = typeEnum;
                metadata.GrabAssetData(realPath);
    #if TEXDRAW_TMP
                string sdfPath = pref.MainFolderPath + "/Fonts/TMPro/" + id + ".asset";
                if (typeEnum == TexFontType.Font && AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(sdfPath))
                {
                    metadata.type = TexFontType.Font_SDF;
                    metadata.GrabAssetData(sdfPath);
                }
    #endif
                metadata.Populate(false);
                fontList.Add(metadata);
            }
        }
        
        static void UpdateProgress(int phase, string name, int idx, int total)
        {
            var prog = idx / (float)total;
            prog = phase * 0.3f + (prog * 0.3f);
            EditorUtility.DisplayProgressBar("Reloading", "Reading " + name + "...", prog);
        }
        
        
        static bool isNameValid (string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsLetter(name[i]))
                    continue;
                else
                    return false;
            }
            return true;
        }
        
        //Converting a hash from XML file to Our Font map,
        static int SyncHash(int origHash, List<int> syncMap)
        {
            if (origHash == -1)
                return -1;
            if (syncMap[origHash >> 8] == -1)
                return -1;
            return (syncMap[origHash >> 8] << 8) + (origHash % 128);
        }
        
        public static void ReadLegacyXMLSymbols(TEXPreference pref, bool isMath, string XMLData)
        {
            XmlDocument doc = new XmlDocument();
	        doc.LoadXml(XMLData);
            TexChar c; 

	        XmlNode SyncMap = doc.SelectSingleNode("/TexSymbols/FontIDs");
	        List<int> sMap = new List<int>();
	        for (int i = 0; i < SyncMap.ChildNodes.Count; i++) {
                XmlAttributeCollection attr = SyncMap.ChildNodes[i].Attributes;
		        int idx = pref.GetFontIndexByID(attr["id"].Value);
		        var max = int.Parse(attr["index"].Value);
		        if(max > sMap.Count)
		        {
		        	for (int j = 0; j < max; j++) {
			        	sMap.Add(j);
		        	}
		        }
	            sMap.Add(idx);
                if (idx >= 0) {
                    var font = pref.fontData[idx];
                    bool catalogChange = false;
                    for (int j = 0; j < attr.Count; j++) {
                        switch (attr[j].Name) {
                            case "xLength":
                                font.sprite_xLength = int.Parse(attr[j].Value);
                                break;
                            case "yLength":
                                font.sprite_yLength = int.Parse(attr[j].Value);
                                break;
                            case "scale":
                                font.sprite_scale = float.Parse(attr[j].Value);
                                break;
                            case "lineOffset":
                                font.sprite_lineOffset = float.Parse(attr[j].Value);
                                break;
                            case "alphaOnly":
                                font.sprite_alphaOnly = int.Parse(attr[j].Value) > 0;
                                break;
                            case "importCatalog":
                                if (catalogChange |= !string.IsNullOrEmpty(attr[j].Value))
                                    font.importCatalog = attr[j].Value;
                                break;
                        }
                    }

                    font.importCatalog = font.importCatalog == null ? TexCharPresets.legacyChars : font.importCatalog;
                    font.Populate(false);
                }
            }

            XmlNode SymbolMap = doc.SelectSingleNode("/TexSymbols/SymbolMap");
	        foreach (XmlNode node in SymbolMap.ChildNodes) {
		        int hash = int.Parse(node.Attributes["hash"].Value);
		        if(!isMath && hash >> 8 < pref.header_mathCount)
			        continue;
		        hash = SyncHash(hash, sMap);
                if (hash >= 0) {
	                c = pref.GetChar(hash);
                    var attr = node.Attributes;
                    for (int j = 0; j < attr.Count; j++) {
                        switch (attr[j].Name) {
                            case "name":
                                c.symbolName = attr[j].Value;
                                break;
                            case "nameAlt":
                                c.symbolAlt = attr[j].Value;
                                break;
                            case "typeId":
                                c.type = (CharType)int.Parse(attr[j].Value);
                                break;
                        }
                    }

 //                   pref.symbolData.Add(c.symbolName, c.ToHash());
   //                 if (!string.IsNullOrEmpty(c.symbolAlt))
     //                   pref.symbolData.Add(c.symbolAlt, c.ToHash());
                }
            }

            XmlNode LargerMap = doc.SelectSingleNode("/TexSymbols/LargerMap");
            foreach (XmlNode node in LargerMap.ChildNodes) {
	            int hash = int.Parse(node.Attributes["hash"].Value);
	            if(!isMath && hash >> 8 < pref.header_mathCount)
		            continue;
	            hash = SyncHash(hash, sMap);
	            if (hash >= 0) {
                    c = pref.GetChar(hash);
                    c.nextLarger = pref.GetChar(SyncHash(int.Parse(node.Attributes["targetHash"].Value), sMap));
                }
            }

            XmlNode ExtensionMap = doc.SelectSingleNode("/TexSymbols/ExtensionMap");
            foreach (XmlNode node in ExtensionMap.ChildNodes) {
	            int hash = int.Parse(node.Attributes["hash"].Value);
	            if(!isMath && hash >> 8 < pref.header_mathCount)
		            continue;
	            hash = SyncHash(hash, sMap);
	            if (hash >= 0) {
                    c = pref.GetChar(hash);
                    c.extensionExist = true;
                    var attr = node.Attributes;
                    for (int j = 0; j < attr.Count; j++) {
                        switch (attr[j].Name) {
                            case "top":
                                c.extentTopHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "middle":
                                c.extentMiddleHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "bottom":
                                c.extentBottomHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "repeat":
                                c.extentRepeatHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "horizontal":
                                c.extensionHorizontal = int.Parse(attr[j].Value) > 0;
                                break;
                        }
                    }
                }
            }

            XmlNode CharMap = doc.SelectSingleNode("/TexSymbols/CharMap");
            foreach (XmlNode node in CharMap.ChildNodes) {
	            int hash = int.Parse(node.Attributes["hash"].Value);
	            if(!isMath && hash >> 8 < pref.header_mathCount)
		            continue;
	            hash = SyncHash(hash, sMap);
	            if (hash >= 0) {
                    c = pref.GetChar(hash); 
                    c.characterMap = int.Parse(node.Attributes["char"].Value);
         //           if (c.characterMap > 0)
       //                 pref.charMapData.Add(TexChar.possibleCharMaps[c.characterMap], c.ToHash());
                }
            }
            pref.PushToDictionaries();
        }
        
        public static void ReadLegacyPreferences(TEXPreference pref, string XMLFontDef, string XMLConfigDef)
        {
            //Clean up first
            pref.glueTable = new int[100];
//            pref.defaultTypefaces = new TexTypeFaceDictionary();

            //Look up all font & sprites
            XmlDocument doc = new XmlDocument();
            XmlNode cNode, configNodes;
            List<TEXConfigurationMember> configs = new List<TEXConfigurationMember>();

            doc.LoadXml(XMLFontDef);

            //Load all configurations. (the thing with bunch of sliders)
            configNodes = doc.SelectSingleNode("/TexFont/Params");
            for (int i = 0; i < configNodes.ChildNodes.Count; i++) {
                cNode = configNodes.ChildNodes[i];
                configs.Add(new TEXConfigurationMember(cNode.Attributes["name"].Value, cNode.Attributes["desc"].Value, 
                        float.Parse(cNode.Attributes["value"].Value), float.Parse(cNode.Attributes["min"].Value), float.Parse(cNode.Attributes["max"].Value)));
            }
            
            //pref.configs = configs.ToArray();
                
            doc.LoadXml(XMLConfigDef);

            // No longer importing config via XML
            /*XmlNode Params = doc.SelectSingleNode("/TEXConfigurations/Parameters");
            for (int i = 0; i < Params.ChildNodes.Count; i++) {
                var att = Params.ChildNodes[i].Attributes[0];
                pref.preferences[att.Name] = float.Parse(att.Value);
                pref.configs.First(x => x.name == att.Name).value = pref.preferences[att.Name];
            }*/

		  /*          XmlNode Typefaces = doc.SelectSingleNode("/TEXConfigurations/DefaultTypefaces");
            foreach (XmlNode node in Typefaces.ChildNodes) {
                pref.defaultTypefaces.Add((TexCharKind)int.Parse(node.Attributes["code"].Value), int.Parse(node.Attributes["fontId"].Value));
            }*/
      
            XmlNode GlueTables = doc.SelectSingleNode("/TexConfigurations/GlueTable");
            foreach (XmlNode node in GlueTables.ChildNodes) {
                pref.glueTable[int.Parse(node.Attributes["leftType"].Value) * 10 + int.Parse(node.Attributes["rightType"].Value)] 
			= int.Parse(node.Attributes["glueSize"].Value);
            }
            
            pref.PushToDictionaries();
            
        }



/*  OBSOLETED IMPORTER PROGRAM PRIOR TO 3.0

    #if UNITY_WEBPLAYER
            // In Webplayer, All I/O Process is prohibited... 
            // incuding these import/export processes

            public static void LoadPrimaryDefinitions(TEXPreference pref)
            {
                throw new System.Exception("Please switch your platform into PC first!");
            }
            
            public static void ReadSymbols(TEXPreference pref)
            {
                throw new System.Exception("Please switch your platform into PC first!");
            }
            
            public static void ReadPreferences(TEXPreference pref)
            {
                throw new System.Exception("Please switch your platform into PC first!");
            }
            
            public static void WriteSymbols(TEXPreference pref)
            {
                throw new System.Exception("Please switch your platform into PC first!");
            }
            
            public static void WritePreferences(TEXPreference pref)
            {
                throw new System.Exception("Please switch your platform into PC first!");
            }

    #else
            
            ///All importing scenario begin in here.
            public static void LoadPrimaryDefinitions(TEXPreference pref)
            {
                //Load all variables and XML Data
                EditorUtility.DisplayProgressBar("Reloading", "Reloading from XML Contents...", 0);
                XmlDocument doc = new XmlDocument();
                XmlNode cNode, configNodes;
                List<TexFont> Datas = new List<TexFont>();
                List<TEXConfigurationMember> configs = new List<TEXConfigurationMember>();

                //Look up all font & sprites
                doc.LoadXml(pref.XMLFontDefinitions.text);

                LoadPrimaryDefinitionSubset(pref, Datas, pref.MainFolderPath + "/Fonts/Math", "t:Font", TexFontType.Font, 0);
                pref.header_mathCount = Datas.Count;
                LoadPrimaryDefinitionSubset(pref, Datas, pref.MainFolderPath + "/Fonts/User", "t:Font", TexFontType.Font, 1);
                pref.header_userCount = Datas.Count;
                LoadPrimaryDefinitionSubset(pref, Datas, pref.MainFolderPath + "/Fonts/Sprites", "t:Sprite", TexFontType.Sprite, 2);
                EditorUtility.DisplayProgressBar("Reloading", "Preparing Stuff...", .93f);

                //Load all configurations. (the thing with bunch of sliders)
                configNodes = doc.SelectSingleNode("/TexFont/Params");
                for (int i = 0; i < configNodes.ChildNodes.Count; i++) {
                    cNode = configNodes.ChildNodes[i];
                    configs.Add(new TEXConfigurationMember(cNode.Attributes["name"].Value, cNode.Attributes["desc"].Value, 
                            float.Parse(cNode.Attributes["value"].Value), float.Parse(cNode.Attributes["min"].Value), float.Parse(cNode.Attributes["max"].Value)));
                }	

                //Push everything to pref
                pref.fontData = Datas.ToArray();
                pref.configs = configs.ToArray();
                pref.PushToDictionaries(true);
            }

            /// Part of PrimaryDefinition, just for the Don't-Repeat-Yourself thing ...
            static void LoadPrimaryDefinitionSubset(TEXPreference pref, List<TexFont> Datas, string folderPath, string typeStr, TexFontType typeEnum, int mode)
            {
                if (!AssetDatabase.IsValidFolder(folderPath))
                    return;
                string[] customF = AssetDatabase.FindAssets(typeStr, new string[] { folderPath });
                for (int i = 0; i < customF.Length; i++)
                {
                    if (Datas.Count >= 31)
                    {
                        Debug.LogWarning("Font/Sprite database count are beyond 31, ignoring any assets after " + Datas[Datas.Count - 1].id);
                        break;
                    }
                    string realPath = AssetDatabase.GUIDToAssetPath(customF[i]);
                    string id = Path.GetFileNameWithoutExtension(realPath).ToLower();
                    UpdateProgress(mode, id, i, customF.Length);
                    if (!Datas.Any(x => x.id == id))
                    {
    #if TEXDRAW_TMP
                        string sdfPath = pref.MainFolderPath + "/Fonts/TMPro/" + id + ".asset";
                        if (typeEnum == TexFontType.Font && AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(sdfPath))
                        {
                            TexFont f;
                            Datas.Add(f = new TexFont(id, Datas.Count, TexFontType.Font_SDF, sdfPath));
                            // Also keep the Font file
                            f.Font_Asset = AssetDatabase.LoadAssetAtPath<Font>(realPath);
                        }
                        else
    #endif
    
                        Datas.Add(new TexFont(id, Datas.Count, typeEnum, realPath));
                }
            }
        }

        static void UpdateProgress(int phase, string name, int idx, int total)
        {
            var prog = idx / (float)total;
            prog = phase * 0.3f + (prog * 0.3f);
            EditorUtility.DisplayProgressBar("Reloading", "Reading " + name + "...", prog);
        }


	    public static void ReadSymbols(TEXPreference pref, bool isMath)
        {
            XmlDocument doc = new XmlDocument();
	        doc.LoadXml(isMath ? pref.XMLMathDefinitions.text : pref.XMLSymbolDefinitions.text);
            TexChar c; 

	        XmlNode SyncMap = doc.SelectSingleNode("/TexSymbols/FontIDs");
	        List<int> sMap = new List<int>();
	        for (int i = 0; i < SyncMap.ChildNodes.Count; i++) {
                XmlAttributeCollection attr = SyncMap.ChildNodes[i].Attributes;
		        int idx = pref.GetFontIndexByID(attr["id"].Value);
		        var max = int.Parse(attr["index"].Value);
		        if(max > sMap.Count)
		        {
		        	for (int j = 0; j < max; j++) {
			        	sMap.Add(j);
		        	}
		        }
	            sMap.Add(idx);
                if (idx >= 0) {
                    var font = pref.fontData[idx];
                    bool catalogChange = false;
                    for (int j = 0; j < attr.Count; j++) {
                        switch (attr[j].Name) {
                            case "xLength":
                                font.sprite_xLength = int.Parse(attr[j].Value);
                                break;
                            case "yLength":
                                font.sprite_yLength = int.Parse(attr[j].Value);
                                break;
                            case "scale":
                                font.sprite_scale = float.Parse(attr[j].Value);
                                break;
                            case "lineOffset":
                                font.sprite_lineOffset = float.Parse(attr[j].Value);
                                break;
                            case "alphaOnly":
                                font.sprite_alphaOnly = int.Parse(attr[j].Value) > 0;
                                break;
                            case "importCatalog":
                                if (catalogChange |= !string.IsNullOrEmpty(attr[j].Value))
                                    font.importCatalog = attr[j].Value;
                                break;
                        }
                    }

                    font.importCatalog = font.importCatalog == null ? TexCharPresets.legacyChars : font.importCatalog;
                    font.Populate(false);
                }
            }

            XmlNode SymbolMap = doc.SelectSingleNode("/TexSymbols/SymbolMap");
	        foreach (XmlNode node in SymbolMap.ChildNodes) {
		        int hash = int.Parse(node.Attributes["hash"].Value);
		        if(!isMath && hash >> 8 < pref.header_mathCount)
			        continue;
		        hash = SyncHash(hash, sMap);
                if (hash >= 0) {
	                c = pref.GetChar(hash);
                    var attr = node.Attributes;
                    for (int j = 0; j < attr.Count; j++) {
                        switch (attr[j].Name) {
                            case "name":
                                c.symbolName = attr[j].Value;
                                break;
                            case "nameAlt":
                                c.symbolAlt = attr[j].Value;
                                break;
                            case "typeId":
                                c.type = (CharType)int.Parse(attr[j].Value);
                                break;
                        }
                    }

                    pref.symbolData.Add(c.symbolName, c.ToHash());
                    if (!string.IsNullOrEmpty(c.symbolAlt))
                        pref.symbolData.Add(c.symbolAlt, c.ToHash());
                }
            }

            XmlNode LargerMap = doc.SelectSingleNode("/TexSymbols/LargerMap");
            foreach (XmlNode node in LargerMap.ChildNodes) {
	            int hash = int.Parse(node.Attributes["hash"].Value);
	            if(!isMath && hash >> 8 < pref.header_mathCount)
		            continue;
	            hash = SyncHash(hash, sMap);
	            if (hash >= 0) {
                    c = pref.GetChar(hash);
                    c.nextLarger = pref.GetChar(SyncHash(int.Parse(node.Attributes["targetHash"].Value), sMap));
                }
            }

            XmlNode ExtensionMap = doc.SelectSingleNode("/TexSymbols/ExtensionMap");
            foreach (XmlNode node in ExtensionMap.ChildNodes) {
	            int hash = int.Parse(node.Attributes["hash"].Value);
	            if(!isMath && hash >> 8 < pref.header_mathCount)
		            continue;
	            hash = SyncHash(hash, sMap);
	            if (hash >= 0) {
                    c = pref.GetChar(hash);
                    c.extensionExist = true;
                    var attr = node.Attributes;
                    for (int j = 0; j < attr.Count; j++) {
                        switch (attr[j].Name) {
                            case "top":
                                c.extentTopHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "middle":
                                c.extentMiddleHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "bottom":
                                c.extentBottomHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "repeat":
                                c.extentRepeatHash = SyncHash(int.Parse(attr[j].Value), sMap);
                                break;
                            case "horizontal":
                                c.extensionHorizontal = int.Parse(attr[j].Value) > 0;
                                break;
                        }
                    }
                }
            }

            XmlNode CharMap = doc.SelectSingleNode("/TexSymbols/CharMap");
            foreach (XmlNode node in CharMap.ChildNodes) {
	            int hash = int.Parse(node.Attributes["hash"].Value);
	            if(!isMath && hash >> 8 < pref.header_mathCount)
		            continue;
	            hash = SyncHash(hash, sMap);
	            if (hash >= 0) {
                    c = pref.GetChar(hash); 
                    c.characterMap = int.Parse(node.Attributes["char"].Value);
                    if (c.characterMap > 0)
                        pref.charMapData.Add(TexChar.possibleCharMaps[c.characterMap], c.ToHash());
                }
            }
        }

        public static void ReadPreferences(TEXPreference pref)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(pref.XMLConfiguration.text);

            XmlNode Params = doc.SelectSingleNode("/TEXConfigurations/Parameters");
            for (int i = 0; i < Params.ChildNodes.Count; i++) {
                var att = Params.ChildNodes[i].Attributes[0];
                pref.preferences[att.Name] = float.Parse(att.Value);
                pref.configs.First(x => x.name == att.Name).value = pref.preferences[att.Name];
            }

            XmlNode Typefaces = doc.SelectSingleNode("/TEXConfigurations/DefaultTypefaces");
            foreach (XmlNode node in Typefaces.ChildNodes) {
                pref.defaultTypefaces.Add((TexCharKind)int.Parse(node.Attributes["code"].Value), int.Parse(node.Attributes["fontId"].Value));
            }
      
            XmlNode GlueTables = doc.SelectSingleNode("/TEXConfigurations/GlueTable");
            foreach (XmlNode node in GlueTables.ChildNodes) {
                pref.glueTable[int.Parse(node.Attributes["leftType"].Value) * 10 + int.Parse(node.Attributes["rightType"].Value)] 
			= int.Parse(node.Attributes["glueSize"].Value);
            }
        }

        const string symbolNotice = "AUTO-GENERATED: this file contains per-character configurations, do not modify directly unless you know what are you doing";

	    public static void WriteSymbols(TEXPreference pref, bool isMath)
        {
            StringBuilder syncMap = new StringBuilder();
            StringBuilder symbolMap = new StringBuilder();
            StringBuilder largerMap = new StringBuilder();
            StringBuilder extensionMap = new StringBuilder();
	        StringBuilder charMap = new StringBuilder();
	        var limit = pref.header_mathCount;
	        var maxPool = (isMath ? limit : pref.fontData.Length);
	        for (int i = isMath? 0 : limit ; i < maxPool; i++) {
                TexFont f = pref.fontData[i];
                if (f.type == TexFontType.Font)
                    syncMap.AppendFormat("     <Font index=\"{0}\" id=\"{1}\"/>\r\n", i, f.id);
                else
                    syncMap.AppendFormat("     <Font index=\"{0}\" id=\"{1}\" xLength=\"{2}\" yLength=\"{3}\" scale=\"{4}\" lineOffset=\"{5}\" detectAlpha=\"{6}\" alphaOnly=\"{7}\"/>\r\n"
                        , i, f.id, f.sprite_xLength, f.sprite_yLength, f.sprite_scale, f.sprite_lineOffset, f.sprite_detectAlpha ? 1 : 0, f.sprite_alphaOnly ? 1 : 0); 
            }
	        for (int i = isMath? 0 : limit ; i < maxPool; i++) {
		        for (int j = 0; j < 128; j++) {
                    TexChar c = pref.GetChar(i, j);
                    if (c.supported) {
                        int h = j | (i << 8);//c.ToHash();
                        if (!string.IsNullOrEmpty(c.symbolName))
                            symbolMap.AppendFormat("\t\t<C{0:X} hash=\"{0}\" name=\"{1}\" nameAlt=\"{2}\" typeId=\"{3}\"/>\n", h, c.symbolName, c.symbolAlt, (int)c.type);
                        if (c.nextLarger != null)
                            largerMap.AppendFormat("\t\t<C{0:X} hash=\"{0}\" targetHash=\"{1}\"/>\n", h, c.nextLarger.ToHash());
                        if (c.extensionExist)
                            extensionMap.AppendFormat("        <C{0:X} hash=\"{0}\" top=\"{1}\" middle=\"{2}\" bottom=\"{3}\" repeat=\"{4}\" horizontal=\"{5}\"/>\r\n", h, 
                                c.extentTopHash, c.extentMiddleHash, c.extentBottomHash, c.extentRepeatHash, c.extensionHorizontal ? 1 : 0);
                        if (c.characterMap > 0)
                            charMap.AppendFormat("\t\t<C{0:X} hash=\"{0}\" char=\"{1}\"/>\n", h, c.characterMap);
                    }
                }	
            }
            string outputString = string.Format("<?xml version='1.0'?>\r\n<!-- {0} -->\r\n<TexSymbols>\r\n    <FontIDs>\r\n{1} </FontIDs>\r\n    <SymbolMap>\r\n{2} </SymbolMap>\r\n    <LargerMap>\r\n{3} </LargerMap>\r\n    <ExtensionMap>\r\n{4} </ExtensionMap>\r\n    <CharMap>\r\n{5} </CharMap>\r\n</TexSymbols>", 
                                      symbolNotice, syncMap, symbolMap, largerMap, extensionMap, charMap);
            //Pro Tips: Use .NET Function to write XML Data
	        if(isMath)
		        File.WriteAllText(Application.dataPath + (pref.MainFolderPath + "/XMLs/TexMathDefinitions.xml").Substring(6), outputString);
	        else
	        File.WriteAllText(Application.dataPath + (pref.MainFolderPath + "/XMLs/TexSymbolDefinitions.xml").Substring(6), outputString);
        }

        const string preferenceNotice = "AUTO-GENERATED: this file contains global TEXDraw preferences, do not modify directly unless you know what are you doing";

        public static void WritePreferences(TEXPreference pref)
        {
            StringBuilder parameters = new StringBuilder();
            StringBuilder typefaces = new StringBuilder();
            StringBuilder glues = new StringBuilder();
            foreach (var dir in pref.preferences)
            {
                parameters.AppendFormat("\t\t<Param {0}=\"{1}\"/>\n",dir.Key, dir.Value);
            }
            for (int i = 0; i < pref.preferences.Count; i++)
            {
            }
            for (int i = 0; i < pref.defaultTypefaces.Count; i++)
            {
                typefaces.AppendFormat("\t\t<MapStyle code=\"{0}\" fontId=\"{1}\"/>\n", i, pref.defaultTypefaces[(TexCharKind)i]);
            }
            for (int i = 0; i < pref.glueTable.Length; i++)
            {
                if (pref.glueTable[i] > 0)
                    glues.AppendFormat("\t\t<Glue leftType=\"{0}\" rightType=\"{1}\" glueSize=\"{2}\"/>\n", i / 10, i % 10, pref.glueTable[i]);
            }
            string outputString = string.Format("<?xml version='1.0'?>\n<!-- {0} -->\n<TEXConfigurations>\n\t<Parameters>\n{1}\t</Parameters>\n\t<DefaultTypefaces>\n{2}\t</DefaultTypefaces>\n\t<GlueTable>\n{3}\t</GlueTable>\n</TEXConfigurations>",
                                      preferenceNotice, parameters, typefaces, glues);
            //Pro Tips: Use .NET Function to write XML Data
            File.WriteAllText(Application.dataPath + (pref.MainFolderPath + "/XMLs/TEXConfigurations.xml").Substring(6), outputString);
        }
        #endif

*/
    }
}
#endif