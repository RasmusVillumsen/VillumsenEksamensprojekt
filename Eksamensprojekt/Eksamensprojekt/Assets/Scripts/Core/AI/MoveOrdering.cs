using System.Collections;
using System.Collections.Generic;

namespace Chess {
	public class MoveOrdering {

		int[] moveScores;
		const int maxMoveCount = 218;

		const int squareControlledByOpponentPawnPenalty = 350;
		const int capturedPieceValueMultiplier = 10;

		MoveGenerator moveGenerator;
		TranspositionTable transpositionTable;
		Move invalidMove;

		public MoveOrdering (MoveGenerator moveGenerator, TranspositionTable tt) {
			moveScores = new int[maxMoveCount];
			this.moveGenerator = moveGenerator;
			this.transpositionTable = tt;
			invalidMove = Move.InvalidMove;
		}

		public void OrderMoves (Board board, List<Move> moves, bool useTT) {
			Move hashMove = invalidMove;
			if (useTT) {
				hashMove = transpositionTable.GetStoredMove ();
			}

			for (int i = 0; i < moves.Count; i++) {
				int score = 0;
				int movePieceType = Piece.BrikType (board.Square[moves[i].StartSquare]);
				int capturePieceType = Piece.BrikType (board.Square[moves[i].TargetSquare]);
				int flag = moves[i].MoveFlag;

				if (capturePieceType != Piece.Ingen) {
					// Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
					// The capturedPieceValueMultiplier is used to make even 'bad' captures like QxP rank above non-captures
					score = capturedPieceValueMultiplier * GetPieceValue (capturePieceType) - GetPieceValue (movePieceType);
				}

				if (movePieceType == Piece.pawn) {

					if (flag == Move.Flag.ForfremTilqueen) {
						score += Evaluation.queenValue;
					} else if (flag == Move.Flag.ForfremTilknight) {
						score += Evaluation.knightValue;
					} else if (flag == Move.Flag.ForfremTilrook) {
						score += Evaluation.rookValue;
					} else if (flag == Move.Flag.ForfremTilbishop) {
						score += Evaluation.bishopValue;
					}
				} else {
					// Penalize moving piece to a square attacked by opponent pawn
					if (BitBoardUtility.ContainsSquare (moveGenerator.opponentPawnAttackMap, moves[i].TargetSquare)) {
						score -= squareControlledByOpponentPawnPenalty;
					}
				}
				if (Move.SameMove (moves[i], hashMove)) {
					score += 10000;
				}

				moveScores[i] = score;
			}

			Sort (moves);
		}

		static int GetPieceValue (int pieceType) {
			switch (pieceType) {
				case Piece.queen:
					return Evaluation.queenValue;
				case Piece.rook:
					return Evaluation.rookValue;
				case Piece.knight:
					return Evaluation.knightValue;
				case Piece.bishop:
					return Evaluation.bishopValue;
				case Piece.pawn:
					return Evaluation.pawnValue;
				default:
					return 0;
			}
		}

		void Sort (List<Move> moves) {
			// Sort the moves list based on scores
			for (int i = 0; i < moves.Count - 1; i++) {
				for (int j = i + 1; j > 0; j--) {
					int swapIndex = j - 1;
					if (moveScores[swapIndex] < moveScores[j]) {
						(moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
						(moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
					}
				}
			}
		}
	}
}