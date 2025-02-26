using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {
    
	[SerializeField] private float rotateSensitivity;
	[SerializeField] private float moveSensitivity;
	[SerializeField] private float scrollSensitivity;

	//set by the settings controller
	private float sensitivityMultiplier;

	//origin keeps track of the camera position offset
	private Vector3 cameraOrigin;
	
	//stores the camera rotation so it can be reapplied later
	private Quaternion defaultRotation;

	private void Start(){
		defaultRotation = transform.rotation;
	}

	private void Update(){
		//users cannot interact with the scene when hovering over UI
		if(EventSystem.current.IsPointerOverGameObject())
			return;
		
		//get the mouse movement
		float x = Input.GetAxis("Mouse X");
		float y = Input.GetAxis("Mouse Y");

		//makes movement/zooming more sensitive when zoomed out
		float zoomSensitivityMultiplier = (transform.position - cameraOrigin).magnitude;

		//checks for middle mouse button
		if(Input.GetMouseButton(2)){
			//offset as calculated from the mouse movement
			//offset is local to the camera orientation
			Vector3 offset = Vector3.zero;
			
			offset += transform.right * x;

			Vector3 forward = transform.forward;
			forward.y = 0;
			forward.Normalize();
			offset += forward * y;

			//offset is multiplied by the combined move sensitivity
			float moveStep = -moveSensitivity * sensitivityMultiplier * zoomSensitivityMultiplier * Time.deltaTime;
			offset *= moveStep;

			//add calculated offset to the camera origin
			cameraOrigin += offset;
			
			//move the actual camera transform
			transform.Translate(offset, Space.World);
		}

		//checks for right mouse button
		if(Input.GetMouseButton(1)){
			float rotationStep = rotateSensitivity * sensitivityMultiplier * Time.deltaTime;
		
			//rotates the camera around the origin point based on mouse movement
			transform.RotateAround(cameraOrigin, Vector3.up, x * rotationStep); 
			transform.RotateAround(cameraOrigin, transform.right, -y * rotationStep);
		}

		//zoom based on mouse wheel scroll input
		float scrollStep = scrollSensitivity * zoomSensitivityMultiplier;
		transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * scrollStep);
	}

	//allows the settings sensitivity slider to affect camera sensitivity
	public void SetSensitivityMultiplier(float sensitivityMultiplier){
		this.sensitivityMultiplier = sensitivityMultiplier;
	}

	//resets camera orientation when user generates a new maze
	public void ResetCameraOrientation(float zoomAmount){
		cameraOrigin = Vector3.zero;
		transform.rotation = defaultRotation;
		
		transform.position = cameraOrigin - transform.forward * zoomAmount;
	}
}
