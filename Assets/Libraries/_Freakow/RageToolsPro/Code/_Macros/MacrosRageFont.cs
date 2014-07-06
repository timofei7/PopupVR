#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary> This Macro prepares an imported SVG file, in the RageFont format, for use with RageText.
/// It must be fired with the root of the RageFont selected. 
/// It expect RageChars as children and RageChar elements as grandchildren of the RageFont root.
/// </summary>

public class MacrosRageFont : EditorWindow {
	bool processCharacters = true;
	bool processColliders = true;
    bool createSpace = true;
	float Kerning = 10f;
	float ColliderZDepth = 10f;
    float WidthPercentage = 0.4f;
    static int MaxChildCount = 0;       // Used to create empty children when needed, for char normalization

	[MenuItem("Component/RageTools/Macros/RageFont - Setup")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosRageFont),true,"RageFont Setup");
		window.maxSize = new Vector2 (250f, 160f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		GUILayout.Label("* Keep the RageFont root selected", EditorStyles.whiteMiniLabel);

		processCharacters = EditorGUILayout.BeginToggleGroup("Format Chars", processCharacters);
        EditorGUILayout.BeginHorizontal();
        createSpace = EditorGUILayout.BeginToggleGroup("Create Space", createSpace);
        EditorGUILayout.Space();
        //EditorGUIUtility.LookLikeControls(60f,10f);
		EditorGUIUtility.labelWidth = 60f;
		EditorGUIUtility.fieldWidth = 10f;
        WidthPercentage = EditorGUILayout.FloatField("Width %", WidthPercentage);
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndToggleGroup();
        //EditorGUIUtility.LookLikeControls();
		processColliders = EditorGUILayout.BeginToggleGroup("Create Colliders", processColliders);
			Kerning = EditorGUILayout.Slider("Kerning", Kerning, -10, 100);
			ColliderZDepth = EditorGUILayout.FloatField("Collider Z Depth", ColliderZDepth);
		EditorGUILayout.EndToggleGroup();
		if (GUILayout.Button("Process")) {
            var maxBounds = new Bounds();
			if (processCharacters) RageCharactersSetup(ref createSpace);
			if (processColliders) ColliderGeneration(Kerning, ColliderZDepth, out maxBounds);
            if (createSpace) {
                Bounds bounds = maxBounds;
                var halfWidth = (maxBounds.size.x * WidthPercentage)/2;
                bounds.SetMinMax(new Vector3(maxBounds.center.x - halfWidth, maxBounds.min.y, maxBounds.min.z),
                                  new Vector3(maxBounds.center.x + halfWidth, maxBounds.max.y, maxBounds.max.z));
                CreateSpace(bounds);
            }
			GetWindow(typeof(MacrosRageFont)).Close();
		}
	}

    public static void RageCharactersSetup(ref bool createSpace) {
		#region Error Detection
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select the Root Game gO of an imported Font.");
			return;
		}

		if (Selection.activeTransform.childCount == 0) {
			Debug.Log("Macro Error: Game gO has no children. Please select the RageFont root.");
			return;
		}
		#endregion Error Detection

		var keycodes = new Dictionary<string, string>();
		keycodes = InitializeKeycodes(keycodes);

		foreach (Transform rageChar in Selection.activeTransform) {
			// Activates the RageChar and contained elements temporarily
			rageChar.gameObject.RecursiveActivate(true);

			var thisRageChar = rageChar.gameObject;
			if (thisRageChar.name.EndsWith("_1_"))
				thisRageChar.name = thisRageChar.name.Replace("_1_", "");

            // When a keycode is found, replace it with the related string;
			var rageCharKeyName = thisRageChar.name;
			rageCharKeyName = rageCharKeyName.Substring(0,(int)(Mathf.Min(thisRageChar.name.Length, 5f)));
            // Disable the fill of the space character
			if (rageCharKeyName=="space") {
				var spline = thisRageChar.GetComponentInChildren<RageSpline>();
				if (spline == null)
					Debug.Log("'space' char RageSpline not found. Game gO disabled?");
				else {
                    createSpace = false;
				    spline.gameObject.GetComponent<MeshRenderer>().enabled = false;
				}
			}
			if (keycodes.ContainsKey(rageCharKeyName))
				thisRageChar.name = keycodes[rageCharKeyName];

            #region Cycle through RageChar elements and number them sequentially, while removing Widow/Empty Groups
            int counter = 1;
		    foreach (Transform rageCharElement in rageChar)
		        FlattenHierarchy(rageCharElement);

		    foreach (Transform rageCharElement in rageChar) {
		        var spline = rageCharElement.GetComponent<RageSpline>();
		        if (spline == null) {
		            rageCharElement.gameObject.SmartDestroy();
		            continue;
		        }
		        var thisRageCharElement = rageCharElement.gameObject;
		        thisRageCharElement.name = counter + "";
		        counter++;
		    }
		    if (counter > MaxChildCount) MaxChildCount = counter;

		    #endregion

		}
		// Finally, de-activates the RageFont game gO, chars and elements
		Selection.activeGameObject.RecursiveActivate(false);
	}

    private static void FlattenHierarchy(Transform rageCharElement) {
        if (rageCharElement.childCount == 0) return;
        var splines = rageCharElement.GetComponentsInChildren<RageSpline>();
        foreach (RageSpline spline in splines)
            spline.transform.parent = rageCharElement.parent;
    }

    public static void ColliderGeneration(float kerning, float colliderZDepth, out Bounds maxBounds) {
		#region Error Detection
        maxBounds = new Bounds();
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select the Root Game gO of an imported Font.");
			return;
		}

		if (Selection.activeTransform.childCount == 0) {
			Debug.Log("Macro Error: Selected Game Object has no children. Please select the RageFont root.");
			return;
		}
		#endregion Error Detection

		foreach (Transform rageChar in Selection.activeTransform) {
			var thisRageChar = rageChar.gameObject;
			// If it already has a collider attached, destroy it
			var thisCollider = thisRageChar.GetComponent<BoxCollider>();
			if (thisCollider != null) DestroyImmediate(thisCollider);

			Bounds totalBounds;
			float zPosition;
			FindCompoundBoundaries(rageChar, out totalBounds, out zPosition);
		    var boxCollider = thisRageChar.AddComponent<BoxCollider>();
			boxCollider.center = new Vector3(	(totalBounds.min.x + totalBounds.max.x) / 2, 
												(totalBounds.min.y + totalBounds.max.y) / 2,
												zPosition );
			boxCollider.size = new Vector3(	(totalBounds.max.x - totalBounds.min.x) + kerning,
											(totalBounds.max.y - totalBounds.min.y),
											colliderZDepth );
            if (totalBounds.size.x > maxBounds.size.x) maxBounds = totalBounds;
		}
	}

    private static void FindCompoundBoundaries(Transform rageChar, out Bounds totalBounds, out float zPosition) {
        totalBounds = new Bounds();
        zPosition = 0f; bool firstElement = true;
        foreach (Transform rageCharElement in rageChar) {

            var thisMeshFilter = rageCharElement.GetComponent<MeshFilter>();
            if (!thisMeshFilter) {
                Debug.Log("Macro Error: RageSpline or Mesh Filter not found for character " + rageChar.name);
                continue;
            }
            var boundary = thisMeshFilter.sharedMesh.bounds;

            // Flag Used to get a valid starting reference
            if (firstElement) {
                // Boundaries won't help with the z position due to the way RageSpline build the meshes. 
                // Thus we get the z coordinate of the first spline point, since they're all collinear anyways.
                var spline = rageCharElement.GetComponent<RageSpline>();
                if (spline) zPosition = spline.GetPositionWorldSpace(0).z;

                totalBounds.min = new Vector3(boundary.min.x, boundary.min.y, zPosition);
                totalBounds.max = new Vector3(boundary.max.x, boundary.max.y, zPosition);
                firstElement = false;
                continue;
            }

            if (boundary.min.x < totalBounds.min.x)
                totalBounds.min = new Vector3(boundary.min.x, totalBounds.min.y, totalBounds.min.z);
            if (boundary.max.x > totalBounds.max.x)
                totalBounds.max = new Vector3(boundary.max.x, totalBounds.max.y, totalBounds.max.z);
            if (boundary.min.y < totalBounds.min.y)
                totalBounds.min = new Vector3(totalBounds.min.x, boundary.min.y, totalBounds.min.z);
            if (boundary.max.y > totalBounds.max.y)
                totalBounds.max = new Vector3(totalBounds.max.x, boundary.max.y, totalBounds.max.z);
        }
/*        Debug.Log("Min: " + totalBounds.min + " Max: " + totalBounds.max);*/
    }

    private void CreateSpace(Bounds maxBounds) {
        var rageChar = new GameObject();
        rageChar.name = " ";
        rageChar.transform.localPosition = Vector3.zero;
        rageChar.transform.parent = Selection.activeTransform;
        var boxCollider = rageChar.AddComponent<BoxCollider>();
        boxCollider.center = maxBounds.center;
        boxCollider.size = maxBounds.size;
        for (int i = 0; i < MaxChildCount; i++) {
            var element = new GameObject();
            element.transform.parent = rageChar.transform;
            element.transform.localPosition = Vector3.zero;
            element.name = i+"";
            var spline = element.AddComponent<RageSpline>();
            spline.ClearPoints();
            spline.AddPoint(0, new Vector3(maxBounds.min.x, maxBounds.min.y), Vector3.zero, Vector3.zero, 1f, false);
            spline.AddPoint(1, new Vector3(maxBounds.min.x, maxBounds.max.y), Vector3.zero, Vector3.zero, 1f, false);
            spline.AddPoint(2, new Vector3(maxBounds.max.x, maxBounds.max.y), Vector3.zero, Vector3.zero, 1f, false);
            spline.AddPoint(3, new Vector3(maxBounds.max.x, maxBounds.min.y), Vector3.zero, Vector3.zero, 1f, false);
            spline.RefreshMeshInEditor(true,true,true);
            spline.Visible = false;
        }
        rageChar.RecursiveActivate(false);
    }

	/// <summary> Sets the proper key codes values, used to replace SVG-export symbol group names to their proper symbols
	/// </summary>
	private static Dictionary<string, string> InitializeKeycodes(Dictionary<string, string> keycodes) {
		keycodes = new Dictionary<string, string> {  {"_x27_", "`"}
													,{"_x2A_", "*"}
													,{"_x2B_", "+"}
													,{"_x2C_", ","}
													,{"_x2D_", "-"}
													,{"_x2E_", "/."}
													,{"_x2F_", "//"}
													,{"_x3B_", ";"}
													,{"_x3C_", "<"}
													,{"_x3D_", "="}
													,{"_x3E_", ">"}
													,{"_x3F_", "?"}
													,{"_x5B_", "["}
													,{"_x5C_", "\\"}
													,{"_x5D_", "]"}
													,{"_x5F_", "_"}
													,{"_x21_", "!"}
													,{"_x22_", "\""}
													,{"_x23_", "#"}
													,{"_x24_", "$"}
													,{"_x25_", "%"}
													,{"_x26_", "&"}
													,{"_x28_", "("}
													,{"_x29_", ")"}
													,{"_x30_", "0"}			
													,{"_x31_", "1"}
													,{"_x32_", "2"}
													,{"_x33_", "3"}
													,{"_x34_", "4"}
													,{"_x35_", "5"}
													,{"_x36_", "6"}			
													,{"_x37_", "7"}
													,{"_x38_", "8"}
													,{"_x39_", "9"}
													,{"_x40_", "@"}
													,{"space", " "}
		};
		return keycodes;
	}
}
#endif
