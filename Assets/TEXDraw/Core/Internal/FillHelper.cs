using UnityEngine;
using System.Collections.Generic;

namespace TexDrawLib
{
    public class FillHelper
    {
        // The specs that used in texdraw Are ...
        public List<Vector3> m_Positions = ListPool<Vector3>.Get(); // Character (verts) position (XYZ)
        public List<Color32> m_Colors = ListPool<Color32>.Get();    // Character colors (RGBA)
        public List<Vector2> m_Uv0S = ListPool<Vector2>.Get();      // Contain primary map to each font Texture (UV1)
        public List<Vector2> m_Uv1S = ListPool<Vector2>.Get();      // Additional map (like Filling) (UV2)
        public List<Vector4> m_Tangents = ListPool<Vector4>.Get();  // Contain a special data of which texture are used in shader (alternative for UV3)
        public List<Vector3> m_Normals = ListPool<Vector3>.Get();   // Normal direction (only filled with forwards) (XYZ)
        public List<int> m_Indicies = ListPool<int>.Get();          // Usual triangle list data (Index)

        public int total_Verts;
        public int total_Tris;

        public static readonly Vector4 s_DefaultTangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        public static readonly Vector3 s_DefaultNormal = Vector3.back;
        public static readonly Vector2 s_ZeroVector = Vector2.zero;
        public static readonly Vector4 s_ZeroVector4 = Vector4.zero;

        public FillHelper()
        {
        }

        public FillHelper(Mesh m)
        {
            m_Positions.AddRange(m.vertices);
            m_Colors.AddRange(m.colors32);
            m_Uv0S.AddRange(m.uv);
            m_Uv1S.AddRange(m.uv2);
            m_Normals.AddRange(m.normals);
            m_Tangents.AddRange(m.tangents);
            m_Indicies.AddRange(m.GetIndices(0));
            total_Verts += m.vertices.Length;
            total_Tris += m_Indicies.Count;
        }

        public void Clear()
        {
            /*
            m_Positions.Clear();
            m_Colors.Clear();
            m_Uv0S.Clear();
            m_Uv1S.Clear();
            m_Normals.Clear();
            m_Tangents.Clear();
            m_Indicies.Clear();
            */
            total_Verts = 0;
            total_Tris = 0;
        }
        
        public void Trim ()
        {
            if (m_Positions.Count > total_Verts) {
                m_Positions.RemoveRange(total_Verts, m_Positions.Count  - total_Verts);
                m_Colors.RemoveRange(total_Verts, m_Colors.Count  - total_Verts);
                m_Uv0S.RemoveRange(total_Verts, m_Uv0S.Count  - total_Verts);
                m_Uv1S.RemoveRange(total_Verts, m_Uv1S.Count  - total_Verts);
                m_Normals.RemoveRange(total_Verts, m_Normals.Count  - total_Verts);
                m_Tangents.RemoveRange(total_Verts, m_Tangents.Count  - total_Verts);
            }
            if (m_Indicies.Count > total_Tris) 
                m_Indicies.RemoveRange(total_Tris, m_Indicies.Count  - total_Tris);
            
        }
        
        public void Add(Mesh m) {
            Trim();
            m_Positions.AddRange(m.vertices);
            m_Colors.AddRange(m.colors32);
            m_Uv0S.AddRange(m.uv);
            m_Uv1S.AddRange(m.uv2);
            m_Normals.AddRange(m.normals);
            m_Tangents.AddRange(m.tangents);
            var indices = m.GetIndices(0);
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] += total_Verts;
            }
            m_Indicies.AddRange(indices);
            
