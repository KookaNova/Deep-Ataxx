using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoardState
{
	public Vector2Int[] redPositions;
	public Vector2Int[] greenPositions;
	
	//Simply stores the positions of the current board.
	public BoardState(Vector2Int[] _redPositions,Vector2Int[] _greenPositions){
		redPositions = _redPositions;
		greenPositions = _greenPositions;
	}
}
