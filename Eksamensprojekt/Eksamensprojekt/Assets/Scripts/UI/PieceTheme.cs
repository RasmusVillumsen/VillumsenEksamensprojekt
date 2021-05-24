using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu (menuName = "Theme/Pieces")]
	public class PieceTheme : ScriptableObject {

		public PieceSprites whitePieces;
		public PieceSprites blackPieces;

		public Sprite GetPieceSprite (int piece) {
			PieceSprites pieceSprites = Piece.ErFarve (piece, Piece.Hvid) ? whitePieces : blackPieces;

			switch (Piece.BrikType (piece)) {
				case Piece.pawn:
					return pieceSprites.pawn;
				case Piece.rook:
					return pieceSprites.rook;
				case Piece.knight:
					return pieceSprites.knight;
				case Piece.bishop:
					return pieceSprites.bishop;
				case Piece.queen:
					return pieceSprites.queen;
				case Piece.king:
					return pieceSprites.king;
				default:
					if (piece != 0) {
						Debug.Log (piece);
					}
					return null;
			}
		}

		[System.Serializable]
		public class PieceSprites {
			public Sprite pawn, rook, knight, bishop, queen, king;

			public Sprite this [int i] {
				get {
					return new Sprite[] { pawn, rook, knight, bishop, queen, king }[i];
				}
			}
		}
	}
}