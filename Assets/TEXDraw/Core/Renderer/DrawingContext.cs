
// #define TEXDRAW_PROFILE
#if TEXDRAW_PROFILE && !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
using UnityEngine.Profiling;
#endif

using UnityEngine;
using System;
using System.Collections.Generic;

namespace TexDrawLib
{
    public class DrawingContext
    {
        public TexFormulaParser parser;
        public List<TexFormula> parsed = ListPool<TexFormula>.Get();
        public List<string> linkBoxKey = ListPool<string>.Get();
        public List<Rect> linkBoxRect = ListPool<Rect>.Get();
        public List<Color> linkBoxTint = ListPool<Color>.Get();
        bool hasInit = false;
        public bool parsingComplete = false;
        public FillHelper vertex;
        public Component monoContainer;
        
        public DrawingContext(Component parent)
        {
            vertex = new FillHelper();
            parser = new TexFormulaParser();
            monoContainer = parent;
            hasInit = true;
        }


        static string[] chars = new string[0xffff];

        const int maxPossibleTextureSize = 1024;

        public static bool GetCharInfo(Font f, char Char, int size, FontStyle style, out CharacterInfo c)
        {
           if (!f.dynamic)
                return f.GetCharacterInfo(Char, out c, 0);
            int idx = (int)Char;
            if (chars[idx] == null)
                chars[idx] = Char.ToString();

            if (size > maxPossibleTextureSize)
                size = maxPossibleTextureSize;
            f.RequestCharactersInTexture(chars[idx], size, style);
            return f.GetCharacterInfo(Char, out c, size, style);
        }

        public void Clear()
        {
            vertex.Clear();
        }

        /// Actually returns Vector2
        static public Vector4 VectorCode(int id)
        {
            return new Vector4((id % 8) / 8f, (id >> 3) / 4f);
        }

        public void Draw(int id, Rect v, Vector2[] uv)
        {
            Vector2 c = VectorCode(id);
            int t = vertex.currentVertCount;
            //Top-Left
            vertex.AddVert(new Vector2(v.xMin, v.yMin), TexUtility.RenderColor, uv[0], Vector2.zero, c);
            //Top-Right
            vertex.AddVert(new Vector2(v.xMax, v.yMin), TexUtility.RenderColor, uv[1], Vector2.zero, c);
            //Bottom-Right
            vertex.AddVert(new Vector2(v.xMax, v.yMax), TexUtility.RenderColor, uv[2], Vector2.zero, c);
            //Bottom-Left
            vertex.AddVert(new Vector2(v.xMin, v.yMax), TexUtility.RenderColor, uv[3], Vector2.zero, c);

            vertex.AddTriangle(t + 0, t + 1, t + 2);
            vertex.AddTriangle(t + 2, t + 3, t + 0);
        }

        // Using this variant is much faster than anything else...
        public void Draw(int id, Vector2 vPos, Vector2 vSize, Vector2 uvTL, Vector2 uvTR, Vector2 uvBR, Vector2 uvBL)
        {
            var c = VectorCode(id);
            var t = vertex.currentVertCount;
            var z = Vector2.zero;
            var r = TexUtility.RenderColor;
            //Top-Left
            vertex.AddVert(vPos, r, uvTL, z, c);
            //Top-Right
            vPos.x += vSize.x;
            vertex.AddVert(vPos, r, uvTR, z, c);
            //Bottom-Right
            vPos.y += vSize.y;
            vertex.AddVert(vPos, r, uvBR, z, c);
            //Bottom-Left
            vPos.x -= vSize.x;
            vertex.AddVert(vPos, r, uvBL, z, c);

            vertex.AddTriangle(t + 0, t + 1, t + 2);
            vertex.AddTriangle(t + 2, t + 3, t + 0);
        }

        public void DrawWireDebug(Rect v, Color c)
        {
            int t = vertex.currentVertCount;
            var r = VectorCode(TexUtility.blockFontIndex + 1);
            var z = Vector2.zero;
            vertex.AddVert(new Vector2(v.xMin, v.yMin), c, z, z, r);
            vertex.AddVert(new Vector2(v.xMax, v.yMin), c, z, z, r);
            vertex.AddVert(new Vector2(v.xMax, v.yMax), c, z, z, r);
            vertex.AddVert(new Vector2(v.xMin, v.yMax), c, z, z, r);

            vertex.AddTriangle(t + 0, t + 1, t + 3);
            vertex.AddTriangle(t + 3, t + 1, t + 2);
        }

        public Color DrawLink(Rect v, string key)
        {
            linkBoxKey.Add(key);
            linkBoxRect.Add(v);
            if (linkBoxKey.Count > linkBoxTint.Count)
                linkBoxTint.Add(Color.white);
            return linkBoxTint[linkBoxKey.Count - 1];
        }

