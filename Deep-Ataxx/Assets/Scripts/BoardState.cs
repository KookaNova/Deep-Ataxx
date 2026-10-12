using System;
using System.Collections.Generic;
using UnityEngine;

//Cell and Tile are used interchangeably. Cell is a newer choice
public enum BoardCellKind : byte
{
	Empty,
	Blocked,
	Piece
}

public readonly struct BoardCell
{
	readonly int playerId;

	public BoardCellKind Kind { get; }
	public int PlayerId => Kind == BoardCellKind.Piece ? playerId : -1;

	BoardCell(BoardCellKind kind, int playerId)
	{
		Kind = kind;
		this.playerId = playerId;
	}

	public static BoardCell Empty => new BoardCell(BoardCellKind.Empty, -1);
	public static BoardCell Blocked => new BoardCell(BoardCellKind.Blocked, -1);

	public static BoardCell ForPlayer(int playerId)
	{
		if (playerId < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(playerId));
		}

		return new BoardCell(BoardCellKind.Piece, playerId);
	}
}

public sealed class BoardState
{
	readonly BoardCell[] cells;

	public int Width { get; }
	public int Height { get; }
	public int CurrentPlayerId { get; }

	public BoardState(int width, int height, BoardCell[] cells, int currentPlayerId)
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width));
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(height));
		}
		if (cells == null)
		{
			throw new ArgumentNullException(nameof(cells));
		}
		if (cells.Length != (long)width * height)
		{
			throw new ArgumentException("Cell count must match the board dimensions.", nameof(cells));
		}
		if (currentPlayerId < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(currentPlayerId));
		}

		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i].Kind < BoardCellKind.Empty || cells[i].Kind > BoardCellKind.Piece ||
				(cells[i].Kind == BoardCellKind.Piece && cells[i].PlayerId < 0))
			{
				throw new ArgumentException("The grid contains an invalid cell value.", nameof(cells));
			}
		}

		Width = width;
		Height = height;
		CurrentPlayerId = currentPlayerId;
		this.cells = (BoardCell[])cells.Clone();
	}

	public BoardCell GetCell(int x, int y)
	{
		if (x < 0 || x >= Width)
		{
			throw new ArgumentOutOfRangeException(nameof(x));
		}
		if (y < 0 || y >= Height)
		{
			throw new ArgumentOutOfRangeException(nameof(y));
		}

		return cells[y * Width + x];
	}

	public BoardCell[] CopyCells()
	{
		return (BoardCell[])cells.Clone();
	}

	public BoardState Copy()
	{
		return new BoardState(Width, Height, cells, CurrentPlayerId);
	}
}

public class Tile
{
	public string name; //could be useful for prime moves and finding tile objects
	public Vector2Int position;
	public Vector2Int[] adjacentPositions = new Vector2Int[7];
	public Vector2Int[] reachablePositions = new Vector2Int[23];

	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //create an alphabet for the notation;

	public Tile(Vector2Int _position, Vector2Int[] allPlayableTiles)
	{
		position = _position;
		FindNeighbors(position, allPlayableTiles);
		name = alphabet[position.x] + position.y.ToString();
	}

	void FindNeighbors(Vector2Int _position, Vector2Int[] allPlayableTiles)
	{
		//Find adjacent positions clockwise from bottom left
		List<Vector2Int> adjacentList = new List<Vector2Int>();
		List<Vector2Int> reachableList = new List<Vector2Int>();

		int x = _position.x - 1;
		//finds adjacent tiles.
		foreach (var tile in allPlayableTiles)
		{
			if (position == tile) continue;
			if (Vector2Int.Distance(position, tile) < 2)
			{
				adjacentList.Add(tile);
			}
			if (Vector2Int.Distance(position, tile) < 3)
			{
				reachableList.Add(tile);
			}
		}
		adjacentPositions = adjacentList.ToArray();
		reachablePositions = reachableList.ToArray();
	}
}
