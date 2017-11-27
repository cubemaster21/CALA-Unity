using UnityEngine;

namespace TexDrawLib
{

    public class AttrColorBox : Box
    {

        public static AttrColorBox Get(AttrColorAtom atom, AttrColorBox endBox)
        {
            var box = ObjPool<AttrColorBox>.Get();
            box.renderColor = atom.color;
            box.mixMode = atom.mix;
            box.endBox = endBox;
            box.attachedAtom = atom;
            return box;
        }

        public static AttrColorBox Get(int mixMode, Color color, AttrColorBox endBox)
        {
			var box = ObjPool<AttrColorBox>.Get();
            box.renderColor = color;
            box.mixMode = mixMode;
            box.endBox = endBox;
            return box;
        }

        public AttrColorAtom attachedAtom;
        public Color renderColor;
        public Color endColor;
        //If null, then this is the end box
        public AttrColorBox endBox;

        //0 = Overwrite, 1 = Alpha-Multiply, 2 = RGBA-Multiply
        public int mixMode;

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            var oldColor = TexUtility.RenderColor;
            var newColor = endBox != null ? ProcessFinalColor(oldColor) : (Color32)endColor;

            if (endBox != null)
                endBox.endColor = oldColor;

            TexUtility.RenderColor = newColor;
        }

        Color32 ProcessFinalColor(Color32 old)
        {
            switch (mixMode) {
                case 1:
                    return TexUtility.MultiplyAlphaOnly(renderColor, old.a / 255f);
                case 2:
                    return TexUtility.MultiplyColor(old, renderColor);
            }
            return renderColor;
        }

        public override void Flush()
        {
            base.Flush();
            endBox = null;
            renderColor = Color.clear;
            if (attachedAtom != null)
            {
				attachedAtom.generatedBox = null;
	            attachedAtom = null;
            }
            ObjPool<AttrColorBox>.Release(this);
        }
    }
}