        public void Draw(int id, Vector2[] v, Vector2[] uv)
        {
            Vector2 c = VectorCode(id);
            int t = vertex.currentVertCount;
            //Top-Left
            vertex.AddVert(v[0], TexUtility.RenderColor, uv[0], Vector2.zero, c);
            //Top-Right
            vertex.AddVert(v[1], TexUtility.RenderColor, uv[1], Vector2.zero, c);
            //Bottom-Right
            vertex.AddVert(v[2], TexUtility.RenderColor, uv[2], Vector2.zero, c);
            //Bottom-Left
            vertex.AddVert(v[3], TexUtility.RenderColor, uv[3], Vector2.zero, c);

            vertex.AddTriangle(t + 0, t + 1, t + 2);
            vertex.AddTriangle(t + 2, t + 3, t + 0);
        }

        static readonly char[] newLineChar = new char[] { '\n' };

        public bool Parse(string input, out string errResult, int renderFont = -1)
        {
        	#if TEXDRAW_PROFILE
			Profiler.BeginSample("Parsing");
			#endif

            if (!hasInit)
            {
                vertex = new FillHelper();
                parser = new TexFormulaParser();
            }
            try
            {
                TexUtility.RenderFont = -2;
                TexUtility.RawRenderFont = renderFont;
                parsingComplete = false;
                string[] strings = input.Split(newLineChar, StringSplitOptions.None);
                if (parsed.Count > 0)
                {
                    for (int i = 0; i < parsed.Count; i++)
                        parsed[i].Flush();
                }
                parsed.Clear();
                for (int i = 0; i < strings.Length; i++)
                    parsed.Add(parser.Parse(strings[i]));
                parsingComplete = true;
            }
            catch (Exception ex)
            {
                errResult = ex.Message;
   				#if TEXDRAW_PROFILE
				Profiler.EndSample();
				#endif
				// throw ex;
                return false;
            }
            errResult = String.Empty;
			#if TEXDRAW_PROFILE
			Profiler.EndSample();
			#endif
            return true;
        }

        public bool Parse(string input)
        {
            if (!hasInit)
            {
                vertex = new FillHelper();
                parser = new TexFormulaParser();
            }
            try
            {
                parsingComplete = false;
                string[] strings = input.Split(newLineChar, StringSplitOptions.RemoveEmptyEntries);
                if (parsed.Count > 0)
                {
                    for (int i = 0; i < parsed.Count; i++)
                        parsed[i].Flush();
                }
                parsed.Clear();
                for (int i = 0; i < strings.Length; i++)
                    parsed.Add(parser.Parse(strings[i]));
                parsingComplete = true;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Render(Mesh m, DrawingParams param)
        {
        	#if TEXDRAW_PROFILE
			Profiler.BeginSample("Rendering");
        	#endif

            m.Clear();
            Clear();
            if (parsingComplete)
            {
                TexUtility.RenderColor = param.color;
                param.context = this;
                linkBoxKey.Clear();
                linkBoxRect.Clear();
                param.Render();
            }
            Push2Mesh(m);

			#if TEXDRAW_PROFILE
			Profiler.EndSample();
			#endif
        }

        public void BoxPacking(DrawingParams param)
        {
        	#if TEXDRAW_PROFILE
			Profiler.BeginSample("Boxing");
            param.formulas = ToRenderers(this.parsed, param);
            Profiler.EndSample();
            #else
			param.formulas = ToRenderers(this.parsed, param);
			#endif
        }


        /// Convert Atom into Boxes
        public static List<TexRenderer> ToRenderers(List<TexFormula> formulas, DrawingParams param)
        {
            // Init default parameters
            var list = param.formulas;
            TexUtility.RenderTextureSize = param.fontSize;
            TexUtility.RenderFontStyle = param.fontStyle;
            TexUtility.RenderFont = param.fontIndex;
            TexUtility.AdditionalGlueSpace = 0;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Flush(); 
            }
            list.Clear();

            TexMetaRenderer lastMeta = null;
            for (int i = 0; i < formulas.Count; i++)
            {
                var scale = param.scale;
                if (lastMeta != null || (formulas[i].AttachedMetaRenderer != null && formulas[i].AttachedMetaRenderer.enabled))
                {
                    var meta = formulas[i].AttachedMetaRenderer ?? (formulas[i].AttachedMetaRenderer = lastMeta);
                    meta.ApplyBeforeBoxing(param);
                    if (meta.size != 0)
                        scale = meta.size;
                    lastMeta = meta;
                }
                list.Add(formulas[i].GetRenderer(TexStyle.Display, scale));
            }
            return list;
        }

        protected void Push2Mesh(Mesh m)
        {
            vertex.FillMesh(m);
        }
    }
}