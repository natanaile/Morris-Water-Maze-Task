using UnityEngine;
using System.Collections;

/// <summary>
/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation.
/// 
/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.
///   
/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
/// </summary>
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{

	/// <summary>
	/// Which axes rotated?
	/// </summary>
	public enum RotationAxes
	{
		/// <summary>
		/// The mouse x and y
		/// </summary>
		MouseXAndY = 0,
		/// <summary>
		/// The mouse x
		/// </summary>
		MouseX = 1,
		/// <summary>
		/// The mouse y
		/// </summary>
		MouseY = 2
	}
	/// <summary>
	/// The axes
	/// </summary>
	public RotationAxes axes = RotationAxes.MouseXAndY;
	/// <summary>
	/// The sensitivity x
	/// </summary>
	public float sensitivityX = 15F;
	/// <summary>
	/// The sensitivity y
	/// </summary>
	public float sensitivityY = 15F;

	/// <summary>
	/// The minimum x
	/// </summary>
	public float minimumX = -360F;
	/// <summary>
	/// The maximum x
	/// </summary>
	public float maximumX = 360F;

	/// <summary>
	/// The minimum y
	/// </summary>
	public float minimumY = -60F;
	/// <summary>
	/// The maximum y
	/// </summary>
	public float maximumY = 60F;

	float rotationY = 0F;

	void Update()
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}

	void Start()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}
}