
// #define TEXDRAW_PROFILE
#if TEXDRAW_PROFILE && !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
using UnityEngine.Profiling;
#endif

using UnityEngine;
using System.Collections.Generic;

namespace TexDrawLib
{
    /// A class for handling the math behind the final rendering process.
    /// All layouting stuff is handled here.
    public class DrawingParams
    {
        // ------------ Parameter must be filled by Component ------------------
        /// is it has defined rect bound?
        public bool hasRect;
        /// Auto Fit Mode: 0 = Off, 1 = Down Scale, 2 = Rect Size, 3 = Height Only, 4 = Scale, 5 = Best Fit
        public Fitting autoFit;
        /// Wrap Mode: 0 = No Wrap, 1 = Wrap Letter, 2 = Word Wrap, 3 = Word Wrap Justified
        public Wrapping autoWrap;
        /// Rectangle Area, if rect is defined
        public Rect rectArea;
        /// Scale of rendered Graphic
        public float scale;
        /// Alignment vector (respect to Unity's coordinate system)
        public Vector2 alignment;
        /// Additional Space Size
        public float spaceSize;
        /// Rectangle pivot position, if rect is defined
        public Vector2 pivot;
        /// UV3 Filling: 0 = Off, 1 = Rectangle, 2 = Whole Text, 3 = WT Squared, 4 = Per line, 5 = Per word, 6 = Per character, 7 = PC Squared
        public Filling autoFill;

        // ------------ Parameter must be filled by External Component ------------------
        public Color color;
        public int fontIndex;
        public FontStyle fontStyle;
        public int fontSize;
        //Renderer Parameter
        List<TexRenderer> m_formulas = ListPool<TexRenderer>.Get();

        public List<TexRenderer> formulas
        {
            get { return m_formulas; }
            set
            {
                m_formulas = value;
                PredictSize();
            }
        }

        public DrawingContext context;

        // Internal Computation Results
        public Vector2 size;
        public Vector2 offset;
        public Vector2 layoutSize;
        /// relative final scale ratio
        public float factor = 1;

        public DrawingParams()
        {
        }

        public void Render()
        {
            if (hasRect && autoFit > 0 && (rectArea.size.x <= 0 || rectArea.size.y <= 0))
                return;
            // At first, calculate starting offset
            CalculateRenderedArea(size, alignment, rectArea);
            var lastIsCustomAlign = false;
            //Layout bug fix.
            float shift;
            float lastOffsetX = offset.x;
            float postYOffset = 0f, postYParagraph = 0f; //
            // We'll start from top (line-per-line). This means offset will subtracted decrementally..
            offset.y += size.y;
            TexRenderer box;
            var scaleFactor = scale * factor;
            FillHelper verts = context.vertex;
            for (int i = 0; i < formulas.Count; i++)
            {
                int lastVerts = verts.currentVertCount;

                box = formulas[i];
                // Recalculate offsets
                var alignX = alignment.x;
                var sizeX = size.x;
                offset.y -= (box.Box.totalHeight + 
                    (i > 0 && formulas[i-1].Box.totalHeight > 0 ? spaceSize + postYOffset : 0) + 
                    (box.partOfPreviousLine == 0 ? postYParagraph : 0)) * scaleFactor;
                box.Scale *= factor;
                shift = (box.Box.height - box.Box.depth) * box.Scale;

                // Recalculate again if something defined in box's meta
                if (box.usingMetaRules)
                {
                    float lastY = offset.y;
                    var meta = box.metaRules;
                    alignX = meta.GetAlignment(alignment.x);
                    sizeX = (box.Box.width) * scaleFactor;
                    var rectModify = rectArea;
                    rectModify.width -= (meta.right + meta.left + box.partOfPreviousLine > 0 ? 0 : meta.leading) * scaleFactor;
                    
                    // We only need to adjust horizontal offset
                    CalculateRenderedArea(new Vector2(sizeX, 0), new Vector2(alignX, alignment.y), rectModify);
                    offset.x += ((1-alignX) * meta.left 
                                - (alignX) * meta.right
                                + (box.partOfPreviousLine > 0 ? 0 : (-(alignX * 2 - 1)) * meta.leading)
                                ) * scaleFactor;
                    sizeX -= rectArea.xMax - rectModify.xMax;
                    offset.y = lastY;                           // Don't want thing messed.
                    postYOffset = (meta.spacing); // Additional spacing
                    postYParagraph = box.Box.totalHeight > 0 ? meta.paragraph : 0;
                    lastIsCustomAlign = true;
                }
                else if (lastIsCustomAlign)
                {
                    // This is just for restoring things from beginning (if necessary)
                    postYOffset = 0;
                    postYParagraph = 0;
                    offset.x = lastOffsetX;
                    lastIsCustomAlign = false;
                }


                // Get ready to render
                float x = offset.x + /*box.Box.shift * box.Scale +*/ alignX * (sizeX - box.Scale * box.Box.width);
                float y = offset.y - shift;
                box.Render(context, x, y);

                // After render... uhh it's done... except if there's autoFill per line in there... which need to be done right now ...
                switch (autoFill)
                {
                    case Filling.PerLine:
                        Rect r = new Rect(new Vector2(x, offset.y), box.RenderSize);
                        for (int j = lastVerts; j < verts.currentVertCount; j++)
                        {
                            verts.SetUV2(inverseLerp(r, verts.m_Positions[j]), j);
                        }
                        break;
                        /*case 5:
                            //Toughest filling method: Per word - Not yet available
                            var boxes = box.Box.Children;
                            r = new Rect(offset, box.RenderSize);
                            for (int j = lastVerts; j < verts.currentVertCount; j++) {
                                verts.m_Uv2S[j] = inverseLerp(r, verts.m_Positions[j]);
                            }
                            break;*/
                }
            }
            if (autoFill > 0)
                RenderUV3(verts);
        }

