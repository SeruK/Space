using UnityEngine;
using System.Collections;

namespace SA
{
	static class Simplex
	{
		public static float GenerateOne1D(float x, float freq, int octaves, float lacunarity, float gain, float amplitude, bool normalize = true)
		{
			float sum = 0.0f;
			
			for(int i = 0; i < octaves; ++i)
			{
				sum += amplitude * SimplexNoise1D(x * freq);
				freq *= lacunarity;
				amplitude *= gain;
			}
			
			return normalize ? sum / (float)octaves : sum;
		}
		
		public static float GenerateOne2D(Vector2 vec, float freq, int octaves, float lacunarity, float gain, float amplitude, bool normalize=true)
		{ return GenerateOne2D(vec.x, vec.y, freq, octaves, lacunarity, gain, amplitude, normalize); }	
		
		public static float GenerateOne2D(float x, float y, float freq, int octaves, float lacunarity, float gain, float amplitude, bool normalize=true)
		{					
			float sum = 0.0f;
			
			for(int i = 0; i < octaves; ++i)
			{
				sum += amplitude * SimplexNoise2D(x*freq, y*freq);
				freq *= lacunarity;
				amplitude *= gain;
			}
			
			return normalize ? sum / (float)octaves : sum;
		}
		
		public static float GenerateOne3D(Vector3 vec, float freq, int octaves, float lacunarity, float gain, float amplitude, bool normalize=true)
		{ return GenerateOne3D(vec.x, vec.y, vec.z, freq, octaves, lacunarity, gain, amplitude, normalize); }
		
		public static float GenerateOne3D(float x, float y, float z, float freq, int octaves, float lacunarity, float gain, float amplitude, bool normalize=true)
		{			
			float sum = 0.0f;
			
			for(int i = 0; i < octaves; ++i)
			{
				sum += amplitude * SimplexNoise3D(x*freq, y*freq, z*freq);
				freq *= lacunarity;
				amplitude *= gain;
			}
			
			return normalize ? sum / (float)octaves : sum;
		}
		
		public static float SimplexNoise1D(float x)
		{
			int i0 = Mathf.FloorToInt(x);
			int i1 = i0 + 1;
			float x0 = x - i0;
		  	float x1 = x0 - 1.0f;
			
			float n0, n1;
			
			float t0 = 1.0f - x0*x0;
			
			t0 *= t0;
			int h = perm[i0 & 0xff] & 15;
			float grad = 1.0f + (h & 7);
			if((h & 8) != 0) grad = -grad;
			grad *= x0;
			n0 = t0 * t0 * grad;
			
			float t1 = 1.0f - x1*x1;
			
			t1 *= t1;
			h = perm[i1 & 0xff] & 15;
			grad = 1.0f - (h & 7);
			if((h & 8) != 0) grad = -grad;
			grad *= x1;
			n1 = t1 * t1 * grad;
			// The maximum value of this noise is 8*(3/4)^4 = 2.53125
			// A factor of 0.395 would scale to fit exactly within [-1,1], but
			// we want to match PRMan's 1D noise, so we scale it down some more.
			return 0.25f * (n0 + n1);
		}
		
