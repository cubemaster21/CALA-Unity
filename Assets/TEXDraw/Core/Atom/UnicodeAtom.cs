using UnityEngine;

namespace TexDrawLib
{
    public class UnicodeAtom : Atom
    {
        public static UnicodeAtom Get (int FontIndex, char c)
        {
            var atom = ObjPool<UnicodeAtom>.Get();
            atom.fontIndex = FontIndex;
            atom.charIndex = c;
            return atom;
        }

        public int fontIndex;
        public char charIndex;

        public override Box CreateBox(TexStyle style)
        {
           // CharacterInfo ch;
            var f = TEXPreference.main.fontData[fontIndex];
            CharacterInfo info;
            var c =	f.CreateCharacterDataOnTheFly(charIndex, TexUtility.SizeFactor(style), out info);
            return UnicodeBox.Get(c, fontIndex, info);                
        }

        public override void Flush()
        {
            ObjPool<UnicodeAtom>.Release(this);
        }
    }
}
