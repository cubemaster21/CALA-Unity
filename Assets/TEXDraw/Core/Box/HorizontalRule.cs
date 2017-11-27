
using UnityEngine;

namespace TexDrawLib
{
	// Box representing horizontal line.
	public class HorizontalRule : Box
	{
		public static HorizontalRule Get (float Height, float Width, float Shift)
        {
            var box = ObjPool<HorizontalRule>.Get();
            box.width = Width;
	        box.height = Height;
            box.shift = Shift;
            return box;
        }
		
		public static HorizontalRule Get (float Height, float Width, float Shift, float Depth)
		{
			var box = ObjPool<HorizontalRule>.Get();
			box.width = Width;
			box.height = Height;
			box.depth = Depth;
			box.shift = Shift;
			return box;
		}

		public static HorizontalRule Get (float Height, float Width, float Shift, float Depth, bool UseXDepth)
		{
			var box = ObjPool<HorizontalRule>.Get();
			box.width = Width;
			box.height = Height;
			box.depth = Depth;
			box.shift = Shift;
			box.useXDepth = UseXDepth;
			return box;
		}

		// If yes, this will use texture buffer 32, which means a block font, but rendered on first pass
		// Suitable for background stuff
		public bool useXDepth;		

		public override void Draw (DrawingContext drawingContext, float scale, float x, float y)
		{
            base.Draw (drawingContext, scale, x, y);
            Vector2 z = Vector2.zero;
			drawingContext.Draw (TexUtility.blockFontIndex + (useXDepth ? 1 : 0), new Vector2 (
				(x) * scale, (y - depth) * scale), new Vector2(width * scale, totalHeight * scale)
                , z, z, z, z);
		}
            
        public override void Flush()
        {
            base.Flush();
            useXDepth = false;
            ObjPool<HorizontalRule>.Release(this);
        }
    }
}