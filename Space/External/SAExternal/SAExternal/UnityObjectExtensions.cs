using System;
using UE = UnityEngine;
using System.Diagnostics;

namespace SA {
	public static class UnityObjectExtensions {
		#region Log
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DebugLog( this UE.Object obj, string fmt, params object[] args ) {
			DebugUtil.Log( obj, fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DebugLogIf( this UE.Object obj, bool cond, string fmt, params object[] args ) {
			DebugUtil.LogIf( cond, obj, fmt, args );
		}
		#endregion

		#region LogWarn
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DebugLogWarn( this UE.Object obj, string fmt, params object[] args ) {
			DebugUtil.LogWarn( obj, fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DebugLogWarnIf( this UE.Object obj, bool cond, string fmt, params object[] args ) {
			DebugUtil.LogWarnIf( cond, obj, fmt, args );
		}
		#endregion

		#region LogError
		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DebugLogError( this UE.Object obj, string fmt, params object[] args ) {
			DebugUtil.LogError( obj, fmt, args );
		}

		[Conditional( "SA_DEBUG_LOGGING" )]
		public static void DebugLogErrorIf( this UE.Object obj, bool cond, string fmt, params object[] args ) {
			DebugUtil.LogErrorIf( cond, obj, fmt, args );
		}
		#endregion
	}
}

