#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary> This Macro takes the "Live" state of the currently selected RageMagnet and sets its opposite to all Magnets in the same hierarchy.
/// </summary>
public class MacrosRageMagnet : MonoBehaviour {
	[MenuItem("Component/RageTools/Macros/RageMagnet - Hierarchy Live Toggle")]
	public static void RageMagnetHierarchyLive() {
		if (!ValidSelection()) return;
		var selectedLiveState = Selection.activeGameObject.GetComponentInChildren<RageMagnet>().On;
		var magnetGlobalCollection = Selection.activeTransform.root.GetComponentsInChildren<RageMagnet>();

		foreach (var magnet in magnetGlobalCollection)
			magnet.Live = !selectedLiveState;

        Debug.Log("Macro: All hierarchy Magnets 'Live' setting toggled.");
	}

	[MenuItem("Component/RageTools/Macros/RageMagnet - Hierarchy Set Rest Position")]
	public static void RageMagnetHierarchySetRest() {
        if (!ValidSelection()) return;
        Debug.Log("Macro: Updating Magnets, please wait.");

		var magnetCollection = Selection.activeTransform.root.GetComponentsInChildren<RageMagnet>();
		foreach (RageMagnet magnet in magnetCollection)
			magnet.UpdateRestPosition();
        Debug.Log("Macro: All hierarchy Magnets rest position updated.");
	}


	private static bool ValidSelection() {
        if (Selection.activeTransform == null ) { //|| Selection.activeGameObject.GetComponent<RageMagnet>()
			Debug.LogWarning("Macro Error: First select a game object");
			return false;
		}
		return true;
	}
}
#endif