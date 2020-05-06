using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Select three available hexagon elements for the next action with player input.
/// </summary>
public struct SelectionHandler
{
    private List<SelectionPatterns> oddPatterns;
    private List<SelectionPatterns> evenPatterns;
    private int xTreeSize;
    private int yTreeSize;

    public SelectionHandler(int xTreeSize, int yTreeSize)
    {
        this.xTreeSize = xTreeSize;
        this.yTreeSize = yTreeSize;
        this.oddPatterns = new List<SelectionPatterns>();
        this.evenPatterns = new List<SelectionPatterns>();
        AddMoves();

    }

    /// <summary>
    /// Gets selection pattern with user input. Check every pattern offset to find suitable pattern at the specific location of the grid.
    /// </summary>
    /// <param name="clickedCoord">Clicked coord</param>
    /// <returns>Three coords of selection</returns>
    public List<Vector2> GetSelectionPattern(Vector2 clickedCoord)
    {
        int xCoord = (int)clickedCoord.x;
        int yCoord = (int)clickedCoord.y;
        List<Vector2> selectedCoords = new List<Vector2>(3);
        selectedCoords.Add(clickedCoord);
        List<int> orders = new List<int>();
        if (clickedCoord.x % 2 == 1)
        {
            CheckForOffsets(xCoord, yCoord, this.oddPatterns, selectedCoords, ref orders);
        }
        else
        {
            CheckForOffsets(xCoord, yCoord, this.evenPatterns, selectedCoords, ref orders);
        }
        List<Vector2> sortedMoves = SortSelectedCoords(selectedCoords, orders);
        return sortedMoves;
    }

    /// <summary>
    /// Sort selected coords to make it suitable for every rotation without considering the selection pattern
    /// </summary>
    /// <param name="selectedCoords"></param>
    /// <param name="orders"></param>
    /// <returns></returns>
    private List<Vector2> SortSelectedCoords(List<Vector2> selectedCoords, List<int> orders)
    {
        List<Vector2> sortedMoves = new List<Vector2>();
        for (int i = 0; i < selectedCoords.Count; i++)
        {
            sortedMoves.Add(selectedCoords[orders[i]]);
        }
        return sortedMoves;
    }

    /// <summary>
    /// Checks for every offset value, if it finds three suitable coords to select it breaks.
    /// </summary>
    /// <param name="xCoord">X base coord</param>
    /// <param name="yCoord">Y base coord</param>
    /// <param name="offsetList">Offset list to search</param>
    /// <param name="selectedCoords">Selected three coords</param>
    /// <param name="orders">Orders of the selected coords to sort it later</param>
    /// <param name="canMove"></param>
    private void CheckForOffsets(int xCoord, int yCoord, List<SelectionPatterns> offsetList, List<Vector2> selectedCoords, ref List<int> orders)
    {
        bool canMove = true;
        for (int i = 0; i < offsetList.Count; i++)
        {
            canMove = true;
            foreach (Tuple<int, int> move in offsetList[i].offsets)
            {
                if (!IsInSize(xCoord + move.Item1, yCoord + move.Item2))
                {
                    canMove = false;
                    break;
                }
            }
            if (canMove)
            {
                selectedCoords.Add(new Vector2(xCoord + offsetList[i].offsets[0].Item1, yCoord + offsetList[i].offsets[0].Item2));
                selectedCoords.Add(new Vector2(xCoord + offsetList[i].offsets[1].Item1, yCoord + offsetList[i].offsets[1].Item2));
                orders = offsetList[i].order;
                break;
            }
        }
    }

    /// <summary>
    /// Are coords are valid for the grid?
    /// </summary>
    private bool IsInSize(int x, int y)
    {
        if (x >= this.xTreeSize || y >= this.yTreeSize || x < 0 || y < 0)
            return false;
        return true;
    }

    /// <summary>
    /// All available selection moves for player click
    /// </summary>
    private void AddMoves()
    {
        oddPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(1, 0), new Tuple<int, int>(0, 1) }, new List<int> {0, 2, 1 }));
        oddPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(0, -1), new Tuple<int, int>(1, -1) }, new List<int> { 1,0,2}));
        oddPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(-1, -1), new Tuple<int, int>(0, -1) }, new List<int> { 1,0,2}));
        oddPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(-1, 0), new Tuple<int, int>(-1, -1) }, new List<int> { 2,1,0}));
        oddPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(-1, 0), new Tuple<int, int>(0, 1) }, new List<int> {2,0,1 }));
        evenPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(0, 1), new Tuple<int, int>(1, 1) }, new List<int> { 0,1,2}));
        evenPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(1, 0), new Tuple<int, int>(1, 1) }, new List<int> { 0,2,1}));
        evenPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(-1, 0), new Tuple<int, int>(0, -1) }, new List<int> { 1,0,2}));
        evenPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(-1, 0), new Tuple<int, int>(-1, 1) }, new List<int> { 2,0,1}));
        evenPatterns.Add(new SelectionPatterns(new List<Tuple<int, int>> { new Tuple<int, int>(0, -1), new Tuple<int, int>(1, 0) }, new List<int> { 1,0,2}));
    }
}

public struct SelectionPatterns
{
    public List<Tuple<int, int>> offsets;
    public List<int> order;
    public SelectionPatterns(List<Tuple<int, int>> offsets, List<int> orders)
    {
        this.offsets = offsets;
        this.order = orders;
    }
}