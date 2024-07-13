using OpenTK.Input;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraControls : MonoBehaviour
{
	// Start is called before the first frame update
	Vector3 lookAtPoint = Vector3.zero;
	Transform dummy;
	Camera camera;
	public Transform origin;
	//Quaternion zoomRot;
	public float lerpSpeed = 25f;
	public float zoomSpeed = 0.1f;
	public float horbitSpeed = 0.1f;
	public float vorbitSpeed = 5f;
	Vector3 orbitPoint;
	Vector3 orbit;
	//Vector3 vorbit;
	Vector3 angle;
	//Vector3 vangle;

	Vector3 newzoompos;

	//float radius = 0;

	public bool followAvatar = true;

	void Start()
	{
		camera = gameObject.GetComponent<Camera>();
		dummy = Instantiate(ResourceCache.empty.transform);
		dummy.name = "Camera Dummy";
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		CursorLockMode cursorlockmode = CursorLockMode.None;
		//Cursor.lockState = CursorLockMode.None;
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0))
		{
			followAvatar = false;
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				lookAtPoint = hit.point;

				//float distance = Vector3.Distance(dummy.position, lookAtPoint);

				dummy.LookAt(lookAtPoint, Vector3.up);

				angle = GetAngleRad(orbitPoint, dummy.position);
				//vangle = angle;
				orbit = GetXOrbit(angle.y);

				//Vector3 vorbitPoint = dummy.TransformPoint(lookAtPoint);
				//vorbit = Quaternion.Euler(0f, dummy.rotation.eulerAngles.y, 0f) * GetYOrbit(vangle.x);
				//zoomRot = Quaternion.Euler(GetAngleDeg(dummy.position, lookAtPoint));
			}

		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			lookAtPoint = Vector3.negativeInfinity;
			followAvatar = true;
		}
		float mouseX = Input.GetAxisRaw("Mouse X");
		float mouseY = Input.GetAxisRaw("Mouse Y");

		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl))
		{
			cursorlockmode = CursorLockMode.Locked;

			Zoom(mouseY);
			HorizontalOrbit(mouseX);

		}
		else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
		{
			cursorlockmode = CursorLockMode.Locked;
			VerticalOrbit(mouseY);
			HorizontalOrbit(mouseX);
		}

		if (!Input.GetMouseButton(0))
		{
			//radius = 0f;
			Cursor.lockState = CursorLockMode.None;
		}

		//Vector3 newPos = dummy.position;
		//Quaternion newRot = dummy.rotation;
		if (followAvatar)
		{
			dummy.SetPositionAndRotation(origin.position, origin.rotation);
		}
		Cursor.lockState = cursorlockmode;

		var p = dummy.position;//Vector3.Lerp(transform.position, dummy.position, Time.deltaTime * lerpSpeed);
		var r = dummy.rotation;//Quaternion.Lerp(transform.rotation, dummy.rotation, Time.deltaTime * lerpSpeed);
		transform.SetPositionAndRotation(p, r);
	}


	void Zoom(float mouse)
	{
		float lookAtDistance = Vector3.Distance(dummy.position, lookAtPoint);
		newzoompos = (dummy.forward * lookAtDistance);

		if (mouse != 0f)
		{
			//Vector3 pos = dummy.position;
			float distance = Vector3.Distance(dummy.position, lookAtPoint);

			newzoompos += dummy.forward * (zoomSpeed * -mouse * distance);
			///distance = Vector3.Distance(newzoompos, lookAtPoint);
			//orbit point = orbitPoint + (orbit * radius);
			if (distance < 0.3f)
			{
				newzoompos -= dummy.forward * (0.3f - Vector3.Distance(dummy.position, lookAtPoint));
			}

			dummy.position = lookAtPoint - newzoompos;

		}
	}
	void HorizontalOrbit(float mouse)
	{
		if (mouse != 0f)
		{
			orbitPoint = new Vector3(lookAtPoint.x, dummy.position.y, lookAtPoint.z);
			float orbitRadius = Vector3.Distance(dummy.position, orbitPoint);
			//Vector3 neworbitpos = orbitPoint + (orbit * orbitRadius);

			angle.y += mouse * horbitSpeed;
			orbit = GetXOrbit(angle.y);

			var p = orbitPoint + (orbit * orbitRadius);
			var r = Quaternion.Euler(dummy.rotation.eulerAngles.x, 180f + (angle.y * Mathf.Rad2Deg), dummy.rotation.eulerAngles.z);
			dummy.SetPositionAndRotation(p, r);
		}
	}

	void VerticalOrbit(float mouse)
	{
		//Vector3 vorbitPoint = dummy.TransformPoint(lookAtPoint);
		//Vector3 neworbitpos = lookAtPoint + (vorbit * orbitRadius);
		if (mouse != 0f)
		{
			float orbitRadius = Vector3.Distance(dummy.position, lookAtPoint);
			dummy.position += dummy.forward * orbitRadius;
			dummy.Rotate(mouse * -vorbitSpeed, 0f, 0f);
			dummy.position -= dummy.forward * orbitRadius;
		}
	}

	Vector3 GetXOrbit(float angle)
	{
		Vector3 orbit = Vector3.zero;

		orbit.x = Mathf.Sin(angle);
		orbit.z = Mathf.Cos(angle);

		return orbit;
	}

	//Vector3 GetYOrbit(float angle)
	//{
	//	Vector3 orbit = Vector3.zero;
	//	orbit.y = Mathf.Sin(angle);
	//	orbit.z = Mathf.Cos(angle);
	//	return orbit;
	//}

	//Vector3 GetAngleDeg(Vector3 position, Vector3 target)
	//{
	//	return GetAngleRad(position, target) * Mathf.Rad2Deg;
	//}


	Vector3 GetAngleRad(Vector3 position, Vector3 target)
	{
		Vector3 relative = target - position;
		float angleY = Mathf.Atan2(relative.x, relative.z);
		float angleX = Mathf.Atan2(relative.y, relative.z);
		return new Vector3(-angleX, angleY, 0f);
	}

	public float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
}
