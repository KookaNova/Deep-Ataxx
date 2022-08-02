using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;

/// <summary>
/// Applications in the undo feature and the AI. Used to save a state the board is in and access it again.
/// </summary>
public struct BoardState
{
	public Vector2Int[] redPositions;
	public Vector2Int[] greenPositions;
	public string[] redTileNames {get;}
	public string[] greenTileNames {get;}
	
	//Simply stores the positions of the current board.
	public BoardState(Vector2Int[] _redPositions,Vector2Int[] _greenPositions){
		redPositions = _redPositions;
		greenPositions = _greenPositions;

		string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //create an alphabet for the notation;
		redTileNames = new string[redPositions.Length];
		greenTileNames = new string[greenPositions.Length];

		for(int i = 0; i < redTileNames.Length; i++){
			redTileNames[i] = alphabet[redPositions[i].x] + redPositions[i].y.ToString();
		}
		for(int i = 0; i < greenTileNames.Length; i++){
			greenTileNames[i] = alphabet[greenPositions[i].x] + greenPositions[i].y.ToString();
		}

	}
	//Creates a board state from given pieces
	public BoardState(List<PieceComponent> redPieces, List<PieceComponent> greenPieces){
		redPositions = new Vector2Int[redPieces.Count];
		greenPositions = new Vector2Int[greenPieces.Count];
		for(int i = 0; i < redPieces.Count; i++){
			var piece = redPieces[i];
			redPositions[i] = piece.homeTile.gridPosition;
		}
		for(int i = 0; i < greenPieces.Count; i++){
			var piece = greenPieces[i];
			greenPositions[i] = greenPieces[i].homeTile.gridPosition;
		}

		string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //create an alphabet for the notation;
		redTileNames = new string[redPositions.Length];
		greenTileNames = new string[greenPositions.Length];

		for(int i = 0; i < redTileNames.Length; i++){
			redTileNames[i] = alphabet[redPositions[i].x] + redPositions[i].y.ToString();
		}
		for(int i = 0; i < greenTileNames.Length; i++){
			greenTileNames[i] = alphabet[greenPositions[i].x] + greenPositions[i].y.ToString();
		}

	}
}
