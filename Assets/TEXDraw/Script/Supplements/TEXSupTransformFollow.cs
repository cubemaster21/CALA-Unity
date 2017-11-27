using UnityEngine;

namespace TexDrawLib
{
	[AddComponentMenu("TEXDraw/Supplemets/TEXSup Transform Follow")]
	[TEXSupHelpTip("Repaint whenever transform is changed and optionally fix rendering issue when the UI is rotated")]
	public class TEXSupTransformFollow : TEXDrawMeshEffectBase
    {
        public bool m_FixTangentIssue = false;
        
        public override void ModifyMesh(Mesh m)
        {
            if (!m_FixTangentIssue)
                return;

            #if UNITY_5_6_OR_NEWER
            var tans = ListPool<Vector4>.Get();
            m.GetTangents(tans);
            var count = tans.Count;
            #else
            var tans =  m.tangents;
            var count = tans.Length;
            #endif

            var mtx = Quaternion.Inverse(transform.rotation);
            if (tex is TEXDraw) 
            {
                if (((TEXDraw)tex).canvas)
                {
                    mtx = ((TEXDraw)tex).canvas.transform.rotation * mtx;
                }
            }
            for (int i = 0; i < count; i++)
            {
                tans[i] = mtx * tans[i];
            }

            #if UNITY_5_6_OR_NEWER
            m.SetTangents(tans);
            ListPool<Vector4>.Release(tans);
            #else
            m.tangents = tans;
            #endif
       }
         
        void Update ()
        {
            if (transform.hasChanged && tex != null) {
                tex.SetTextDirty(true);      
            }
        }
        
#if UNITY_EDITOR
        protected override void Reset () {
            base.Reset();
            if (tex is TEXDraw)
                m_FixTangentIssue = true;
        }
#endif
    }
}