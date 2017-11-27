using UnityEngine;
using System.Text.RegularExpressions;

namespace TexDrawLib
{
	[AddComponentMenu("TEXDraw/Supplemets/TEXSup New Line")]
	[TEXSupHelpTip("Detect \\n for new line")]
	public class TEXSupNewLine : TEXDrawSupplementBase
    {
		const string f = @"\\n(?=[^\d\w])(\s*)";
        const string t = "\n";

        public override string ReplaceString(string original)
        {
        	return Replace(original);
        }

        static public string Replace(string original)
        {
			//This will recognize \n as new line
            return Regex.Replace(original, f, t);
        }
    }
}