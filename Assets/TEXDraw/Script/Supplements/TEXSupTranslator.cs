using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TexDrawLib
{
	[AddComponentMenu("TEXDraw/Supplemets/TEXSup Translator")]
	[TEXSupHelpTip("Translate given syntax to form that TEXDraw can understands", true)]
	public class TEXSupTranslator : TEXDrawSupplementBase
    {
    	public enum TranslateType
    	{
			Mixed = -1,
			Default = 0,
			Plain = 1,
			RichTags = 2,
    		Latex = 3,
    		Markdown = 4,
    	}

    	public TranslateType syntax = TranslateType.Latex;
    	[Header("Latex Config")]
    	public bool discardSpaces = false;

    	[System.NonSerialized] StringBuilder b = new StringBuilder();

    	public override string ReplaceString (string original)
		{
			b.Length = 0;
			switch (syntax) {
			case TranslateType.RichTags:
				TranslateRichTags(original, b);
				break;
			case TranslateType.Latex:
				TranslateLatex(original, b);
				break;
			case TranslateType.Plain:
				TranslatePlain(original, b);
				break;
			default:
				b.Append(original);
				break;
			}
			return b.ToString();
		}


		void TranslatePlain (string str, StringBuilder dst)
		{
			var i = 0;
			while (i < str.Length)
			{
				var c = str[i++];
				if (TexFormulaParser.IsParserReserved(c))
					dst.Append("\\" + c);
				else
					dst.Append(c);
			}
		}

		void TranslateRichTags (string str, StringBuilder dst)
		{
			int i = 0;
			while (i < str.Length) {
				var ii = i;
				var c = str[i++];
				var n = i < str.Length ? str[i] : '\0';
				if (c == '<' && char.IsLetter(n))
				{
					var cmd = TexFormulaParser.LookForAWord(str, ref i);
					if (knownRichTagsHash.Contains(cmd))
					{
						i = ii;
						GetRichTag(str, ref i, dst);
						continue;
					}
				}
				dst.Append(c);
			}
		}

		static string[] knownRichTags = { "b", "i", "u", "s", "font", "size", "color" };
		static HashSet<string> knownRichTagsHash = new HashSet<string>(knownRichTags);
		void GetRichTag (string str, ref int i, StringBuilder dst)
		{
			if (i >= str.Length || str[i] != '<')
				return;
			i++;
			var head = TexFormulaParser.LookForAWord(str, ref i);
			TexFormulaParser.SkipWhiteSpace(str, ref i);
			var param = TexFormulaParser.ReadGroup(str, ref i, '<', '>');

			dst.Append(TransparseRichTag(head, param));
			dst.Append('{');

			var end = str.IndexOf("</" + head + ">", i);
			i++;

			if (end < 0)
				end = str.Length;
			while (i < end) {
				var ii = i;
				var c = str[i++];
				var n = i < str.Length ? str[i] : '\0';
				if (c == '<' && char.IsLetter(n))
				{
					var cmd = TexFormulaParser.LookForAWord(str, ref i);
					if (knownRichTagsHash.Contains(cmd))
					{
						i = ii;
						GetRichTag(str, ref i, dst);
						continue;
					}
				}
				dst.Append(c);
			}
			if (i < str.Length) {
				while (str[i++] != '>') {}
			}
			dst.Append('}');
		}

		string TransparseRichTag (string head, string param)
		{
			if (param.Length > 1)
				param = param.Substring(1); // Avoid the '='
			switch (head) {
			case "b":
				return "\\"+ TexUtility.GetFontName(tex.fontIndex) +"[b]";
			case "i":
				return "\\"+ TexUtility.GetFontName(tex.fontIndex) +"[i]";
			case "font":
				return "\\" + param;
			case "size":
				float sz = 1;
				if(float.TryParse(param, out sz))
					sz /= tex.size;
				return "\\size[" +  sz.ToString() + "]";
			case "color":
				return "\\color[" +  param + "]";
			default:
				return head;
			}
		}

		void TranslateLatex (string str, StringBuilder dst)
		{
			var i = 0;
			char n = '\0';
			while (i < str.Length) {
				var c = str[i++];
				n = i < str.Length ? str[i] : '\0';
				if (c == ' ') 
				{
					if (!discardSpaces)
						dst.Append(c);
				}
				else if (c == '\\')
				{
					if (TexFormulaParser.IsParserReserved(n)) {
						dst.Append(c);
						continue; 
					}
					else if (char.IsLetter(n))
					{
						var cmd = TexFormulaParser.LookForAWord(str, ref i);
						TexFormulaParser.SkipWhiteSpace(str, ref i);

						// Symbol renames
						if (cmd == "to") {
							dst.Append("\\rightarrow");
							continue;
						}
						if (i >= str.Length)
						{
							dst.Append("\\" + cmd);
 							continue;
						}
						// Lim (or other funcs) goes to over/under if even there single script
						if (cmd == "lim") {
							c = str[i];
							if (c == '^' || c == '_')
								dst.Append("\\" + cmd + c);
							else
								dst.Append("\\" + cmd);
							continue;
						}
						// color but the id in braces
						if (cmd == "color" && (str[i] == '{')) {
							var arg = TexFormulaParser.ReadGroup(str, ref i, '{', '}');
							if (arg.IndexOf(' ') < 0)
							{
								dst.Append("\\color[" + arg + "]");
							} else
								dst.Append("\\color{" + arg + "}");
							continue;
						}
						// \over... or \under....
						int ou = cmd.IndexOf("over") == 0 ? 2 : (cmd.IndexOf("under") == 0 ? 1 : 0);
						if (ou !=0)
						{
							var second = cmd.Substring(ou == 2 ? 4 : 5);
							if (second == "line")
							{
							 	dst.Append(ou == 2 ? "\\over" : "\\under");
								continue;
							}
							c = str[i];
							if (c == '{')
							{
								var arg1 = "{" + TexFormulaParser.ReadGroup(str, ref i, '{', '}') + "}";
								if (second == "brace")
								{
									// \overbrace or \underbrace
									if (i + 1 < str.Length)
									{
										c = str[i++]; n = str[i];
										if ((c == '^' || c == '_') && n == '{')
										{
											// If there's script we need to think another strategy
											var arg2 = TexFormulaParser.ReadGroup(str, ref i, '{', '}');
											var arg3 = ou == 2 ? "\\lbrace" : "\\rbrace";
											if (c == '^')
												dst.Append("\\nfrac{\\size[.]{" + arg2 + "}__" + arg3 + "}{" + arg1 + "}");
											else
												dst.Append("\\nfrac{" + arg1 + "}{\\size[.]{" + arg2 + "}^^" + arg3 + "}");
											continue;
										} else
											i--;
									} 
								}
								dst.Append(arg1);
								dst.Append(ou == 2 ? "^^" : "__");
								dst.Append("\\" + second);
								continue;
							}
						}

						//Default behav
						dst.Append("\\" + cmd);
					}
				}
				else
					dst.Append(c);
			}	
		}
    }
}