using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour {
    
    //can be listened to to get generator progress updates
    [HideInInspector]
    public UnityEvent<int, int> progressUpdated = new();
    private int progress;
    
    //possible neighbours. order is important as it needs to go clockwise starting from top same as the walls
    private readonly Vector2Int[] neighbourDirections = {
        new (0, 1),
        new (1, 0),
        new (0, -1),
        new (-1, 0),
    };

    //iterative stack based maze generator (as described on maze generation wikipedia)
    public MazeCell[,] Generate(Vector2Int size, ref Vector2Int[] solution){
        //create grid of maze cells
        MazeCell[,] maze = new MazeCell[size.x, size.y];
        
        for(int x = 0; x < size.x; x++){
            for(int y = 0; y < size.y; y++){
                maze[x, y] = new MazeCell();
            }
        }

        //get start point and end point and remove start/end walls
        Vector2Int mazeStart = new Vector2Int(0, 0);
        Vector2Int mazeEnd = new Vector2Int(size.x - 1, size.y - 1);
        
        maze[mazeStart.x, mazeStart.y].RemoveWall(2);
        maze[mazeEnd.x, mazeEnd.y].RemoveWall(0);
        
        //using a stack instead of recursion to be sure it won't exceed recursion depth
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        
        //visit first cell (bottom left) and put it on the stack
        maze[mazeStart.x, mazeStart.y].MarkVisited();
        stack.Push(mazeStart);

        //checks if there's cell positions left on the stack
        while(stack.Count > 0){
            //peek the current position to see if we've reached the end cell
            Vector2Int currentPos = stack.Peek();

            //if we've reached the end cell, put the entire current stack as the maze solution
            if(currentPos == mazeEnd && solution == null){
                solution = stack.ToArray();
                
                //reverse because the stack goes from final cell to first cell
                Array.Reverse(solution);
            }

            //take current cell position from the stack and find its neighbours
            stack.Pop();
            List<Vector2Int> unvisitedNeighbours = GetUnvisitedNeighbours(maze, currentPos, size);

            //if there are unvisited neighbours we can use them to continue generating
            if(unvisitedNeighbours.Count > 0){
                //put current position on the stack
                stack.Push(currentPos);

                //get a random unvisited neighbour
                Vector2Int randomNeighbour = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];

                //get the direction between neighbour and current position to know which wall to remove
                Vector2Int neighbourWallDirection = randomNeighbour - currentPos;
                int wallToRemove = Array.IndexOf(neighbourDirections, neighbourWallDirection);
                maze[currentPos.x, currentPos.y].RemoveWall(wallToRemove);
                
                //+2 to get the opposite wall so it can be removed from the neighbour cell
                int neighbourWall = (wallToRemove + 2) % 4;
                maze[randomNeighbour.x, randomNeighbour.y].RemoveWall(neighbourWall);
                
                //mark the neighbour as visited and put it on the stack
                maze[randomNeighbour.x, randomNeighbour.y].MarkVisited();
                stack.Push(randomNeighbour);

                //update the progress value
                progress++;
                progressUpdated.Invoke(progress, size.x * size.y);
            }
        }

        return maze;
    }

    //gets unvisited neighbours for cell at the specified position
    private List<Vector2Int> GetUnvisitedNeighbours(MazeCell[,] maze, Vector2Int currentPos, Vector2Int size){
        List<Vector2Int> unvisitedNeighbours = new List<Vector2Int>();

        //go through all 4 directions to find neighbours
        foreach(Vector2Int direction in neighbourDirections){
            //add direction to current position to get the (potential) neighbour position
            Vector2Int neighbourPos = currentPos + direction;

            //cannot evaluate a non-existing neighbour, so move on
            if(!NeighbourInBounds(neighbourPos, size))
                continue;
                
            //if neighbour was not visited add it to the list
            if(!maze[neighbourPos.x, neighbourPos.y].WasVisited())
                unvisitedNeighbours.Add(neighbourPos);
        }

        return unvisitedNeighbours;
    }

    //use the maze size to check if position is in bounds
    private bool NeighbourInBounds(Vector2Int pos, Vector2Int size){
        return pos.x >= 0 && pos.x < size.x && pos.y >= 0 && pos.y < size.y;
    }
}
