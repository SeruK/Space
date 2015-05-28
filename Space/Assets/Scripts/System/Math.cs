using UnityEngine;
namespace SA
{
	static class MathUtil
	{
		public static uint Max(uint a, uint b)
		{
			return a <= b ? b : a;
		}
		
		public static uint Min (uint a, uint b)
		{
			return (a >= b) ? b : a;
		}
		
		// http://www.codecodex.com/wiki/Calculate_an_integer_square_root
		public static int Sqrt(int num)
		{
    		if (0 == num) { return 0; }  // Avoid zero divide  
    		int n = (num / 2) + 1;       // Initial estimate, never low  
    		int n1 = (n + (num / n)) / 2;  
    		while (n1 < n) {  
        		n = n1;  
        		n1 = (n + (num / n)) / 2;  
    		}
    		return n;  
		}
	}
	
	/* Cheers! @ http://gizma.com/easing/ */
	public static class Easing
	{
		public delegate float EasingMethod(float fromVal, float toVal, float alpha);
		
		public enum Mode
		{
			In, // Accelerating from zero velocity
			Out, // Decelerating from zero velocity
			InOut // Accelerating until half-way, then decelerating
		}
		
		public enum Algorithm // please do not reorder
		{
			Linear,
			Quadratic,
			Cubic,
			Quartic,
			Quintic,
			Sinusoidal, // this is the best word ever
			Exponential,
			Circular
		}
		
		// Short hand
		public static readonly Mode In = Mode.In;
		public static readonly Mode Out = Mode.Out;
		public static readonly Mode InOut = Mode.InOut;
		
		public const Algorithm Lerp = Algorithm.Linear;
		public const Algorithm Quad = Algorithm.Quadratic;
		public const Algorithm Cube = Algorithm.Cubic;
		public const Algorithm Quart = Algorithm.Quartic;
		public const Algorithm Quint = Algorithm.Quintic;
		public const Algorithm Sine = Algorithm.Sinusoidal;
		public const Algorithm Expo = Algorithm.Exponential;
		public const Algorithm Circ = Algorithm.Circular;
		
		private static EasingMethod[,] methods = {
			{ EaseInQuad, 	EaseOutQuad, 	EaseInOutQuad  },
			{ EaseInCubic, 	EaseOutCubic, 	EaseInOutCubic },
			{ EaseInQuart, 	EaseOutQuart, 	EaseInOutQuart },
			{ EaseInQuint, 	EaseOutQuint, 	EaseInOutQuint },
			{ EaseInSine, 	EaseOutSine, 	EaseInOutSine  },
			{ EaseInExpo, 	EaseOutExpo, 	EaseInOutExpo  },
			{ EaseInCirc, 	EaseOutCirc, 	EaseInOutCirc  }
		};
		
		/**
		 *	Transforms an alpha value to the corresponding value when applied
		 *	by the supplied algorithm in the supplied mode.
		 *
		 *	@param a
		 *		An alpha value between 0.0-1.0.
		 *	@param mode
		 *		The mode to use.
		 *	@param algo
		 *		The algorithm to apply.
		 *
		 *	@return 
		 *		A new alpha value between 0.0-1.0.
		 */
		public static float Alpha(float a, Mode mode, Algorithm algo)
		{
			return Ease(0.0f, 1.0f, a, mode, algo);	
		}
		
		/**
		 *	Returns an interpolated value between from and to, when applying the supplied
		 *	algorithm in the supplied mode.
		 *
		 *	@param fromVal
		 *		The value when a = 0.0.
		 *	@param toVal
		 *		The value when a = 1.0.
		 *	@param a
		 *		An alpha value between 0.0-1.0.
		 *	@param mode
		 *		The mode to use.
		 *	@param algo
		 *		The algorithm to apply.
		 *
		 *	@return 
		 *		The interpolated value between from and to.
		 */
		public static float Ease(float fromVal, float toVal, float a, Mode mode, Algorithm algo)
		{
			if(algo == Algorithm.Linear)
				return Linear(fromVal, toVal, a);
					
			return methods[(int)algo-1, (int)mode](fromVal, toVal, a);
		}
		
		public static float Linear(float fromVal, float toVal, float a)
		{
			return toVal*a + fromVal;
		}
		
		public static float EaseInQuad(float fromVal, float toVal, float a)
		{
			return toVal*a*a + fromVal;
		}
		
		public static float EaseOutQuad(float fromVal, float toVal, float a)
		{
			return -toVal*a*(a-2.0f) + fromVal;
		}
		
