
using UnityEngine;

// Atom representing scripts to attach to other atom.
namespace TexDrawLib
{
	public class ScriptsAtom : Atom
	{
		static SpaceAtom scriptSpaceAtom
		{
			get
			{
				return SpaceAtom.Get (TexUtility.glueRatio, 0, 0);
			}
		}
		public static ScriptsAtom Get (Atom baseAtom, Atom subscriptAtom, Atom superscriptAtom)
		{
            var atom = ObjPool<ScriptsAtom>.Get();
			atom.BaseAtom = baseAtom;
            atom.SubscriptAtom = subscriptAtom;
            atom.SuperscriptAtom = superscriptAtom;
            atom.Type = CharTypeInternal.Inner;
            return atom;
		}

        public Atom BaseAtom;

        public Atom SubscriptAtom;

        public Atom SuperscriptAtom;

		public override Box CreateBox (TexStyle style)
		{
			// Create box for base atom.
			var baseBox = (BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox (style));
			if (SubscriptAtom == null && SuperscriptAtom == null)
				return baseBox;

			// Save it shift and use it later
			var shift = baseBox.shift;
			
            // Create result box.
            var resultBox = HorizontalBox.Get (baseBox);
          
			var subscriptStyle = TexUtility.GetSubscriptStyle (style);
			var superscriptStyle = TexUtility.GetSuperscriptStyle (style);

			// Set delta value and preliminary shift-up and shift-down amounts depending on type of base atom.
			var delta = 0f;
			float shiftUp, shiftDown;

			if (BaseAtom is AccentedAtom)
			{
				var accentedBox = ((AccentedAtom)BaseAtom).BaseAtom.CreateBox (TexUtility.GetCrampedStyle (style));
				shiftUp = accentedBox.height  - TEXConfiguration.main.SupDrop * TexUtility.SizeFactor(superscriptStyle);
				shiftDown = accentedBox.depth + TEXConfiguration.main.SubDrop * TexUtility.SizeFactor(subscriptStyle);
			}
			else if (BaseAtom is CharSymbol)
			{
				var charFont = ((CharSymbol)BaseAtom).GetChar ();
				if (!((CharSymbol)BaseAtom).IsTextSymbol)
					delta = TEXPreference.main.GetCharMetric (charFont, style).advanceDelta;
				if (delta > TexUtility.FloatPrecision && SubscriptAtom == null)
				{
					resultBox.Add (StrutBox.Get (delta, 0, 0, 0));
					delta = 0;
				}

                shiftUp = baseBox.height - TEXConfiguration.main.SupDrop * TexUtility.SizeFactor(superscriptStyle);
                shiftDown = baseBox.depth + TEXConfiguration.main.SubDrop * TexUtility.SizeFactor(subscriptStyle);
            }
			else
			{
				shiftUp = baseBox.height - TEXConfiguration.main.SupDrop * TexUtility.SizeFactor(superscriptStyle);
				shiftDown = baseBox.depth + TEXConfiguration.main.SubDrop * TexUtility.SizeFactor(subscriptStyle);
			}

			Box superscriptBox = null;
			Box superscriptContainerBox = null;
			Box subscriptBox = null;
			Box subscriptContainerBox = null;

			if (SuperscriptAtom != null)
			{
				// Create box for superscript atom.
				superscriptBox = SuperscriptAtom.CreateBox (superscriptStyle);
				superscriptContainerBox = HorizontalBox.Get (superscriptBox);

                // Add box for script space.
                superscriptContainerBox.Add (scriptSpaceAtom.CreateBox (style));

                // Adjust shift-up amount.
                float p;
				if (style == TexStyle.Display)
					p = TEXConfiguration.main.SupMin * TexUtility.SizeFactor(style);
				else// if (TexUtility.GetCrampedStyle (style) == style)
					p = TEXConfiguration.main.SupMinNarrow * TexUtility.SizeFactor(style);
				//else
				//	p = TEXConfiguration.main.SupMinNarrow * TexUtility.SizeFactor(style);
				shiftUp = Mathf.Max (shiftUp, p);
			}

			if (SubscriptAtom != null)
			{
				// Create box for subscript atom.
				subscriptBox = SubscriptAtom.CreateBox (subscriptStyle);
				subscriptContainerBox = HorizontalBox.Get (subscriptBox);

                // Add box for script space.
			  	subscriptContainerBox.Add (scriptSpaceAtom.CreateBox (style));
			}

			// Check if only superscript is set.
			if (subscriptBox == null)
			{
				superscriptContainerBox.shift = -shiftUp;
				resultBox.Add (superscriptContainerBox);
                resultBox.height = shiftUp + superscriptBox.height;
            	return resultBox;
			}

			// Check if only subscript is set.
			if (superscriptBox == null)
			{
				subscriptBox.shift = Mathf.Max (shiftDown, TEXConfiguration.main.SubMinNoSup * TexUtility.SizeFactor(style));
				resultBox.Add (subscriptContainerBox);
                resultBox.depth = shiftDown + subscriptBox.depth;
				return resultBox;
			}

			// Adjust shift-down amount.
			shiftDown = Mathf.Max (shiftDown, TEXConfiguration.main.SubMinOnSup * TexUtility.SizeFactor(style));

			// Space between subscript and superscript.
			float scriptsInterSpace = shiftUp - superscriptBox.depth + shiftDown - subscriptBox.height;
			/*if (scriptsInterSpace < 4 * defaultLineThickness)
			{
				shiftUp += 4 * defaultLineThickness - scriptsInterSpace;

				// Position bottom of superscript at least 4/5 of X-height above baseline.
				float psi = 0.8f * TexUtility.SizeFactor(style) - (shiftUp - superscriptBox.depth);
				if (psi > 0)
				{
					shiftUp += psi;
					shiftDown -= psi;
				}
			}*/
			scriptsInterSpace = shiftUp - superscriptBox.depth + shiftDown - subscriptBox.height;

			// If baseAtom is null, make it right-aligned
			
			if (BaseAtom is SpaceAtom && ((SpaceAtom)BaseAtom).policy == StrutPolicy.Misc) {
				var max = Mathf.Max(superscriptContainerBox.width, subscriptContainerBox.width);
				if (superscriptContainerBox.width < max) 
					superscriptContainerBox.Add(0, StrutBox.Get(max - superscriptContainerBox.width, 0, 0, 0));
				if (subscriptContainerBox.width < max) 
					subscriptContainerBox.Add(0, StrutBox.Get(max - subscriptContainerBox.width, 0, 0, 0));
			}
			
			// Create box containing both superscript and subscript.
            var scriptsBox = VerticalBox.Get();
			scriptsBox.Add (superscriptContainerBox);
			scriptsBox.Add (StrutBox.Get (0, scriptsInterSpace, 0, 0));
			scriptsBox.Add (subscriptContainerBox);
			scriptsBox.height = shiftUp + superscriptBox.height;
			scriptsBox.depth = shiftDown + subscriptBox.depth;
            scriptsBox.shift = shift;
            resultBox.Add (scriptsBox);

			return resultBox;
		}
		
	

		

		public override CharType GetLeftType ()
		{
			return BaseAtom.GetLeftType ();
		}

		public override CharType GetRightType ()
		{
			return BaseAtom.GetRightType ();
		}

        public override void Flush()
        {
            if(BaseAtom != null)
            {
                BaseAtom.Flush();
                BaseAtom = null;
            }
            if(SuperscriptAtom != null)
            {
                SuperscriptAtom.Flush();
                SuperscriptAtom = null;
            }
            if(SubscriptAtom != null)
            {
                SubscriptAtom.Flush();
                SubscriptAtom = null;
            }
             ObjPool<ScriptsAtom>.Release(this);
        }

	}
}