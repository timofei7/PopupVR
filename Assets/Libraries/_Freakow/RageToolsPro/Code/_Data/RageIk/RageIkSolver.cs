using UnityEngine;

public static class RageIkSolver {

	const int MaxIterations = 20;
	const float Tolerance = 0.0001f;

	private static bool IsTargetUnreachable(RageIkChain chain) {
		float rootToTargetDist = Vector3.Distance(chain.Joints[0].position, chain.Target.position);
		return (rootToTargetDist > chain.Length);
	}

	public static void Solve(RageIkChain chain, bool ccwBias) {
		if (chain.Joints.Count < 2) return;
		chain.Init();

		if (IsTargetUnreachable(chain)) {
			for (int i = 0; i < chain.Joints.Count - 1; i++) {
				Quaternion rotation = Quaternion.FromToRotation(chain.Joints[i + 1].position - chain.Joints[i].position,
					chain.Target.position - chain.Joints[i].position);
				AddRotation(chain.Joints[i], rotation, chain);

				var limiter = chain.Joints[i].GetComponent<RageIkJoint>();
				if (limiter == null || !limiter.Live) continue;

				if (limiter.ValidVector(limiter.RestDirection)) continue;
				if (Vector3.Angle(limiter.RestDirection, limiter.MaxAngle) > Vector3.Angle(limiter.RestDirection, limiter.MinAngle)) {
					AddRotation(chain.Joints[i], Quaternion.FromToRotation(limiter.RestDirection, limiter.MinAngle), chain);
					continue;
				}
				AddRotation(chain.Joints[i], Quaternion.FromToRotation(limiter.RestDirection, limiter.MaxAngle), chain);
			}
            if (chain.AlignEnd) ChangeLastElementRotation(chain);
			return;
		}

		int tries = 0;

		Vector3 rootInitial = chain.Joints[0].position;

		float targetDistance = Vector3.Distance(chain.Joints[chain.Joints.Count - 1].position, chain.Target.position);

		var desiredPositions = new Vector3[chain.Joints.Count];
		for (int i = 0; i < chain.Joints.Count; i++) {
			desiredPositions[i] = chain.Joints[i].position;
		}

		while (targetDistance > Tolerance && tries < MaxIterations) {
			ForwardReachingPhase (chain, desiredPositions);
			BackwardReachingPhase (chain, desiredPositions, rootInitial, ccwBias);

			targetDistance = Vector3.Distance(desiredPositions[chain.Joints.Count - 1], chain.Target.position);
			tries++;
		}
	}

	private static void ForwardReachingPhase (RageIkChain chain, Vector3[] desiredPositions) {
		desiredPositions[chain.Joints.Count - 1] = chain.Target.position;
		
		for (int i = desiredPositions.Length - 2; i > 0; i--){
			float lambda = chain.SegmentLengths[i] / Vector3.Distance (desiredPositions[i + 1], desiredPositions[i]);
			desiredPositions[i] = (1 - lambda) * desiredPositions[i + 1] + lambda * desiredPositions[i];
		}
	}

	private static void BackwardReachingPhase (RageIkChain chain, Vector3[] desiredPositions, Vector3 rootInitial, bool ccwBias) {
		desiredPositions[0] = rootInitial;

		for (int i = 0; i < chain.Joints.Count - 1; i++) {
			float lambda = chain.SegmentLengths[i] / Vector3.Distance (desiredPositions[i + 1], desiredPositions[i]);
			desiredPositions[i + 1] = (1 - lambda) * desiredPositions[i] + lambda * desiredPositions[i + 1];
            if (i == 0)
                desiredPositions[i + 1] = FixFirstRotation(desiredPositions[i], desiredPositions[i + 1], chain.Target.position, ccwBias);
			EnforceLimits(chain, desiredPositions, i);
		}
        if (chain.AlignEnd) ChangeLastElementRotation(chain);
	}

    private static Vector3 FixFirstRotation(Vector3 startPosition, Vector3 firstJointPosition, Vector3 targetPosition, bool ccw)
    {
        Vector3 jointVector = firstJointPosition - startPosition;
        Vector3 targetVector = targetPosition - startPosition;
        float angle = RageIk.FullAngle(targetVector, jointVector);
        if (ccw)
            return FixFirstRotationCCW(ref startPosition, ref firstJointPosition, ref jointVector, angle);
        return FixFirstRotationCW(ref startPosition, ref firstJointPosition, ref jointVector, angle);
    }

