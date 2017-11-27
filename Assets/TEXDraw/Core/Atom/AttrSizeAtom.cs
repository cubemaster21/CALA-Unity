
namespace TexDrawLib
{
    public class AttrSizeAtom : Atom
    {
        const string dotStr = ".";
		const string dotdotStr = "..";

        public static AttrSizeAtom Get(Atom baseAtom, string sizeStr)
        {
            var atom = ObjPool<AttrSizeAtom>.Get();
            atom.BaseAtom = baseAtom;
            if (sizeStr != null) {
            	if (sizeStr.Length == 0) {
            		atom.Offset = 0;
            		atom.Size = float.NaN;
            	}
                else if (sizeStr == dotStr) {
                    atom.Offset = 0;
                    atom.Size = TexUtility.SizeFactor(TexStyle.Script);
                } else if (sizeStr == dotdotStr) {
					atom.Offset = 0;
               		atom.Size = TexUtility.SizeFactor(TexStyle.ScriptScript);
                } else {
                    int pos = sizeStr.IndexOf('-');
                    if (pos < 0)
                        pos = sizeStr.IndexOf('+');
                    if (pos < 0 || !float.TryParse(sizeStr.Substring(pos), out atom.Offset))
                        atom.Offset = 0;
                    if (pos < 1 || !float.TryParse(sizeStr.Substring(0, pos), out atom.Size)) {
                        if (pos == 0 || !float.TryParse(sizeStr, out atom.Size))
                            atom.Size = 1;
                    }
                }
            } else {
                atom.Size = 1;
                atom.Offset = 0;
            }
            return atom;
        }

        public Atom BaseAtom;

        public float Size;

        public float Offset;

        public override Box CreateBox(TexStyle style)
	    {
		    // This SizeBox doesn't need start..end block, since we can do the change on RenderSizeFactor instead
            if (BaseAtom == null)
                return StrutBox.Empty;
            else {
                var oldSize = TexUtility.RenderSizeFactor;
				var nan = float.IsNaN(Size);
                var size = nan ? 1f : Size;

                TexUtility.RenderSizeFactor = size;
				var box = BaseAtom.CreateBox(nan ? TexStyle.Display : style);
                box.shift += Offset;
                TexUtility.RenderSizeFactor = oldSize;
                return box;
            }
        }

        public override void Flush()
        {
            if (BaseAtom != null) {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            ObjPool<AttrSizeAtom>.Release(this);
        }
    }
}

