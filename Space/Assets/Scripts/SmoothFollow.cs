using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
	public Transform target;
	public float smoothDampTime = 0.2f;
	[HideInInspector]
	public new Transform transform;
	public Vector3 cameraOffset;
	public Vector3 extraCameraOffset;
	public bool useFixedUpdate = false;
	
	private Vector3 _smoothDampVelocity;

	void Awake() {
		transform = gameObject.transform;
	}
		
	void LateUpdate() {
		if( !useFixedUpdate )
			updateCameraPosition();
	}


	void FixedUpdate() {
		if( useFixedUpdate )
			updateCameraPosition();
	}


	void updateCameraPosition()	{
		// TODO: Cache this
		var playerController = target.GetComponent<CharacterController2D>();

		if( playerController == null )
		{
			transform.position = Vector3.SmoothDamp( transform.position, target.position - ( cameraOffset + extraCameraOffset ), ref _smoothDampVelocity, smoothDampTime );
			return;
		}
		
		if( playerController.velocity.x > 0 )
		{
			transform.position = Vector3.SmoothDamp( transform.position, target.position - ( cameraOffset + extraCameraOffset ), ref _smoothDampVelocity, smoothDampTime );
		}
		else
		{
			var leftOffset = ( cameraOffset + extraCameraOffset );
			leftOffset.x *= -1;
			transform.position = Vector3.SmoothDamp( transform.position, target.position - leftOffset, ref _smoothDampVelocity, smoothDampTime );
		}
	}
	
}
