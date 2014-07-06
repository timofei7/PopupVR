import UnityEngine
import UnityEditor

[CustomEditor(typeof (RagePivotools))]
public class RagePivotoolsEditor (RageToolsEdit): 

	private _pivotools as RagePivotools

	protected override def OnDrawInspectorHeaderLine():
		_pivotools = target if _pivotools == null

		ifdef not UNITY_FLASH:
			GUILayout.Space(7f)
			EasyToggle "In Place", _pivotools.InPlace, MaxWidth(65f)
			GUILayout.Space(10f)
			EasyToggle "Preserve Child Pivots", _pivotools.PreserveChildPivots
	

	ifdef not UNITY_FLASH:

		protected override def OnDrawInspectorGUI():
			_pivotools = target if _pivotools == null
			EasyRow:
				//EasyCol:
				EditorGUILayout.BeginVertical(GUILayout.MaxWidth(120f))
				GUILayout.Space(4f)
				LookLikeControls(38f, 20f)
				EasyPopup "Mode", _pivotools.CenteringType, MinWidth(120f), MaxWidth(120f)
				EditorGUILayout.EndVertical()
				EasyCol:
					if GUILayout.Button("Apply", GUILayout.MaxHeight(20f)): 
						//Undo.RegisterSceneUndo("RagePivotools: Applied new Pivot")
						Undo.RecordObject(target,"RagePivotools: Applied new Pivot");
						_pivotools.CenterPivot()				
			EasyRow:
				if GUILayout.Button("Freeze Rotation & Scale", GUILayout.MinHeight(23f), GUILayout.MaxHeight(16f)):
					Undo.RecordObject(target,"RagePivotools: Freeze Rotation+Scale")
					_pivotools.FreezeRotationAndScale()

			if _pivotools.CenteringType == RagePivotools.CenteringMode.Reference:
				EasyRow:
					GUILayout.Label("Reference:", GUILayout.MaxWidth(70f))
					EasyObjectField "", _pivotools.RefTransform, Transform
				if _pivotools.RefTransform==null:
					Warning("Please assign the reference Transform.")

			if _pivotools.CenteringType == RagePivotools.CenteringMode.PerBranch:
				EasyRow:
					EasyToggle "Delete Pivot References", _pivotools.DeletePivotReferences

			if _pivotools.CenteringType == RagePivotools.CenteringMode.Interactive:
				EasyRow:
					LookLikeControls(80f, 20f)
					EasyFloatField "Gizmo Scale", _pivotools.GizmoSizeMult, GUILayout.MaxWidth(120f)
				Warning("Drag the gizmo and press 'Enter' to set the pivot")

		public def ConsumeEvent():
			EditorUtility.SetDirty(target)
			Event.current.Use()
			
		public def OnSceneGUI():
			_pivotools = target if _pivotools == null
			if _pivotools.CenteringType == RagePivotools.CenteringMode.Interactive:
				//Debug.Log(_pivotools.RefPosition)
				_pivotools.RefPosition = Handles.FreeMoveHandle(_pivotools.RefPosition,Quaternion.identity,_pivotools.GizmoBaseSize*_pivotools.GizmoSizeMult,Vector3.zero,Handles.ConeCap)
				_pivotools.RefPosition = Vector3(_pivotools.RefPosition.x, _pivotools.RefPosition.y, _pivotools.transform.position.z)

			if (Event.current.type == EventType.KeyDown):
				if (Event.current.keyCode == KeyCode.Return or Event.current.keyCode == KeyCode.P):
					Undo.RecordObject(target,"RagePivotools: Applied new Pivot")
					_pivotools.CenterPivot()
					ConsumeEvent()
				if (Event.current.keyCode == KeyCode.G):
					_pivotools.CenteringType = RagePivotools.CenteringMode.Geometric
					ConsumeEvent()
				if (Event.current.keyCode == KeyCode.R):
					_pivotools.CenteringType = RagePivotools.CenteringMode.Reference
					ConsumeEvent()
				if (Event.current.keyCode == KeyCode.T):
					_pivotools.CenteringType = RagePivotools.CenteringMode.PerItem
					ConsumeEvent()
				if (Event.current.keyCode == KeyCode.B):
					_pivotools.CenteringType = RagePivotools.CenteringMode.PerBranch
					ConsumeEvent()
				if (Event.current.keyCode == KeyCode.I):
					_pivotools.CenteringType = RagePivotools.CenteringMode.Interactive
					ConsumeEvent()
