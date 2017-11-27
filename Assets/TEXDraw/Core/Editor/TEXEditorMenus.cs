using UnityEngine;
using UnityEditor;
using TexDrawLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[InitializeOnLoadAttribute()]
public static class TEXEditorMenus
{

    [MenuItem("Tools/TEXDraw/Show Preference",false,5)]
    public static void OpenPreference()
    {
        Selection.activeObject = TEXPreference.main;
    }
    
    [MenuItem("Tools/TEXDraw/Rebuild Font Data",false,6)]
    public static void ImportPreference()
    {
        TEXPreference.main.Reload();
    }
     
    [MenuItem("Tools/TEXDraw/Rebuild Material",false,7)]
    public static void ImportMaterial()
    {
        TEXPreference.main.RebuildMaterial();
    }
    
    [MenuItem("Tools/TEXDraw/Rebuild TEXDraw on Scene %&R",false,7)]
    public static void RepaintTEXDraws()
    {
        TEXPreference.main.CallRedraw();
    }
    
    [MenuItem("Tools/TEXDraw/Select Default Material",false,7)]
    public static void SelectDefaultMaterial()
    {
         Selection.activeObject = TEXPreference.main.defaultMaterial;
    }
    
    [MenuItem("Tools/TEXDraw/Pool Checks", false, 20)]
    public static void ShowPoolCheck()
    {
        var w = ScriptableObject.CreateInstance<EditorObjectPool>();
        w.Show();
    }
    
    [MenuItem("Tools/TEXDraw/Font Swapper Tool", false, 20)]
    public static void ShowFontSwapper()
    {
        var w = ScriptableObject.CreateInstance<EditorTEXFontSwapper>();
        w.Show();
    }
    
    [MenuItem("Tools/TEXDraw/Quick Editor Tool", false, 20)]
    public static void ShowQuickEditor()
    {
        var w = ScriptableObject.CreateInstance<EditorTEXUIQuickEditors>();
        w.Show();
    }
    
    const string lorem1K = @"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero. 
Sit amet commodo magna eros quis urna. Nunc viverra imperdiet enim. Fusce est. Vivamus a tellus. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. 
Proin pharetra nonummy pede. Mauris et orci. Aenean nec lorem. In porttitor. Donec laoreet nonummy augue. Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. 
Mauris eget neque at sem venenatis eleifend. Ut nonummy. Fusce aliquet pede non pede. Suspendisse dapibus lorem pellentesque magna. Integer nulla. Donec blandit feugiat ligula. Donec hendrerit. 
Felis et imperdiet euismod, purus ipsum pretium metus, in lacinia nulla nisl eget sapien. Donec ut est in lectus consequat consequat. Etiam eget dui. Aliquam erat volutpat. Sed at lorem in nunc porta tristique.";

    [MenuItem("Tools/TEXDraw/Fill with Lorem Ipsum",false, 60)]
    public static void FillLoremIpsum()
    {
        var sels = Selection.gameObjects;
        foreach (var obj in sels)
        {
            var tex = GetTexDraw(obj);
            if (tex != null) {
                Undo.RecordObject((Object)tex, "Change TexDraw texts");
                tex.text = lorem1K;
                if (tex.autoWrap == Wrapping.NoWrap)
                    tex.autoWrap = Wrapping.WordWrap;
                tex.autoFit = Fitting.BestFit;
                EditorUtility.SetDirty((Object)tex);
            }
        }
    }
    
    static readonly string[] progressContents = new string [] {
        "Measuring Supplement Performance ..",
        "Measuring Parsing Performance ..",
        "Measuring Boxing Performance ..",
        "Measuring Rendering Performance ..",
        "Measuring Post-Effect Performance .."
    };

    static readonly string[] logContents = new string[] {
        "Supplement-ing {0} times in 5 seconds, average {1:F2} ms, normally makeup {2:P2} (of 60 FPS) CPU Performance",
        "Parsing {0} times in 5 seconds, average {1:F2} ms, normally makeup {2:P2} (of 60 FPS) CPU Performance",
        "Boxing {0} times in 5 seconds, average {1:F2} ms, normally makeup {2:P2} (of 60 FPS) CPU Performance",
        "Rendering {0} times in 5 seconds, average {1:F2} ms, normally makeup {2:P2} (of 60 FPS) CPU Performance",
        "Post-Effect-ing {0} times in 5 seconds, average {1:F2} ms, normally makeup {2:P2} (of 60 FPS) CPU Performance"
    };


