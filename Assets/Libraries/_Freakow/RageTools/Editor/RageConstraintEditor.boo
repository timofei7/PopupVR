import UnityEngine
//import UnityEditor
[CustomEditor(typeof(RageConstraint))]
public class RageConstraintEditor (RageToolsEdit): 
	
	private _rageConstraint as RageConstraint
	private _Edgetune as RageEdgetune
	private _refreshButton as Texture2D = Resources.Load('refresh', Texture2D)

	public def OnDrawInspectorHeaderLine():
		_rageConstraint = target if _rageConstraint == null
		
		LookLikeControls(20f, 1f)
		EasyToggle "Visible", _rageConstraint.Visible, MaxWidth(60f)
		LookLikeControls(60f, 1f)		
		EasyObjectField	"Follower:", _rageConstraint.Follower, typeof(GameObject)
		if GUILayout.Button(GUIContent(_refreshButton, "Refresh"), GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
			_rageConstraint.AttachedComponentsCheck()

	public def OnDrawInspectorGUI():
		_rageConstraint = target if _rageConstraint == null
		
		LookLikeControls(60f)
		EasyRow:		
			EasyToggle "Position", _rageConstraint.FollowPosition
			EasyToggle "Rotation", _rageConstraint.FollowRotation		
			EasyToggle "Scale", _rageConstraint.FollowScale
			EasyToggle "Local", _rageConstraint.Local

		if _rageConstraint.FollowerIsGroup:
			EasyRow:
				//EasyToggle "Visible", _rageConstraint.Visible, MaxWidth(80f)
				if _rageConstraint.FollowerIsGroup:
					if _rageConstraint.FollowerGroup.Proportional:	
						LookLikeControls(110f,30f)
						EasyPercent "Opacity x", _rageConstraint.FollowerGroup.OpacityMult, 1
					else:
						LookLikeControls(110f,30f)
						EasyPercent "Opacity", _rageConstraint.FollowerGroup.Opacity, 1

		EasySettings:
			EasyRow:
				LookLikeControls(70f, 10f)
				EasyFloatField "Snap: Pos", _rageConstraint.PositionSnap, MaxWidth(100f)
				LookLikeControls(30f, 1f)
				EasyFloatField "Rot", _rageConstraint.RotationSnap, MaxWidth(60f)
				EasyFloatField "Scl", _rageConstraint.ScaleSnap, MaxWidth(60f)
			if (_rageConstraint.FollowPosition):
				EasyRow:
					GUILayout.Label("Position:", MaxWidth(60f))
					EasyToggle "x", _rageConstraint.FollowPositionX, MaxWidth(30f)
					EasyToggle "y", _rageConstraint.FollowPositionY, MaxWidth(30f)
					EasyToggle "z", _rageConstraint.FollowPositionZ, MaxWidth(30f)
			if (_rageConstraint.FollowRotation):
				EasyRow:
					GUILayout.Label("Rotation:", MaxWidth(60f))
					EasyToggle "x", _rageConstraint.FollowRotationX, MaxWidth(30f)
					EasyToggle "y", _rageConstraint.FollowRotationY, MaxWidth(30f)
					EasyToggle "z", _rageConstraint.FollowRotationZ, MaxWidth(30f)
			if (_rageConstraint.FollowScale):
				EasyRow:
					GUILayout.Label("Scale:", MaxWidth(60f))
					EasyToggle "x", _rageConstraint.FollowScaleX, MaxWidth(30f)
					EasyToggle "y", _rageConstraint.FollowScaleY, MaxWidth(30f)
					EasyToggle "z", _rageConstraint.FollowScaleZ, MaxWidth(30f)

		EditorUtility.SetDirty (_rageConstraint)
		if not _rageConstraint.FollowerGroup == null:
			EditorUtility.SetDirty (_rageConstraint.FollowerGroup)

