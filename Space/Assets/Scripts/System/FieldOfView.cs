using UnityEngine;
using System.Collections;

namespace SausageAssassins
{
	public delegate bool TileIsOpaque(uint x, uint y);
	public delegate void SetTileVisibility(uint x, uint y, bool visible);
	
	public static class FieldOfView
	{
		public static void LightenPoint(Vector2i origin, uint radius, uint overshoot, uint mapWidth, uint mapHeight, 
										TileIsOpaque tileIsOpaque, SetTileVisibility setTileVisibility)
		{
			if((origin.x+radius) < 0 || (origin.y+radius < 0) || (origin.x-radius) >= mapWidth || (origin.y-radius >= mapHeight))
			{
				return;	
			}
			
			for (uint i = 0; i < 8; i++) 
			{
				castLight(origin, radius, overshoot, mapWidth, mapHeight, tileIsOpaque, setTileVisibility, 1, 1.0f, 0.0f, multipliers[0, i],
                multipliers[1, i], multipliers[2, i], multipliers[3, i]);
    		}
		}
		
		private static int[,] multipliers = {
    		{1, 0, 0, -1, -1, 0, 0, 1},
    		{0, 1, -1, 0, 0, -1, 1, 0},
    		{0, 1, 1, 0, 0, -1, -1, 0},
    		{1, 0, 0, 1, -1, 0, 0, -1}
		};
		
		private static void castLight(Vector2i origin, uint radius, uint overshoot, uint mapWidth, uint mapHeight, 
									  TileIsOpaque tileIsOpaque, SetTileVisibility setTileVisibility,
									  int row, float startSlope, float endSlope, int xx, int xy, int yx,
        							  int yy)
		{
			if (startSlope < endSlope) {
		        return;
		    }
		    float nextStartSlope = startSlope;
		    for (int i = row; i <= radius; i++) {
		        bool blocked = false;
		        for (int dx = -i, dy = -i; dx <= 0; dx++) {
		            float lSlope = (dx - 0.5f) / (dy + 0.5f);
		            float rSlope = (dx + 0.5f) / (dy - 0.5f);
		            if (startSlope < rSlope) {
		                continue;
		            } else if (endSlope > lSlope) {
		                break;
		            }
		
		            int sax = dx * xx + dy * xy;
		            int say = dx * yx + dy * yy;
		            if ((sax < 0 && (uint)Mathf.Abs(sax) > origin.x) ||
		                    (say < 0 && (uint)Mathf.Abs(say) > origin.y)) {
		                continue;
		            }
		            int ax = origin.x + sax;
		            int ay = origin.y + say;
		            if (ax < 0 || ay < 0 || ax >= mapWidth || ay >= mapHeight) {
		                continue;
		            }
		
		            uint radius2 = radius * radius;
					uint distance = (uint)(dx * dx + dy * dy);
		            if (distance < radius2) {
		                setTileVisibility((uint)ax, (uint)ay, true);
		            }
		
		            if (blocked) {
		                if (tileIsOpaque((uint)ax, (uint)ay)) {
		                    nextStartSlope = rSlope;
		                    continue;
		                } else {
		                    blocked = false;
		                    startSlope = nextStartSlope;
		                }
		            } else if (tileIsOpaque((uint)ax, (uint)ay)) {
		                blocked = true;
		                nextStartSlope = rSlope;
						castLight(origin, radius, overshoot, mapWidth, mapHeight, tileIsOpaque, setTileVisibility, i + 1, startSlope, 
								  lSlope, xx, xy, yx, yy);
		            }
		        }
		        if (blocked) {
		            break;
		        }
		    }
		}
	}
}