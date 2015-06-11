using System;
using UnityEngine;

namespace SA
{
	public struct Recti
	{
		public Vector2i origin;
		public Size2i size;
		
		public Recti(int x, int y, int width, int height)
		{
			this.origin = new Vector2i(x, y);
			this.size = new Size2i(width, height);
		}
		
		override public string ToString()
		{
			return "{"+origin.x+", "+origin.y+", "+size.width+", "+size.height+"}";
		}
		
		public static bool operator == (Recti lhs, Recti rhs)
		{
			return ((lhs.origin == rhs.origin) && (lhs.size == rhs.size));
		}
		
		public static bool operator != (Recti lhs, Recti rhs)
		{
			return ((lhs.origin != rhs.origin) || (lhs.size != rhs.size));
		}
		
		public override bool Equals (object other)
		{
			if (!(other is Recti))
			{
				return false;
			}
			Recti rect = (Recti)other;
			return this.origin.Equals(rect.origin) && this.size.Equals(rect.size);
		}
		
		public override int GetHashCode()
		{
			return  this.origin.x.GetHashCode () ^ 
				this.size.width.GetHashCode () << 2 ^ 
					this.origin.y.GetHashCode () >> 2 ^ 
					this.size.height.GetHashCode () >> 1;
		}
		
		public int MinX
		{ get { return size.width < 0 ? origin.x + size.width : origin.x; } }
		
		public int MaxX
		{ get { return size.width < 0 ? origin.x : origin.x + size.width; } }
		
		public int MinY
		{ get { return size.height < 0 ? origin.y + size.height : origin.y; } }
		
		public int MaxY
		{ get { return size.height < 0 ? origin.y : origin.y + size.height; } }
		
		public int MidX
		{ get { return MaxX - Mathf.Abs(size.width/2); } }
		
		public int MidY
		{ get { return MaxY - Mathf.Abs(size.height/2); } }
		
		public static Recti Zero
		{
			get { return new Recti(0,0,0,0); }
		}
	}
}