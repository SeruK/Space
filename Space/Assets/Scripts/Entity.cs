using UnityEngine;
using System.Collections;

[RequireComponent( typeof(CharacterController2D) )]
public class Entity : BaseEntity {
	// TODO: Make private + serialize
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
	
	public CharacterController2D CharController {
		get { return charController; }
	}
	public SpriteRenderer Visual {
		get { return spriteRenderer; }
	}

	private bool didJump;
	
	private CharacterController2D charController;
	private Animator animator;
	private Transform spriteTransform;
	private SpriteRenderer spriteRenderer;

	public int Direction {
		get { return spriteTransform.localScale.x < 0 ? -1 : 1; }
		set {
			if( value == 0 ) return;
			Vector3 ls = spriteTransform.localScale;
			if( value > 0 && ls.x < 0 ) {
				ls.x = -ls.x;
			} else if ( value < 0 && ls.x > 0 ) {
				ls.x = -ls.x;
			}
			spriteTransform.localScale = ls;
		}
	}

	protected void OnEnable() {
		if (charController == null) {
			charController = GetComponentInChildren<CharacterController2D>();
		}
		if (animator == null) {
			animator = GetComponentInChildren<Animator>();
		}
		if( spriteTransform == null ) {
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			if( spriteRenderer != null ) {
				spriteTransform = spriteRenderer.transform;
			}
		}
	}

	protected void Update() {
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
				PlayAnimation( IdleAnimationName );
		} else if( RequestedHorizontalSpeed > 0.0f ) {
			normalizedHorizontalSpeed = 1;
			if( spriteTransform.localScale.x < 0f )
				spriteTransform.localScale = new Vector3( -spriteTransform.localScale.x, spriteTransform.localScale.y, spriteTransform.localScale.z );
			
			if( charController.isGrounded )
				PlayAnimation( WalkAnimationName );
		} else {
			normalizedHorizontalSpeed = -1;
			if( spriteTransform.localScale.x > 0f )
				spriteTransform.localScale = new Vector3( -spriteTransform.localScale.x, spriteTransform.localScale.y, spriteTransform.localScale.z );
			
			if( charController.isGrounded )
				PlayAnimation( WalkAnimationName );
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
			PlayAnimation( anim );
		}
	}

	private void PlayAnimation( string anim ) {
		if( string.IsNullOrEmpty( anim ) ) {
			return;
		}
		animator.Play( Animator.StringToHash( anim ) );
	}
}
