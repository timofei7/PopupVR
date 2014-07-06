using UnityEngine;

/// <summary> Constrains (follows) a certain object transform position, rotation and scale, selectively </summary>
[AddComponentMenu("RageTools/Rage Constraint")]
[ExecuteInEditMode]
public class RageConstraint : MonoBehaviour {

	[SerializeField]private GameObject _follower;
	public GameObject Follower {
		get { return _follower; }
		set {
			if (_follower == value) return;
			_follower = value;
			if (_follower == null) return;
			AttachedComponentsCheck();
		}
	}
    [SerializeField]private Transform _followerTransform;
	public Transform FollowerTransform {
		get {	if (_followerTransform == null) _followerTransform = Follower.transform;
				return _followerTransform; }
		set {	_followerTransform = value; }
	}
// 	public bool GroupVisible {
// 		get {	if (!FollowerIsGroup) return false;
// 				Visible = FollowerGroup.Visible;
// 				return Visible;
// 			}
// 		set { SetGroupVisibility(value); }
// 	}
	//public bool Live;
	public bool FollowPosition, FollowPositionX = true, FollowPositionY = true, FollowPositionZ = true;
	public bool FollowRotation, FollowRotationX = true, FollowRotationY = true, FollowRotationZ = true;
	public bool FollowScale, FollowScaleX = true, FollowScaleY = true, FollowScaleZ = true;
	public bool Local;

	[SerializeField]private bool _visible;
	public bool Visible {
		get { return _visible; }
		set {
			_visible = value;
            if (FollowerIsGroup) { FollowerGroup.Visible = value; return; }
            if (FollowerIsSpline) FollowerSpline.Visible = value;
		}
	}

	public float RotationSnap;
	public float PositionSnap;
	public float ScaleSnap;
	public RageGroup FollowerGroup;
	public bool FollowerIsGroup { get { return FollowerGroup != null; } }
	private RageEdgetune _edgetune;
    public RageSpline FollowerSpline;
    private bool FollowerIsSpline { get { return FollowerSpline != null; } }

    public void Update() {
		if (Follower == null) return;
		UpdateFollower();

		if (_edgetune == null) return;
		_edgetune.Group.QueueRefresh();
	}

	private void UpdateFollower( ) {
		if (FollowPosition) CopyPosition();

		if (FollowRotation) CopyRotation();

		if (FollowScale) CopyScale();
	}

	private void CopyScale( ) {
		if (!FollowerTransform.localScale.Equals (transform.localScale))
			FollowerTransform.localScale = Mathf.Approximately(ScaleSnap, 0f) ? transform.localScale
											: Vector3.Lerp(Follower.transform.localScale, transform.localScale, ScaleSnap * Time.deltaTime);
		//TODO: .lossyScale
	}

	private void CopyPosition( ) {
		if (Local) {
			if (!FollowerTransform.localPosition.Equals(transform.localPosition))
				FollowerTransform.localPosition = transform.localPosition;
			return;
		}
		CopyTransformPosition();
	}

    private void CopyTransformPosition( ) {
		if (FollowerTransform.position.Equals(transform.position)) return;
		Vector3 targetPositon = (FollowPositionX && FollowPositionY && FollowPositionZ)? 
								transform.position
		                        : new Vector3 (	FollowPositionX? transform.position.x : FollowerTransform.position.x,
												FollowPositionY? transform.position.y : FollowerTransform.position.y,
												FollowPositionZ? transform.position.z : FollowerTransform.position.z);
		FollowerTransform.position = Mathf.Approximately(PositionSnap, 0f)
										? targetPositon
										: Vector3.Lerp(FollowerTransform.position, targetPositon, PositionSnap * Time.deltaTime);
	}

	private void CopyRotation( ) {
		if (Local) {
			if (!FollowerTransform.localRotation.Equals(transform.localRotation))
				FollowerTransform.localRotation = transform.localRotation;
			return;
		}
		CopyTransformRotation();
	}

    private void CopyTransformRotation() {
        if (!FollowerTransform.rotation.Equals(transform.rotation))
            FollowerTransform.rotation = Mathf.Approximately(RotationSnap, 0f)
                                             ? transform.rotation
                                             : Quaternion.Slerp(FollowerTransform.rotation, transform.rotation,
                                                                RotationSnap*Time.deltaTime);
    }

    public void AttachedComponentsCheck() {
        FollowerGroup = _follower.GetComponent<RageGroup>();
        FollowerSpline = _follower.GetComponent<RageSpline>();
        _edgetune = Follower.GetComponent<RageEdgetune>();
    }

    // v^v^v^v^v^
	// SWITCHSETS 
	// v^v^v^v^v^
	// (Pass-through methods)

	/// <summary> Switches the first occurrence of an Item Id found in the switchsets </summary>
	public void SwitchsetItem (string switchItem) {
		if (!FollowerIsGroup) return;
		FollowerGroup.SwitchsetItem(switchItem);
	}

}