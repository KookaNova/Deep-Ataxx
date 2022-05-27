using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHistoryComponent : MonoBehaviour
{
    public List<BoardState> history;
	public int undoIndex = 0;
	
	public void AddNewBoard(BoardState newBoard){
		//Clear redo posibilities
		for(int i = 0; i < undoIndex; i++){
			history.RemoveAt(i);
		}
		
		
		history.Insert(0, newBoard); //Add new board state at 0
		undoIndex = 0; //Set index back to 0
	}
	
	public void Undo(){
		undoIndex++;
		
		//Code that loads the board into play.
		//Likely destroys pieces of the old board and 
		//generates new ones foreach item in the BoardState
		//We can also move existing pieces to avoid overusing "Instantiate" and "Destroy"
		//This may need to communicate or be done through the GameManager class
	}
}
