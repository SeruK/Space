using UnityEngine;
using System;
using System.IO;
using System.Text;
using SA;

public static class Util {
	public static MemoryStream StringToMemoryStream( string s ) {
		byte[] a = Encoding.ASCII.GetBytes( s );
		return new MemoryStream( a );
	}

	public static string MemoryStreamToString( MemoryStream ms ) {
		byte[] ByteArray = ms.ToArray();
		return Encoding.ASCII.GetString( ByteArray );
	}

	public static void CopyStream( Stream src, Stream dest ) {
		byte[] buffer = new byte[ 1024 ];
		int len = src.Read( buffer, 0, buffer.Length );
		while( len > 0 ) {
			dest.Write( buffer, 0, len );
			len = src.Read( buffer, 0, buffer.Length );
		}
		dest.Flush();
	}

	public static Color ParseColorString( string colorString, Color def ) {
		if( string.IsNullOrEmpty( colorString ) || colorString.Length != 7 ) {
			return def;
		}
		
		// Could be done much faster by converting to int and bitshifting
		// Format: #ffaaff
		UInt16 red, green, blue;
		try {
			red   = Convert.ToUInt16( colorString.Substring( 1, 2 ), 16 );
			green = Convert.ToUInt16( colorString.Substring( 3, 2 ), 16 );
			blue  = Convert.ToUInt16( colorString.Substring( 5, 2 ), 16 );
		} catch( System.Exception e ) {
			SA.Debug.LogError( "Invalid color string: " + colorString + " || " + e );
			return def;
		}
		
		return new Color( red / 255.0f, green / 255.0f, blue / 255.0f );
	}

	public static Rect ScreenRect {
		get {
			return new Rect( 0, 0, Screen.width, Screen.height );
		}
	}

	public static Vector3 ScreenPointToGUI( Vector3 p ) {
		return new Vector3( p.x, Screen.height - p.y, p.z );
	}

	public static Vector3 GUIPointToScreen( Vector3 p ) {
		return new Vector3( p.x, p.y + Screen.height, p.z );
	}

	public static readonly string ResourcesPath = UnityEngine.Application.dataPath + "/Resources";
}
