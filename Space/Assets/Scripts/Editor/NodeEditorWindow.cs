using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using SA;

// Based on: http://forum.unity3d.com/threads/simple-node-editor.189230/
public abstract class NodeEditorWindow : EditorWindow {
	private static readonly float MIN_SIZE = 300.0f;

	private Dictionary<int, NodeWindow> windows;
	private List<NodeConnection> connections;

	public void OnEnable() {
		minSize = new Vector2( MIN_SIZE, MIN_SIZE );
		windows = new Dictionary<int, NodeWindow>();
		connections = new List<NodeConnection>();
		//AddWindow( new NodeWindow( 0, "First", new Rect( 10, 10, 100, 100 ) ) );
		//AddWindow( new NodeWindow( 1, "Second", new Rect( 210, 210, 100, 100 ) ) );
		//AddConnection( 0, 1 );
	}

	public virtual void AddWindow( NodeWindow window ) {
		windows[ window.id ] = window;
	}

	public virtual void AddConnection( int from, int to ) {
		//connections.Add( new NodeConnection { from = from, to = to } );
	}
   
	void OnGUI() {
		BeginWindows();
		foreach( NodeWindow window in windows.Values ) {
			window.OnGUI();
			Rect clamped = window.Rect;
			clamped.x = Mathf.Max( clamped.x, 0.0f );
			clamped.y = Mathf.Max( clamped.y, 0.0f );
			window.Rect = clamped;
		}
		EndWindows();

		connections.RemoveAll( conn => !windows.ContainsKey( conn.from ) || !windows.ContainsKey( conn.to ) );
		foreach( NodeConnection conn in connections ) {
			DrawNodeCurve( windows[ conn.from ].Rect, windows[ conn.to ].Rect );
		}
		
	}

	private void DrawNodeCurve( Rect start, Rect end ) {
		Vector3 startPos = new Vector3( start.x + start.width, start.y + start.height / 2, 0 );
		Vector3 endPos = new Vector3( end.x, end.y + end.height / 2, 0 );
		Vector3 startTan = startPos + Vector3.right * 50;
		Vector3 endTan = endPos + Vector3.left * 50;
		Color shadowCol = new Color( 0, 0, 0, 0.06f );
		for( int i = 0; i < 3; i++ ) { // Draw a shadow
			Handles.DrawBezier( startPos, endPos, startTan, endTan, shadowCol, null, ( i + 1 ) * 5 );
		}
		Handles.DrawBezier( startPos, endPos, startTan, endTan, Color.black, null, 1 );
	}
}
