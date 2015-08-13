using UnityEngine;

[RequireComponent( typeof(Rigidbody2D) )]
public class Projectile : BaseEntity {
	[SerializeField]
	private float lifeTime;
	[SerializeField]
	private float speed;

	public System.Action<Projectile> OnDeath;

	private float lifeTimer;
	private float currentSpeed;

	public void ShootTowards( Vector2 point ) {
		Vector2 v = point - (Vector2)transform.position;
		float angle = Mathf.Atan2( v.y, v.x ) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler( 0.0f, 0.0f, angle );
		currentSpeed = speed;
		lifeTimer = lifeTime;
	}

	protected void Update() {
		transform.position += transform.TransformDirection( Vector3.forward ) * currentSpeed;

		lifeTimer -= lifeTime;
		if( lifeTimer < 0.0f && OnDeath != null ) {
			OnDeath( this );
		}
	}
}
