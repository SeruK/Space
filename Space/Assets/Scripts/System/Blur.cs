using UnityEngine;
	
namespace SausageAssassins
{
	static class Blur
	{
		// Stack Blur Algorithm by Mario Klingemann <mario@quasimondo.com>
		// http://incubator.quasimondo.com/processing/stackblur.pde
		public static void StackBlurRGB(ref uint[] dest, ref uint[] source, uint width, uint height, uint in_radius)
		{
			if (in_radius == 0) return;
			
			int radius = (int)in_radius;
			int w = (int)width;
			int h = (int)height;
			int wm = w - 1;
			int hm = h - 1;
			int wh = w * h;
			int div = radius + radius + 1;
			var r = new int[wh];
			var g = new int[wh];
			var b = new int[wh];
			int rsum, gsum, bsum, x, y, i, p1, p2, yi;
			var vmin = new int[Mathf.Max((int)w, (int)h)];
			var vmax = new int[Mathf.Max((int)w, (int)h)];
			
			var dv = new int[256 * div];
			for (i = 0; i < 256 * div; i++) {
				dv[i] = (i / div);
			}
			
			int yw = yi = 0;
			
			for (y = 0; y < h; y++) { // blur horizontal
				rsum = gsum = bsum = 0;
				for (i = -radius; i <= radius; i++) {
			        int p = unchecked((int)source[yi + Mathf.Min(wm, Mathf.Max(i, 0))]);
			        rsum += (p & 0xff0000) >> 16;
			        gsum += (p & 0x00ff00) >> 8;
			        bsum += (p & 0x0000ff);
				}
				for (x = 0; x < w; x++) {
					
			        r[yi] = dv[rsum];
			        g[yi] = dv[gsum];
			        b[yi] = dv[bsum];
					
			        if (y == 0) {
						vmin[x] = Mathf.Min(x + radius + 1, wm);
						vmax[x] = Mathf.Max(x - radius, 0);
			        }
			        p1 = unchecked((int)source[yw + vmin[x]]);
			        p2 = unchecked((int)source[yw + vmax[x]]);
					
			        rsum += ((p1 & 0xff0000) - (p2 & 0xff0000)) >> 16;
			        gsum += ((p1 & 0x00ff00) - (p2 & 0x00ff00)) >> 8;
			        bsum += (p1 & 0x0000ff) - (p2 & 0x0000ff);
			        yi++;
				}
				yw += w;
			}
			
			for (x = 0; x < w; x++) { // blur vertical
				rsum = gsum = bsum = 0;
				int yp = -radius * w;
				for (i = -radius; i <= radius; i++) {
			        yi = Mathf.Max(0, yp) + x;
			        rsum += r[yi];
			        gsum += g[yi];
			        bsum += b[yi];
			        yp += w;
				}
				yi = x;
				for (y = 0; y < h; y++) {
			        dest[yi] = (0xff000000u | (uint)(dv[rsum] << 16) | (uint)(dv[gsum] << 8) | (uint)dv[bsum]);
			        if (x == 0) {
						vmin[y] = Mathf.Min(y + radius + 1, hm) * w;
						vmax[y] = Mathf.Max(y - radius, 0) * w;
			        }
			        p1 = x + vmin[y];
			        p2 = x + vmax[y];
					
			        rsum += r[p1] - r[p2];
			        gsum += g[p1] - g[p2];
			        bsum += b[p1] - b[p2];
					
			        yi += w;
				}
			}
		}
		
		public static void StackBlur(ref System.UInt16[] dest, ref System.UInt16[] source, uint width, uint height, uint in_radius)
		{
			if (in_radius == 0) return;
			
			int radius = (int)in_radius;
			int w = (int)width;
			int h = (int)height;
			int wm = w - 1;
			int hm = h - 1;
			int wh = w * h;
			int div = radius + radius + 1;
			var r = new int[wh];
			int rsum, x, y, i, p1, p2, yi;
			var vmin = new int[Mathf.Max((int)w, (int)h)];
			var vmax = new int[Mathf.Max((int)w, (int)h)];
			
			var dv = new int[256 * div];
			for (i = 0; i < 256 * div; i++) {
				dv[i] = (i / div);
			}
			
			int yw = yi = 0;
			
			for (y = 0; y < h; y++) { // blur horizontal
				rsum = 0;
				for (i = -radius; i <= radius; i++) {
			        int p = unchecked((int)source[yi + Mathf.Min(wm, Mathf.Max(i, 0))]);
			        rsum += (p & 0x0000ff);
				}
				for (x = 0; x < w; x++) {
					
			        r[yi] = dv[rsum];
					
			        if (y == 0) {
						vmin[x] = Mathf.Min(x + radius + 1, wm);
						vmax[x] = Mathf.Max(x - radius, 0);
			        }
			        p1 = unchecked((int)source[yw + vmin[x]]);
			        p2 = unchecked((int)source[yw + vmax[x]]);
					
			        rsum += ((p1 & 0x0000ff) - (p2 & 0x0000ff));
			        yi++;
				}
				yw += w;
			}
			
			for (x = 0; x < w; x++) { // blur vertical
				rsum = 0;
				int yp = -radius * w;
				for (i = -radius; i <= radius; i++) {
			        yi = Mathf.Max(0, yp) + x;
			        rsum += r[yi];
			        yp += w;
				}
				yi = x;
				for (y = 0; y < h; y++) {
			        dest[yi] = unchecked((System.UInt16)dv[rsum]);
			        if (x == 0) {
						vmin[y] = Mathf.Min(y + radius + 1, hm) * w;
						vmax[y] = Mathf.Max(y - radius, 0) * w;
			        }
			        p1 = x + vmin[y];
			        p2 = x + vmax[y];
					
			        rsum += r[p1] - r[p2];
					
			        yi += w;
				}
			}
		}
	}
}