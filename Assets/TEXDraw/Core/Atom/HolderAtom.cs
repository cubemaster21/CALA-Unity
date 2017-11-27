
using UnityEngine;

namespace TexDrawLib
{
    public class HolderAtom : Atom
    {
	    public static HolderAtom Get(Atom baseAtom, float Width, float Height, TexAlignment Alignment)
        {
            var atom = ObjPool<HolderAtom>.Get();
            atom.BaseAtom = baseAtom;
            atom.size = new Vector2(Width, Height);
	        atom.align = Alignment;
	       
	        atom.Type = CharTypeInternal.Inner;
            return atom;
        }


        public Atom BaseAtom;

	    public Vector2 size = Vector2.zero;
        public TexAlignment align;

        public override Box CreateBox(TexStyle style)
	    {
		    var width = size.x;
		    var height = size.y;
			
		    Box result;
            if (BaseAtom == null)
                result = StrutBox.Get(width, height, 0, 0);
            else if (BaseAtom is SpaceAtom)
                result = StrutBox.Get(width, height, 0, 0);
            else
            {
            	if (width == 0 && BaseAtom is SymbolAtom)
                    result = VerticalBox.Get(DelimiterFactory.CreateBox(((SymbolAtom)BaseAtom).Name, height, style), height, align);
                else if (height == 0 && BaseAtom is SymbolAtom)
                    result = HorizontalBox.Get(DelimiterFactory.CreateBoxHorizontal(((SymbolAtom)BaseAtom).Name, width, style), width, align);
                else if (width == 0)
	                result = VerticalBox.Get(BaseAtom.CreateBox(style), height, align);
                else if (height == 0)
	                result = HorizontalBox.Get(BaseAtom.CreateBox(style), width, align);
	            else
					result = VerticalBox.Get(HorizontalBox.Get(BaseAtom.CreateBox(style), width, align), height, align);
	        }
            TexUtility.CentreBox(result, style);
            return result;
        }

        public override void Flush()
        {
            if (BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<HolderAtom>.Release(this);
        }


    }
}
