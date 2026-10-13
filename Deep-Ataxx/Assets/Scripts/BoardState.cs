using System;
using System.Collections.Generic;
using UnityEngine;

//Cell and Tile are used interchangeably. Cell is a newer choice
public enum BoardCellContent : byte
{
	Empty,
	Blocked,
	Piece
}

public readonly struct BoardCell
{
	readonly int playerId;

	public BoardCellContent Content { get; }
	public int PlayerId => Content == BoardCellContent.Piece ? playerId : -1;

	/// <summary>Creates a cell with the specified content and owning player.</summary>
	BoardCell(BoardCellContent content, int playerId)
	{
		Content = content;
		this.playerId = playerId;
	}

	public static BoardCell Empty => new BoardCell(BoardCellContent.Empty, -1);
	public static BoardCell Blocked => new BoardCell(BoardCellContent.Blocked, -1);

	/// <summary>Creates a piece cell owned by the specified player.</summary>
	public static BoardCell ForPlayer(int playerId)
	{
		if (playerId < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(playerId));
		}

		return new BoardCell(BoardCellContent.Piece, playerId);
	}
}

public sealed class BoardState
{
	readonly BoardCell[] cells;

	public int Width { get; }
	public int Height { get; }
	public int CurrentPlayerId { get; }

	/// <summary>Creates a validated, immutable snapshot of a board.</summary>
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
			if (cells[i].Content < BoardCellContent.Empty || cells[i].Content > BoardCellContent.Piece ||
				(cells[i].Content == BoardCellContent.Piece && cells[i].PlayerId < 0))
			{
				throw new ArgumentException("The grid contains an invalid cell value.", nameof(cells));
			}
		}

		Width = width;
		Height = height;
		CurrentPlayerId = currentPlayerId;
		this.cells = (BoardCell[])cells.Clone();
	}

	/// <summary>Returns the cell at the specified board coordinates.</summary>
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

	/// <summary>Returns a copy of the board's cell array.</summary>
	public BoardCell[] CopyCells()
	{
		return (BoardCell[])cells.Clone();
	}

	/// <summary>Creates an independent copy of this board state.</summary>
	public BoardState Copy()
	{
		return new BoardState(Width, Height, cells, CurrentPlayerId);
	}
}

public enum BoardMoveType : byte
{
	Spread,
	Hop
}

public readonly struct BoardMove
{
	public int FromX { get; }
	public int FromY { get; }
	public int ToX { get; }
	public int ToY { get; }
	public BoardMoveType Type { get; }

	/// <summary>Creates a move from one board coordinate to another.</summary>
	public BoardMove(int fromX, int fromY, int toX, int toY, BoardMoveType type)
	{
		FromX = fromX;
		FromY = fromY;
		ToX = toX;
		ToY = toY;
		Type = type;
	}
}

public static class BoardRules
{
	public const int PlayerOneId = 0;
	public const int PlayerTwoId = 1;
	const int MaximumMoveDistanceSquared = 9;
	const int SpreadDistanceSquared = 4;
	const int InfectionDistanceSquared = 4;

