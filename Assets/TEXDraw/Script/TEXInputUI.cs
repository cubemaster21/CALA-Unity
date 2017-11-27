using UnityEngine;
using System;

namespace TexDrawLib {
	[RequireComponent(typeof(RectTransform))]
	public class TEXInputUI : MonoBehaviour {
	
		
		public TEXPreference pref;
		
		[TextArea(3, 15)][SerializeField]
		string m_Text = "TEXDraw";
		[NonSerialized]
		public bool m_TextDirty = true;
		
		public virtual string text
		{
			get { return m_Text; }
			set {
				if (m_Text != value) {
					m_Text = value;
					m_TextDirty = true;
					
				}
			}
		}
		
		
		[SerializeField]
		int m_FontIndex = -1;
		
		public virtual int fontIndex
		{
			get { return m_FontIndex; }
			set {
				if (m_FontIndex != value) {
					m_FontIndex = Mathf.Clamp(value, -1, 15);
					m_TextDirty = true;
				}
			}
		}
		
		[SerializeField]
		float m_Size = 50f;
		
		public virtual float size
		{
			get { return m_Size; }
			set {
				if (m_Size != value) {
					m_Size = Mathf.Max(value, 0f);
					m_TextDirty = true;
				}
			}
		}
		
		[SerializeField]
		Fitting m_AutoFit = Fitting.DownScale;
		
		public virtual Fitting autoFit
		{
			get { return m_AutoFit; }
			set {
				if (m_AutoFit != value) {
					m_AutoFit = value;
					m_TextDirty = true;
				}
			}
		}
		
		[SerializeField]
		Wrapping m_AutoWrap = 0;
		
		public virtual Wrapping autoWrap
		{
			get { return m_AutoWrap; }
			set {
				if (m_AutoWrap != value) {
					m_AutoWrap = value;
					m_TextDirty = true;
				}
			}
		}
		
		
		[SerializeField]
		[Range(0, 2)]
		float m_SpaceSize = 0.2f;
		
		public virtual float spaceSize
		{
			get { return m_SpaceSize; }
			set {
				if (m_SpaceSize != value) {
					m_SpaceSize = value;
					m_TextDirty = true;
				}
			}
		}
		
		[SerializeField]
		Filling m_AutoFill = 0;
		
		public virtual Filling autoFill
		{
			get { return m_AutoFill; }
			set {
				if (m_AutoFill != value) {
					m_AutoFill = value;
					m_TextDirty = true;
				}
			}
		}
		
		[SerializeField]
		Vector2 m_Align = new Vector2(0.5f, 0.5f);
		
		public virtual Vector2 alignment
		{
			get { return m_Align; }
			set {
				if (m_Align != value) {
					m_Align = value;
					m_TextDirty = true;
				}
			}
		}
		
		public string debugReport = string.Empty;
		
		RectTransform m_rectTransform;
		
		public RectTransform rectTransform {
			get {
				if (!m_rectTransform)
					m_rectTransform = GetComponent<RectTransform>();
				return m_rectTransform;
			}	
		}
		
		void Update () {
			
		}
		
		//TEXInputChildMode rootChild;
		
		public void RebuildNow () {
			
		}
	}
}