using System;

namespace TexDrawLib
{
    // Creates boxes containing delimeter symbol that exists in different sizes.
    public static class DelimiterFactory
    {
        public static Box CreateBox(string symbol, float minHeight, TexStyle style)
        {
            minHeight += TEXConfiguration.main.DelimiterRecursiveOffset;
            var charInfo = TEXPreference.main.GetCharMetric(symbol, style);

            // Find first version of character that has at least minimum height.
            var totalHeight = charInfo.height + charInfo.depth;
            //var alter = 0;
            while (totalHeight <= minHeight && charInfo.ch.nextLargerExist)
            {
                //Debug.LogWarningFormat("will: {3} min: {0} cur: {1}+{2}", minHeight, charInfo.height, charInfo.depth, alter);
                charInfo = TEXPreference.main.GetCharMetric(charInfo.ch.nextLarger, style);
                totalHeight = charInfo.height + charInfo.depth;
                //alter++;
            }

            if (totalHeight > minHeight)
            {
                // Character of sufficient height was found.
                //Debug.LogFormat("owning: {2} min: {0} cur: {1}", minHeight, totalHeight, alter);
                return CharBox.Get(style, charInfo);
            }
            else if (charInfo.ch.extensionExist && !charInfo.ch.extensionHorizontal)
            {
                var resultBox = VerticalBox.Get();
                resultBox.ExtensionMode = true;
                // Construct box from extension character.
                var extension = charInfo.ch.GetExtentMetrics(style);
                if (extension[0] != null)
                    resultBox.Add(CharBox.Get(style, extension[0]));
                if (extension[1] != null)
                    resultBox.Add(CharBox.Get(style, extension[1]));
                if (extension[2] != null)
                    resultBox.Add(CharBox.Get(style, extension[2]));

                // Insert repeatable part multiple times until box is high enough.
                if (extension[3] != null)
                {
                    var repeatBox = CharBox.Get(style, extension[3]);
                    if(repeatBox.totalHeight <= 0)
                        throw new ArgumentOutOfRangeException("PULL CHAR DEL ZERO");
                    while (resultBox.height + resultBox.depth <= minHeight)
                    {
                        if (extension[0] != null && extension[2] != null)
                        {
                            resultBox.Add(1, repeatBox);
                            if (extension[1] != null)
                                resultBox.Add(resultBox.children.Count - 1, repeatBox);
                        }
                        else if (extension[2] != null)
                            resultBox.Add(0, repeatBox);
                        else
                            resultBox.Add(repeatBox);
                    }
                }
                return resultBox;
            }
            else
            {
                // No extensions available, so use tallest available version of character.
                return CharBox.Get(style, charInfo);
            }
        }

        public static Box CreateBoxHorizontal(string symbol, float minWidth, TexStyle style)
        {
            var charInfo = TEXPreference.main.GetCharMetric(symbol, style);
	        
	        var charInfo2 = TEXPreference.main.GetChar(symbol);
	        bool isAlreadyHorizontal = true;
            while (charInfo2 != null)
	        {
				if(charInfo2.extensionExist && !charInfo2.extensionHorizontal)
	        	{
				 	isAlreadyHorizontal = false;
		        	break;
	        	}
	        	charInfo2 = charInfo2.nextLarger;
	        }
	        
            // Find first version of character that has at least minimum width.
            var totalWidth = isAlreadyHorizontal ? charInfo.bearing + charInfo.italic : charInfo.totalHeight;
            while (totalWidth < minWidth && charInfo.ch.nextLargerExist)
            {
                charInfo = TEXPreference.main.GetCharMetric(charInfo.ch.nextLarger, style);
				totalWidth = isAlreadyHorizontal ? charInfo.bearing + charInfo.italic : charInfo.totalHeight;
            }

            if (totalWidth >= minWidth)
            {
                // Character of sufficient height was found.
	            if(isAlreadyHorizontal)
		            return CharBox.Get(style, charInfo);
	            else
	            	return RotatedCharBox.Get(style, charInfo);
            }
            else if (charInfo.ch.extensionExist)
            {
                var resultBox = HorizontalBox.Get();
                resultBox.ExtensionMode = true;
                // Construct box from extension character.
	            var extension = charInfo.ch.GetExtentMetrics(style);
	            if(isAlreadyHorizontal)
	            {
	                if (extension[0] != null)
	                    resultBox.Add(CharBox.Get(style, extension[0]));
	                if (extension[1] != null)
	                    resultBox.Add(CharBox.Get(style, extension[1]));
	                if (extension[2] != null)
		                resultBox.Add(CharBox.Get(style, extension[2]));
	            } else {
                    if (extension[2] != null)
                        resultBox.Add(RotatedCharBox.Get(style, extension[2]));
                    if (extension[1] != null)
                        resultBox.Add(RotatedCharBox.Get(style, extension[1]));
		            if (extension[0] != null)
			            resultBox.Add(RotatedCharBox.Get(style, extension[0]));
	            }

                // Insert repeatable part multiple times until box is high enough.
                if (extension[3] != null)
                {
	                Box repeatBox;
	                if(isAlreadyHorizontal)
		                 repeatBox = CharBox.Get(style, extension[3]);
	                 else
		                 repeatBox = RotatedCharBox.Get(style, extension[3]);
	                do
                    {
                        if (extension[0] != null && extension[2] != null)
                        {
                            resultBox.Add(1, repeatBox);
                            if (extension[1] != null)
                                resultBox.Add(resultBox.children.Count - 1, repeatBox);
                        }
                        else if (extension[2] != null)
                            resultBox.Add(0, repeatBox);
                        else
                            resultBox.Add(repeatBox);
                    }
                    while (resultBox.width < minWidth);
                }
                return resultBox;
            }
            else
            {
                // No extensions available, so use tallest available version of character.
	            if(isAlreadyHorizontal)
		            return CharBox.Get(style, charInfo);
	            else
	            	return RotatedCharBox.Get(style, charInfo);
            }
        }
   
    }
}