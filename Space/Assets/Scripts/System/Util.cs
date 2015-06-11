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
}