	/// <summary>Returns legal moves for the player whose turn it is in the state.</summary>
	public static List<BoardMove> GetLegalMoves(BoardState state)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}

		return GenerateMoves(state, state.CurrentPlayerId);
	}

	/// <summary>Returns legal moves for the specified player.</summary>
	public static List<BoardMove> GetLegalMoves(BoardState state, int playerId)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}

		return GenerateMoves(state, playerId);
	}

	/// <summary>Applies a legal move and returns the resulting board state.</summary>
	public static BoardState ApplyMove(BoardState state, BoardMove move)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}

		ValidatePlayerId(state.CurrentPlayerId);
		ValidateMove(state, move);

		var cells = state.CopyCells();
		int sourceIndex = move.FromY * state.Width + move.FromX;
		int destinationIndex = move.ToY * state.Width + move.ToX;

		if (move.Type == BoardMoveType.Hop)
		{
			cells[sourceIndex] = BoardCell.Empty;
		}
		cells[destinationIndex] = BoardCell.ForPlayer(state.CurrentPlayerId);

		for (int offsetY = -1; offsetY <= 1; offsetY++)
		{
			for (int offsetX = -1; offsetX <= 1; offsetX++)
			{
				if (offsetX == 0 && offsetY == 0)
				{
					continue;
				}

				int x = move.ToX + offsetX;
				int y = move.ToY + offsetY;
				if (!IsInBounds(state, x, y) || offsetX * offsetX + offsetY * offsetY >= InfectionDistanceSquared)
				{
					continue;
				}

				int index = y * state.Width + x;
				if (cells[index].Content == BoardCellContent.Piece)
				{
					cells[index] = BoardCell.ForPlayer(state.CurrentPlayerId);
				}
			}
		}

		int nextPlayerId = GetOtherPlayerId(state.CurrentPlayerId);
		var nextState = new BoardState(state.Width, state.Height, cells, nextPlayerId);
		if (GetLegalMoves(nextState).Count > 0)
		{
			return nextState;
		}

		if (GenerateMoves(nextState, state.CurrentPlayerId).Count > 0)
		{
			return new BoardState(state.Width, state.Height, cells, state.CurrentPlayerId);
		}

		return nextState;
	}

	/// <summary>Advances the turn when the current player has no legal moves.</summary>
	public static BoardState PassTurn(BoardState state)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}

		ValidatePlayerId(state.CurrentPlayerId);
		if (GetLegalMoves(state).Count > 0)
		{
			throw new InvalidOperationException("A player with legal moves cannot pass.");
		}

		return new BoardState(state.Width, state.Height, state.CopyCells(), GetOtherPlayerId(state.CurrentPlayerId));
	}

	/// <summary>Returns whether the state meets a game-ending condition.</summary>
	public static bool isGameOver(BoardState state)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}

		ValidatePlayerId(state.CurrentPlayerId);
		int playerOnePieces = 0;
		int playerTwoPieces = 0;
		int emptyCells = 0;

		for (int y = 0; y < state.Height; y++)
		{
			for (int x = 0; x < state.Width; x++)
			{
				var cell = state.GetCell(x, y);
				if (cell.Content == BoardCellContent.Empty)
				{
					emptyCells++;
				}
				else if (cell.Content == BoardCellContent.Piece && cell.PlayerId == PlayerOneId)
				{
					playerOnePieces++;
				}
				else if (cell.Content == BoardCellContent.Piece && cell.PlayerId == PlayerTwoId)
				{
					playerTwoPieces++;
				}
			}
		}

		return playerOnePieces == 0 || playerTwoPieces == 0 || emptyCells == 0 ||
			(GetLegalMoves(state, PlayerOneId).Count == 0 && GetLegalMoves(state, PlayerTwoId).Count == 0);
	}

	/// <summary>Returns the other supported player's ID.</summary>
	public static int GetOtherPlayerId(int playerId)
	{
		ValidatePlayerId(playerId);
		return playerId == PlayerOneId ? PlayerTwoId : PlayerOneId;
	}

	/// <summary>Generates all moves available to the specified player.</summary>
	private static List<BoardMove> GenerateMoves(BoardState state, int playerId)
	{
		ValidatePlayerId(playerId);
		var moves = new List<BoardMove>();

		for (int fromY = 0; fromY < state.Height; fromY++)
		{
			for (int fromX = 0; fromX < state.Width; fromX++)
			{
				var source = state.GetCell(fromX, fromY);
				if (source.Content != BoardCellContent.Piece || source.PlayerId != playerId)
				{
					continue;
				}

				for (int offsetY = -2; offsetY <= 2; offsetY++)
				{
					for (int offsetX = -2; offsetX <= 2; offsetX++)
					{
						int distanceSquared = offsetX * offsetX + offsetY * offsetY;
						if (distanceSquared == 0 || distanceSquared >= MaximumMoveDistanceSquared)
						{
							continue;
						}

						int toX = fromX + offsetX;
						int toY = fromY + offsetY;
						if (!IsInBounds(state, toX, toY) || state.GetCell(toX, toY).Content != BoardCellContent.Empty)
						{
							continue;
						}

						var moveType = distanceSquared >= SpreadDistanceSquared ? BoardMoveType.Hop : BoardMoveType.Spread;
						moves.Add(new BoardMove(fromX, fromY, toX, toY, moveType));
					}
				}
			}
		}

		return moves;
	}

	/// <summary>Throws when a move is outside the board or violates the movement rules.</summary>
	private static void ValidateMove(BoardState state, BoardMove move)
	{
		if (!IsInBounds(state, move.FromX, move.FromY) || !IsInBounds(state, move.ToX, move.ToY))
		{
			throw new ArgumentOutOfRangeException(nameof(move), "Move coordinates must be inside the board.");
		}

		var source = state.GetCell(move.FromX, move.FromY);
		if (source.Content != BoardCellContent.Piece || source.PlayerId != state.CurrentPlayerId)
		{
			throw new ArgumentException("The source cell does not contain the current player's piece.", nameof(move));
		}
		if (state.GetCell(move.ToX, move.ToY).Content != BoardCellContent.Empty)
		{
			throw new ArgumentException("The destination cell must be empty.", nameof(move));
		}

		int offsetX = move.ToX - move.FromX;
		int offsetY = move.ToY - move.FromY;
		int distanceSquared = offsetX * offsetX + offsetY * offsetY;
		if (distanceSquared == 0 || distanceSquared >= MaximumMoveDistanceSquared)
		{
			throw new ArgumentException("The destination is outside the piece's movement range.", nameof(move));
		}

		BoardMoveType expectedType = distanceSquared >= SpreadDistanceSquared ? BoardMoveType.Hop : BoardMoveType.Spread;
		if (move.Type != expectedType)
		{
			throw new ArgumentException("Move type does not match the source and destination distance.", nameof(move));
		}
	}

	/// <summary>Returns whether the coordinates are inside the board.</summary>
	private static bool IsInBounds(BoardState state, int x, int y)
	{
		return x >= 0 && x < state.Width && y >= 0 && y < state.Height;
	}

	/// <summary>Throws when the player ID is not one of the supported IDs.</summary>
	private static void ValidatePlayerId(int playerId)
	{
		if (playerId != PlayerOneId && playerId != PlayerTwoId)
		{
			throw new ArgumentOutOfRangeException(nameof(playerId), "Current gameplay supports player IDs 0 and 1.");
		}
	}
}