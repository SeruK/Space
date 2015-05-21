using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {
	public float Gravity = -25f;
	public float RunSpeed = 8f;
	public float GroundDamping = 20f; // how fast do we change direction? higher means faster
	public float InAirDamping = 5f;
	public float JumpHeight = 3f;
	public float DuckHeight = 0.2f;

	public string IdleAnimationName;
	public string WalkAnimationName;
	public string JumpAnimationName;
	public string HoverAnimationName;

	[HideInInspector]
	public float RequestedHorizontalSpeed;
	[HideInInspector]
	public bool  RequestedJump;
	[HideInInspector]
	public bool  RequestedDuck;

	private bool didJump;

	private CharacterController2D charController;
	private Animator animator;
	private Transform spriteTransform;

	protected void OnEnable() {
		if (charController == null) {
			charController = GetComponentInChildren<CharacterController2D>();
		}
		if (animator == null) {
			animator = GetComponentInChildren<Animator>();
		}
		if( spriteTransform == null ) {
			var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			if( spriteRenderer != null ) {
				spriteTransform = spriteRenderer.transform;
			}
		}
	}

	public void Update() {
		if( charController == null || animator == null ) {
			return;
		}
		
		// grab our current _velocity to use as a base for all calculations
		Vector3 velocity = charController.velocity;
		
		if( charController.isGrounded ) {
			didJump = false;
			velocity.y = 0;
		}
		
		float normalizedHorizontalSpeed = 0;

		if( Mathf.Approximately( RequestedHorizontalSpeed, 0.0f ) ) {
			normalizedHorizontalSpeed = 0;
			if( charController.isGrounded )
				animator.Play( Animator.StringToHash( IdleAnimationName ) );
		} else if( RequestedHorizontalSpeed > 0.0f ) {
			normalizedHorizontalSpeed = 1;
			if( spriteTransform.localScale.x < 0f )
				spriteTransform.localScale = new Vector3( -spriteTransform.localScale.x, spriteTransform.localScale.y, spriteTransform.localScale.z );
			
			if( charController.isGrounded )
				animator.Play( Animator.StringToHash( WalkAnimationName ) );
		} else {
			normalizedHorizontalSpeed = -1;
			if( spriteTransform.localScale.x > 0f )
				spriteTransform.localScale = new Vector3( -spriteTransform.localScale.x, spriteTransform.localScale.y, spriteTransform.localScale.z );
			
			if( charController.isGrounded )
				animator.Play( Animator.StringToHash( WalkAnimationName ) );
		}

		float scaleY = 1.0f;

		// we can only jump whilst grounded
		if( charController.isGrounded ) {
			if( RequestedJump ) {
				velocity.y = Mathf.Sqrt( 2f * JumpHeight * -Gravity );
				didJump = true;
			} else if( RequestedDuck ) {
				scaleY = 1.0f - DuckHeight;
			}
		} /* else if( !RequestedJump && velocity.y > 0.0f ) {
			velocity.y = 0.0f;
		} */
		scaleY = Mathf.Lerp( spriteTransform.localScale.y, scaleY, 0.01f );
		spriteTransform.localScale = new Vector3( spriteTransform.localScale.x, scaleY, spriteTransform.localScale.z );
		

		// apply horizontal speed smoothing it
		var smoothedMovementFactor = charController.isGrounded ? GroundDamping : InAirDamping; // how fast do we change direction?
		velocity.x = Mathf.Lerp( velocity.x, normalizedHorizontalSpeed * RunSpeed, Time.deltaTime * smoothedMovementFactor );
		
		// apply gravity before moving
		velocity.y += Gravity * Time.deltaTime;
		
		charController.move( velocity * Time.deltaTime );

		if( !charController.isGrounded ) {
			string hover = string.IsNullOrEmpty( HoverAnimationName ) ? JumpAnimationName : HoverAnimationName;
			string anim  = hover;
			if( velocity.y > 0.0f && didJump ) {
				anim = JumpAnimationName;
			}
			animator.Play( Animator.StringToHash( anim ) );
		}
	}
}