    [MenuItem("Tools/TEXDraw/Benchmark for 15+ Seconds",false, 60)]
    public static void Benchmark ()
    {
        var tex = GetTexDraw(Selection.activeGameObject);
        if (tex == null)
            return;
        try{
            // Init
            var title = "Benchmarking " + Selection.activeGameObject.name;

            int supplementCount = 0, parseCount = 0, boxCount = 0, renderCount = 0, postEffectCount = 0;

            var supplements = Selection.activeGameObject.GetComponents<TEXDrawSupplementBase>().Where(x => x.enabled).ToArray();
            var postEffects = Selection.activeGameObject.GetComponents<BaseMeshEffect>().Where(x => x.enabled).ToArray();
            var text = tex.text;
            var parser = tex.drawingContext;
            var mesh = new Mesh();
            var param = tex.drawingParams;
            //Supplement
            if (supplements.Length > 0) {
                for (int i = 0; i++ < 10;)
                {
                    var now = EditorApplication.timeSinceStartup;
                    do
                    {
                        PerformSupplements(supplements, text);
                        supplementCount++;
                    } while ((EditorApplication.timeSinceStartup - now) < 0.5);
                    EditorUtility.DisplayProgressBar(title, progressContents[0], i / 50f + 0f);
                }
                Debug.LogFormat(logContents[0], supplementCount, 5000.0 / supplementCount, 5.0 * 60.0 / supplementCount);
            }
            //Parsing
            {
                for (int i = 0; i++ < 10;)
                {
                    var now = EditorApplication.timeSinceStartup;
                    do
                    {
                        parser.Parse(text);
                        parseCount++;
                    } while ((EditorApplication.timeSinceStartup - now) < 0.5);
                    EditorUtility.DisplayProgressBar(title, progressContents[1], i / 50f + .2f);
                }
                Debug.LogFormat(logContents[1], parseCount, 5000.0 / parseCount, 5.0 * 60.0 / parseCount);
            }
            //Boxing
            {
                for (int i = 0; i++ < 10;)
                {
                    var now = EditorApplication.timeSinceStartup;
                    do
                    {
                        param.formulas = DrawingContext.ToRenderers(parser.parsed, param);
                        boxCount++;
                    } while ((EditorApplication.timeSinceStartup - now) < 0.5);
                    EditorUtility.DisplayProgressBar(title, progressContents[2], i / 50f + .4f);
                }
                Debug.LogFormat(logContents[2], boxCount, 5000.0 / boxCount, 5.0 * 60.0 / boxCount);
            }
            //Rendering
            {
                for (int i = 0; i++ < 10;)
                {
                    var now = EditorApplication.timeSinceStartup;
                    do
                    {
                        parser.Render(mesh, param);
                        renderCount++;
                    } while ((EditorApplication.timeSinceStartup - now) < 0.5);
                    EditorUtility.DisplayProgressBar(title, progressContents[3], i / 50f + .6f);
                }
                Debug.LogFormat(logContents[3], renderCount, 5000.0 / renderCount, 5.0 * 60.0 / renderCount);
            }
            //Post-Effect
            if (postEffects.Length > 0){
                for (int i = 0; i++ < 10;)
                {
                    var now = EditorApplication.timeSinceStartup;
                    do
                    {
                        PerformPostEffects(postEffects, mesh);
                        postEffectCount++;
                    } while ((EditorApplication.timeSinceStartup - now) < 0.5);
                    EditorUtility.DisplayProgressBar(title, progressContents[4], i / 50f + .8f);
                }
                Debug.LogFormat(logContents[4], postEffectCount, 5000.0 / postEffectCount, 5.0 * 60.0 / postEffectCount);
            }
            Object.DestroyImmediate(mesh);

            var totalTime = (supplementCount > 0 ? 5.0 / supplementCount : 0.0) + (postEffectCount > 0 ? 5.0 / postEffectCount : 0.0) +
                 5.0 / parseCount + 5.0 / boxCount + 5.0 / renderCount; // in seconds
            if (totalTime > 1 / 60.0)
                Debug.LogFormat("Complete build time takes <b>{0:F2}</b> ms, makeup <b>{1:P2}</b> of CPU Time at 60 FPS. <color={2}><b><i>Potentially Breakup Game Performance</i></b></color>", totalTime * 1000.0, totalTime * 60.0, EditorGUIUtility.isProSkin ? "yellow" : "brown");
            else
                Debug.LogFormat("Complete build time takes <b>{0:F2}</b> ms, makeup <b>{1:P2}</b> of CPU Time at 60 FPS, or up to <b>{2}</b> build times at 60 FPS", totalTime * 1000.0, totalTime * 60.0, (int)(1.0 / (totalTime * 60.0)));

        }
        catch {}
        
        
        EditorUtility.ClearProgressBar();
    }
    
