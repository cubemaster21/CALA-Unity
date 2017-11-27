
using UnityEngine;
using UnityEngine.UI;
using System;
using TexDrawLib;
using System.Collections.Generic;

[AddComponentMenu("TEXDraw/TEXDraw UI", 1)]
public class TEXDraw : MaskableGraphic, ITEXDraw, ILayoutElement, ILayoutSelfController
{
    [SerializeField] TEXPreference pref;
    
    public TEXPreference preference { get { return pref; } }

    private DrivenRectTransformTracker layoutTracker;

    public string debugReport = string.Empty;

    [NonSerialized]     bool            m_TextDirty     = true;    
    [NonSerialized]     bool            m_BoxDirty      = false;
    [SerializeField]    string          m_Text          = "TEXDraw";
    [SerializeField]    int             m_FontIndex     = -1;
    [SerializeField]    float           m_Size          = 50f;
    [SerializeField]    Fitting         m_AutoFit       = Fitting.DownScale;
    [SerializeField]    Wrapping        m_AutoWrap      = 0;
    [SerializeField]    Filling         m_AutoFill      = 0;
    [SerializeField, Range(0, 2)] float m_SpaceSize     = 0.2f;
    [SerializeField]    Vector2         m_Align         = new Vector2(0.5f, 0.5f);

    public virtual string text
    {
        get { return m_Text; }
        set {
            if (m_Text != value) {
                m_Text = value;
                SetTextDirty(true);
            }
        }
    }


    public virtual int fontIndex
    {
        get { return m_FontIndex; }
        set {
            if (m_FontIndex != value) {
                m_FontIndex = Mathf.Clamp(value, -1, 31);
                SetTextDirty(true);
            }
        }
    }

