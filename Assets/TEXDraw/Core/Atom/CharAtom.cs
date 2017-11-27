
using UnityEngine;

// Atom representing single character in specific text style.
namespace TexDrawLib
{
	public class CharAtom : CharSymbol
	{
 
		public static CharAtom Get (char character, int FontIndex)
		{
            var atom = ObjPool<CharAtom>.Get();
			atom.Character = character;
			atom.FontIndex = FontIndex;
            atom.FontStyle = TexUtility.RenderFontStyle;
            return atom;
		}

		public static CharAtom Get (char character)
		{
            return Get(character, TexUtility.RenderFont);
		}

        public char Character;

        public int FontIndex;

        public FontStyle FontStyle;

		public override Box CreateBox (TexStyle style)
		{
			var pref = TEXPreference.main;
			var font = FontIndex == -2 ? TexUtility.RenderFont : FontIndex;
			var FStyle = FontStyle == TexUtility.FontStyleDefault ? TexUtility.RenderFontStyle : FontStyle;
			if (font >= 0 && !pref.IsCharAvailable(font, Character))
			{
				// It's unicode, do return Unicode
				CharacterInfo info;
				var c =	pref.fontData[font].CreateCharacterDataOnTheFly(Character, TexUtility.SizeFactor(style), out info);
				return UnicodeBox.Get(c, font, info);
			} else {
				if (font == -1) {
					//var chSymbol = pref.charMapData[Character, -1];
					//if (chSymbol == -1)
						return CharBox.Get (style, pref.GetCharMetric (Character, style), FStyle);
				}
				else
					return CharBox.Get (style, pref.GetCharMetric (font, Character, style), FStyle);
			}
		}

		public TexCharMetric GetChar (TexStyle style)
		{
			if (FontIndex == -1)
				return TEXPreference.main.GetCharMetric (Character, style);
			if (FontIndex == -2) {
				if (TexUtility.RenderFont == -1)
					return TEXPreference.main.GetCharMetric (Character, style);
				else
					return TEXPreference.main.GetCharMetric (TexUtility.RenderFont, Character, style);
			}
			else
				return TEXPreference.main.GetCharMetric (FontIndex, Character, style);
		}

		public override TexChar GetChar ()
		{
			return GetChar(TexStyle.Display).ch;
		}

        public override void Flush()
        {
            Character = default(char);
            FontIndex = -1;
            ObjPool<CharAtom>.Release(this);
        }


	}
}