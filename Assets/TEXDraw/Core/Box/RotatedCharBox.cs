using System;
using UnityEngine;

namespace TexDrawLib
{
    // Clockwise Rotated Character, right now it's used mainly just for braces
    public class RotatedCharBox : Box
    {
        public static RotatedCharBox Get(TexStyle style, TexCharMetric Char)
        {
            return Get(style, Char, TexUtility.RenderFontStyle);
        }

        public static RotatedCharBox Get(TexStyle style, TexCharMetric Char, FontStyle fontStyle)
        {
            var box = ObjPool<RotatedCharBox>.Get();
            if (Char == null)
                throw new NullReferenceException();
            box.character = Char;
            //I can't say more but our cached glyph is slightly incorrect because
            //the usage of int in character Info, so we need to...
            var font = Char.ch.font;
            if (font.type == TexFontType.Font && Char.ch.supported) {
                CharacterInfo c;
                if (DrawingContext.GetCharInfo(font.Font_Asset, Char.ch.characterIndex,
                    (int)(TexUtility.RenderTextureSize * Char.appliedScale) + 1, TexUtility.TexStyle2FontStyle(fontStyle), out c)) {
                    float ratio = (c.size == 0 ? (float)TexUtility.RenderTextureSize : c.size) / Char.appliedScale;
                    //Swap XY
                    box.bearing = 0;
                    box.italic = (c.maxY - c.minY) / ratio;
                    box.depth =  c.maxX / ratio;
                    box.height = (-c.minX) / ratio;
                    //No kerning applied?
                    box.width = box.italic;
                    box.c = c;
                    return box;
                }
            } 
			
            box.bearing = 0;
            box.italic = Char.height + Char.depth;
            box.depth = 0;
            box.height = Char.italic + Char.bearing;
            box.width = box.italic;
			
            return box;
        }

        public TexCharMetric character;
		
        public CharacterInfo c;
		
        public float bearing;
		
        public float italic;

        public override void Draw(DrawingContext drawingContext, float scale, float x, float y)
        {
            base.Draw(drawingContext, scale, x, y);
			
            // Draw character at given position.
            Vector2 vPos = new Vector2((x - bearing) * scale, (y - depth) * scale); 
            Vector2 vSize = new Vector2((bearing + italic) * scale, totalHeight * scale);
            TexChar ch = character.ch;
            if (ch.font.type == TexFontType.Font) {
                drawingContext.Draw(ch.fontIndex, vPos, vSize, 
                    c.uvBottomRight, c.uvTopRight, c.uvTopLeft, c.uvBottomLeft);
            } else {
                Rect u = ch.sprite_uv;
                if (!ch.font.sprite_alphaOnly) {
                    //Using RGB? then the color should be black
                    //see the shader why it's happen to be like that
                    Color tmpC = TexUtility.RenderColor;
                    TexUtility.RenderColor = Color.black;
                    drawingContext.Draw(ch.fontIndex, vPos, vSize, 
                        new Vector2(u.xMax, u.yMin), u.max,
                        new Vector2(u.xMin, u.yMax), u.min);
                    TexUtility.RenderColor = tmpC;
                } else {
                    drawingContext.Draw(ch.fontIndex, vPos, vSize, 
                        new Vector2(u.xMax, u.yMin), u.max,
                        new Vector2(u.xMin, u.yMax), u.min);
                }
            }
        }

        public override void Flush()
        {
            base.Flush();
            if (character != null) {
                character.Flush();
                character = null;
            }
            ObjPool<RotatedCharBox>.Release(this);
        }
    }
}