    private static Vector3 FixFirstRotationCW(ref Vector3 startPosition, ref Vector3 firstJointPosition, ref Vector3 jointVector, float angle)
    {
        if (angle < 180f) return firstJointPosition;
        jointVector = Quaternion.AngleAxis(2 * (360 - angle), Vector3.forward) * jointVector;
        return startPosition + jointVector;
    }

    private static Vector3 FixFirstRotationCCW(ref Vector3 startPosition, ref Vector3 firstJointPosition, ref Vector3 jointVector, float angle)
    {
        if (angle >= 180f) return firstJointPosition;
        jointVector = Quaternion.AngleAxis(2 * angle, Vector3.back) * jointVector;
        return startPosition + jointVector;
    }

	private static void EnforceLimits (RageIkChain chain, Vector3[] desiredPositions, int i) {
		Quaternion rotation = Quaternion.FromToRotation (chain.Joints[i + 1].position - chain.Joints[i].position,
		                                                 desiredPositions[i + 1] - chain.Joints[i].position);
		AddRotation (chain.Joints[i], rotation, chain);

		var limiter = chain.Joints[i].GetComponent<RageIkJoint>();
		if (limiter == null) return;

		if (limiter.ValidVector (limiter.RestDirection)) return;
		if (Vector3.Angle (limiter.RestDirection, limiter.MaxAngle) > Vector3.Angle (limiter.RestDirection, limiter.MinAngle)) {
			RotateToDirection (chain, desiredPositions, i, limiter.RestDirection, limiter.MinAngle);
			return;
		}
		RotateToDirection (chain, desiredPositions, i, limiter.RestDirection, limiter.MaxAngle);
	}

	private static void RotateToDirection (RageIkChain chain, Vector3[] desiredPositions, int idx, Vector3 fromDirection, Vector3 toDirection) {
		var rotation = Quaternion.FromToRotation (fromDirection, toDirection);
		AddRotation (chain.Joints[idx], rotation, chain, false);
		desiredPositions[idx + 1] = chain.Joints[idx + 1].position;
	}

	private static void ChangeLastElementRotation(RageIkChain chain) {

		Transform lastElement = (chain.Joints[chain.Joints.Count - 1 ].name == "endOffset") ? 
								chain.Joints[chain.Joints.Count - 2] : chain.Joints[chain.Joints.Count - 1];
		
		Quaternion lastElementRotation = chain.Target.rotation;

		SetRotation(lastElement, lastElementRotation, chain);

		var lastElementLimiter = lastElement.GetComponent<RageIkJoint>();
		if (lastElementLimiter == null) return;

		if (lastElementLimiter.ValidVector(lastElementLimiter.RestDirection)) return;
		if (Vector3.Angle(lastElementLimiter.RestDirection, lastElementLimiter.MaxAngle) > 
			Vector3.Angle(lastElementLimiter.RestDirection, lastElementLimiter.MinAngle)) {
			AddRotation(lastElement, Quaternion.FromToRotation(lastElementLimiter.RestDirection, lastElementLimiter.MinAngle), chain, false);
			return;
		}

		AddRotation(lastElement, Quaternion.FromToRotation(lastElementLimiter.RestDirection, lastElementLimiter.MaxAngle), chain);
	}

	private static void AddRotation(Transform joint, Quaternion rotation, RageIkChain chain, bool smoothRotations = true) {
		SetRotation(joint, rotation * joint.rotation, chain, smoothRotations);
	}

	private static void SetRotation(Transform joint, Quaternion rotation, RageIkChain chain, bool smoothRotations = true) {
		if (chain.TwoDeeMode) {
			var smoothRotation = rotation;
			if (smoothRotations && chain.Snap < 1f) 
				smoothRotation = Quaternion.Slerp(joint.rotation, rotation, Time.deltaTime * chain.Snap * 10);
			joint.eulerAngles = new Vector3(0, 0, smoothRotation.eulerAngles.z);
			return;
		}
		joint.rotation = rotation;
	}
}
