using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TexDrawLib
{
    // Represents mathematical formula that can be rendered.
    public sealed class TexFormula : IFlushable
    {
        public static TexFormula Get(IList<TexFormula> formulaList)
        {
            var formula = ObjPool<TexFormula>.Get();
            if (formulaList.Count == 1)
                formula.Add(formulaList[0]);
            else
                formula.RootAtom = RowAtom.Get(formulaList);
            return formula;
        }

        public static TexFormula Get(TexFormula formula)
        {
            var formulas = ObjPool<TexFormula>.Get();
            formulas.Add(formula);
            return formulas;
        }

        public TexFormula()
        {
        }

        public string TextStyle;

        public Atom RootAtom;
        
        public TexMetaRenderer AttachedMetaRenderer;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        /// extract the content and flush this formula
        public Atom GetRoot
        {
            get {
                Atom root = RootAtom;
                RootAtom = null;
                ObjPool<TexFormula>.Release(this);
                return root;
            }
        }

        public TexRenderer GetRenderer(TexStyle style, float scale)
        {
            // For compactness:
            // Scale isn't saved, but stored in TexUtility.RenderSizeFactor (and normalized)
            // The actual scaling (param above) is just saved in here, not on each boxes.
            // Color isn't saved, but stored in TexUtility.RenderColor or as a box of AttrColorBox (for \color).
            // FontIndex & FontStyle isn't saved, but stored in TexUtility.RenderFont & TexUtility.RenderFontStyle
            try
            {
                TexUtility.RenderSizeFactor = 1;
                return TexRenderer.Get(CreateBox(style), scale, AttachedMetaRenderer);
            }
#if UNITY_EDITOR
            catch (Exception e)
            {
                throw e;
            }
#else
            catch (Exception) {
            }
            return null;
#endif
        }

        public void Add(TexFormula formula)
        {
            if (formula.RootAtom is RowAtom)
                Add(RowAtom.Get(formula.GetRoot));
            else
                Add(formula.RootAtom);
        }

        public void Add(Atom atom)
        {
            if (RootAtom == null)
                RootAtom = atom;
            else {
                if (!(RootAtom is RowAtom))
                    RootAtom = RowAtom.Get(RootAtom);
                ((RowAtom)RootAtom).Add(atom);
            }
        }

        public Box CreateBox(TexStyle style)
        {
           if (RootAtom == null) 
                return  AttachedMetaRenderer == null ? StrutBox.EmptyLine : StrutBox.Empty;
           else
                return RootAtom.CreateBox(style);
        }

        public void Flush()
        {
            if (RootAtom != null) {
                RootAtom.Flush();
                RootAtom = null;
            }
            if (AttachedMetaRenderer != null) {
                AttachedMetaRenderer.Flush();
                AttachedMetaRenderer = null;
            }
            ObjPool<TexFormula>.Release(this);
        }

        bool m_flushed = false;
        public bool IsFlushed { get { return m_flushed; } set { m_flushed = value; } }

    }
}