[CustomEditor(typeof(RageIkJoint))]
public class RageIkJointEditor (RageToolsEdit): 
	
	private _rageIkJoint as RageIkJoint

	protected override def OnDrawInspectorHeaderLine():
		_rageIkJoint = target if _rageIkJoint == null
		EasyRow:
			EasyToggle "Live", _rageIkJoint.Live
			EasyFloatField "Rest Angle", _rageIkJoint.RestAngle	
		
	protected override def OnDrawInspectorGUI():
		_rageIkJoint = target if _rageIkJoint == null
		EasyRow:
			LookLikeControls(70f, 30f)
			EasyFloatField "Min Angle", _rageIkJoint.MinLimiterAngle
			EasyFloatField "Max Angle", _rageIkJoint.MaxLimiterAngle
		EasyRow:
			EasyToggle "Draw gizmos", _rageIkJoint.DrawGizmos
			if _rageIkJoint.DrawGizmos:	
				LookLikeControls(50f,50f)	
				EasyFloatField "Radius", _rageIkJoint.GizmoRadius	
		
	public def OnSceneGUI():
		_rageIkJoint = target if _rageIkJoint == null
		
		return if (not _rageIkJoint.DrawGizmos)
		
		MaxAngle as Vector3
		MaxAngle = _rageIkJoint.MaxAngle  * _rageIkJoint.GizmoRadius
		MaxAngle = Handles.PositionHandle (MaxAngle + _rageIkJoint.transform.position, Quaternion.identity) - _rageIkJoint.transform.position
		_rageIkJoint.MaxAngle = MaxAngle;
		
		MinAngle as Vector3
		MinAngle = _rageIkJoint.MinAngle  * _rageIkJoint.GizmoRadius
		MinAngle = Handles.PositionHandle (MinAngle + _rageIkJoint.transform.position, Quaternion.LookRotation(Vector3.down)) - _rageIkJoint.transform.position
		_rageIkJoint.MinAngle = MinAngle;
			
		Handles.color = Color(1,1,1,0.2)	
		Handles.DrawSolidArc(_rageIkJoint.transform.position, Vector3.forward, MaxAngle , _rageIkJoint.AngleLimits(), _rageIkJoint.GizmoRadius)
		
		Handles.color = Color(1,0,0,1) 
		if (_rageIkJoint.ValidVector(_rageIkJoint.RestDirection)):
	    	Handles.color = Color(0,1,0,1)
		Handles.DrawLine(_rageIkJoint.transform.position, _rageIkJoint.transform.position + _rageIkJoint.RestDirection * _rageIkJoint.GizmoRadius)
