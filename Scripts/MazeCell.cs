public class MazeCell{

    //top, right, bottom, left (clockwise)
    private bool[] walls = {true, true, true, true};

    //keep track if the cell has been visited during maze generation
    private bool visited;

    public bool WasVisited(){
        return visited;
    }

    public void MarkVisited(){
        visited = true;
    }

    //returns whether the specified wall exists for this cell
    public bool HasWall(int wall){
        return walls[wall];
    }

    //removes the specified wall from this cell
    public void RemoveWall(int wall){
        walls[wall] = false;
    }
}
