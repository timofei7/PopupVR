using System.Reflection;
using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
[AddComponentMenu("RageTools/Rage CanvasAlign")]
public class RageCanvasAlign : MonoBehaviour {

	public enum HorizontalAlignType { Left = 0, Right = 1, Center = 2 }
	public enum VerticalAlignType { Top = 0, Bottom = 1, Center = 2 }

	public BoxCollider Boundaries;
	public HorizontalAlignType HorizontalAlign;
	public VerticalAlignType VerticalAlign;
    public string[] CameraNames;
    [SerializeField]private int _useCameraIdx;
    public int UseCameraIdx {
        get { return _useCameraIdx; }
        set {
            if (value == _useCameraIdx) return;
            _useCameraIdx = value;
            UseCamera = Camera.allCameras[_useCameraIdx];
            if (UseCamera.GetComponent<RageCamera>() == null)
                UseCamera.gameObject.AddComponent<RageCamera>();
        }
    }

    [SerializeField]private Camera _useCamera;
    public Camera UseCamera {
        get {
            if (!_useCamera) _useCamera = Camera.main;
            return (_useCamera);
        }
        set { _useCamera = value; }
    }

    [SerializeField]private bool _startOnly;
	public bool StartOnly {
		get { return _startOnly; }
		set {
			if (_startOnly==value) return;
			_startOnly = value;
			UpdateActions();
		}
	}

	private float _offsetX;
	private float _offsetY;
	private Type _gameView;
	public Action UpdateAction;
	public Action StartAction;

	public void NoAction() {}

	public void Awake() {
		if (UpdateAction == null || StartAction == null)
			UpdateActions();
        UpdateCameraList();
		Boundaries = GetComponent<BoxCollider>();
        if (Boundaries != null) {
            Boundaries.enabled = true;
            Boundaries.isTrigger = true;
        }
        UpdateAction.Invoke();
	}

	public void OnGUI() {
		if (UpdateAction == null) UpdateActions();  
		if (UpdateAction == NoAction) return;
        if(UpdateAction == null) return;  
		UpdateAction.Invoke(); }

	public void Start() {
		if (StartAction == null) UpdateActions(); 
		if (StartAction == NoAction) return;
        if(StartAction == null) return;  
		StartAction.Invoke(); }

	public void UpdateActions() {
		StartAction = _startOnly ? DoAlignToCanvas : (Action)NoAction;
		UpdateAction = _startOnly ? (Action)NoAction : DoAlignToCanvas;
	}

    public void UpdateCameraList() {
        CameraNames = camera.List();
    }

	public void DoAlignToCanvas() {

		if (UseCamera.orthographic &&
			Mathfx.Approximately(UseCamera.orthographicSize, 0)) return;

		var screenSize = GetGameViewSize();
		var bottomLeft = UseCamera.ScreenToWorldPoint(new Vector3(0, 0, UseCamera.nearClipPlane));
		var topRight = UseCamera.ScreenToWorldPoint(new Vector3(screenSize.x, screenSize.y, UseCamera.farClipPlane));

		if (Boundaries == null) return;

		switch (HorizontalAlign) {

			case HorizontalAlignType.Left:
				_offsetX = bottomLeft.x + transform.lossyScale.x * (- Boundaries.center.x + Boundaries.size.x / 2 );
				break;

			case HorizontalAlignType.Right:
				_offsetX = topRight.x + transform.lossyScale.x * ( - Boundaries.center.x - Boundaries.size.x / 2);
				break;

			case HorizontalAlignType.Center:
				_offsetX = (topRight.x + bottomLeft.x) / 2 + transform.lossyScale.x * -Boundaries.center.x; 
				break;
		}

		switch (VerticalAlign){

			case VerticalAlignType.Bottom:
				_offsetY = bottomLeft.y + transform.lossyScale.x * ( - Boundaries.center.y + (Boundaries.size.y/2)); 
				break;

			case VerticalAlignType.Top:
				_offsetY =  topRight.y + transform.lossyScale.x * ( - Boundaries.center.y - (Boundaries.size.y/2));
				break;

			case VerticalAlignType.Center:
				_offsetY = (topRight.y + bottomLeft.y) / 2 + transform.lossyScale.x * -Boundaries.center.y;
				break;
		}

		transform.position = new Vector3(_offsetX, _offsetY, transform.position.z);
	}

	/// <summary> Get Camera view size. If it's in editor, get Game View size through reflection </summary>
	public static Vector2 GetGameViewSize() {
		if (!Application.isEditor)
		return new Vector2(Screen.width, Screen.height);
		Type gameView = Type.GetType("UnityEditor.GameView,UnityEditor");
		if (gameView == null) return Vector2.zero;
		
		MethodInfo methodInfo = gameView.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
		System.Object res = methodInfo.Invoke(null, null);
		return (Vector2) res;
	}
}
