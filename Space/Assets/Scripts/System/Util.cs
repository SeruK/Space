public static class Util {
	public static System.IO.MemoryStream StringToMemoryStream( string s ) {
		byte[] a = System.Text.Encoding.ASCII.GetBytes( s );
		return new System.IO.MemoryStream( a );
	}

	public static string MemoryStreamToString( System.IO.MemoryStream ms ) {
		byte[] ByteArray = ms.ToArray();
		return System.Text.Encoding.ASCII.GetString( ByteArray );
	}

	public static void CopyStream( System.IO.Stream src, System.IO.Stream dest ) {
		byte[] buffer = new byte[ 1024 ];
		int len = src.Read( buffer, 0, buffer.Length );
		while( len > 0 ) {
			dest.Write( buffer, 0, len );
			len = src.Read( buffer, 0, buffer.Length );
		}
		dest.Flush();
	}

	public static UnityEngine.Color ParseColorString( string colorString, UnityEngine.Color def ) {
		if( string.IsNullOrEmpty( colorString ) || colorString.Length != 7 ) {
			return def;
		}
		
		// Could be done much faster by converting to int and bitshifting
		// Format: #ffaaff
		System.UInt16 red, green, blue;
		try {
			red   = System.Convert.ToUInt16( colorString.Substring( 1, 2 ), 16 );
			green = System.Convert.ToUInt16( colorString.Substring( 3, 2 ), 16 );
			blue  = System.Convert.ToUInt16( colorString.Substring( 5, 2 ), 16 );
		} catch( System.Exception e ) {
			DebugUtil.LogError( "Invalid color string: " + colorString + " || " + e );
			return def;
		}
		
		return new UnityEngine.Color( red / 255.0f, green / 255.0f, blue / 255.0f );
	}

	public static readonly string ResourcesPath = UnityEngine.Application.dataPath + "/Resources";
}
