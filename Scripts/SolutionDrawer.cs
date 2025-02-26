using System.Collections;
using UnityEngine;

public class SolutionDrawer : MonoBehaviour{

    //using a line renderer to display the maze solution
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float endPieceLength;

    private Coroutine solutionRoutine;

    //clears existing solution line and starts drawing current solution
    public void ShowSolution(MazeDrawer mazeDrawer){
        ClearSolution();

        solutionRoutine = StartCoroutine(DrawSolution(mazeDrawer));
    }
    
    //stop the drawing coroutine and reset the line
    public void ClearSolution(){
        if(solutionRoutine != null)
            StopCoroutine(solutionRoutine);
        
        lineRenderer.positionCount = 0;
    }

    //returns whether a solution is currently being shown based on line position count
    public bool IsShowingSolution(){
        return lineRenderer.positionCount > 0;
    }

    //actually draws the maze solution
    private IEnumerator DrawSolution(MazeDrawer mazeDrawer){
        //retrieve the solution in the form of a positions array
        Vector2Int[] solution = mazeDrawer.GetSolution();
        
        if(solution == null)
            yield break;
        
        //get information about the maze size to scale the solution line accordingly
        Vector2Int size = mazeDrawer.GetSize();
        float cellSize = mazeDrawer.GetCellSize();
        float yOffset = mazeDrawer.GetWallHeight() / 2f;

        //put the line start just outside the maze
        lineRenderer.positionCount = 1;

        Vector3 startPoint = GetWorldPositionForPoint(size, solution[0].x, solution[0].y, cellSize, yOffset);
        startPoint -= Vector3.forward * cellSize * endPieceLength;
        lineRenderer.SetPosition(0, startPoint);
        
        //increase size of the line over time and add positions from the solution
        for(int i = 0; i < solution.Length; i++){
            lineRenderer.positionCount++;
            
            Vector3 pos = GetWorldPositionForPoint(size, solution[i].x, solution[i].y, cellSize, yOffset);
            lineRenderer.SetPosition(i + 1, pos);

            //scales the wait time between drawing new solution points based on inverse maze size
            //this way drawing the solution takes similar amount of time regardless of the maze
            if(i % (Mathf.Ceil(solution.Length/200f)) == 0)
                yield return new WaitForSeconds(0.01f);
        }

        //put the line end just outside the top of the maze
        lineRenderer.positionCount++;
        Vector3 endPoint = GetWorldPositionForPoint(size, solution[^1].x, solution[^1].y, cellSize, yOffset);
        endPoint += Vector3.forward * cellSize * endPieceLength;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPoint);
    }

    //transforms 2d solution position into world space 3d position
    private Vector3 GetWorldPositionForPoint(Vector2Int size, int x, int y, float cellSize, float yOffset){
        float xPos = -(size.x - 1) / 2f + x;
        float yPos = -(size.y - 1) / 2f + y;
            
        Vector3 pos = new Vector3(xPos * cellSize, yOffset, yPos * cellSize);

        return pos;
    }
}
