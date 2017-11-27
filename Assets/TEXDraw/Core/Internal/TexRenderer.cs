using UnityEngine;

namespace TexDrawLib
{	
	public class TexRenderer : IFlushable
	{
		public static TexRenderer Get (Box box, float scale, TexMetaRenderer meta)
		{
            var renderer = ObjPool<TexRenderer>.Get();
            renderer.Box = box;
            renderer.Scale = scale;
			renderer.metaRules = meta;
			// There's an additional step if meta 'line' are declared
			if (renderer.usingMetaRules && renderer.metaRules.line != 0 && box.totalHeight > 0) {
				if (!(box is HorizontalBox))
					renderer.Box = box = HorizontalBox.Get(box);
				box.height = renderer.metaRules.line;
				box.depth = 0;
			}
            return renderer;
		}

        public Box Box;

        public float Scale;

        /// Used for internal param rendering. 0 = No (Meaning it's first line of paragraph), 1 = Yes, 2 = Yes (and there's a space erased)
		public int partOfPreviousLine = 0;
		
		public TexMetaRenderer metaRules;
		
		public bool usingMetaRules {
			get { return metaRules != null && metaRules.enabled; }
		}
		
		/// This Penaltied amounts is depends on metaRules
		public float PenaltyWidth {
			get {
				if (!usingMetaRules)
					return 0;
				return metaRules.left + metaRules.right + (partOfPreviousLine == 0 ? metaRules.leading : 0);
			}
		}
		
		/// This Penaltied amounts is depends on metaRules
		public float PenaltyHeight {
			get {
				if (!usingMetaRules)
					return 0;
				return metaRules.spacing + (partOfPreviousLine == 0 ? metaRules.paragraph : 0);
			}
		}
		
		public float PenaltySpacing {
			get {
				if (!usingMetaRules)
					return 0;
				return metaRules.spacing;
			}
		}
		
		public float PenaltyParagraph {
			get {
				if (!usingMetaRules)
					return 0;
				return (partOfPreviousLine == 0 ? metaRules.paragraph : 0);
			}
		}
		
		public Vector2 RenderSize
		{
			get
			{
				return new Vector2 (Box.width * Scale, Box.totalHeight * Scale);
			}
		}

		public float Baseline
		{
			get
			{
				return Box.height / Box.totalHeight * Scale;
			}
		}

		public void Render (DrawingContext drawingContext, float x, float y)
		{
            if(Box != null)
                Box.Draw (drawingContext, Scale, x / Scale, y / Scale + Box.height);
        }

        public void Flush()
        {
            if(Box != null)
            {
                Box.Flush();
                Box = null;
            }
			partOfPreviousLine = 0;
			if (metaRules != null)
			{
				//metaRules.Flush();
				metaRules = null;
			}
		    ObjPool<TexRenderer>.Release(this);
        }

        bool m_flushed = false;
        public bool IsFlushed { get { return m_flushed; } set { m_flushed = value; } }

	}
}