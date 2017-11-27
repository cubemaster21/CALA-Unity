
using System.Text.RegularExpressions;
using System.Text;

namespace TexDrawLib
{
	
	public abstract class TEXPerCharacterBase : TEXDrawSupplementBase {
		
        public ScaleOffset m_Factor = ScaleOffset.identity;
		
        const string m_pattern = @"(\\[\w]+[\s]*\[.*?[^\\]\])|(\\[\w]+)|([^\s\\\{\}\^\_])|(\\[\\\{\}\^\_])";
		/*
			REGEX Pattern explanation ...
			there's 4 kind of group, if one of them match, then it'll captured into list:
			1. (\\[\w]+[\s]*\[.*?[^\\]\])	: Match kind like \cmd[] (with bracket) into one group (right now nested bracket isn't supported)
			2. (\\[\w]+)					: Match kind like \cmd (no bracket) into one group
			3. ([^\s\\\{\}\^\_])			: Match any character except spaces, or other preserved chars, separately
			4. (\\[\\\{\}\^\_])				: Special case like \_, \^, etc. should be merged into one
		*/
		
		public override string ReplaceString(string original)
		{
			var reg = Regex.Matches (original, m_pattern);
			if (reg.Count == 0)
				return original;
			var sub = new StringBuilder (original);
			var count = (float)(reg.Count - 1);
			var offset = 0;
			var penalty = 0;
			OnBeforeSubtitution(count);
			for (int i = 0; i <= count; i++) {
				var mac = reg [i];
				var val = mac.Value;
				/*if (commandStrict) {
					if(IsItRegisteredCommand(val)) {
						lastIsSkippedCommand = true;
						continue;
					}
					if(lastIsSkippedCommand && val == "[") {
						SkipUntilEnclosed(reg, ref i);
						continue;
					}
					lastIsSkippedCommand = false;
				}*/
				if (val.Length > 2) {
					penalty++;
					continue;
				}
				sub.Remove (mac.Index + offset, mac.Length);
				var subtituted = Subtitute(val, m_Factor.Evaluate((i - penalty) / (count - penalty)));
				sub.Insert (mac.Index + offset, subtituted);
				offset += subtituted.Length -  mac.Length;
			}
			return sub.ToString ();
			
		}
		
		
		bool IsItRegisteredCommand (string command)
		{
			if (command.Length == 1)
				return false;
			command = command.Substring (1);
			if (TexFormulaParser.isCommandRegistered (command))
				return true;
			if (TEXPreference.main.GetFontIndexByID (command) >= 0)
				return true;
			return false;
		}
		
		void SkipUntilEnclosed(MatchCollection reg, ref int index)
		{
			var group = 0;
			index++;
			while (index < reg.Count && !(reg[index].Value == "]" && group == 0)) {
				if (reg[index].Value == "[")
					group++;
				else if (reg[index].Value == "]")
					group--;
				index++;
			}
		}
		
		protected abstract string Subtitute(string match, float m_Factor);
		
		protected virtual void OnBeforeSubtitution (float count)
		{
		}

    }
}