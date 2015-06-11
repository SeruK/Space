using UnityEngine;
using System.Collections;

namespace SA {
	[System.Serializable]
	public struct Size2
	{
		public float width;
		public float height;
		
		public Size2(float width, float height){
			this.width = width; this.height = height;	
		}
		
		public Size2(Size2 s){
			width = s.width; height = s.height;
		}

		public static explicit operator Size2(Vector2 v){
			return new Size2(v.x, v.y);
		}

		public static implicit operator Vector2(Size2 v){
			return new Vector2(v.width, v.height);	
		}
		
		override public string ToString()
		{
			return "{"+width+", "+height+"}";
		}
		
		public void Set(float w, float h){
			this.width = w; this.height = h;
		}
		
		public void Set(Size2 v)
		{
			width = v.width; height = v.height;
		}
		
		public static bool operator == (Size2 lhs, Size2 rhs)
		{
			return Mathf.Approximately(lhs.width, rhs.width) && Mathf.Approximately(lhs.height, rhs.height);
		}
		
		public static bool operator != (Size2 lhs, Size2 rhs)
		{
			return !Mathf.Approximately(lhs.width, rhs.width) || !Mathf.Approximately(lhs.height, rhs.height);
		}
		
		public static Size2 operator + (Size2 a, Size2 b)
		{
			return new Size2(a.width + b.width, a.height + b.height);
		}
		
		public static Size2 operator - (Size2 a, Size2 b)
		{
			return new Size2(a.width - b.width, a.width - b.width);
		}
		
		public static Size2 operator - (Size2 a)
		{
			return new Size2(-a.width, -a.height);
		}
		
		public override bool Equals (object other)
		{
			if (!(other is Size2))
			{
				return false;
			}
			Size2 vector = (Size2)other;
			return this.width.Equals(vector.width) && this.height.Equals(vector.height);
		}
		
		public override int GetHashCode()
		{
			return this.width.GetHashCode() ^ this.height.GetHashCode() << 2;
		}
	}
}