    public virtual float size
    {
        get { return m_Size; }
        set {
            if (m_Size != value) {
                m_Size = Mathf.Max(value, 0f);
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    public virtual Fitting autoFit
    {
        get { return m_AutoFit; }
        set {
            if (m_AutoFit != value) {
                layoutTracker.Clear();
                m_AutoFit = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }
 
    public virtual Wrapping autoWrap
    {
        get { return m_AutoWrap; }
        set {
            if (m_AutoWrap != value) {
                m_AutoWrap = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    public virtual float spaceSize
    {
        get { return m_SpaceSize; }
        set {
            if (m_SpaceSize != value) {
                m_SpaceSize = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    public virtual Filling autoFill
    {
        get { return m_AutoFill; }
        set {
            if (m_AutoFill != value) {
                m_AutoFill = value;
                SetVerticesDirty();
            }
        }
    }

    public virtual Vector2 alignment
    {
        get { return m_Align; }
        set {
            if (m_Align != value) {
                m_Align = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    #if UNITY_EDITOR
    protected override void Reset()
    {
        pref = TEXPreference.main;
    }

    [ContextMenu("Repick Preference Asset")]
    public void PickPreferenceAsset()
    {
        pref = TEXPreference.main;
    }

    [ContextMenu("Open Preference")]
    public void OpenPreference()
    {
        UnityEditor.Selection.activeObject = pref;   
    }
    #endif

    protected override void OnEnable()
    {
        base.OnEnable();
        m_TextDirty = true;
        #if UNITY_EDITOR
        if (!pref) {
            pref = TEXPreference.main;
            if (!pref)
                Debug.LogWarning("A TEXDraw Component hasn't the preference yet");
        }
        #else
        if(!TEXPreference.main)
            TEXPreference.main = pref; //Assign the Preference to main stack
        else if(!pref)
            pref = TEXPreference.main; //This component may added runtimely
        #endif

        UpdateSupplements();
        Font.textureRebuilt += TextureRebuilted;

        #if UNITY_5_6_OR_NEWER
        if (canvas)
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent | AdditionalCanvasShaderChannels.TexCoord1;
        #endif
    }

    protected override void OnDisable()
    {
        Font.textureRebuilt -= TextureRebuilted;
        base.OnDisable();
        layoutTracker.Clear();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        #if UNITY_5_6_OR_NEWER
        if (canvas)
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent | AdditionalCanvasShaderChannels.TexCoord1;
        #endif
    }

    void TextureRebuilted(Font obj)
    {
        Invoke("SetVerticesDirty", 0);
    }

    #region Engine

    DrawingContext m_cachedDrawing;
    
    DrawingParams cacheParam;

    public DrawingContext drawingContext
    {
        get {
            if (m_cachedDrawing == null)
                m_cachedDrawing = new DrawingContext(this);
            return m_cachedDrawing;
        }
    }
    
    public DrawingParams drawingParams
    {
        get {
            if (cacheParam == null) {
                (cacheParam = new DrawingParams()).hasRect = true;
            }
            return cacheParam;
        }
    }

    protected virtual void FillMesh(Mesh m)
    {
        if (pref == null)
            pref = TEXPreference.main;
         
        #if UNITY_EDITOR
        if (pref.editorReloading)
            return;
        #endif

        CheckTextDirty();
        drawingContext.Render(m, cacheParam);
    }

    public void SetTextDirty()
    {
        SetTextDirty(false);
    }

    public void SetTextDirty(bool forceRedraw)
    {
        m_BoxDirty = true;
        m_TextDirty = true;
        if (forceRedraw)
            SetAllDirty();
    }

    void CheckTextDirty()
    {
        #if UNITY_EDITOR
        if (pref.editorReloading)
            return;
        #endif
        if (m_TextDirty) {
            drawingContext.Parse(PerformSupplements(m_Text), out debugReport, m_FontIndex);
            m_TextDirty = false;
        }
        if (m_BoxDirty || (cacheParam.rectArea != rectTransform.rect)) {
            //if (canvas == null)
            //    return;
            GenerateParam();
            drawingContext.BoxPacking(cacheParam);
            m_BoxDirty = false;
        }
    }

    public override void SetVerticesDirty()
    {
        m_BoxDirty = true;
        base.SetVerticesDirty();
    }

    public void GenerateParam()
    {
        if (cacheParam == null) {
            cacheParam = new DrawingParams();
            cacheParam.hasRect = true;
        }
        cacheParam.autoFit = m_AutoFit;
        cacheParam.autoWrap = m_AutoFit == Fitting.RectSize ? Wrapping.NoWrap : m_AutoWrap;
        cacheParam.autoFill = m_AutoFill;
        cacheParam.alignment = m_Align;
        cacheParam.color = color;
        cacheParam.fontIndex = m_FontIndex;
        cacheParam.fontStyle = 0;
        cacheParam.fontSize = (int)(m_Size * (canvas ? canvas.scaleFactor : 1));
        cacheParam.pivot = rectTransform.pivot;
        cacheParam.rectArea = rectTransform.rect;
        cacheParam.scale = m_Size;
        cacheParam.spaceSize = m_SpaceSize;
    }

    public override Material defaultMaterial
    {
        get { return pref.defaultMaterial; }
    }
    
    // Debugging purpose
    public Mesh m_mesh;
    protected override void UpdateGeometry()
    {
        FillMesh(workerMesh);
        m_mesh = workerMesh;
        PerformPostEffects(workerMesh);
        canvasRenderer.SetMesh(workerMesh);
    }

    #endregion

    #region Layout

    public virtual void CalculateLayoutInputHorizontal()
    {
    }

    public virtual void CalculateLayoutInputVertical()
    {
    }

    public virtual void SetLayoutHorizontal()
    {
        CheckTextDirty();
        layoutTracker.Clear();
        if (m_AutoFit == Fitting.RectSize) {
            layoutTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cacheParam.layoutSize.x);
        }
    }

    public virtual void SetLayoutVertical()
    {
        CheckTextDirty();
        if (m_AutoFit == Fitting.RectSize || m_AutoFit == Fitting.HeightOnly) {
            layoutTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cacheParam.layoutSize.y);
        }
    }

    public virtual float minWidth
    {
        get { return -1; }
    }

    public virtual float preferredWidth
    {
        get {
            CheckTextDirty();
            return cacheParam.layoutSize.x;
        }
    }

    public virtual float flexibleWidth { get { return -1; } }

    public virtual float minHeight
    {
        get { return -1; }
    }

    public virtual float preferredHeight
    {
        get {
            
            CheckTextDirty();
            return cacheParam.layoutSize.y;
        }
    }

    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return 0; } }

    #endregion

    #region Supplements

    List<BaseMeshEffect> postEffects = new List<BaseMeshEffect>();
    List<TEXDrawSupplementBase> supplements = new List<TEXDrawSupplementBase>();

    public void SetSupplementDirty()
    {
        UpdateSupplements();
        SetTextDirty(true);
    }

    void UpdateSupplements()
    {
        GetComponents<TEXDrawSupplementBase>(supplements);
        GetComponents<BaseMeshEffect>(postEffects);
    }

    string PerformSupplements(string original)
    {
        if (supplements == null)
            return original;
        TEXDrawSupplementBase s;
        for (int i = 0; i < supplements.Count; i++) 
            if ((s = supplements[i]) && s.enabled)
                original = s.ReplaceString(original);
        
        return original;
    }
    
    void PerformPostEffects(Mesh m)
    {
        if (postEffects == null)
            return;
        #if UNITY_EDITOR
        if (!Application.isPlaying)
            GetComponents<BaseMeshEffect>(postEffects);
        #endif
        BaseMeshEffect p;
        for (int i = 0; i < postEffects.Count; i++) 
            if ((p = postEffects[i]) && p.enabled)
                p.ModifyMesh(m);
        
    }

    #endregion
}
