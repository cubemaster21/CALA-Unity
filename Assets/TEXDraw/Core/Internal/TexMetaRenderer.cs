using UnityEngine;

namespace TexDrawLib
{
    	
	public class TexMetaRenderer : IFlushable
	{
		public bool enabled;
		// ------- These props can and have to be handled before boxing ---------
		
		/// -1 if use default, 1 = Bold, 2 = Italic, 4 = Underline, 8 = Strikethough
		public int style = -1;
		/// -2 if use default, -1 if it math, others are font index
		public int font = -2;
		public float kerning;
		public float size;
		
		// ------- These props is handled only in DrawingParams ---------
		
		/// -1 if use default
		public int align = -1;
		/// 0 if use default, 1 if normal, 2 if reversed
		public int wrap;
		/// First line indentation of paragraph
		public float leading;
		/// Left margin (horizontal)
		public float left;
		/// Right margin (horizontal)
		public float right;
		/// Fixed Line Height
		public float line;
		/// Standard spacing
		public float spacing;
		/// Paragraph spacing
		public float paragraph;
		
		static string[] longTokens = {"style", "font", "lead", "left", "right", "kern", "line", "space", "para", "size", "align", "wrap"};
		static char[] shortTokens = { 't'	 , 'f'	 , 'l'	 , 'b'	 , 'r'	  , 'k'	  , 'h'   , 'n'	   , 'p'   , 's'   , 'x'	, 'w'};
		static char[] styleTokens = {'b', 'i'};
		static char[] alignTokens = {'l', 'c', 'r'};
		static char[] wrapTokens = {'c', 'l', 'r'};

		public void Reset () {
			enabled = false;
			style = -1;
			font = -2;
			size = 0;
			leading = 0;
			left = 0;
			right = 0;
			kerning = 0;
			line = 0;
			spacing = 0;
			paragraph = 0;
			align = -1;
			wrap = 0;
		}
		
		public void ApplyBeforeBoxing () {
            if (!enabled)
                return;
			if (style != -1)
                TexUtility.RenderFontStyle = (FontStyle)(style); //Only Bold or Italic
            if (font != -2)
                TexUtility.RenderFont = font;
            if (kerning != 0)
                TexUtility.AdditionalGlueSpace = kerning;
		}
        
        public void ApplyBeforeBoxing (DrawingParams param) {
            if (!enabled)
                return;
            ApplyBeforeBoxing();
			// Return to param if not set
			if (style == -1)
                TexUtility.RenderFontStyle = (FontStyle)param.fontStyle; //Only Bold or Italic
            if (font == -2)
                TexUtility.RenderFont = param.fontIndex;
            if (kerning == 0)
                TexUtility.AdditionalGlueSpace = 0;
		}
        /*
        public void ApplyPostBoxing (TexRenderer postParamBox) {
            bool isLead = postParamBox.partOfPreviousLine == 0;
            
        }*/
        
        public float GetAlignment(float def) {
            switch (align)
            {
                case 0:     return 0;
                case 1:     return .5f;
                case 2:     return 1;
                default:    return def;
            }
        }

		public bool GetWrappingReversed(bool def) {
            switch (wrap)
            {
                case 0:     return def;
                case 1:     return false;
                case 2:     return true;
                default:    return def;
            }
        }
		
		public void ParseString(string raw) {
			enabled = true;
			if (string.IsNullOrEmpty(raw))
				return;
			int pos = 0;
			bool shortMode = raw[0] == '@';
			if (shortMode)
				pos++;
			while (pos < raw.Length)
			{
				if (char.IsWhiteSpace(raw[pos])) {
					pos++;
					continue;
				}
				int token;
				if (shortMode) {
					token = System.Array.IndexOf(shortTokens, raw[pos++]);
     			} else {
					var start = pos;
					while (pos < raw.Length)
					{
						if (!char.IsLetter(raw[pos]))
							break;
						pos++;
					}
					token = System.Array.IndexOf(longTokens, raw.Substring(start, pos - start));
				}
				if (token == -1) {
					// Sometimes we forgot about it. So error throwing is sometimes helpful
#if UNITY_EDITOR
					if (shortMode)
						throw new TexParseException("Short token isn't available. Possible options are:\n" +
						"t, f, l, b, r, k, h, n, p, s, x, w.\nDon't use @ for extra explanation");
					else
						throw new TexParseException("Token isn't available. Possible options are:\n" +
						"style, font, lead, left, right, kern, line, space, para, size, align, wrap\nUse @ for activate short-mode");
#else
					return;
#endif
				}
				if (raw[pos] == '=')
					pos++;
				ProcessToken(token, raw, ref pos);
			}
		}
		
		void ProcessToken (int tokenIdx, string raw, ref int pos) {
			char[] token = null;
			switch (tokenIdx) {
			case 0:		token = styleTokens; break;
			case 10:	token = alignTokens; break;
			case 11:	token = wrapTokens; break;
			}
			ProcessToken(tokenIdx, raw, ref pos, token);
		}
		
		static int PowTwo (int p) {
			int times = 0;
			int result = 1;
			while (++times < p)
			{
				result *= 2;
			}
			return result;
		}
		void ProcessToken (int tokenIdx, string raw, ref int pos, char[] presetTokens) {
			int start = pos;
			float parsed = 0;
			int parsedInt = 0;
			try {
				if (presetTokens == null) {
					while (pos < raw.Length)
					{
						var ch = raw[pos++];
						if(!char.IsDigit(ch) && ch != '.'  && ch != '-') {
							pos--;
							break;
						}
					}
					float.TryParse(raw.Substring(start, pos - start), out parsed);
				} else {
					// if token is style... we need to repeat again
					if (tokenIdx == 0) {
						do {
						var op = System.Array.IndexOf(presetTokens, raw[pos++]);
						if (op == -1)
							break;
						parsedInt |= PowTwo(op+1);
					} while (pos < raw.Length);
					} else
						parsedInt = System.Array.IndexOf(presetTokens, raw[pos++]);
					
				}
			} catch (System.Exception) {
				// Also useful here
				#if UNITY_EDITOR || UNITY_STANDALONE
				if (presetTokens == null)
					throw new TexParseException("No arguments for token " + longTokens[tokenIdx] + ". Type a float Number (Ex: token=3)");
				else
					throw new TexParseException("No arguments for token " + longTokens[tokenIdx] + ". Choices are " 
					+ string.Join(", ", System.Array.ConvertAll<char, string>(presetTokens, x => x.ToString())));
				#endif
			}
			switch (tokenIdx)
			{
				// "style", "font", "lead", "left", "right", "kern", "space", "para", "size", "align", "wrap"
				case 0:		style = parsedInt;		break;
				case 1:		font = (int)parsed;		break;
				case 2:		leading = parsed;		break;
				case 3:		left = parsed;			break;
				case 4:		right = parsed;			break;
				case 5:		kerning = parsed;		break;
				case 6:		line = parsed;			break;
				case 7:		spacing = parsed;		break;
				case 8:		paragraph = parsed;		break;
				case 9:		size = parsed;			break;
				case 10:	align = parsedInt;		break;
				case 11:	wrap = parsedInt;		break;
			}
		}
		
		public void Flush () 
		{
			Reset();
			ObjPool<TexMetaRenderer>.Release(this);
		}
		
        bool m_flushed = false;
        public bool IsFlushed { get { return m_flushed; } set { m_flushed = value; } }
	}
}