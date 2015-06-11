namespace SausageAssassins {
	public static class MaximumRect {
		public delegate bool TileIsOpaque(uint x, uint y);

		public static Recti Find(uint mapWidth, uint mapHeight) {
			return new Recti( 0, 0, 0, 0 );
		}
	}
}
