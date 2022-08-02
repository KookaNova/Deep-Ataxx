using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;

/// <summary>
/// Applications in the undo feature and the AI. Used to save a state the board is in and access it again.
/// </summary>
public struct BoardState
{
	public Vector2Int[] p1_Positions;
	public Vector2Int[] p2_Positions;
	public string[] p1_TileNames {get;}
	public string[] p2_TileNames {get;}
	
	//Simply stores the positions of the current board.
	public BoardState(Vector2Int[] _p1_positions,Vector2Int[] _p2_positions){
		p1_Positions = _p1_positions;
		p2_Positions = _p2_positions;

		string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //create an alphabet for the notation;
		p1_TileNames = new string[p1_Positions.Length];
		p2_TileNames = new string[p2_Positions.Length];

		for(int i = 0; i < p1_TileNames.Length; i++){
			p1_TileNames[i] = alphabet[p1_Positions[i].x] + p1_Positions[i].y.ToString();
		}
		for(int i = 0; i < p2_TileNames.Length; i++){
			p2_TileNames[i] = alphabet[p2_Positions[i].x] + p2_Positions[i].y.ToString();
		}

	}
	//Creates a board state from given pieces
	public BoardState(List<PieceComponent> p1_pieces, List<PieceComponent> p2_pieces){
		p1_Positions = new Vector2Int[p1_pieces.Count];
		p2_Positions = new Vector2Int[p2_pieces.Count];
		for(int i = 0; i < p1_pieces.Count; i++){
			var piece = p1_pieces[i];
			p1_Positions[i] = piece.homeTile.gridPosition;
		}
		for(int i = 0; i < p2_pieces.Count; i++){
			var piece = p2_pieces[i];
			p2_Positions[i] = p2_pieces[i].homeTile.gridPosition;
		}

		string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //create an alphabet for the notation;
		p1_TileNames = new string[p1_Positions.Length];
		p2_TileNames = new string[p2_Positions.Length];

		for(int i = 0; i < p1_TileNames.Length; i++){
			p1_TileNames[i] = alphabet[p1_Positions[i].x] + p1_Positions[i].y.ToString();
		}
		for(int i = 0; i < p2_TileNames.Length; i++){
			p2_TileNames[i] = alphabet[p2_Positions[i].x] + p2_Positions[i].y.ToString();
		}

	}
}