            total_Verts += m.vertices.Length;
            total_Tris += m_Indicies.Count;
        }

        public int currentVertCount
        {
            get { return total_Verts; }
        }

        public int currentIndexCount
        {
            get { return total_Tris; }
        }

        public void PopulateUIVertex(ref UIVertex vertex, int i)
        {
            vertex.position = m_Positions[i];
            vertex.color = m_Colors[i];
            vertex.uv0 = m_Uv0S[i];
            vertex.uv1 = m_Uv1S[i];
            vertex.normal = m_Normals[i];
            vertex.tangent = m_Tangents[i];
        }

        public void SetUIVertex(UIVertex vertex, int i)
        {
            m_Positions[i] = vertex.position;
            m_Colors[i] = vertex.color;
            m_Uv0S[i] = vertex.uv0;
            m_Uv1S[i] = vertex.uv1;
            m_Normals[i] = vertex.normal;
            m_Tangents[i] = vertex.tangent;
        }

        public void FillMesh(Mesh mesh)
        {
            mesh.Clear();

            if (total_Verts >= 65000)
                throw new System.ArgumentException("Mesh can not have more than 65000 verticies");

            if (m_Positions.Count > total_Verts) {
                var delta = m_Positions.Count - total_Verts;
                m_Positions.RemoveRange(total_Verts, delta);
                m_Colors.RemoveRange(total_Verts, delta);
                m_Uv0S.RemoveRange(total_Verts, delta);
                m_Uv1S.RemoveRange(total_Verts, delta);
                m_Normals.RemoveRange(total_Verts, delta);
                m_Tangents.RemoveRange(total_Verts, delta);
            }
            if (m_Indicies.Count > total_Tris)
                m_Indicies.RemoveRange(total_Tris, m_Indicies.Count - total_Tris);

            mesh.SetVertices(m_Positions);
            mesh.SetColors(m_Colors);
            mesh.SetUVs(0, m_Uv0S);
            mesh.SetUVs(1, m_Uv1S);
            mesh.SetNormals(m_Normals);
            mesh.SetTangents(m_Tangents);
            mesh.SetTriangles(m_Indicies, 0);
            mesh.RecalculateBounds();
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1, Vector4 uv2, Vector3 normal)
        {
            if (total_Verts == m_Positions.Count) {
                m_Positions.Add(position);
                m_Colors.Add(color);
                m_Uv0S.Add(uv0);
                m_Uv1S.Add(uv1);
                m_Tangents.Add(uv2);
                m_Normals.Add(normal);
            } else {
                m_Positions[total_Verts] = (position);
                m_Colors[total_Verts] = (color);
                m_Uv0S[total_Verts] = (uv0);
                m_Uv1S[total_Verts] = (uv1);
                m_Tangents[total_Verts] = (uv2);
                m_Normals[total_Verts] = (normal);
            }
            total_Verts++;
        }
        
        public void SetUV2(Vector2 uv, int idx)
        {
            m_Uv1S[idx] = uv;
        }
        
        public void SetUV3(Vector4 uv, int idx)
        {
            // It doesn't clear what Tangents are actually for... we we'll take benefit of it
            m_Tangents[idx] = uv;
        }
        
        public void AddVert(Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1, Vector4 uv2)
        {
            AddVert(position, color, uv0, uv1, uv2, s_DefaultNormal);
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1)
        {
            AddVert(position, color, uv0, uv1, s_ZeroVector4, s_DefaultNormal);
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0)
        {
            AddVert(position, color, uv0, s_ZeroVector, s_ZeroVector4, s_DefaultNormal);
        }

        public void AddVert(UIVertex v)
        {
            AddVert(v.position, v.color, v.uv0, v.uv1, v.tangent, v.normal);
        }

        public void AddTriangle(int idx0, int idx1, int idx2)
        {
            if (total_Tris == m_Indicies.Count) {
                m_Indicies.Add(idx0);
                m_Indicies.Add(idx1);
                m_Indicies.Add(idx2);
            } else {
                m_Indicies[total_Tris] = (idx0);
                m_Indicies[total_Tris + 1] = (idx1);
                m_Indicies[total_Tris + 2] = (idx2);
            }
            total_Tris += 3;
        }
    }
}