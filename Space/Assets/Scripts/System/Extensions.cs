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

public static class RectExtensions {
	public static Rect Inset( this Rect rect, float amount ) {
		rect.xMin += amount;
		rect.yMin += amount;
		rect.xMin -= amount;
		rect.xMax -= amount;
		return rect;
	}
}