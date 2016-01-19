using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using SA;

public class NodeWindow {
	public readonly int id;
	public Rect Rect { get; set; }
	public virtual string title { get; set; }
	public virtual Color color { get; set; }

	public NodeWindow( int id, string title, Rect rect ) {
		this.id = id;
		this.title = title;
		this.Rect = rect;
	}

	public virtual void OnGUI() {
		Rect = GUI.Window( id, Rect, InternalDraw, title );
	}

	private void InternalDraw( int id ) {
		Color c = color;
		c.a = 0.2f;
		GUI.color = c;
		GUI.DrawTexture( new Rect( 0, 16, Rect.width, Rect.height ), Texture2D.whiteTexture, ScaleMode.StretchToFill );
		GUI.color = Color.white;

		Rect contentRect = new Rect( 3, 18, Rect.width, Rect.height );
		GUILayout.BeginArea( contentRect );
		Draw();
		GUILayout.EndArea();
		GUI.DragWindow();
	}

	protected virtual void Draw() {
		//GUILayout.Label( "Test" );
	}
}
