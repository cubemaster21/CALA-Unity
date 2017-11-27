using UnityEngine;

namespace TexDrawLib
{
    public class AttrBgAtom : Atom
    {
		      
		public static AttrBgAtom Get(Atom baseAtom, string colorStr, bool horizontalMargin)
        {
            var atom = ObjPool<AttrBgAtom>.Get();
            atom.baseAtom = baseAtom;
            atom.horizontalMargin = horizontalMargin;
            if (colorStr == null)
                atom.color = Color.white;
            else if (colorStr.Length == 1)
                atom.color = AttrColorAtom.ModifiedTerminalColor(colorStr[0]);
            else if (!ColorUtility.TryParseHtmlString(colorStr, out atom.color)) {
                if (!ColorUtility.TryParseHtmlString("#" + colorStr, out atom.color))
                    atom.color = Color.white;        
            }

            return atom;
        }

        public Color color = Color.white;
        public Atom baseAtom;
        public bool horizontalMargin;

        public override Box CreateBox(TexStyle style)
        {

        	var result = HorizontalBox.Get();
        	var margin = TEXConfiguration.main.BackdropMargin * TexUtility.SizeFactor(style);
        	var box = baseAtom.CreateBox(style);
			box = VerticalBox.Get(HorizontalBox.Get(box, box.width + (horizontalMargin ? margin * 2 : 0), TexAlignment.Center), box.totalHeight + margin * 2, TexAlignment.Center);
			var endColor = AttrColorBox.Get(0, color, null);
			var startColor = AttrColorBox.Get(0, color, endColor);
        	var bg = HorizontalRule.Get(box.height, box.width, 0, box.depth, true);

        	result.Add(startColor);
        	result.Add(bg);
        	result.Add(endColor);

        	result.Add(StrutBox.Get(-box.width, 0, 0, 0));
        	result.Add(box);

        	return result;
        }

        public override void Flush()
        {
        	base.Flush();
            color = Color.clear;
            if (baseAtom != null)
            {
            	baseAtom.Flush();
            	baseAtom = null;
            }
            ObjPool<AttrBgAtom>.Release(this);
        }
	    
    }
}
