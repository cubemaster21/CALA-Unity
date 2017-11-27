using UnityEngine;
using System.Collections.Generic;
using System;
using TexDrawLib;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
[AddComponentMenu("TEXDraw/TEXDraw 3D", 2)]
public class TEXDraw3D : MonoBehaviour, ITEXDraw
{
    public TEXPreference pref;

    public TEXPreference preference { get { return pref; } }

    const string assetID = "TEXDraw 3D Instance";
    [TextArea(3, 15)]
    [SerializeField]
    string
        m_Text = "TEXDraw";

    public virtual string text
    {
        get { return m_Text; }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                Redraw();
            }
            else if (m_Text != value)
            {
                m_Text = value;
                Redraw();
            }
        }
    }

    [SerializeField]
    int m_FontIndex = -1;

    public virtual int fontIndex
    {
        get { return m_FontIndex; }
        set
        {
            if (m_FontIndex != value)
            {
                m_FontIndex = Mathf.Clamp(value, -1, 31);
                Redraw();
            }
        }
    }

    [SerializeField]
    float
        m_Size = 10f;

    public virtual float size
    {
        get { return m_Size; }
        set
        {
            if (m_Size != value)
            {
                m_Size = Mathf.Max(value, 0f);
                Redraw();
            }
        }
    }

    [Range(1, 200)]
    [SerializeField]
    int
        m_FontSize = 40;

    public virtual int fontSize
    {
        get { return m_FontSize; }
        set
        {
            if (m_FontSize != value)
            {
                m_FontSize = Mathf.Max(value, 1);
                Redraw();
            }
        }
    }

    [SerializeField]
    Color
        m_Color = Color.white;

    public virtual Color color
    {
        get { return m_Color; }
        set
        {
            if (m_Color != value)
            {
                m_Color = value;
                Redraw();
            }
        }
    }

    [SerializeField]
    Material
        m_Material;

    public virtual Material material
    {
        get { return m_Material; }
        set
        {
            if (m_Material != value)
            {
                m_Material = value;
                Repaint();
            }
        }
    }

    public virtual Fitting autoFit
    {
        get { return m_AutoFit; }
        set
        {
            if (m_AutoFit != value)
            {
                m_AutoFit = value;
                Redraw();
            }
        }
    }

    public virtual Wrapping autoWrap
    {
        get { return m_AutoWrap; }
        set
        {
            if (m_AutoWrap != value)
            {
                m_AutoWrap = value;
                Redraw();
            }
        }
    }

    public virtual Filling autoFill
    {
        get { return m_AutoFill; }
        set
        {
            if (m_AutoFill != value)
            {
                m_AutoFill = value;
                Redraw();
            }
        }
    }


    [SerializeField]
    [Range(0, 2)]
    float m_SpaceSize = 0.2f;

    public virtual float spaceSize
    {
        get { return m_SpaceSize; }
        set
        {
            if (m_SpaceSize != value)
            {
                m_SpaceSize = value;
                Redraw();
            }
        }
    }

    [SerializeField]
    Vector2
        m_Align = new Vector2(0.5f, 0.5f);

    public virtual Vector2 alignment
    {
        get { return m_Align; }
        set
        {
            if (m_Align != value)
            {
                m_Align = value;
                Redraw();
            }
        }
    }

    [SerializeField]
    Fitting
        m_AutoFit = Fitting.DownScale;

    [SerializeField]
    Wrapping
        m_AutoWrap = Wrapping.NoWrap;

    [SerializeField]
    Filling
        m_AutoFill = Filling.None;




    public string debugReport = String.Empty;

#if UNITY_EDITOR
    void Reset()
    {
        pref = TEXPreference.main;
        GetComponent<MeshRenderer>().material = pref.defaultMaterial;
    }
    [ContextMenu("Open Preference")]    
    public void OpenPreference()
    {
        UnityEditor.Selection.activeObject = preference;
    }
    [ContextMenu("Repick Preference Asset")]
    public void PickPreferenceAsset()
    {
        pref = TEXPreference.main;
    }

    void OnEnable()
    {
        if (!pref)
        {
            pref = TEXPreference.main;
            if (!pref)
                Debug.LogWarning("A TEXDraw 3D Component hasn't the preference yet");
        }

        Font.textureRebuilt += OnFontRebuild;
        if (!mesh)
        {
            mesh = new Mesh();
            mesh.name = assetID;
        }
        Redraw();
        Repaint();
    }
#else
	void OnEnable()
	{
		if(!TEXPreference.main)
			TEXPreference.main = pref; //Assign the Preference to main stack
		else if(!pref)
			pref = TEXPreference.main; //This component may added runtimely
        Font.textureRebuilt += OnFontRebuild;
    	if(!mesh)
    	{
	        mesh=new Mesh();
        	mesh.name=assetID;
	    }
        Redraw();
        Repaint();
    }
