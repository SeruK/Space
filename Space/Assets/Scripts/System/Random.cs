using System;

namespace SA
{	
	static class Random
	{
		public static UInt32 Xor128(UInt32 x, UInt32 y,
							 		UInt32 z, UInt32 w)
		{
			System.UInt32 t = x ^ (x << 11);
 			x = y; y = z; z = w;
  			return (w = w ^ (w >> 19) ^ (t ^ (t >> 8)));
		}
		
		public delegate T RandomMethod<T>();
		public delegate T RandomRangeMethod<T>(T max);
		public delegate T RandomRangeValuesMethod<T>(T min, T max);
		
		public static int Weighted(UInt32[] weights, RandomRangeMethod<UInt32> random)
		{
			UInt32 sum = 0;
			int numChoices = weights.Length;
			for(int i = 0; i < numChoices; ++i) 
			{
			   sum += weights[i];
			}
			
			UInt32 rnd = random(sum);
			
			for(int i = 0; i < numChoices; ++i) 
			{
			  	if(rnd < weights[i])
				{
			    	return i;
				}
				
			  	rnd -= weights[i];
			}
			
			throw new Exception("Should never get here!");
		}

		public static T InArray<T>(T[] array, RandomRangeMethod<UInt32> random)
		{
			return array[random((UInt32)array.Length)];
		}

		public static T WeightedInArray<T>(T[] array, UInt32[] weights, RandomRangeMethod<UInt32> random)
		{
			if(array.Length != weights.Length)
			{
				throw new Exception("Array["+array.Length+"] and weights["+weights.Length+"] were not the same lengths.");
			}
			
			return array[Weighted(weights, random)];
		}
		
		public static bool CoinToss(RandomMethod<UInt32> random)
		{
			return (random() % 2) == 0;
		}
		
		public static bool FradulentCoinToss(UInt32 falseWeight, UInt32 trueWeight, RandomRangeMethod<UInt32> random)
		{
			bool[] bools = {false, true};
			UInt32[] weights = {falseWeight, trueWeight};
			return WeightedInArray(bools, weights, random);
		}
	}
	
	public class RandomizerXor128
	{
		UInt32 x;
		UInt32 y;
		UInt32 z;
		UInt32 w;
		
		public RandomizerXor128(UInt32 x=123456789u, UInt32 y=362436069u,
							 	UInt32 z=521288629u, UInt32 w=88675123u)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
		
		public UInt32 GetNext()
		{
			System.UInt32 t = x ^ (x << 11);
 			x = y; y = z; z = w;
  			return (w = w ^ (w >> 19) ^ (t ^ (t >> 8)));
		}
		
		public UInt32 GetNext(UInt32 max)
		{
			return GetNext(0u, max);
		}
		
		public UInt32 GetNext(UInt32 min, UInt32 max)
		{
			return (UInt32)((float)(max - min) * Value) + min;
		}
		
		public float Value
		{
			get { return (float)GetNext() / (float)(System.UInt32.MaxValue-1u); }
		}
	}
}