import UnityEngine
//import Com.Freakow.BooInspector

[CustomEditor(typeof(RageEdgetune))]
public class RageEdgetuneEditor (RageToolsEdit): 

	private _edgetune as RageEdgetune
	private _updateButton = Resources.Load("refresh", typeof(Texture2D)) as Texture2D
	
	public def OnDrawInspectorHeaderLine():
		_edgetune = target if _edgetune == null	
		
		EasyToggle "Live", _edgetune.On, MaxWidth(60f)
		EasyToggle "Start Only", _edgetune.StartOnly, GUILayout.MaxWidth(96f)
//		if _edgetune.Data == null:
//			_edgetune.Data = ScriptableObject.CreateInstance(typeof(RageEdgetuneData))
			
		if GUILayout.Button("Initialize"):
			//Undo.SetSnapshotTarget(target, "RageEdgetune Initialize")
			Undo.RecordObject(target, "RageEdgetune Initialize")
			//CreateAndRegisterUndoSnapshot(target)
			_edgetune.ScheduleInitialize()
				
	public def OnDrawInspectorGUI():
		_edgetune = target if _edgetune == null
		_edgetune.RefreshCheck()
		
		EasyRow:
			EasyToggle "Auto Density", _edgetune.Data.AutomaticLod, MinWidth(100f), MaxWidth(100f)
			if _edgetune.Data.AutomaticLod:			
				LookLikeControls(30, 10)		
				EasyIntField "Max", _edgetune.Data.MaxDensity, MaxWidth(56)
				LookLikeControls(120f)
				if GUILayout.Button("Guess"):
					CreateAndRegisterUndoSnapshot(target)
					_edgetune.GuessMaxDensity()
		
		EasySettings:
			EasyRow:
				LookLikeControls(73f)
				EasyObjectField "RageGroup", _edgetune.Group, typeof(RageGroup)
					
			EasyRow:
				LookLikeControls(65f, 1f)		
				EasyFloatField "AA Factor", _edgetune.Data.AaFactor, MaxWidth(105f)						
				LookLikeControls(90f, 1f)
				EasyFloatField "Density Factor", _edgetune.Data.DensityFactor

			EasyRow:
				LookLikeControls(105f, 1f)
				EasyFloatField "Perspective Blur", _edgetune.Data.PerspectiveBlur

			EasyRow:
				LookLikeControls(100f, 60f)
				EasyObjectField "Debug TextMesh", _edgetune.DebugTextMesh, typeof(TextMesh)
				EasyToggle "Density", _edgetune.DebugDensity, MaxWidth(60f)

			EasyRow:
				LookLikeControls(60f, 1f)
				_edgetune.UseCameraIdx = EditorGUILayout.Popup("Camera", _edgetune.UseCameraIdx, _edgetune.CameraNames, GUILayout.MinWidth(210f))
				if (GUILayout.Button(GUIContent(_updateButton, "Update Camera List"), GUILayout.Width(22f), GUILayout.Height(16f))):
					_edgetune.UpdateCameraList()

		EditorUtility.SetDirty(_edgetune)
		EditorUtility.SetDirty(_edgetune.Group) if (_edgetune.Group)