#endif

    #region Engine

    DrawingContext m_drawingContext;

    public DrawingContext drawingContext
    {
        get
        {
            if (m_drawingContext == null)
                m_drawingContext = new DrawingContext(this);
            return m_drawingContext;
        }
    }

    DrawingParams cacheParam;

    public DrawingParams drawingParams
    {
        get
        {
            if (cacheParam == null)
                (cacheParam = new DrawingParams()).hasRect = false;
            return cacheParam;
        }
    }

    Mesh mesh;

    public void SetTextDirty()
    {
        Redraw();
    }

    public void SetTextDirty(bool now)
    {
        Redraw();
    }
    
    void LateUpdate ()
    {
        if (drawingParams.hasRect && rectTransform.hasChanged)
            Redraw();
    }
    bool _onRendering = false;
    public void Redraw()
    {
        // Multi-threading issue with OnFontRebuilt
        if (_onRendering)
            return;
        if (isActiveAndEnabled)
        {
            _onRendering = true;
            if (drawingContext == null || pref == null)
                OnEnable();
            try
            {
#if UNITY_EDITOR
                if (pref.editorReloading)
                {
                    _onRendering = false;
                    return;
                }
#endif
                if (supplementIsDirty)
                    UpdateSupplements();
                drawingContext.Parse(PerformSupplements(m_Text), out debugReport);
                GenerateParam();
                drawingContext.BoxPacking(cacheParam);
                drawingContext.Render(mesh, cacheParam);
                PerformMeshEffect(mesh);
                PerformAutoLayout();
                mesh.RecalculateBounds();
                GetComponent<MeshFilter>().mesh = mesh;
                _onRendering = false;
            }
            catch (System.Exception)
            {
                _onRendering = false;
            }
        }
    }

    public void Repaint()
    {
        if (!m_Material)
            GetComponent<MeshRenderer>().material = pref.defaultMaterial;
        else
            GetComponent<MeshRenderer>().material = m_Material;
    }
    
    void PerformAutoLayout ()
    {
        if (!cacheParam.hasRect)
            return;
        if (cacheParam.autoFit == Fitting.RectSize)
            rectTransform.sizeDelta = cacheParam.layoutSize;
        else if (cacheParam.autoFit == Fitting.HeightOnly)
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, cacheParam.layoutSize.y);
        else 
            return;
        rectTransform.hasChanged = false;
    }

    RectTransform _cachedRectTransform;
    public RectTransform rectTransform
    {
        get
        {
            return _cachedRectTransform ? _cachedRectTransform : (_cachedRectTransform = GetComponent<RectTransform>());
        }
    }
    
    public void GenerateParam()
    {
        if (cacheParam == null)
        {
            cacheParam = new DrawingParams();
            cacheParam.hasRect = false;
            cacheParam.rectArea = new Rect();
        }
        cacheParam.hasRect = rectTransform;
        cacheParam.rectArea = cacheParam.hasRect ? rectTransform.rect : new Rect();
        cacheParam.pivot = cacheParam.hasRect ? rectTransform.pivot : Vector2.zero;
        cacheParam.alignment = m_Align;
        cacheParam.color = color;
        cacheParam.fontSize = m_FontSize;
        cacheParam.fontIndex = m_FontIndex;
        cacheParam.scale = m_Size;
        cacheParam.spaceSize = m_SpaceSize;
        cacheParam.autoFit = m_AutoFit;
        cacheParam.autoWrap = cacheParam.hasRect && m_AutoFit == Fitting.RectSize ? Wrapping.NoWrap : m_AutoWrap;
        cacheParam.autoFill = m_AutoFill;
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(mesh);
        }
        else
        {
#endif
            Destroy(mesh);
#if UNITY_EDITOR
        }
#endif
    }

    void OnDisable()
    {
        Font.textureRebuilt -= OnFontRebuild;
    }

    void OnFontRebuild(Font f)
    {
        Redraw();
    }

    #endregion

    #region Supplements

    [NonSerialized]
    List<TEXDrawSupplementBase> supplements = new List<TEXDrawSupplementBase>();
    bool supplementIsDirty = false;

    [NonSerialized]
    List<BaseMeshEffect> postEffects = new List<BaseMeshEffect>();
    
    public void SetSupplementDirty()
    {
        supplementIsDirty = true;
        SetTextDirty();
    }

    void UpdateSupplements()
    {
        GetComponents<TEXDrawSupplementBase>(supplements);
        GetComponents<BaseMeshEffect>(postEffects);

        supplementIsDirty = false;
    }

    string PerformSupplements(string original)
    {
        if (supplements == null)
            return original;
        for (int i = 0; i < supplements.Count; i++)
        {
            if (supplements[i] && supplements[i].enabled)
                original = supplements[i].ReplaceString(original);
        }
        return original;
    }

    void PerformMeshEffect(Mesh m)
    {
        if (postEffects.Count == 0)
            return;
        #if UNITY_EDITOR
        if (!Application.isPlaying)
            GetComponents<BaseMeshEffect>(postEffects);
        #endif
        for (int i = 0; i < postEffects.Count; i++)
        {
            if (postEffects[i] && postEffects[i].enabled)
                postEffects[i].ModifyMesh(m);
        }
    }

    #endregion

}

