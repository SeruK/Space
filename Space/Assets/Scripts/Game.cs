using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	public Entity Player;
	public SmoothFollow CameraController;
	public TextDisplay TextDisplay;

	// TODO: This nicer
	public GameObject OrbPrefab;

	protected void OnEnable() {
		if( Player != null ) {		
			Player.CharController.onControllerCollidedEvent += OnPlayerCollided;
			Player.CharController.onTriggerEnterEvent += OnPlayerEnteredTrigger;
		}
	}

	protected void OnDisable() {
		if( Player != null ) {
			Player.CharController.onControllerCollidedEvent -= OnPlayerCollided;
			Player.CharController.onTriggerEnterEvent -= OnPlayerEnteredTrigger;
		}
	}

	protected void Update() {
		UpdateInput();
	}

	protected void UpdateInput() {
		if( Player == null ) {
			return;
		}

		Player.RequestedHorizontalSpeed = Input.GetKey( KeyCode.LeftArrow ) ? -1.0f :
			Input.GetKey( KeyCode.RightArrow ) ? 1.0f : 0.0f;
		Player.RequestedJump = Input.GetKey( KeyCode.UpArrow );
		if( Input.GetKey( KeyCode.DownArrow ) && CameraController != null ) {
			CameraController.extraCameraOffset = new Vector3( 0.0f, 2.0f, 0.0f );
		} else {
			CameraController.extraCameraOffset = Vector3.zero;
		}
	}

	protected void OnPlayerCollided( RaycastHit2D hit ) {
		var obstacle = hit.collider.GetComponentInChildren<Obstacle>();
		if( obstacle != null ) {
			if( obstacle.KnockForce > 0.0f ) {
				Vector3 force = hit.normal * obstacle.KnockForce;
				Player.CharController.move( force );
			}
		}
	}

	protected void OnPlayerEnteredTrigger( Collider2D collider ) {
		var pickup = collider.GetComponentInChildren<Pickup>();
		if( pickup != null ) {
			if( TextDisplay != null ) {
				TextDisplay.TypeTextThenDisplayFor( "Picked up a " + pickup.DisplayName, 3.0f );
			}
			DestroyPickup( pickup );
		}
	}

	// Pickups

	private void DestroyPickup( Pickup pickup ) {
		Destroy( pickup.gameObject );
	}
}
