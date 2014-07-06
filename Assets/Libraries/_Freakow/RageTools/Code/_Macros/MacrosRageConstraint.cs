#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MacrosRageConstraint : MonoBehaviour {

	/// <summary> Quick creation and setup of a new external controller, tied to the current selection </summary>
	[MenuItem("Component/RageTools/Macros/RageConstraint - Create Controller")]
	public static void RageConstraintQuickCreate() {
		#region Error Detection
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select one or more game objects.");
			return;
		}
		#endregion Error Detection

		var selectionTransforms = new List<Transform>();
		var createdControllers = new Dictionary<Transform, Transform>();	// key = controller, value = the controlled

		var controllersRoot = GameObject.Find ("_Controllers");
		if (controllersRoot == null || controllersRoot.transform.parent != null)
			controllersRoot = new GameObject("_Controllers");

		foreach (GameObject selectionItem in Selection.gameObjects) {
			selectionTransforms.Add (selectionItem.transform);
			var controller = new GameObject("Controller" + selectionItem.name);
			createdControllers.Add (controller.transform, selectionItem.transform);
			controller.transform.parent = selectionItem.transform;
			controller.transform.localPosition = Vector3.zero;
			controller.transform.localRotation = Quaternion.identity;
			controller.transform.localScale = selectionItem.transform.lossyScale;
			controller.transform.parent = controllersRoot.transform;
			var rageConstraint = controller.AddComponent<RageConstraint>();
			//rageConstraint.enabled = false;
			rageConstraint.Visible = true;
			rageConstraint.Follower = selectionItem;
			rageConstraint.FollowRotation = true;
			Selection.activeGameObject = controller;
			var rageHandle = controller.AddComponent<RageHandle>();
			rageHandle.GizmoFile = "pole";
			rageHandle.Live = true;
		}
		AssignControllerParents(createdControllers, selectionTransforms);
	}

	/// <summary> If the controlled has its parent in the current selection, mirror the controller parenting </summary>
	private static void AssignControllerParents (Dictionary<Transform, Transform> createdControllers, List<Transform> selectionTransforms) {
		foreach (var item in createdControllers) {
			var control = item.Key;
			var controlled = item.Value;
			foreach (Transform selectionItem in selectionTransforms) {
				if (controlled.parent == selectionItem) {
					//Debug.Log ("controlled found parent: " + controlled.name + " selection parent: " + selectionItem.name);
					var newParent = GetParentController(selectionItem, createdControllers);
					if (newParent) control.parent = newParent;
				}
			}
		}
	}

	private static Transform GetParentController(Transform targetControlledParent, Dictionary<Transform, Transform> createdControllers) {
		foreach (var item in createdControllers) {
			if (item.Value == targetControlledParent) return item.Key;
		}
		return null;
	}
}
#endif
