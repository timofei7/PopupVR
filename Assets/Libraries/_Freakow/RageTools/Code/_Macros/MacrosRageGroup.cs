#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MacrosRageGroup : EditorWindow {
	private Material _material;
	private static RageSpline _refSpline;
	private static RageGroup _group;

	[MenuItem("Component/RageTools/Macros/RageGroup - Hierarchy Group Update")]
	public static void RageGroupHierarchyUpdate() {
		if (!ValidSelection(false)) return;
		var groupGlobalCollection = Selection.activeTransform.root.GetComponentsInChildren<RageGroup>();

		foreach (var group in groupGlobalCollection) 
			group.UpdatePathList();
	}

	[MenuItem("Component/RageTools/Macros/RageGroup - Apply Texturing")]
	public static void RageGroupApplyTexturing() {
		var selectedGameObject = Selection.activeTransform.gameObject;
		if (!HasSplineAndGroup(selectedGameObject)) {
			Debug.Log ("Macro Error: First select a RageSpline object which has a parent or attached RageGroup");
			return;
		}
		Vector2 offset = _refSpline.GetTextureOffset();
		float angleDeg = _refSpline.GetTextureAngleDeg();
		float scaleInv = _refSpline.GetTextureScaleInv();
		Vector2 offset2 = _refSpline.GetTextureOffset2();
		float angle2Deg = _refSpline.GetTextureAngle2Deg();
		float scale2Inv = _refSpline.GetTextureScale2Inv();
		foreach (RageGroupElement groupItem in _group.List) {
			RageSpline thisSpline = groupItem.Spline.Rs;
			Vector2 offsetDelta = _refSpline.transform.position - thisSpline.transform.position;
			RageSpline.CopyStyling (ref _refSpline, thisSpline);
			thisSpline.CopyMaterial (_refSpline);
			thisSpline.SetTextureOffset (offset + offsetDelta);
			thisSpline.SetTextureAngleDeg(angleDeg);
			thisSpline.SetTextureScaleInv(scaleInv);
			thisSpline.SetTextureOffset2(offset2 + offsetDelta);
			thisSpline.SetTextureAngle2Deg(angle2Deg);
			thisSpline.SetTextureScale2Inv(scale2Inv);
			thisSpline.RefreshMeshInEditor(true,true,true);
		}
		Debug.Log ("Macro: Texturing applied to all RageGroup members.");
	}

	private static bool HasSplineAndGroup(GameObject gameObject) {
		_refSpline = gameObject.GetComponent<RageSpline>();
		if (_refSpline == null) return false;
		_group = gameObject.FindParentRageGroup();
		if (_group == null) return false;
		return true;
	}

	[MenuItem("Component/RageTools/Macros/RageGroup - Apply Material")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosRageGroup), true, "RageGroup Apply Material");
		window.maxSize = new Vector2(245f, 55f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		//EditorGUIUtility.LookLikeControls(60f);
		EditorGUIUtility.labelWidth = 60f;
		_material = (Material) EditorGUILayout.ObjectField("Material", _material, typeof(Material), true);
		if (GUILayout.Button("Process"))
			if (ValidSelection(true)) {
				var group = Selection.activeTransform.GetComponent<RageGroup>();
				RageGroupApplyMaterial (group, _material);
			}
	}

	public static void RageGroupApplyMaterial(RageGroup group, Material material) {
		foreach (var item in group.List)
			item.Spline.GameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
	}

	private static bool ValidSelection(bool groupCheck) {
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select a Game gO in the desired hierarchy.");
			return false;
		}
		if (groupCheck && Selection.activeTransform.GetComponent<RageGroup>() == null) {
			Debug.Log("Macro Error: No RageGroup in the selected Game gO.");
			return false;
		}
		return true;
	}
}
#endif
