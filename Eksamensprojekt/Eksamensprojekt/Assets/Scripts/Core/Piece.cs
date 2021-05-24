namespace Chess {
	public static class Piece {

		public const int Ingen = 0;
		public const int Konge = 1;
		public const int Bonde = 2;
		public const int Rytter = 3;
		public const int Biskop = 5;
		public const int Tårn = 6;
		public const int Dronning = 7;
		public const int Hvid = 8;
		public const int Sort = 16;

		const int typeMask = 0b00111;
		const int sortMask = 0b10000;
		const int hvidMask = 0b01000;
		const int farveMask = hvidMask | sortMask;

		public static bool ErFarve (int piece, int colour) {
			return (piece & farveMask) == colour;
		}

		public static int Farve (int piece) {
			return piece & farveMask;
		}

		public static int BrikType (int piece) {
			return piece & typeMask;
		}

		public static bool ErTårnEllerDronning (int piece) {
			return (piece & 0b110) == 0b110;
		}

		public static bool ErBiskopEllerDronning (int piece) {
			return (piece & 0b101) == 0b101;
		}

		public static bool ErGlidendeBrik (int piece) {
			return (piece & 0b100) != 0;
		}
	}
}