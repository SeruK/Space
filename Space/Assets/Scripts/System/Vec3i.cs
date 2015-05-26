using UnityEngine;
using System.Collections;

namespace SausageAssassins 
{
	[System.Serializable]
	public struct Size2i
	{
		public int width;
		public int height;
		
		public Size2i(int width, int height){
			this.width = width; this.height = height;	
		}
		
		public Size2i(Size2i s){
			width = s.width; height = s.height;
		}
		
		public static implicit operator Vector2i(Size2i v){
			return new Vector2i(v.width, v.height);	
		}
		
		override public string ToString()
		{
			return "{"+width+", "+height+"}";
		}
		
		public void Set(int w, int h){
			this.width = w; this.height = h;
		}
		
		public void Set(Size2i v)
		{
			width = v.width; height = v.height;
		}
		
		public static bool operator == (Size2i lhs, Size2i rhs)
		{
			return (lhs.width == rhs.width)	&& (lhs.height == rhs.height);
		}
		
		public static bool operator != (Size2i lhs, Size2i rhs)
		{
			return (lhs.width != rhs.width)	|| (lhs.height != rhs.height);
		}
		
		public static Size2i operator + (Size2i a, Size2i b)
		{
			return new Size2i(a.width + b.width, a.height + b.height);
		}
		
		public static Size2i operator - (Size2i a, Size2i b)
		{
			return new Size2i(a.width - b.width, a.width - b.width);
		}
		
		public static Size2i operator - (Size2i a)
		{
			return new Size2i(-a.width, -a.height);
		}
		
		public override bool Equals (object other)
		{
			if (!(other is Size2i))
			{
				return false;
			}
			Size2i vector = (Size2i)other;
			return this.width.Equals(vector.width) && this.height.Equals(vector.height);
		}
		
		public override int GetHashCode()
		{
			return this.width.GetHashCode() ^ this.height.GetHashCode() << 2;
		}
	}
	
	[System.Serializable]
	public struct Vector2i
	{
		public int x;
		public int y;
		
		public Vector2i(Vector2i v){
			x = v.x; y = v.y;
		}
		
		public Vector2i(int x, int y){
			this.x = x; this.y = y;
		}
		
		public void Set(int x, int y){
			this.x = x; this.y = y;
		}
		
		public void Set(Vector2i v)
		{
			x = v.x; y = v.y;
		}
		
		public int Magnitude
		{
			get { return MathUtil.Sqrt(x*x + y*y); }	
		}
		
		public static int Distance(Vector2i v1, Vector2i v2)
		{
			return (v1-v2).Magnitude;	
		}
		
		public static explicit operator Vector2i(Vector2 v){
			return new Vector2i((int)v.x, (int)v.y);	
		}
		
		override public string ToString()
		{
			return "{"+x+", "+y+"}";
		}
		
		public static bool operator == (Vector2i lhs, Vector2i rhs)
		{
			return (lhs.x == rhs.x)	&& (lhs.y == rhs.y);
		}
		
		public static bool operator != (Vector2i lhs, Vector2i rhs)
		{
			return (lhs.x != rhs.x)	|| (lhs.y != rhs.y);
		}
		
		public static Vector2i operator + (Vector2i a, Vector2i b)
		{
			return new Vector2i(a.x + b.x, a.y + b.y);
		}
		
		public static Vector2i operator - (Vector2i a, Vector2i b)
		{
			return new Vector2i(a.x - b.x, a.y - b.y);
		}
		
		public static Vector2i operator - (Vector2i a)
		{
			return new Vector2i(-a.x, -a.y);
		}
		
		public override bool Equals (object other)
		{
			if (!(other is Vector2i))
			{
				return false;
			}
			Vector2i vector = (Vector2i)other;
			return this.x.Equals(vector.x) && this.y.Equals(vector.y);
		}
		
		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
		}
	}
	
	public class Vector3i 
	{
		private int[] array = new int[3];
		
		public enum Axis
		{
			X = 0,
			Y = 1,
			Z = 2
		}
		
		public int x {
			get { return array[0]; }
			set { array[0] = value; }
		}
		
		public int y {
			get { return array[1]; }
			set { array[1] = value; }
		}
		
		public int z {
			get { return array[2]; }
			set { array[2] = value; }
		}
		
		public Vector3i(){
			this.x = 0; this.y = 0; this.z = 0;	
		}
		
		public Vector3i(Vector3i v){
			x = v.x; y = v.y; z = v.z;
		}
		
		public Vector3i(int x, int y, int z){
			this.x = x; this.y = y; this.z = z;	
		}
		
		public void Set(int x, int y, int z){
			this.x = x; this.y = y; this.z = z;	
		}
		
		public static explicit operator Vector3i(Vector3 v){
			return new Vector3i((int)v.x, (int)v.y, (int)v.z);	
		}
		
		public int this[Vector3i.Axis i]
		{
			get { 
				return array[(int)i];
			}
			
			set {
				array[(int)i] = value;	
			}
		}
		
		override public string ToString()
		{
			return "{"+x+", "+y+", "+z+"}";
		}
	}
	
	[System.Serializable]
	public class Vector3iField : System.Object
	{
		public int x;
		public int y;
		public int z;
		
		public static implicit operator Vector3i(Vector3iField v){
			return new Vector3i((int)v.x, (int)v.y, (int)v.z);	
		}
		
		public static implicit operator Vector3iField(Vector3i v){
			Vector3iField f = new Vector3iField();	
			f.x = v.x; f.y = v.y; f.z = v.z;
			return f;
		}
	}
}