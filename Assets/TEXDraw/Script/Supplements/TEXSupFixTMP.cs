using UnityEngine;

namespace TexDrawLib
{
	[AddComponentMenu("TEXDraw/Supplemets/TEXSup Fix TMP", 16), ExecuteInEditMode]
	[TEXSupHelpTip("Special modifier to fix UV2 behavior when using TMP")]
	public class TEXSupFixTMP : TEXDrawMeshEffectBase
	{
		[Range(0.001f, 5f)]
		public float sharpnessRatio = 1;
		public bool uniformSharpness = false;
		public override void ModifyMesh(Mesh m)
		{
			#if TEXDRAW_TMP
            var xScale = transform.lossyScale.y * tex.drawingParams.factor * sharpnessRatio;

			#if UNITY_5_6_OR_NEWER
            var tans = ListPool<Vector4>.Get();
			m.GetTangents(tans);
			var count = tans.Count;
			#else
            var tans = m.tangents;
			var count = tans.Length;
			#endif

            if (!uniformSharpness) {
				for (int i = 0; i < count; i++)
				{
					var tan = tans[i];
					tan.z = xScale;
					tans[i] = tan;
				}
			}			else {
				var verts = m.vertices;
				xScale /= tex.size;
				for (int i = 0; i < count;)
				{
					var min = verts[i];
					var max = verts[i+2];
					var xScale2 = xScale * (max.y - min.y);
					for (int j = 0; j < 4; j++)
					{
						var tan = tans[i];
						tan.z = xScale2;
						tans[i++] = tan;
					}
				}
			}

			#if UNITY_5_6_OR_NEWER
            m.SetTangents(tans);
			ListPool<Vector4>.Release(tans);
			#else
            m.tangents = tans;
			#endif
#endif
		}
	}
}