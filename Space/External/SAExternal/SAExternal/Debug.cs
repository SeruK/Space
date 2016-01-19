using System;
using System.Diagnostics;
using UE = UnityEngine;

namespace SA {
	public static class Debug {
		#region Asserts
		public static void Assert( bool cond ) {
			if (!cond) {
				throw new Exception( "Assertion failed" );
			}
		}

		public static void Assert( bool cond, string fmt, params object[] args ) {
			if( !cond ) {
				throw new Exception( string.Format( fmt, args ) );
			}
		}
		#endregion

		#region Log
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void Log( string fmt, params object[] args ) {
			UE.Debug.LogFormat( fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void Log( UE.Object ctx, string fmt, params object[] args ) {
			UE.Debug.LogFormat( ctx, fmt, args );
		}
		
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogIf( bool cond, string fmt, params object[] args ) {
			if( cond ) {
				UE.Debug.LogFormat( fmt, args );
			}
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogIf( bool cond, UE.Object ctx, string fmt, params object[] args ) {
			if( cond ) {
				UE.Debug.LogFormat( ctx, fmt, args );
			}
		}
		#endregion

		#region LogWarn
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogWarn( string fmt, params object[] args ) {
			UE.Debug.LogWarningFormat( fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogWarn( UE.Object ctx, string fmt, params object[] args ) {
			UE.Debug.LogWarningFormat( ctx, fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogWarnIf( bool cond, string fmt, params object[] args ) {
			if( cond ) {
				UE.Debug.LogWarningFormat( fmt, args );
			}
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogWarnIf( bool cond, UE.Object ctx, string fmt, params object[] args ) {
			if( cond ) {
				UE.Debug.LogWarningFormat( ctx, fmt, args );
			}
		}
		#endregion

		#region LogError
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogError( string fmt, params object[] args ) {
			UE.Debug.LogErrorFormat( fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogError( UE.Object ctx, string fmt, params object[] args ) {
			UE.Debug.LogErrorFormat( ctx, fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogErrorIf( bool cond, string fmt, params object[] args ) {
			if( cond ) {
				UE.Debug.LogErrorFormat( fmt, args );
			}
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogErrorIf( bool cond, UE.Object ctx, string fmt, params object[] args ) {
			if( cond ) {
				UE.Debug.LogErrorFormat( ctx, fmt, args );
			}
		}
		#endregion

		#region LogException
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogException( Exception exc ) {
			UE.Debug.LogException( exc );
		}
		
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void LogException( UE.Object ctx, Exception exc ) {
			UE.Debug.LogException( exc, ctx );
		}
		#endregion

		#region DrawLine
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DrawLine( UE.Vector3 start, UE.Vector3 end ) {
			UE.Debug.DrawLine( start, end );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DrawLine( UE.Vector3 start, UE.Vector3 end, UE.Color color, float duration = 0.0f, bool depthTest = true ) {
			UE.Debug.DrawLine( start, end, color, duration, depthTest );
		}
		#endregion
	}
}

