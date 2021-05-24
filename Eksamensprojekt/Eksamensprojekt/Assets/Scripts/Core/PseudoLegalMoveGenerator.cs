namespace Chess {
	using System.Collections.Generic;
	using static PrecomputedMoveData;
	using static BoardRepresentation;
	
	public class PseudoLegalMoveGenerator {

		// ---- Instance variables ----
		List<Move> moves;
		bool isWhiteToMove;
		int friendlyColour;
		int opponentColour;
		int friendlyKingSquare;
		int friendlyColourIndex;
		int opponentColourIndex;

		bool genQuiets;
		bool genUnderpromotions;
		Board board;

		// Generates list of legal moves in current position.
		// Quiet moves (non captures) can optionally be excluded. This is used in quiescence search.
		public List<Move> GenerateMoves (Board board, bool includeQuietMoves = true, bool includeUnderPromotions = true) {
			this.board = board;
			genQuiets = includeQuietMoves;
			genUnderpromotions = includeUnderPromotions;
			Init ();
			GenerateKingMoves ();

			GenerateSlidingMoves ();
			GenerateKnightMoves ();
			GeneratePawnMoves ();

			return moves;
		}

		public bool Illegal () {
			return SquareAttacked (board.KingSquare[1 - board.ColourToMoveIndex], board.ColourToMove);
		}

		public bool SquareAttacked (int attackSquare, int attackerColour) {

			int attackerColourIndex = (attackerColour == Piece.Hvid) ? Board.WhiteIndex : Board.BlackIndex;
			int friendlyColourIndex = 1 - attackerColourIndex;
			int friendlyColour = (attackerColour == Piece.Hvid) ? Piece.Sort : Piece.Hvid;

			int startDirIndex = 0;
			int endDirIndex = 8;

			int opponentKingSquare = board.KingSquare[attackerColourIndex];
			if (kingDistance[opponentKingSquare, attackSquare] == 1) {
				return true;
			}

			if (board.queens[attackerColourIndex].Count == 0) {
				startDirIndex = (board.rooks[attackerColourIndex].Count > 0) ? 0 : 4;
				endDirIndex = (board.bishops[attackerColourIndex].Count > 0) ? 8 : 4;
			}

			for (int dir = startDirIndex; dir < endDirIndex; dir++) {
				bool isDiagonal = dir > 3;

				int n = numSquaresToEdge[attackSquare][dir];
				int directionOffset = directionOffsets[dir];

				for (int i = 0; i < n; i++) {
					int squareIndex = attackSquare + directionOffset * (i + 1);
					int piece = board.Square[squareIndex];

					// This square contains a piece
					if (piece != Piece.Ingen) {
						if (Piece.ErFarve (piece, friendlyColour)) {
							break;
						}
						// This square contains an enemy piece
						else {
							int pieceType = Piece.BrikType (piece);

							// Check if piece is in bitmask of pieces able to move in current direction
							if (isDiagonal && Piece.ErbishopEllerqueen (pieceType) || !isDiagonal && Piece.Errooksllerqueen (pieceType)) {
								return true;
							} else {
								// This enemy piece is not able to move in the current direction, and so is blocking any checks/pins
								break;
							}
						}
					}
				}
			}

			// Knight attacks
			var knightAttackSquares = knightMoves[attackSquare];
			for (int i = 0; i < knightAttackSquares.Length; i++) {
				if (board.Square[knightAttackSquares[i]] == (Piece.knight | attackerColour)) {
					return true;
				}
			}

			// check if enemy pawn is controlling this square
			for (int i = 0; i < 2; i++) {
				// Check if square exists diagonal to friendly king from which enemy pawn could be attacking it
				if (numSquaresToEdge[attackSquare][pawnAttackDirections[friendlyColourIndex][i]] > 0) {
					// move in direction friendly pawns attack to get square from which enemy pawn would attack
					int s = attackSquare + directionOffsets[pawnAttackDirections[friendlyColourIndex][i]];

					int piece = board.Square[s];
					if (piece == (Piece.pawn | attackerColour)) // is enemy pawn
					{
						return true;
					}
				}
			}

			return false;
		}

		// Note, this will only return correct value after GenerateMoves() has been called in the current position
		public bool InCheck () {
			return false;
			//return SquareAttacked (friendlyKingSquare, board.ColourToMoveIndex);
		}

		void Init () {
			moves = new List<Move> (64);

			isWhiteToMove = board.ColourToMove == Piece.Hvid;
			friendlyColour = board.ColourToMove;
			opponentColour = board.OpponentColour;
			friendlyKingSquare = board.KingSquare[board.ColourToMoveIndex];
			friendlyColourIndex = (board.WhiteToMove) ? Board.WhiteIndex : Board.BlackIndex;
			opponentColourIndex = 1 - friendlyColourIndex;
		}

		void GenerateKingMoves () {
			for (int i = 0; i < kingMoves[friendlyKingSquare].Length; i++) {
				int targetSquare = kingMoves[friendlyKingSquare][i];
				int pieceOnTargetSquare = board.Square[targetSquare];

				// Skip squares occupied by friendly pieces
				if (Piece.ErFarve (pieceOnTargetSquare, friendlyColour)) {
					continue;
				}

				bool isCapture = Piece.ErFarve (pieceOnTargetSquare, opponentColour);
				if (!isCapture) {
					// King can't move to square marked as under enemy control, unless he is capturing that piece
					// Also skip if not generating quiet moves
					if (!genQuiets) {
						continue;
					}
				}

				// Safe for king to move to this square

				moves.Add (new Move (friendlyKingSquare, targetSquare));

				// Castling:
				if (!isCapture && !SquareAttacked (friendlyKingSquare, opponentColour)) {
					// Castle kingside
					if ((targetSquare == f1 || targetSquare == f8) && HasKingsideCastleRight) {
						if (!SquareAttacked (targetSquare, opponentColour)) {
							int castleKingsideSquare = targetSquare + 1;
							if (board.Square[castleKingsideSquare] == Piece.Ingen) {
								moves.Add (new Move (friendlyKingSquare, castleKingsideSquare, Move.Flag.Rokade));

							}
						}
					}
					// Castle queenside
					else if ((targetSquare == d1 || targetSquare == d8) && HasqueensideCastleRight) {
						if (!SquareAttacked (targetSquare, opponentColour)) {
							int castlequeensideSquare = targetSquare - 1;
							if (board.Square[castlequeensideSquare] == Piece.Ingen && board.Square[castlequeensideSquare - 1] == Piece.Ingen) {
								moves.Add (new Move (friendlyKingSquare, castlequeensideSquare, Move.Flag.Rokade));
							}
						}
					}
				}

			}
		}

		void GenerateSlidingMoves () {
			PieceList rooks = board.rooks[friendlyColourIndex];
			for (int i = 0; i < rooks.Count; i++) {
				GenerateSlidingPieceMoves (rooks[i], 0, 4);
			}

			PieceList bishops = board.bishops[friendlyColourIndex];
			for (int i = 0; i < bishops.Count; i++) {
				GenerateSlidingPieceMoves (bishops[i], 4, 8);
			}

			PieceList queens = board.queens[friendlyColourIndex];
			for (int i = 0; i < queens.Count; i++) {
				GenerateSlidingPieceMoves (queens[i], 0, 8);
			}

		}

		void GenerateSlidingPieceMoves (int startSquare, int startDirIndex, int endDirIndex) {

			for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++) {
				int currentDirOffset = directionOffsets[directionIndex];

				for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];

					// Blocked by friendly piece, so stop looking in this direction
					if (Piece.ErFarve (targetSquarePiece, friendlyColour)) {
						break;
					}
					bool isCapture = targetSquarePiece != Piece.Ingen;

				if (genQuiets || isCapture) {
						moves.Add (new Move (startSquare, targetSquare));
					}

					// If square not empty, can't move any further in this direction
					// Also, if this move blocked a check, further moves won't block the check
					if (isCapture) {
						break;
					}
				}
			}
		}

		void GenerateKnightMoves () {
			PieceList myknights = board.knights[friendlyColourIndex];

			for (int i = 0; i < myknights.Count; i++) {
				int startSquare = myknights[i];

				for (int knightMoveIndex = 0; knightMoveIndex < knightMoves[startSquare].Length; knightMoveIndex++) {
					int targetSquare = knightMoves[startSquare][knightMoveIndex];
					int targetSquarePiece = board.Square[targetSquare];
					bool isCapture = Piece.ErFarve (targetSquarePiece, opponentColour);
					if (genQuiets || isCapture) {
						// Skip if square contains friendly piece, or if in check and knight is not interposing/capturing checking piece
						if (Piece.ErFarve (targetSquarePiece, friendlyColour)) {
							continue;
						}
						moves.Add (new Move (startSquare, targetSquare));
					}
				}
			}
		}

		void GeneratePawnMoves () {
			PieceList myPawns = board.pawns[friendlyColourIndex];
			int pawnOffset = (friendlyColour == Piece.Hvid) ? 8 : -8;
			int startRank = (board.WhiteToMove) ? 1 : 6;
			int finalRankBeforePromotion = (board.WhiteToMove) ? 6 : 1;

			int enPassantFile = ((int) (board.currentGameState >> 4) & 15) - 1;
			int enPassantSquare = -1;
			if (enPassantFile != -1) {
				enPassantSquare = 8 * ((board.WhiteToMove) ? 5 : 2) + enPassantFile;
			}

			for (int i = 0; i < myPawns.Count; i++) {
				int startSquare = myPawns[i];
				int rank = RankIndex (startSquare);
				bool oneStepFromPromotion = rank == finalRankBeforePromotion;

				if (genQuiets) {

					int squareOneForward = startSquare + pawnOffset;

					// Square ahead of pawn is empty: forward moves
					if (board.Square[squareOneForward] == Piece.Ingen) {
						// Pawn not pinned, or is moving along line of pin

						if (oneStepFromPromotion) {
							MakePromotionMoves (startSquare, squareOneForward);
						} else {
							moves.Add (new Move (startSquare, squareOneForward));
						}

						// Is on starting square (so can move two forward if not blocked)
						if (rank == startRank) {
							int squareTwoForward = squareOneForward + pawnOffset;
							if (board.Square[squareTwoForward] == Piece.Ingen) {
								// Not in check, or pawn is interposing checking piece

								moves.Add (new Move (startSquare, squareTwoForward, Move.Flag.pawnToFremad));

							}
						}

					}
				}

				// Pawn captures.
				for (int j = 0; j < 2; j++) {
					// Check if square exists diagonal to pawn
					if (numSquaresToEdge[startSquare][pawnAttackDirections[friendlyColourIndex][j]] > 0) {
						// move in direction friendly pawns attack to get square from which enemy pawn would attack
						int pawnCaptureDir = directionOffsets[pawnAttackDirections[friendlyColourIndex][j]];
						int targetSquare = startSquare + pawnCaptureDir;
						int targetPiece = board.Square[targetSquare];

						// Regular capture
						if (Piece.ErFarve (targetPiece, opponentColour)) {

							if (oneStepFromPromotion) {
								MakePromotionMoves (startSquare, targetSquare);
							} else {
								moves.Add (new Move (startSquare, targetSquare));
							}
						}

						// Capture en-passant
						if (targetSquare == enPassantSquare) {
							int epCapturedPawnSquare = targetSquare + ((board.WhiteToMove) ? -8 : 8);

							moves.Add (new Move (startSquare, targetSquare, Move.Flag.EnPassantErobring));

						}
					}
				}
			}
		}

		void MakePromotionMoves (int fromSquare, int toSquare) {
			moves.Add (new Move (fromSquare, toSquare, Move.Flag.ForfremTilqueen));
			if (genUnderpromotions) {
				moves.Add (new Move (fromSquare, toSquare, Move.Flag.ForfremTilknight));
				moves.Add (new Move (fromSquare, toSquare, Move.Flag.ForfremTilrook));
				moves.Add (new Move (fromSquare, toSquare, Move.Flag.ForfremTilbishop));
			}
		}

		bool HasKingsideCastleRight {
			get {
				int mask = (board.WhiteToMove) ? 1 : 4;
				return (board.currentGameState & mask) != 0;
			}
		}

		bool HasqueensideCastleRight {
			get {
				int mask = (board.WhiteToMove) ? 2 : 8;
				return (board.currentGameState & mask) != 0;
			}
		}

	}

}