		public static float SimplexNoise2D(float in_x, float in_y)
		{
			float n0, n1, n2; // Noise contributions from the three corners	
			
			// Skew the input space to determine which simplex cell we're in
			float F2 = 0.5f*(Mathf.Sqrt(3.0f)-1.0f);
			float s = (in_x+in_y)*F2; // Hairy factor for 2D
			
			int i = Mathf.FloorToInt(in_x+s);
			int j = Mathf.FloorToInt(in_y+s);
			
			float G2 = (3.0f-Mathf.Sqrt(3.0f))/6.0f;
			float t = (i+j)*G2;
			float X0 = i-t;
			// Unskew the cell origin back to (x,y) space
			float Y0 = j-t;
			float x0 = in_x-X0;
			// The x,y distances from the cell origin
			float y0 = in_y-Y0;
			
			 // For the 2D case, the simplex shape is an equilateral triangle.
			// Determine which simplex we are in.
			int i1, j1;
			// Offsets for second (middle) corner of simplex in (i,j) coords
			if(x0>y0) // lower triangle, XY order: (0,0)->(1,0)->(1,1)
			{
				i1=1; 
				j1=0;
			} 
			else // upper triangle, YX order: (0,0)->(0,1)->(1,1)
			{
				i1=0; 
				j1=1;
			}
			
			 // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
			// a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
			// c = (3-sqrt(3))/6
			float x1 = x0 - i1 + G2;
			// Offsets for middle corner in (x,y) unskewed coords
			float y1 = y0 - j1 + G2;
			float x2 = x0 - 1.0f + 2.0f * G2;
			// Offsets for last corner in (x,y) unskewed coords
			float y2 = y0 - 1.0f + 2.0f * G2;
			
			// Work out the hashed gradient indices of the three simplex corners
			int ii = i & 255;
			int jj = j & 255;
			int gi0 = perm[ii+perm[jj]] % 12;
			int gi1 = perm[ii+i1+perm[jj+j1]] % 12;
			int gi2 = perm[ii+1+perm[jj+1]] % 12;
			
			// Calculate the contribution from the three corners
			float t0 = 0.5f - x0*x0-y0*y0;
			if(t0<0) 
			{
				n0 = 0.0f;
			}
			else 
			{
				t0 *= t0;
				n0 = t0 * t0 * Vector2.Dot(new Vector2(grad3[gi0,0],grad3[gi0,1]), new Vector2(x0, y0));
				// (x,y) of grad3 used for 2D gradient
			}
			float t1 = 0.5f - x1*x1-y1*y1;
			if(t1<0)
			{
				n1 = 0.0f;
			}
			else 
			{
				t1 *= t1;
				n1 = t1 * t1 * Vector2.Dot(new Vector2(grad3[gi1,0], grad3[gi1,1]), new Vector2(x1, y1));
			}
			
			float t2 = 0.5f - x2*x2-y2*y2;
			if(t2<0)
			{
				n2 = 0.0f;
			}
			else 
			{
				t2 *= t2;
				n2 = t2 * t2 * Vector2.Dot(new Vector2(grad3[gi2,0], grad3[gi2,1]), new Vector2(x2, y2));
			}
			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			return 70.0f * (n0 + n1 + n2);
		}
		
