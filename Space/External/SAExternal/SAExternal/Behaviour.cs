using System;
using System.Diagnostics;
using UE = UnityEngine;

namespace SA {
	public abstract class Behaviour : UE.MonoBehaviour {
		#region Log
		[Conditional( "SA_DEBUG_LOGGING" )]
		public void DebugLog( string fmt, params object[] args ) {
			DebugUtil.Log( fmt, args);
		}
		
		[Conditional( "SA_DEBUG_LOGGING" )]
		public void DebugLogIf( bool cond, string fmt, params object[] args ) {
			DebugUtil.LogIf( cond, fmt, args );
		}
		#endregion
		
		#region LogWarn
		[Conditional( "SA_DEBUG_LOGGING" )]
		public void DebugLogWarn( string fmt, params object[] args ) {
			DebugUtil.LogWarn( fmt, args );
		}
		
		[Conditional( "SA_DEBUG_LOGGING" )]
		public void DebugLogWarnIf( bool cond, string fmt, params object[] args ) {
			DebugUtil.LogWarnIf( cond, fmt, args );
		}
		#endregion
		
		#region LogError
		[Conditional( "SA_DEBUG_LOGGING" )]
		public void DebugLogError( string fmt, params object[] args ) {
			DebugUtil.LogError( fmt, args );
		}
		
		[Conditional( "SA_DEBUG_LOGGING" )]
		public void DebugLogErrorIf( bool cond, string fmt, params object[] args ) {
			DebugUtil.LogErrorIf( cond, fmt, args );
		}
		#endregion

		#region DebugInfo
		public virtual string DebugInfo {
			get { return string.Format( "{0} ({1})", name, GetType().Name ); }
		}
		#endregion
	}
}

