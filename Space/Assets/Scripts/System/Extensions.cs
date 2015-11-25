using UnityEngine;

public static class StringExtensions {
	public static string Fmt( this string str, params object[] args ) {
		return string.Format( str, args );
	}
}

public static class CameraExtensions {
	public static RaycastHit2D ScreenPointToRay2D( this Camera cam, Vector3 position ) {
		return Physics2D.Raycast( cam.ScreenToWorldPoint( position), Vector2.zero );
	}
}