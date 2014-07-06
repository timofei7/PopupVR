using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RageCanvasAlign))]
public class RageCanvasAlignEditor : Editor
{
    private Texture2D _updateButton = Resources.Load("refresh", typeof(Texture2D)) as Texture2D;

	public override void OnInspectorGUI(){

		var canvasAlign = target as RageCanvasAlign;
		if (canvasAlign == null) return;

		EditorGUILayout.Separator();
		//---------

        GuiX.Horizontal(() => {// 			var icon = (Texture2D)Resources.Load("ragetoolsicon");
// 			if (icon != null)
// 				GUILayout.Box(icon, GUILayout.MinHeight(22f), GUILayout.MinWidth(22f),
// 								GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

			EditorGUILayout.BeginVertical(); {
				GUILayout.Label("Hor:");
			} EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(); {
				//EditorGUIUtility.LookLikeControls(50f, 60f);
				EditorGUIUtility.labelWidth = 50f;
				EditorGUIUtility.fieldWidth = 60f;

				canvasAlign.HorizontalAlign = (RageCanvasAlign.HorizontalAlignType)
											  EditorGUILayout.EnumPopup(canvasAlign.HorizontalAlign);
			} EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(); {
				GUILayout.Label("Ver:");
			} EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(); {
				//EditorGUIUtility.LookLikeControls(50f, 60f);
				EditorGUIUtility.labelWidth = 50f;
				EditorGUIUtility.fieldWidth = 60f;
				canvasAlign.VerticalAlign = (RageCanvasAlign.VerticalAlignType)
											EditorGUILayout.EnumPopup(canvasAlign.VerticalAlign);
			} EditorGUILayout.EndVertical();

        });

        GuiX.Horizontal(() => {
            //canvasAlign.StartOnly = GUILayout.Toggle(canvasAlign.StartOnly, "Start Only", GUILayout.Width(75f), GUILayout.MaxHeight(18f));
			EditorGUIUtility.labelWidth = 70f;
            //EditorGUIUtility.LookLikeControls(70f);
            canvasAlign.StartOnly = EditorGUILayout.Toggle("Start Only", canvasAlign.StartOnly, GUILayout.Width(95f), GUILayout.MaxHeight(18f));
			EditorGUIUtility.labelWidth = 62f;
			EditorGUIUtility.fieldWidth = 50f;
            //EditorGUIUtility.LookLikeControls(62f, 50f);
            canvasAlign.UseCameraIdx = EditorGUILayout.Popup(" Camera:", canvasAlign.UseCameraIdx, canvasAlign.CameraNames, GUILayout.MinWidth(130f));
            if (GUILayout.Button(new GUIContent(_updateButton, "Update Camera List"), GUILayout.Width(22f), GUILayout.Height(16f)))
                canvasAlign.UpdateCameraList();
        });

		// For the script to be updated every frame (and re-check the canvas size), setdirty must be unconditional
		// And the function must be iterated on OnGUI, not OnUpdate
		EditorUtility.SetDirty(target);
	}
}