        public static Vector2 inverseLerp(Rect area, Vector2 pos)
        {
            pos.x = InverseLerp(area.xMin, area.xMax, pos.x);
            pos.y = InverseLerp(area.yMin, area.yMax, pos.y);
            return pos;
        }

        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return /*Mathf.Clamp01*/((value - a) / (b - a));
            }
            return 0;
        }

        float originalScale;

        public void PredictSize()
        {
            // Make a backup
            originalScale = scale;
            PredictSizeInternal();
        }
        
        private void PredictSizeInternal()
        {
        	#if TEXDRAW_PROFILE
            Profiler.BeginSample("Compositing");
            #endif
            size = Vector2.zero;
            //Predict dirty draw size
            for (int i = 0; i < formulas.Count;)
            {
                var form = formulas[i];
                Box box = form.Box;
                size.x = Mathf.Max(size.x, (box.width + form.PenaltyWidth) * scale);
                if (box.totalHeight == 0) { // Probably an empty block but there's meta over there. Therefore, skip it
                    i++;
                    continue;
                }
                if (++i < formulas.Count)
                    size.y += (box.totalHeight + spaceSize + form.PenaltyHeight) * scale;
                else {
                    size.y += (box.totalHeight) * scale; // The last spacing shouldn't included
              //      size.y -= form.partOfPreviousLine > 0 && form.usingMetaRules ? form.metaRules.paragraph * scale : 0; // ... and last paragraph too (if exist)..
                }
            }
            
            //size.y -= (spaceSize) * scale;
            layoutSize = size;
            factor = 1;

            //Zero means auto, let's change our rect size
            if (rectArea.width == 0)
                rectArea.width = layoutSize.x;
            if (rectArea.height == 0)
                rectArea.height = layoutSize.y;

            //Autowrap? only do if needed
			#if TEXDRAW_PROFILE
			Profiler.BeginSample("Wrapping");
			#endif
            if (autoWrap > 0 && size.x > rectArea.width) 
                HandleWrapping();               

			#if TEXDRAW_PROFILE
            Profiler.EndSample();
            #endif

            //Autofit? then resize the prediction
            if (autoFit == Fitting.Scale || autoFit == Fitting.BestFit || autoFit == Fitting.DownScale)
            {
                factor = autoFit == Fitting.Scale ? 1000 : 1;
                if (size.x > 0)
                    factor = Mathf.Min(factor, rectArea.width / size.x);
                if (size.y > 0)
                    factor = Mathf.Min(factor, rectArea.height / size.y);
                size *= factor;
            }
            if (autoFit == Fitting.BestFit)
            {
                if (factor < 1 && scale > 0.001f)
                {
                    factor = 1;
                    scale -= 1f;
                    RevertBackList();
                    //Start again (That's why it's expensive)
                    PredictSizeInternal(); 
                }
                else
                {
                    // Expected scale reached. now apply it
                    factor = scale / originalScale;
                    scale = originalScale;
                }
            }
			#if TEXDRAW_PROFILE
            Profiler.EndSample();
            #endif
        }

        public void HandleWrapping()
        {
        	var _wrap = (int)autoWrap - 1;
            _useWordWrap = (_wrap % 3) >= 1;
			_doJustify = (_wrap % 3) == 2;
			var _defaultReversed = _wrap >= 3;
            _realBlankSpaceWidth = TexUtility.spaceWidth;
            size.x = 0;
            int i = 0;

            while (i < formulas.Count)
            {
                Box box = formulas[i].Box;
                // Dirty way to check if this line needs to be splitted or not
                if (!(box is HorizontalBox && (box.width + formulas[i].PenaltyWidth) * scale > rectArea.width))
                {
                    i++;
                    if (_doJustify && box is HorizontalBox)
                        ((HorizontalBox)box).Recalculate();
                    continue;
                }

                if (formulas[i].usingMetaRules && formulas[i].metaRules.GetWrappingReversed(_defaultReversed))
	                HandlePerLineWrappingReversed(box, i);
	            else if (_defaultReversed)
					HandlePerLineWrappingReversed(box, i);
	            else
	            	HandlePerLineWrapping(box, i);
				_spaceIdxs.Clear();

                i++;
            }

            //Rescale again
            if (rectArea.width == layoutSize.x)
                rectArea.width = size.x;
            if (rectArea.height == layoutSize.y)
                rectArea.height = size.y;
            layoutSize = size;

        }

		float _realBlankSpaceWidth;
		bool _doJustify;
		bool _useWordWrap;
		static List<int> _spaceIdxs = new List<int>();

        void HandlePerLineWrapping (Box box, int i)
        {
			float x = formulas[i].PenaltyWidth, xPenalty = x, xOri = 0, y = 0, lastSpaceX = 0;
            int lastSpaceIdx = -1;
            //Begin Per-character pooling
            for (int j = 0; j < box.children.Count; j++)
            {
                var child = box.children[j];
                //White line? make a mark
                if (child is StrutBox && ((StrutBox)child).policy == StrutPolicy.BlankSpace)
                {
                    lastSpaceIdx = j; //last space, index
                    lastSpaceX = x; //last space, x position
                    _spaceIdxs.Add(lastSpaceIdx); //record that space
                    child.width = _realBlankSpaceWidth; // All spaces must have this width (they may modified after WordWarpJusified).
                 }
                    x += box.children[j].width;
                xOri = x - xPenalty;
                //Total length not yet to break our rect limit? continue 
                if (x * scale <= rectArea.width) {
                    if (_doJustify && (box.children.Count - 1 == j))
                       ((HorizontalBox)box).Recalculate();
                    continue;
                    
                }
                //Now j is maximum limit character length. Now move any
                //character before that to the new previous line

                //Did we use word wrap? Track the last space index
                 if (_useWordWrap && lastSpaceIdx >= 0)
                {
                    //Omit the last space by + 1
                    j = lastSpaceIdx + 1;
                    x = lastSpaceX;
                    xOri = lastSpaceX - xPenalty + box.children[j - 1].width;
                    //Justify too? then expand our spaces width
                    if (_doJustify && _spaceIdxs.Count > 1)
                    {
                        float normalizedWidth = rectArea.width / scale;
                        float extraWidth = (normalizedWidth - x) / (float)(_spaceIdxs.Count - 1);
                        for (int k = 0; k < _spaceIdxs.Count; k++)
                        {
                            box.children[_spaceIdxs[k]].width += extraWidth;
                        }
                        x = normalizedWidth;
                    }
                }
                else if (box.children[j] is StrutBox && ((StrutBox)box.children[j]).policy == StrutPolicy.BlankSpace)
                {
                    x -= box.children[j].width;
                    j += 1;
                }
                else
                {
                    x -= box.children[j].width;
                    xOri = x - xPenalty;
                }
                if (j < 1)
                {
                    x += box.children[j].width;
                    xOri = x - xPenalty;
                    continue;
                }
                int oriPartMark = m_formulas[i].partOfPreviousLine;
                m_formulas[i].partOfPreviousLine = (_useWordWrap && lastSpaceIdx >= 0) ? 2 : 1;
                m_formulas.Insert(i, TexRenderer.Get(HorizontalBox.Get(
                            box.children.GetRangePool(0, j)), originalScale, m_formulas[i].metaRules)); //Add to previous line,
                box.children.RemoveRange(0, j);
                //Update our measurements, remember now m_formulas[i] is different with box
                if (oriPartMark > 0)
                    m_formulas[i].partOfPreviousLine = oriPartMark;
                box.width -= xOri;
                y = m_formulas[i].Box.totalHeight;
                if (oriPartMark > 0)
                    y += m_formulas[i].PenaltyParagraph;
                formulas[i].Box.width = x - xPenalty;
                size.x = Mathf.Max(size.x, x);
                size.y += (spaceSize + m_formulas[i].PenaltySpacing + y) * scale;
                break;
            }
        }

        // Branched wrapping algorithm for RTL support based above.
		void HandlePerLineWrappingReversed (Box box, int i)
        {
            float x = formulas[i].PenaltyWidth, xPenalty = x, xOri = 0, y = 0, lastSpaceX = 0;
            int lastSpaceIdx = -1;
            bool requestToSkip = false;
            //Begin Per-character pooling
            for (int j = box.children.Count; j --> 0;)
            {
                var child = box.children[j];
                //White line? make a mark
                if (child is StrutBox && ((StrutBox)child).policy == StrutPolicy.BlankSpace)
                {
                    lastSpaceIdx = j; //last space, index
                    lastSpaceX = x; //last space, x position
                    _spaceIdxs.Add(lastSpaceIdx); //record that space
                    child.width = _realBlankSpaceWidth; // All spaces must have this width (they may modified after WordWarpJusified).
                 }
                x += box.children[j].width;
                xOri = x - xPenalty;
                //Total length not yet to break our rect limit? continue 
                if (x * scale <= rectArea.width) {
                    if (_doJustify && (0 == j))
                       ((HorizontalBox)box).Recalculate();
                    continue;
                }
                //Now j is maximum limit character length. Now move any
                //character before that to the new previous line

                //Did we use word wrap? Track the last space index
                if (_useWordWrap && lastSpaceIdx >= 0 && lastSpaceIdx < box.children.Count - 1)
                {
                    //Already ommited since it is exluded
                    j = lastSpaceIdx;
                    x = lastSpaceX;
                    xOri = lastSpaceX - xPenalty + box.children[lastSpaceIdx].width;
                    requestToSkip = true;
                    //Justify too? then expand our spaces width
                    if (_doJustify && _spaceIdxs.Count > 1)
                    {
                        float normalizedWidth = rectArea.width / scale;
                        float extraWidth = (normalizedWidth - x) / (float)(_spaceIdxs.Count - 1);
                        for (int k = 0; k < _spaceIdxs.Count; k++)
                        {
                            box.children[_spaceIdxs[k]].width += extraWidth;
                        }
                        x = normalizedWidth;
                    }
                }
                else if (box.children[j] is StrutBox && ((StrutBox)box.children[j]).policy == StrutPolicy.BlankSpace)
                {
                    x -= box.children[j].width;
                    requestToSkip = true;
                }
                else
                {
                    x -= box.children[j].width;
                    xOri = x - xPenalty;
                }
                if (j > box.children.Count - 2)
                {
                    x += box.children[j].width;
                    xOri = x - xPenalty;
                    continue;
                }
				int oriPartMark = m_formulas[i].partOfPreviousLine;
                m_formulas[i].partOfPreviousLine = (_useWordWrap && lastSpaceIdx >= 0) ? 2 : 1;
				if (requestToSkip)
                    j--; // Skip the unneeded space char
                m_formulas.Insert(i, TexRenderer.Get(HorizontalBox.Get(
                            box.children.GetRangePool(j + 1, box.children.Count - j - 1)), originalScale, m_formulas[i].metaRules)); //Add to previous line,
                box.children.RemoveRange(j + 1, box.children.Count - j - 1);
                //Update our measurements, remember now m_formulas[i] is different with box
                if (oriPartMark > 0)
                    m_formulas[i].partOfPreviousLine = oriPartMark;
                box.width -= xOri;
                y = m_formulas[i].Box.totalHeight;
                if (oriPartMark > 0)
                    y += m_formulas[i].PenaltyParagraph;
                if (requestToSkip)   
					formulas[i].Box.shift -= m_formulas[i].Box.children[0].width;
				formulas[i].Box.width = x - xPenalty;
                size.x = Mathf.Max(size.x, x);
                size.y += (spaceSize + m_formulas[i].PenaltySpacing + y) * scale;
                break;
            }
        }

        public void RevertBackList()
        {
            var RTL = autoWrap > Wrapping.WordWrapJustified;
            var rtl = RTL;
            for (int i = 0; i < m_formulas.Count; i++)
            {
                if (m_formulas[i].partOfPreviousLine > 0)
                {
                    Box box = m_formulas[i].Box;
                    Box prevBox = m_formulas[i - 1].Box;
                    
                    //Space char is there, so we won't need to add anymore
                    if (prevBox is HorizontalBox)
                    {
                        //((HorizontalBox)prevBox).Recalculate();
						rtl  = m_formulas[i].usingMetaRules ? m_formulas[i].metaRules.GetWrappingReversed(RTL) : rtl;
                        if (rtl)
                             ((HorizontalBox)prevBox).AddRange(box, 0);
                        else
                             ((HorizontalBox)prevBox).AddRange(box);
                       // ((HorizontalBox)prevBox).Recalculate();
                    }
                    else
                        prevBox.Add(box); // Should never happen
                    m_formulas[i].Box = null;
                    m_formulas[i].Flush();
                    m_formulas.RemoveAt(i);
                    i--;
                }
            }
        }

        public void CalculateRenderedArea(Vector2 size, Vector2 align, Rect rectArea)
        {
            if (hasRect)
            {
                //Configure offset & alignment, Just comment out one of these things if you don't understood this ;)
                offset = -(
                    VecScale(size, (align)) + //Make sure the drawing pivot affected with aligment
                    VecScale(rectArea.size, VecNormal(align)) + //Make sure it stick on rect bound
                    -(rectArea.center)); //Make sure we calculate it from center (inside) of Rect no matter transform pivot has
            }
            else
            {
                //Miss lot of features
                offset = -VecScale(size, align);
            }
        }

        Vector2 VecScale(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        Vector2 VecNormal(Vector2 a)
        {
            return new Vector2(-a.x + 0.5f, -a.y + 0.5f);
        }

        void RenderUV3(FillHelper verts)
        {
            var count = verts.currentVertCount;
            switch (autoFill)
            {
                case Filling.Rectangle:
                    Rect r;
                    if (hasRect)
                        r = rectArea;
                    else
                        r = new Rect(-VecScale(size, alignment), size);
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(inverseLerp(r, verts.m_Positions[i]), i);
                    }
                    break;
                case Filling.WholeText:
                    if (hasRect)
                        r = new Rect(-(
                        VecScale(size, (alignment)) +
                        VecScale(rectArea.size, VecNormal(alignment)) +
                        -(rectArea.center)), size);
                    else
                        r = new Rect(-VecScale(size, alignment), size);
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(inverseLerp(r, verts.m_Positions[i]), i);
                    }
                    break;
                case Filling.WholeTextSquared:
                    if (hasRect)
                        r = new Rect(-(
                        VecScale(size, (alignment)) +
                        VecScale(rectArea.size, VecNormal(alignment)) +
                        -(rectArea.center)), size);
                    else
                        r = new Rect(-VecScale(size, alignment), size);

                    var max = Mathf.Max(r.width, r.height);
                    var center = r.center;
                    r.size = Vector2.one * max;
                    r.center = center;
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(inverseLerp(r, verts.m_Positions[i]), i);
                    }
                    break;
                case Filling.PerCharacter:
                    for (int i = 0; i < count; i++)
                    {
                        int l = i % 4;
                        verts.SetUV2(new Vector2(l == 0 | l == 3 ? 0 : 1, l < 2 ? 0 : 1), i);
                    }
                    break;
                case Filling.PerCharacterSquared:
                    for (int i = 0; i < count; i += 4)
                    {
                        Vector2 sz = verts.m_Positions[i + 2] - verts.m_Positions[i];
                        if (sz.x <= 0 || sz.y <= 0)
                        {
                            for (int l = 0; l < 4; l++)
                            {
                                verts.SetUV2(new Vector2(l == 0 | l == 3 ? 0 : 1, l < 2 ? 0 : 1), i);
                            }
                            continue;
                        }
                        float xMin, xMax, yMin, yMax;
                        if (sz.x > sz.y)
                        {
                            var h = sz.y / sz.x;
                            xMin = 0;
                            xMax = 1;
                            yMin = (1 - h) / 2;
                            yMax = 1 - yMin;
                        }
                        else
                        {
                            var v = sz.x / sz.y;
                            yMin = 0;
                            yMax = 1;
                            xMin = (1 - v) / 2;
                            xMax = 1 - xMin;
                        }
                        for (int l = 0; l < 4; l++)
                        {
                            verts.SetUV2(new Vector2(l == 0 | l == 3 ? xMin : xMax, l < 2 ? yMin : yMax), i + l);
                        }
                    }
                    break;
                case Filling.LocalContinous:
                    var ratio = 1 / (factor * scale);
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(verts.m_Positions[i] * ratio, i);
                    }
                    break;
                case Filling.WorldContinous:
                    ratio = 1 / (scale);
                    var transform = context.monoContainer.transform;
                    for (int i = 0; i < count; i++)
                    {
                        verts.SetUV2(transform.TransformPoint(verts.m_Positions[i]) * ratio, i);
                    }
                    break;
            }
        }

    }
}