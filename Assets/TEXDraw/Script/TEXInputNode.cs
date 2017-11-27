using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TexDrawLib {
	
	public class TEXInputNode : MonoBehaviour {
		
		public TEXInputUI root;
		public TEXInputNode parent;
		public List<TEXInputNode> childrens;
		
		public TEXInputChildMode mode = TEXInputChildMode.Nothing;
		
		TEXDraw child_0;
		Image child_1;
		HorizontalLayoutGroup child_2;
		VerticalLayoutGroup child_3;
		
		RectTransform m_rectTransform;
		
		public RectTransform rectTransform {
			get {
				if (!m_rectTransform)
					m_rectTransform = GetComponent<RectTransform>();
				return m_rectTransform;
			}	
		}
		
		void SetMode (TEXInputChildMode modeNow) {
			if (modeNow == mode)
				return;
			if(child_0)
				DestroyImmediate(child_0);
			if(child_1)
				DestroyImmediate(child_1);
			if(child_2)
				DestroyImmediate(child_2);
			if(child_3)
				DestroyImmediate(child_3);
			switch (modeNow)
			{
			case TEXInputChildMode.Character:
				child_0 = gameObject.AddComponent<TEXDraw>();
				break;
			case TEXInputChildMode.BlockChar:
				child_1 = gameObject.AddComponent<Image>();
				break;
			case TEXInputChildMode.HorizontalBox:
				child_2 = gameObject.AddComponent<HorizontalLayoutGroup>();
				break;
			case TEXInputChildMode.VerticalBox:
				child_3 = gameObject.AddComponent<VerticalLayoutGroup>();
				break;
			}
		}
		
		public void ProcessBox (Box box) {
			if (box is CharBox) {
				//				var ch = (CharBox)box;
				
			}
		}
	}
	
	public enum TEXInputChildMode {
		Nothing = -1,
		Character = 0,
		BlockChar = 1,
		HorizontalBox = 2,
		VerticalBox = 3
	}
}