using System;
using System.Reflection;
// using System.Xml.Serialization;
// using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

//namespace Rage {
	internal static class RageToolsExtension {

		/// <summary> Updates the Vertex Density on every element of a path list.</summary>
		/// <param name="list"></param>
		/// <param name="density">Absolute density or additive factor, per path</param>
		public static void UpdateDensity (this List<RageGroupElement> list, int density) {
			if (list == null) return;
			foreach (RageGroupElement rageGroupItem in list) {
				ISpline path = rageGroupItem.Spline;
				if (!path.IsVisible()) continue;
				if (path.VertexDensity == density) continue;

				path.VertexDensity = density;
			}
		}

		/// <summary> Multiplies the Vertex Density on every element of a path list by a factor.</summary>
		/// <param name="list">Group List</param>
		/// <param name="densityMultiplier">Factor to multiply the default density by</param>
		/// <param name="maxDensity">Maximum Density cap to prevent performance issues</param>
		public static void UpdateDensity (this List<RageGroupElement> list, float densityMultiplier, int maxDensity) {
			if (list == null) return;
			foreach (RageGroupElement rageGroupItem in list) {
				if (rageGroupItem == null || rageGroupItem.Spline == null)
					continue;
				rageGroupItem.Spline.VertexDensity = Mathf.RoundToInt(
				   Mathf.Clamp(rageGroupItem.DefaultDensity * densityMultiplier, 1, maxDensity));
			}
		}

		/// <summary> Updates the Anti-Aliasing on every element of a RageGroup.  </summary>
		/// <param name="list">Group List</param>
		/// <param name="antiAlias">Absolute AA or multiplying factor, per path</param>
		/// <param name="proportional">If proportional, antiAlias is a multiplying factor to the default value</param>
		/// <param name="refresh">If false, only updates the AA data, doesn't refresh</param>
		public static void UpdateAa (this List<RageGroupElement> list, float antiAlias, bool proportional, bool refresh) {
			if (list == null) return;
			foreach (RageGroupElement rageGroupItem in list) {
				if (rageGroupItem == null || rageGroupItem.Spline == null)
					continue;
				ISpline path = rageGroupItem.Spline;
				if (path == null) continue;
				if (proportional)
					path.Outline.AntialiasingWidth = rageGroupItem.DefaultAa * antiAlias;
				else
					path.Outline.AntialiasingWidth = antiAlias;
				if (refresh) path.Rs.RefreshMesh (true, true, true);
			}
		}

        /// <summary> Updates the Outline Width of every element of a RageGroup.  </summary>
        /// <param name="list">Group List</param>
        /// <param name="outlineWidth">Absolute outline Width or multiplying factor, per path</param>
        /// <param name="proportional">If proportional, outlineWidth is a multiplying factor to the current value</param>
        /// <param name="refresh">If false, only updates the AA data, doesn't refresh</param>
        public static void UpdateOutlineWidth(this List<RageGroupElement> list, float outlineWidth, bool proportional, bool refresh) {
            if (list == null) return;
            foreach (RageGroupElement rageGroupItem in list) {
                ISpline path = rageGroupItem.Spline;
                if (path == null) continue;
                if (proportional)
                    path.Outline.Width = path.Outline.Width * outlineWidth;
                else
                    path.Outline.Width = outlineWidth;
                if (refresh) path.Rs.RefreshMesh(true, true, true);
            }
        }

        public static void SetLocalAa(this List<RageGroupElement> list, bool value, bool refresh = true) {
            if (list == null) return;
            foreach (RageGroupElement rageGroupItem in list) {
                ISpline path = rageGroupItem.Spline;
                if (path == null) continue;
                path.Rs.WorldAaWidth = value;
                if (refresh) path.Rs.RefreshMesh(true, true, true);
            }
        }

		/// <summary> Updates the Opacity on every element of a RageGroup.  </summary>
		/// <param name="list">Group List</param>
		/// <param name="opacity">Absolute opacity or multiplying factor, per path</param>
		/// <param name="proportional">If proportional, opacity is a multiplying factor to the default value</param>
		/// <param name="fastMode">If false, makes a full, "quality" refresh</param>
		/// <param name="refresh">If false, only updates the opacity data, doesn't refresh</param>
		public static void UpdateOpacity (this List<RageGroupElement> list, float opacity, bool proportional, bool refresh) {
			if (list == null) return;
			foreach (RageGroupElement rageGroupItem in list) {
				ISpline path = rageGroupItem.Spline;
				if (path == null) continue;
				Color color;
				if (proportional) {
					color = path.FillColor;
					path.FillColor = new Color (color.r, color.g, color.b, rageGroupItem.DefaultFillColor1.a * opacity);
					color = path.Rs.GetFillColor2();
					path.Rs.SetFillColor2 (new Color (color.r, color.g, color.b, rageGroupItem.DefaultFillColor2.a * opacity));
					color = path.OutlineColor;
					path.OutlineColor = new Color (color.r, color.g, color.b, rageGroupItem.DefaultOutlineColor.a * opacity);
				} else {
					color = path.FillColor;
					path.FillColor = new Color (color.r, color.g, color.b, opacity);
					color = path.Rs.GetFillColor2();
					path.Rs.SetFillColor2 (new Color (color.r, color.g, color.b, opacity));
					color = path.OutlineColor;
					path.OutlineColor = new Color (color.r, color.g, color.b, opacity);
				}
				if (refresh) path.Rs.RefreshMesh (true, true, true);
			}
		}

        /// <summary> Updates the 'optimize' setting on all group elements. Angle is optional, won't update if not provided. </summary>
        public static void UpdateOptimize(this List<RageGroupElement> list, bool optimize, float angle = -1f) {
            if (list == null) return;
            foreach (RageGroupElement rageGroupItem in list) {
                rageGroupItem.Spline.Optimize = optimize;
                if (!Mathf.Approximately(angle, -1f))
                    rageGroupItem.Spline.OptimizeAngle = angle;
            }
        }

		public static void CleanupElementNames (this GameObject gameObj) {
			if (gameObj == null) return;
			foreach (Transform transform in gameObj.GetComponentsInChildren<Transform>()) {
				if (!transform.name.Contains ("_")) continue;
				transform.name = transform.name.Split ('_')[0];
			}
		}

		public static void SmartDestroy (this GameObject gameObject) {
			if (gameObject == null) return;
			MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
			if (meshFilters != null)
				foreach (MeshFilter meshFilter in meshFilters)
					Object.DestroyImmediate (meshFilter.sharedMesh);
			if (Application.isEditor) {
				Object.DestroyImmediate (gameObject);
				return;
			}
			Object.Destroy(gameObject);
		}

		public static void SmartDestroy(this Object objectToDestroy) {
			if (objectToDestroy == null) return;
			if (Application.isEditor) {
				Object.DestroyImmediate(objectToDestroy);
				return;
			}
			Object.Destroy(objectToDestroy);
		}

		public static bool RemoveOverlap (this IRageSpline path, int index0, int index1, float snapRadius, bool debug) {
			if (index1 == 0) index0 = path.GetPointCount() - 1;
			if (Vector3.Distance (path.GetPositionWorldSpace (index0), path.GetPositionWorldSpace (index1)) <= snapRadius) {
				if (debug) Debug.Log ("overlap path pos = " + path.GetPositionWorldSpace (index0));
				path.SetNatural (index1, false);
				path.SetInControlPositionWorldSpace (index1, path.GetInControlPositionWorldSpace (index0));
				path.RemovePoint (index0); // Then deletes the duplicate previous point
				return true;
			}
			return false;
		}

		/// <summary> Special case for start-end points, where the out tangent must be copied instead of the in tangent </summary>
		public static bool MergeStartEndPoints (this IRageSpline rageSpline, bool debug) {
			if (rageSpline.GetPointCount() <= 2) return false;
			var lastPointIdx = rageSpline.GetPointCount() - 1;
			var lastPointPos = rageSpline.GetPositionWorldSpace (lastPointIdx);
			var firstPointPos = rageSpline.GetPositionWorldSpace (0);
			if (Vector3.Distance (firstPointPos, lastPointPos) < 0.0001f) {
				if (debug) Debug.Log ("\t Removing endpoint overlap ");
				rageSpline.SetInControlPositionWorldSpace (0, rageSpline.GetInControlPositionWorldSpace (lastPointIdx));
				rageSpline.RemovePoint (lastPointIdx); // Then deletes the last point
				return true;
			}
			return false;
		}

		/// <summary> If it's in editor, get Game View size through reflection </summary>
		/// <returns></returns>
		public static Vector2 GetGameViewSize( ) {
			if (!Application.isEditor)
				return new Vector2(Screen.width, Screen.height);

			Type gameView = Type.GetType ("UnityEditor.GameView,UnityEditor");
			if (gameView == null) return Vector2.zero;

			MethodInfo methodInfo = gameView.GetMethod ("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
			System.Object res = methodInfo.Invoke (null, null);
			return (Vector2) res;
		}

		//***
		//*** Formerly on Util.cs (Commons/Code)
		//***

		public static void RecursiveActivate(this IEnumerable<GameObject> collection, bool status) {
			foreach (GameObject go in collection)
				go.SetActive(status);
        }

		public static RageGroup FindParentRageGroup(this GameObject go) {
			var rageGroup = go.transform.GetComponent<RageGroup>();
			if (rageGroup != null) return rageGroup;
			if (go.transform.parent == null) return null;
			var parentGameObject = go.transform.parent.gameObject;
			return parentGameObject == null ? null : FindParentRageGroup(parentGameObject);
		}

        public static void RecursiveActivate(this GameObject go, bool status) {
			go.SetActive(status);
        }

		public static string FindPath(this Transform transform) {
			if (transform.parent != null)
				return FindPath(transform.parent) + "/" + transform.gameObject.name;

			return transform.gameObject.name;
		}

// 		public static string Serialize<T>(this T pool) {
// 			var serializer = new XmlSerializer(pool.GetType());
// 			var writer = new StringWriter();
// 			serializer.Serialize(writer, pool);
// 			return writer.ToString();
// 		}
// 
// 		public static T Deserialize<T>(this string xml) {
// 			var serializer = new XmlSerializer(typeof(T));
// 			var reader = new StringReader(xml);
// 			return (T) serializer.Deserialize(reader);
// 		}

		public static bool PolygonContainsPoint(Vector2[] polygon, Vector2 point) {
			var j = polygon.Length - 1;
			var oddNodes = false;

			for (var i = 0; i < polygon.Length; i++) {
				if (polygon[i].y < point.y && polygon[j].y >= point.y || polygon[j].y < point.y && polygon[i].y >= point.y)
					if (polygon[i].x + (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < point.x)
						oddNodes = !oddNodes;
				j = i;
			}

			return oddNodes;
		}

        public static String[] List(this Camera camera) {
            var cameraNames = new string[Camera.allCameras.Length];
            for (int i = 0; i < Camera.allCameras.Length; i++)
                cameraNames[i] = Camera.allCameras[i].name;

            return cameraNames;
        }

        //CameraNames = Camera.List();

	}
//}