		public static float SimplexNoise3D(float in_x, float in_y, float in_z)
		{
			float n0, n1, n2, n3;
			
			// Skew the input space to determine which simplex cell we're in
			float F3 = 1.0f/3.0f;
			float s = (in_x+in_y+in_z)*F3; // Very nice and simple skew factor for 3D
			int i = Mathf.FloorToInt(in_x+s);
			int j = Mathf.FloorToInt(in_y+s);
			int k = Mathf.FloorToInt(in_z+s);
			
			float G3 = 1.0f/6.0f; // Very nice and simple unskew factor, too
			float t = (i+j+k)*G3;
			float X0 = i-t; // Unskew the cell origin back to (x,y,z) space
			float Y0 = j-t;
			float Z0 = k-t;
			float x0 = in_x-X0; // The x,y,z distances from the cell origin
			float y0 = in_y-Y0;
			float z0 = in_z-Z0;
			
			// For the 3D case, the simplex shape is a slightly irregular tetrahedron.
			// Determine which simplex we are in.
			int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
			int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords
			
			if(x0>=y0) 
			{
				if(y0>=z0)
				{ i1=1; j1=0; k1=0; i2=1; j2=1; k2=0; } // X Y Z order
				else if(x0>=z0) { i1=1; j1=0; k1=0; i2=1; j2=0; k2=1; } // X Z Y order
				else { i1=0; j1=0; k1=1; i2=1; j2=0; k2=1; } // Z X Y order
			}
			else 
			{
				// x0<y0
				if(y0<z0) { i1=0; j1=0; k1=1; i2=0; j2=1; k2=1; } // Z Y X order
				else if(x0<z0) { i1=0; j1=1; k1=0; i2=0; j2=1; k2=1; } // Y Z X order
				else { i1=0; j1=1; k1=0; i2=1; j2=1; k2=0; } // Y X Z order
			}
			
			// A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
			// a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
			// a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
			// c = 1/6.
			float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
			float y1 = y0 - j1 + G3;
			float z1 = z0 - k1 + G3;
			float x2 = x0 - i2 + 2.0f*G3; // Offsets for third corner in (x,y,z) coords
			float y2 = y0 - j2 + 2.0f*G3;
			float z2 = z0 - k2 + 2.0f*G3;
			float x3 = x0 - 1.0f + 3.0f*G3; // Offsets for last corner in (x,y,z) coords
			float y3 = y0 - 1.0f + 3.0f*G3;
			float z3 = z0 - 1.0f + 3.0f*G3;
			
			 // Work out the hashed gradient indices of the four simplex corners
			int ii = i & 255;
			int jj = j & 255;
			int kk = k & 255;
			int gi0 = perm[ii+perm[jj+perm[kk]]] % 12;
			int gi1 = perm[ii+i1+perm[jj+j1+perm[kk+k1]]] % 12;
			int gi2 = perm[ii+i2+perm[jj+j2+perm[kk+k2]]] % 12;
			int gi3 = perm[ii+1+perm[jj+1+perm[kk+1]]] % 12;
			
			 // Calculate the contribution from the four corners
			float t0 = 0.6f - x0*x0 - y0*y0 - z0*z0;
			if(t0<0) n0 = 0.0f;
			else {
			t0 *= t0;
			n0 = t0 * t0 * Vector3.Dot(new Vector3(grad3[gi0,0], grad3[gi0,1], grad3[gi0,2]), new Vector3(x0, y0, z0));
			}
			float t1 = 0.6f - x1*x1 - y1*y1 - z1*z1;
			if(t1<0) n1 = 0.0f;
			else {
			t1 *= t1;
			n1 = t1 * t1 * Vector3.Dot(new Vector3(grad3[gi1,0],grad3[gi1,1],grad3[gi1,2]), new Vector3(x1, y1, z1));
			}
			float t2 = 0.6f - x2*x2 - y2*y2 - z2*z2;
			if(t2<0) n2 = 0.0f;
			else {
			t2 *= t2;
			n2 = t2 * t2 * Vector3.Dot(new Vector3(grad3[gi2,0],grad3[gi2,1],grad3[gi2,2]), new Vector3(x2, y2, z2));
			}
			float t3 = 0.6f - x3*x3 - y3*y3 - z3*z3;
			if(t3<0) n3 = 0.0f;
			else {
			t3 *= t3;
			n3 = t3 * t3 * Vector3.Dot(new Vector3(grad3[gi3,0],grad3[gi3,1],grad3[gi3,2]), new Vector3(x3, y3, z3));
			}
			// Add contributions from each corner to get the final noise value.
			// The result is scaled to stay just inside [-1,1]
			return 32.0f*(n0 + n1 + n2 + n3);
		}
			
		private static int[,] grad3 = {{1,1,0},{-1,1,0},{1,-1,0},{-1,-1,0},
			{1,0,1},{-1,0,1},{1,0,-1},{-1,0,-1},
			{0,1,1},{0,-1,1},{0,1,-1},{0,-1,-1}};
		
//		Permutation table
//		private static int[] p = {151,160,137,91,90,15,
//			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
//			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
//			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
//			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
//			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
//			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
//			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
//			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
//			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
//			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
//			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
//			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180};
		
//		 To remove the need for index wrapping, double the permutation table length:
//			for(int i=0; i<512; i++)
//			{
//				perm[i]=p[i & 255];
//			}
		private static int[] perm = {151,
			160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,
			37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,
			32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,
			27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,200,196,135,130,
			116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,38,147,118,
			126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,
			152,2,44,154,163,70,221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,
			113,224,232,178,185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,
			81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,
			45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,
			156,180,151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,
			142,8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,
			117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,
			134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,
			245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,200,
			196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,
			38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,
			213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,129,22,39,253,19,98,
			108,110,79,113,224,232,178,185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,
			179,162,241,81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,
			115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,
			66,215,61,156,180 	
			};
	}
}
