using OpenTK.Input;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum CameraMode { Follow, Orbit, Mouselook }

public class CameraControls : MonoBehaviour
{
    public CameraMode Mode { get; private set; } = CameraMode.Follow;

    private PlayerMovementController movementController;
    private float currentZoom = 5.0f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

	// Start is called before the first frame update
	Vector3 lookAtPoint = Vector3.zero;
	Transform dummy;
	Camera camera;
	public Transform origin;
    public float mouselookSensitivity = 2.0f;
    public float zoomSpeed = 5.0f;
    public float minZoomDistance = 2.0f;
    public float maxZoomDistance = 15.0f;
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

	void Start()
	{
		camera = gameObject.GetComponent<Camera>();
		dummy = Instantiate(ResourceCache.empty.transform);
		dummy.name = "Camera Dummy";
        if (origin != null)
        {
            movementController = origin.root.GetComponent<PlayerMovementController>();
        }
	}

	// Update is called once per frame
	void LateUpdate()
	{
        if (origin == null)
        {
            // If we have no origin (avatar), we can't do anything.
            // Try to find it, in case we were initialized before the avatar.
            if (movementController == null)
            {
                var playerMovementController = FindObjectOfType<PlayerMovementController>();
                if(playerMovementController != null)
                {
                    movementController = playerMovementController;
                    origin = playerMovementController.transform;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        HandleModeSwitching();

        switch (Mode)
        {
            case CameraMode.Follow:
                HandleFollowMode();
                break;
            case CameraMode.Orbit:
                HandleOrbitMode();
                break;
            case CameraMode.Mouselook:
                HandleMouselookMode();
                break;
        }
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

    void HandleModeSwitching()
    {
        // Enter Mouselook on Right Mouse Down, exit on Right Mouse Up
        if (Input.GetMouseButtonDown(1))
        {
            Mode = CameraMode.Mouselook;
            if (movementController != null) movementController.MouselookEnabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Mode = CameraMode.Follow;
            if (movementController != null) movementController.MouselookEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Enter Orbit on Alt + Left Mouse
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0))
        {
            Mode = CameraMode.Orbit;
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				lookAtPoint = hit.point;
				dummy.LookAt(lookAtPoint, Vector3.up);
				angle = GetAngleRad(orbitPoint, dummy.position);
				orbit = GetXOrbit(angle.y);
			}
        }

        // Exit any mode with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Mode = CameraMode.Follow;
            if (movementController != null) movementController.MouselookEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleFollowMode()
    {
        // Simple follow, maybe with some smoothing later
        dummy.SetPositionAndRotation(origin.position, origin.rotation);
        // Lerp for smoothness
		var p = Vector3.Lerp(transform.position, dummy.position, Time.deltaTime * lerpSpeed);
		var r = Quaternion.Lerp(transform.rotation, dummy.rotation, Time.deltaTime * lerpSpeed);
		transform.SetPositionAndRotation(p, r);
    }

    void HandleOrbitMode()
    {
		float mouseX = Input.GetAxisRaw("Mouse X");
		float mouseY = Input.GetAxisRaw("Mouse Y");

		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Zoom(mouseY);
			HorizontalOrbit(mouseX);
		}
		else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
		{
			Cursor.lockState = CursorLockMode.Locked;
			VerticalOrbit(mouseY);
			HorizontalOrbit(mouseX);
		}
        else
        {
            // If we release the buttons, go back to follow mode
            Mode = CameraMode.Follow;
        }

		var p = dummy.position;
		var r = dummy.rotation;
		transform.SetPositionAndRotation(p, r);
    }

    void HandleMouselookMode()
    {
        // Zoom logic
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoomDistance, maxZoomDistance);

        // Mouse rotation
        rotationY += Input.GetAxis("Mouse X") * mouselookSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * mouselookSensitivity;
        rotationX = Mathf.Clamp(rotationX, -60f, 90f);

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentZoom);
        Vector3 position = rotation * negDistance + origin.position;

        transform.SetPositionAndRotation(position, rotation);
    }
}
