#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MacrosTransform : EditorWindow {

	//[MenuItem ("Select Group (parent) _g")]
	[MenuItem("Component/RageTools/Macros/Transform - Select Group (parent) &g")]
	public static void SelectGroup () {
		if (!ValidSelection()) return;
		Selection.activeTransform = Selection.activeTransform.parent;
	}

	[MenuItem("Component/RageTools/Macros/Transform - Select Root &#g")]
	public static void SelectGroupRoot() {
		if (!ValidSelection()) return;
		Selection.activeTransform = Selection.activeTransform.root;
	}

	private static bool ValidSelection() {
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select a game object.");
			return false;
		}
		return true;
	}

}
#endif