    static string PerformSupplements(TEXDrawSupplementBase[] supplements, string original)
    {
        if (supplements == null)
            return original;
        TEXDrawSupplementBase s;
        for (int i = 0; i < supplements.Length; i++) 
            if ((s = supplements[i]) && s.enabled)
                original = s.ReplaceString(original);
        return original;
    }
    
    static void PerformPostEffects(BaseMeshEffect[] postEffects, Mesh m)
    {
        if (postEffects == null)
            return;
        BaseMeshEffect p;
        for (int i = 0; i < postEffects.Length; i++) 
            if ((p = postEffects[i]) && p.enabled)
                p.ModifyMesh(m);
    }
    
    [MenuItem("Tools/TEXDraw/Benchmark for 15+ Seconds", true, 20)]
    [MenuItem("Tools/TEXDraw/Fill with Lorem Ipsum", true, 20)]
    public static bool CanBenchmark ()
    {
        return GetTexDraw(Selection.activeGameObject) != null;
    }

    public static ITEXDraw GetTexDraw(GameObject obj)
    {
        if (!obj)
            return null;
        ITEXDraw target = null;
        List<MonoBehaviour> l = ListPool<MonoBehaviour>.Get();
        obj.GetComponents<MonoBehaviour>(l);
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i] is ITEXDraw)
            {
                target = (ITEXDraw)l[i];
                break;
            }
        }
       ListPool<MonoBehaviour>.ReleaseNoFlush(l);
       return target;
    }
    
        
    // -----------------------------------------------------------------------------------------------------
    
    [MenuItem("Tools/TEXDraw/Show Info on Supplements", false, 60)]
    public static void ToggleAdditionalInfo()
    {
        var v = TEXSupplementEditor.isHelpShown = !TEXSupplementEditor.isHelpShown;
        EditorPrefs.SetBool("TEXDraw_ShowTipOnSupplement", v);
        if (Selection.activeGameObject)
            EditorUtility.SetDirty(Selection.activeGameObject);
    }

    [MenuItem("Tools/TEXDraw/Show Info on Supplements", true, 60)]
    public static bool InvalidateToggleAdditionalInfo()
    {
        Menu.SetChecked("Tools/TEXDraw/Show Info on Supplements", TEXSupplementEditor.isHelpShown);
        return true;
    }
    
    // -----------------------------------------------------------------------------------------------------

    static TEXEditorMenus () {
        TEXSupplementEditor.isHelpShown = EditorPrefs.GetBool("TEXDraw_ShowTipOnSupplement", true);
    }
    
    [MenuItem("Tools/TEXDraw/Set Selected as Template", false, 80)]
    public static void SetDefaultTemplate () {
        var obj = Selection.activeGameObject;
        var tex = GetTexDraw(obj);
        var name = (tex.GetType().Name);
        var respond = EditorUtility.DisplayDialogComplex("Confirm set as Template",
            string.Format("Do you want to set selected {0} as a template every time you create a new {0} object?" +
        "\n(This includes its components and values expect the text itself)\n(This template will only affect project-wide environment)", name)
        , "Yes", "Yes, but keep the text", "Cancel");
        if (respond == 2)
            return;
        var path = TEXPreference.main.MainFolderPath + "/Template-" + name + ".prefab";
        obj = GameObject.Instantiate(obj);
        if (respond == 0)
            GetTexDraw(obj).text = "TEXDraw";
        PrefabUtility.CreatePrefab(path, obj, ReplacePrefabOptions.Default);
        GameObject.DestroyImmediate(obj, false);
    }

    [MenuItem("Tools/TEXDraw/Set Selected as Template", true, 80)]
    public static bool ValidateDefaultTemplate()
    {
        return GetTexDraw(Selection.activeGameObject) != null;
    }
    
    [MenuItem("Tools/TEXDraw/Clear Template", false, 80)]
    public static void ClearDefaultTemplates () {
        if (!EditorUtility.DisplayDialog("Confirm clear Template",
        "This action will clear every TEXDraw template you made in this project.\n(You actually can do this manually via the root of TEXDraw folder)\n(Can't be undone)", "OK", "Cancel"))
            return;
        var files = AssetDatabase.FindAssets("Template-", new string[]{ TEXPreference.main.MainFolderPath});
        foreach (var guid in files)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
    
    [MenuItem("Tools/TEXDraw/Clear Template", true, 80)]
    public static bool ValidateTemplateExist()
    {
       var files = AssetDatabase.FindAssets("Template-", new string[]{ TEXPreference.main.MainFolderPath});
        return files.Length > 0;
    }

    // -----------------------------------------------------------------------------------------------------

    [MenuItem("Tools/TEXDraw/Enable NGUI Integration", false, 100)]
    public static void SetNGUIIntegration()
    {
        EditorUtility.DisplayDialog("For your Information", "Enabling integration with NGUI only requires you to Import the NGUI extension in TEXDraw/GuideAndExtras." +
        "\nPlease ensure that you using latest Unity Version and have latest working version of NGUI imported in your project. The known working version of NGUI with TEXDraw is 3.11.3.", "OK");
    }

    [MenuItem("Tools/TEXDraw/Enable TextMeshPro Integration", false, 100)]
    public static void SetTextMeshProIntegration() {
       if(IsDefined("TEXDRAW_TMP")) {
            var respond = EditorUtility.DisplayDialogComplex("Confirmation to remove TEXDRAW_TMP script definition",
                 "Do you want to disable TEXDraw Integration with TextMeshPro?\nSaved TMP Asset will be ignored in case of you change your mind again.\nPlease rebuid font data after script recompile.",
                 "OK", "Disable for all platforms too", "Cancel");
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_TMP", false, respond == 1);
        } else {
            var respond = EditorUtility.DisplayDialogComplex("Humanity Check", "Do you already have imported TextMeshPro in your project?",
                "Yes", "Visit Asset Store", "Cancel");
            if (respond == 1)
                Help.BrowseURL("https://www.assetstore.unity3d.com/en/#!/content/84126");
            if (respond != 0)
                return;
            
            respond = EditorUtility.DisplayDialogComplex("Confirmation to add TEXDRAW_TMP script definition",
                 "Do you want to enable TEXDraw Integration with TextMeshPro?\nPlease ensure that (you can do these later):" + 
                 "- Generate TMP assets in Import section of TEXDraw Preference (later)\n- Import the TEXDraw-TMP Shader extension\n- Set Default Material shader to TEXDraw/TextMeshPro.\n\nSee the Doc for more info. This action will only add scripting symbol for current platform",
                 "OK", "Enable for all platforms too", "Cancel");            
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_TMP", true, respond == 1);
        }
    }
    
    [MenuItem("Tools/TEXDraw/Enable TextMeshPro Integration", true, 100)]
    static bool ValidateTextMeshProIntegration() {
        Menu.SetChecked("Tools/TEXDraw/Enable TextMeshPro Integration", IsDefined("TEXDRAW_TMP"));
        return true;
    }
    
    [MenuItem("Tools/TEXDraw/RTL Extras/Enable ReversedWrapping Integration", false, 100)]
    public static void SetReversedWrappingIntegration() {
       if(IsDefined("TEXDRAW_REVERSEDWRAPPING")) {
            var respond = EditorUtility.DisplayDialogComplex("Confirmation to remove TEXDRAW_REVERSEDWRAPPING script definition",
                 "Do you want to disable TEXDraw Integration with ReversedWrapping?\nDon't do this unless you are really sure not use it elsewhere",
                 "OK", "Disable for all platforms too", "Cancel");
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_REVERSEDWRAPPING", false, respond == 1);
        } else {         
            var respond = EditorUtility.DisplayDialogComplex("Confirmation to add TEXDRAW_REVERSEDWRAPPING script definition",
                 "Do you want to enable TEXDraw Integration with ReversedWrapping?\n" + 
                 "This action will reveal hidden wrapping options in TEXDraw components which helpful to solve the wrapping problem in displaying RTL paragraphs.\n" + 
                 "This action will only add scripting symbol for current platform.",
                 "OK", "Enable for all platforms too", "Cancel");            
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_REVERSEDWRAPPING", true, respond == 1);
        }
    }
    
    [MenuItem("Tools/TEXDraw/RTL Extras/Enable ArabicSupport Integration", false, 100)]
    public static void SetArabicSupportIntegration() {
       if(IsDefined("TEXDRAW_ARABICSUPPORT")) {
            var respond = EditorUtility.DisplayDialogComplex("Confirmation to remove TEXDRAW_ARABICSUPPORT script definition",
                 "Do you want to disable TEXDraw Integration with ArabicSupport?\nDon't do this unless you are really sure not use it elsewhere",
                 "OK", "Disable for all platforms too", "Cancel");
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_ARABICSUPPORT", false, respond == 1);
        } else {
            var respond = EditorUtility.DisplayDialogComplex("Humanity Check", "Do you already have imported ArabicSupport in your project?",
                "Yes", "Visit GitHub to download", "Cancel");
            if (respond == 1)
                Help.BrowseURL("https://github.com/Konash/arabic-support-unity/");
            if (respond != 0) 
                return;
            
            respond = EditorUtility.DisplayDialogComplex("Confirmation to add TEXDRAW_ARABICSUPPORT script definition",
                 "Do you want to enable TEXDraw Integration with ArabicSupport?\n" + 
                 "This action will only add scripting symbol for current platform",
                 "OK", "Enable for all platforms too", "Cancel");            
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_ARABICSUPPORT", true, respond == 1);
        }
    }
    
    [MenuItem("Tools/TEXDraw/RTL Extras/Enable PersianFixer Integration", false, 100)]
    public static void SetPersianFixerIntegration() {
       if(IsDefined("TEXDRAW_PERSIANFIXER")) {
            var respond = EditorUtility.DisplayDialogComplex("Confirmation to remove TEXDRAW_PERSIANFIXER script definition",
                 "Do you want to disable TEXDraw Integration with PersianFixer?\nDon't do this unless you are really sure not use it elsewhere",
                 "OK", "Disable for all platforms too", "Cancel");
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_PERSIANFIXER", false, respond == 1);
        } else {
            var respond = EditorUtility.DisplayDialogComplex("Humanity Check", "Do you already have imported PersianFixer in your project?",
                "Yes", "Visit GitHub to download", "Cancel");
            if (respond == 1)
				Help.BrowseURL("https://github.com/HamidMoghaddam/unitypersiansupport/");
            if (respond != 0) 
                return;
            
            respond = EditorUtility.DisplayDialogComplex("Confirmation to add TEXDRAW_PERSIANFIXER script definition",
                 "Do you want to enable TEXDraw Integration with Persian Fixer?\n" + 
                 "This action will only add scripting symbol for current platform",
                 "OK", "Enable for all platforms too", "Cancel");            
            if (respond == 2)
                return;
            SetDefined("TEXDRAW_PERSIANFIXER", true, respond == 1);
        }
    }
    
    [MenuItem("Tools/TEXDraw/RTL Extras/Enable ReversedWrapping Integration", true, 100)]
    static bool ValidateReversedWrappingIntegration() {
        Menu.SetChecked("Tools/TEXDraw/RTL Extras/Enable ReversedWrapping Integration", IsDefined("TEXDRAW_REVERSEDWRAPPING"));
        return true;
    }
    
    [MenuItem("Tools/TEXDraw/RTL Extras/Enable ArabicSupport Integration", true, 100)]
    static bool ValidateArabicSupportIntegration() {
        Menu.SetChecked("Tools/TEXDraw/RTL Extras/Enable ArabicSupport Integration", IsDefined("TEXDRAW_ARABICSUPPORT"));
        return true;
    }
    
    [MenuItem("Tools/TEXDraw/RTL Extras/Enable PersianFixer Integration", true, 100)]
    static bool ValidatePersianFixerIntegration() {
        Menu.SetChecked("Tools/TEXDraw/RTL Extras/Enable PersianFixer Integration", IsDefined("TEXDRAW_PERSIANFIXER"));
        return true;
    }

    
    private static void SetDefined(string defineName, bool enable, bool forAll)
    {
        if (!forAll)
        {
            SetDefined(defineName, enable, EditorUserBuildSettings.selectedBuildTargetGroup);
            return;
        }
        foreach (var group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            SetDefined(defineName, enable, (BuildTargetGroup)group);
        }
    }
    
    public static bool IsDefined(string defineName)
    {
        var defines = GetDefinesList();
        return defines.Contains(defineName);
    }
    
    public static void SetDefined(string defineName, bool enable, BuildTargetGroup group)
    {

        //Debug.Log("setting "+defineName+" to "+enable);
        
        var defines = GetDefinesList(group);
        if (enable)
        {
            if (defines.Contains(defineName))
            {
                return;
            }
            defines.Add(defineName);
        }
        else
        {
            if (!defines.Contains(defineName))
            {
                return;
            }
            while (defines.Contains(defineName))
            {
                defines.Remove(defineName);
            }
        }
        string definesString = string.Join(";", defines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
    }


    public static List<string> GetDefinesList()
    {
        return GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
    }

    public static List<string> GetDefinesList(BuildTargetGroup group)
    {
        return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
    }


}