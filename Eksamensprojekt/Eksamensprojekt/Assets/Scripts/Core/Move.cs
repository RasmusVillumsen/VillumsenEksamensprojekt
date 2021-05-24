/* 
To preserve memory during search, moves are stored as 16 bit numbers.
The format is as follows:

bit 0-5: from square (0 to 63)
bit 6-11: to square (0 to 63)
bit 12-15: flag
*/
namespace Chess {

	public readonly struct Move {

		public readonly struct Flag {
			public const int Ingen = 0;
			public const int EnPassantErobring = 1;
			public const int Rokade = 2;
			public const int ForfremTilDronning = 3;
			public const int ForfremTilRytter = 4;
			public const int ForfremTilTårn = 5;
			public const int ForfremTilBiskop = 6;
			public const int BondeToFremad = 7;
		}

		readonly ushort moveValue;

		const ushort startSquareMask = 0b0000000000111111;
		const ushort targetSquareMask = 0b0000111111000000;
		const ushort flagMask = 0b1111000000000000;

		public Move (ushort moveValue) {
			this.moveValue = moveValue;
		}

		public Move (int startSquare, int targetSquare) {
			moveValue = (ushort) (startSquare | targetSquare << 6);
		}

		public Move (int startSquare, int targetSquare, int flag) {
			moveValue = (ushort) (startSquare | targetSquare << 6 | flag << 12);
		}

		public int StartSquare {
			get {
				return moveValue & startSquareMask;
			}
		}

		public int TargetSquare {
			get {
				return (moveValue & targetSquareMask) >> 6;
			}
		}

		public bool IsPromotion {
			get {
				int flag = MoveFlag;
				return flag == Flag.ForfremTilDronning || flag == Flag.ForfremTilTårn || flag == Flag.ForfremTilRytter || flag == Flag.ForfremTilBiskop;
			}
		}

		public int MoveFlag {
			get {
				return moveValue >> 12;
			}
		}

		public int PromotionPieceType {
			get {
				switch (MoveFlag) {
					case Flag.ForfremTilTårn:
						return Piece.Tårn;
					case Flag.ForfremTilRytter:
						return Piece.Rytter;
					case Flag.ForfremTilBiskop:
						return Piece.Biskop;
					case Flag.ForfremTilDronning:
						return Piece.Dronning;
					default:
						return Piece.Ingen;
				}
			}
		}

		public static Move InvalidMove {
			get {
				return new Move (0);
			}
		}

		public static bool SameMove (Move a, Move b) {
			return a.moveValue == b.moveValue;
		}

		public ushort Value {
			get {
				return moveValue;
			}
		}

		public bool IsInvalid {
			get {
				return moveValue == 0;
			}
		}

		public string Name {
			get {
				return BoardRepresentation.SquareNameFromIndex (StartSquare) + "-" + BoardRepresentation.SquareNameFromIndex (TargetSquare);
			}
		}
	}
}
