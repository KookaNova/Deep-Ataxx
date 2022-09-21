using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cox.Infection.Management;

/// <summary>
/// Applications in the undo feature and the AI. Used to save a state the board is in and access it again.
/// </summary>
public class BoardState
{
	public Vector2Int[] p1_Positions {get;}
	public Vector2Int[] p2_Positions {get;}
	public Vector2Int[] invalidTiles {get;}
	public Vector2Int[] allTiles {get;}
	public int emptyTiles {get;}

	public string[] p1_TileNames;
	public string[] p2_TileNames;
	
	//Simply stores the positions of the current board.
	public BoardState(Vector2Int[] _p1_positions,Vector2Int[] _p2_positions, Vector2Int[] blockedTiles, Vector2Int[] _allTiles){
		p1_Positions = _p1_positions;
		p2_Positions = _p2_positions;
		allTiles = _allTiles;

		//problem located somewhere in here potentially. 0,0 is invalid twice

		invalidTiles = new Vector2Int[p1_Positions.Length + p2_Positions.Length + blockedTiles.Length];
		p1_Positions.CopyTo(invalidTiles, 0);
		p2_Positions.CopyTo(invalidTiles, p1_Positions.Length);
		blockedTiles.CopyTo(invalidTiles, p1_Positions.Length + p2_Positions.Length); 

		emptyTiles = allTiles.Length - invalidTiles.Length;

		AssignNames();
	}

	//Creates a board state from given pieces
	public BoardState(List<PieceComponent> p1_pieces, List<PieceComponent> p2_pieces, Vector2Int[] blockedTiles, Vector2Int[] _allTiles){
		p1_Positions = new Vector2Int[p1_pieces.Count];
		p2_Positions = new Vector2Int[p2_pieces.Count];
		//convert to positions
		for(int i = 0; i < p1_pieces.Count; i++){
			var piece = p1_pieces[i];
			p1_Positions[i] = piece.homeTile.gridPosition;
		}
		for(int i = 0; i < p2_pieces.Count; i++){
			var piece = p2_pieces[i];
			p2_Positions[i] = p2_pieces[i].homeTile.gridPosition;
		}
		allTiles = _allTiles;

		invalidTiles = new Vector2Int[p1_Positions.Length + p2_Positions.Length + blockedTiles.Length];
		p1_Positions.CopyTo(invalidTiles, 0);
		p2_Positions.CopyTo(invalidTiles, 0);
		blockedTiles.CopyTo(invalidTiles, 0); 

		emptyTiles = allTiles.Length - invalidTiles.Length;

		AssignNames();
	}

	void AssignNames(){
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

public class Tile{
        public string name; //could be useful for prime moves and finding tile objects
        public Vector2Int position;
        public Vector2Int[] adjacentPositions = new Vector2Int[7];
        public Vector2Int[] reachablePositions = new Vector2Int[23];
        

        public Tile(Vector2Int _position, Vector2Int[] allPlayableTiles){
            position = _position;
            FindNeighbors(position, allPlayableTiles);
			
			//assign name
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //create an alphabet for the notation;
            name = alphabet[position.x] + position.y.ToString();
        }

        void FindNeighbors(Vector2Int _position, Vector2Int[] allPlayableTiles){
            //Find adjacent positions clockwise from bottom left
            List<Vector2Int> adjacentList = new List<Vector2Int>();
            List<Vector2Int> reachableList = new List<Vector2Int>();

            int x = _position.x - 1;
            //finds adjacent tiles.
            foreach(var tile in allPlayableTiles){
                if(position == tile)continue;
                if(Vector2Int.Distance(position, tile) < 2){
                    adjacentList.Add(tile);
                }
                if(Vector2Int.Distance(position, tile) < 3){
                    reachableList.Add(tile);
                }
            }
            adjacentPositions = adjacentList.ToArray();
            reachablePositions = reachableList.ToArray();

            
        }
    }
