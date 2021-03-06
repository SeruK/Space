﻿using UnityEngine;
using System;
using System.Collections;

#if COMPILE_NAMESPACE
using SA;

namespace SA {
#endif
public interface IDebugSystem {
	// If an instance of DebugSystem is registered as Dbg.DebugSystem, this hook
	// will be called by Dbg.Log(...) if an IDebugContext was provided.
	// Squelch indicates whether to output the message to the console. Note that
	// context implementing IDebugSquelcher can also chose to squelch the message.
	void AddLogEntry( Dbg.LogType logType, IDebugContext ctx, Exception exc, string message, out bool squelch );
}
#if COMPILE_NAMESPACE
}
#endif