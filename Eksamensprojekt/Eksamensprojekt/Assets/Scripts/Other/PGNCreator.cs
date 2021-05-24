﻿namespace Chess {
	public static class PGNCreator {

		public static string CreatePGN (Move[] moves) {
			string pgn = "";
			Board board = new Board ();
			board.LoadStartPosition ();

			for (int plyCount = 0; plyCount < moves.Length; plyCount++) {
				string moveString = NotationFromMove (board, moves[plyCount]);
				board.MakeMove (moves[plyCount]);

				if (plyCount % 2 == 0) {
					pgn += ((plyCount / 2) + 1) + ". ";
				}
				pgn += moveString + " ";
			}

			return pgn;
		}

		public static string NotationFromMove (string currentFen, Move move) {
			Board board = new Board ();
			board.LoadPosition (currentFen);
			return NotationFromMove (board, move);
		}

		static string NotationFromMove (Board board, Move move) {

			MoveGenerator moveGen = new MoveGenerator ();

			int movePieceType = Piece.BrikType (board.Square[move.StartSquare]);
			int capturedPieceType = Piece.BrikType (board.Square[move.TargetSquare]);

			if (move.MoveFlag == Move.Flag.Rokade) {
				int delta = move.TargetSquare - move.StartSquare;
				if (delta == 2) {
					return "O-O";
				} else if (delta == -2) {
					return "O-O-O";
				}
			}

			string moveNotation = GetSymbolFromPieceType (movePieceType);

			// check if any ambiguity exists in notation (e.g if e2 can be reached via Nfe2 and Nbe2)
			if (movePieceType != Piece.pawn && movePieceType != Piece.king) {
				var allMoves = moveGen.GenerateMoves (board);

				foreach (Move altMove in allMoves) {

					if (altMove.StartSquare != move.StartSquare && altMove.TargetSquare == move.TargetSquare) { // if moving to same square from different square
						if (Piece.BrikType (board.Square[altMove.StartSquare]) == movePieceType) { // same piece type
							int fromFileIndex = BoardRepresentation.FileIndex (move.StartSquare);
							int alternateFromFileIndex = BoardRepresentation.FileIndex (altMove.StartSquare);
							int fromRankIndex = BoardRepresentation.RankIndex (move.StartSquare);
							int alternateFromRankIndex = BoardRepresentation.RankIndex (altMove.StartSquare);

							if (fromFileIndex != alternateFromFileIndex) { // pieces on different files, thus ambiguity can be resolved by specifying file
								moveNotation += BoardRepresentation.fileNames[fromFileIndex];
								break; // ambiguity resolved
							} else if (fromRankIndex != alternateFromRankIndex) {
								moveNotation += BoardRepresentation.rankNames[fromRankIndex];
								break; // ambiguity resolved
							}
						}
					}

				}
			}

			if (capturedPieceType != 0) { // add 'x' to indicate capture
				if (movePieceType == Piece.pawn) {
					moveNotation += BoardRepresentation.fileNames[BoardRepresentation.FileIndex (move.StartSquare)];
				}
				moveNotation += "x";
			} else { // check if capturing ep
				if (move.MoveFlag == Move.Flag.EnPassantErobring) {
					moveNotation += BoardRepresentation.fileNames[BoardRepresentation.FileIndex (move.StartSquare)] + "x";
				}
			}

			moveNotation += BoardRepresentation.fileNames[BoardRepresentation.FileIndex (move.TargetSquare)];
			moveNotation += BoardRepresentation.rankNames[BoardRepresentation.RankIndex (move.TargetSquare)];

			// add promotion piece
			if (move.IsPromotion) {
				int promotionPieceType = move.PromotionPieceType;
				moveNotation += "=" + GetSymbolFromPieceType (promotionPieceType);
			}

			board.MakeMove (move, inSearch : true);
			var legalResponses = moveGen.GenerateMoves (board);
			// add check/mate symbol if applicable
			if (moveGen.InCheck ()) {
				if (legalResponses.Count == 0) {
					moveNotation += "#";
				} else {
					moveNotation += "+";
				}
			}
			board.UnmakeMove (move, inSearch : true);

			return moveNotation;
		}

		static string GetSymbolFromPieceType (int pieceType) {
			switch (pieceType) {
				case Piece.pawn:
					return "R";
				case Piece.knight:
					return "N";
				case Piece.bishop:
					return "B";
				case Piece.queen:
					return "D";
				case Piece.king:
					return "K";
				default:
					return "";
			}
		}

	}
}