		public static float EaseInOutQuad(float fromVal, float toVal, float a)
		{
			a *= 2.0f;
			if(a < 1.0f)
				return toVal/2.0f*a*a + fromVal;
			a -= 2.0f;
			return -toVal/2.0f * (a*(a-2.0f) - 1.0f) + fromVal;
		}
		
		public static float EaseInCubic(float fromVal, float toVal, float a)
		{
    		return toVal*(a*a*a) + fromVal;
		}
		
		public static float EaseOutCubic(float fromVal, float toVal, float a)
		{
    		--a;
    		return toVal*(a*a*a + 1.0f) + fromVal;
		}
		
		public static float EaseInOutCubic(float fromVal, float toVal, float a)
		{
			a *= 2.0f;
			if(a < 1.0f)
				return toVal/2.0f*a*a*a + fromVal;
			a -= 2.0f;
			return toVal/2.0f * (a*a*a + 2.0f) + fromVal;
		}
		
		public static float EaseInQuart(float fromVal, float toVal, float a)
		{
			return toVal*a*a*a*a + fromVal;
		}
		
		public static float EaseOutQuart(float fromVal, float toVal, float a)
		{
			--a;
			return -toVal*(a*a*a*a - 1.0f) + toVal;
		}
		
		public static float EaseInOutQuart(float fromVal, float toVal, float a)
		{
			a *= 2.0f;
			if(a < 1.0f)
				return toVal/2.0f*a*a*a*a + fromVal;
			a -= 2.0f;
			return -toVal/2.0f * (a*a*a*a + 2.0f) + fromVal;	
		}
		
		public static float EaseInQuint(float fromVal, float toVal, float a)
		{
			return toVal*a*a*a*a*a + fromVal;	
		}
		
		public static float EaseOutQuint(float fromVal, float toVal, float a)
		{
			--a;
			return toVal*(a*a*a*a*a + 1.0f) + fromVal;	
		}
		
		public static float EaseInOutQuint(float fromVal, float toVal, float a)
		{
			a *= 2.0f;
			if(a < 1.0f)
				return toVal/2.0f*a*a*a*a*a + fromVal;
			a -= 2.0f;
			return toVal/2.0f*(a*a*a*a*a + 1.0f) + fromVal;
		}
		
		public static float EaseInSine(float fromVal, float toVal, float a)
		{
			return -toVal * Mathf.Cos(a * (Mathf.PI/2.0f)) + toVal + fromVal;
		}
		
		public static float EaseOutSine(float fromVal, float toVal, float a)
		{
			return toVal * Mathf.Sin(a * (Mathf.PI/2.0f)) + fromVal;	
		}
		
		public static float EaseInOutSine(float fromVal, float toVal, float a)
		{
			return -toVal/2.0f * (Mathf.Cos(Mathf.PI*a) - 1.0f) + fromVal;
		}
		
		public static float EaseInExpo(float fromVal, float toVal, float a)
		{
			return toVal * Mathf.Pow(2.0f, 10.0f * (a-1.0f)) + fromVal;
		}
		
		public static float EaseOutExpo(float fromVal, float toVal, float a)
		{
			return toVal * (-Mathf.Pow(2.0f, -10.0f * a) + 1.0f) + fromVal;	
		}
		
		public static float EaseInOutExpo(float fromVal, float toVal, float a)
		{
			a *= 2.0f;
			if(a < 1.0f)
				return toVal/2.0f * Mathf.Pow(2.0f, 10.0f * (a - 1.0f)) + fromVal;
			--a;
			return toVal/2.0f * (-Mathf.Pow(2.0f, -10.0f * a) + 2.0f) + fromVal;
		}
		
		public static float EaseInCirc(float fromVal, float toVal, float a)
		{
			return -toVal * (Mathf.Sqrt(1.0f - a*a) - 1.0f) + fromVal;	
		}
		
		public static float EaseOutCirc(float fromVal, float toVal, float a)
		{
			--a;
			return toVal * Mathf.Sqrt(1.0f - a*a) + fromVal;
		}
		
		public static float EaseInOutCirc(float fromVal, float toVal, float a)
		{
			a *= 2.0f;
			if(a < 1.0f)
				return -toVal/2.0f * (Mathf.Sqrt(1.0f - a*a) - 1.0f) + fromVal;
			a -= 2.0f;
			return toVal/2.0f * (Mathf.Sqrt(1.0f - a*a) + 1.0f) + fromVal;
		}
	}
}