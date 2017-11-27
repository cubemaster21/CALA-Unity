using UnityEngine;

namespace TexDrawLib
{
    [AddComponentMenu("TEXDraw/Supplemets/TEXSup Vertex Gradient", 16), ExecuteInEditMode]
    [TEXSupHelpTip("Blend vertex colors on each vertex corner")]
	public class TEXSupVertexGradient : TEXDrawMeshEffectBase
    {
      
        public Color topLeft = Color.white;
        public Color topRight = Color.white;
        public Color bottomRight = Color.white;
        public Color bottomLeft = Color.white;
        public override void ModifyMesh(Mesh m)
        {
            #if UNITY_5_6_OR_NEWER
            var colors = ListPool<Color32>.Get();
            m.GetColors(colors);
            var count = colors.Count;
            #else
            var colors =  m.colors32;
            var count = colors.Length;
            #endif

            for (int i = 0; i < count;)
            {
                colors[i++] *= bottomLeft;
                colors[i++] *= bottomRight;
                colors[i++] *= topRight;
                colors[i++] *= topLeft;
            }

            #if UNITY_5_6_OR_NEWER
            m.SetColors(colors);
            ListPool<Color32>.Release(colors);
            #else
            m.colors32 = colors;
            #endif
        }
    }
}