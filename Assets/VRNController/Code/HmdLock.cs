using UnityEngine;
using System.Collections;

/// <summary>
/// selectively lock axes of an HMD to prevent motion
/// </summary>
public class HmdLock : MonoBehaviour
{

	/// <summary>
	/// rotation axes
	/// </summary>
	public bool rollLock, pitchLock, yawLock;

	/// <summary>
	/// translation axes
	/// </summary>
	public bool xLock, yLock, zLock;

	//private Vector3 lastPosn;
	//private Quaternion lastRotation;
	private PlayerController mPlayerController;

	// Use this for initialization
	void Start()
	{
		mPlayerController = GetComponentInParent<PlayerController>();

		//lastPosn = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye);
		//lastRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
	}

	// Update is called once per frame
	void Update()
	{
		if (rollLock || pitchLock || yawLock)
		{
			// undo rotation change since last frame
			//Quaternion currentRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
			//Quaternion rotationChange = RelativeOrientation(lastRotation, currentRotation);
			//Vector3 rotationChangeEulers = rotationChange.eulerAngles;

			Vector3 rotationChangeEulers = new Vector3(0f, mPlayerController.rotationAngle, 0f); // undo rotation of body

			Vector3 rotationInverse = new Vector3();
			if (rollLock)
			{
				rotationInverse.z = -rotationChangeEulers.z;
			}

			if (pitchLock)
			{
				rotationInverse.x = -rotationChangeEulers.x;
			}

			if (yawLock)
			{
				rotationInverse.y = -rotationChangeEulers.y;
			}

			this.gameObject.transform.Rotate(rotationInverse, Space.World);
		}

		if ((xLock || yLock || zLock))
		{
			// undo position change since last frame
			Vector3 currentPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
			Vector3 positionChange = currentPosition;

			if (!xLock)
			{
				positionChange.x = 0f;
			}

			if (!yLock)
			{
				positionChange.y = 0f;
			}

			if (!zLock)
			{
				positionChange.z = 0f;
			}

			//this.gameObject.transform.Translate(positionChange, Space.Self);
			this.gameObject.transform.localPosition = -positionChange;
		}

		// update last frame
		//lastPosn = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye);
		//lastRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
	}

	//// LateUpdate is called every frame, if the Behaviour is enabled (Since v1.0)
	//public void LateUpdate()
	//{
	//	if ((xLock || yawLock || zLock))
	//	{
	//		// set position to be the same as it was last frame
	//		this.gameObject.transform.localPosition = lastPosn;
	//	}

	//	if (rollLock || pitchLock || yawLock)
	//	{
	//		// set rotation to be the same as last frame
	//		this.gameObject.transform.localRotation = lastRotation;
	//	}

	//	// update last frame
	//	lastPosn = this.gameObject.transform.localPosition;
	//	lastRotation = this.gameObject.transform.localRotation;
	//}

	/// <summary>
	/// Compute the relative rotation between two quaternions. This is done by computing the conjugate of the reference quaternion and multiplying by the other quaternion.
	/// </summary>
	/// <param name="reference">angle relative to which the result should be calculated.</param>
	/// <param name="other"></param>
	/// <returns>The angle that 'reference' would need to be rotated through to get 'other'</returns>
	private static Quaternion RelativeOrientation(Quaternion reference, Quaternion other)
	{
		Quaternion resultant;

		Quaternion conjugate = new Quaternion
			(
				-reference.x,
				-reference.y,
				-reference.z,
				reference.w
			);

		resultant = other * conjugate;

		return resultant;
	}


	/// <summary>
	/// reset the corrections of this lock to default values. Call this when the person is known to be looking
	/// in a specific direction (e.g. when resetting the HMD to get rid of drift)
	/// </summary>
	public void ResetCorrection()
	{
		this.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

		this.transform.localRotation = Quaternion.identity;
	}
}
