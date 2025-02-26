using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MazeGenerator))]
public class MazeDrawer : MonoBehaviour{

    //actions so the UI manager knows when it's drawing a maze
    public Action OnStartDrawing;
    public Action OnEndDrawing;

    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject floor;
    
    [Space]
    [SerializeField] private Vector2Int mazeSize;
    [SerializeField] private float cellSize;
    [SerializeField] private float wallThickness;
    [SerializeField] private float wallHeight;
    
    [SerializeField] private float chunkDrawDelay;

    [Tooltip("Cap max chunk size so mesh combiner will work correctly")]
    [SerializeField] private int maxChunkSize;
    [SerializeField] private bool useCustomChunkSize;
    [SerializeField] private int customChunkSize;

    [SerializeField] private UnityEvent onDrawChunk;

    private MazeGenerator generator;

    //not required but important for performance with large mazes
    private MeshCombiner meshCombiner;

    private MazeCell[,] maze;
    private Vector2Int[] solution;

    private Coroutine drawRoutine;
    
    //holds the entire generated maze object
    private Transform mazeTransform;

    private void Awake(){
        generator = GetComponent<MazeGenerator>();
        meshCombiner = GetComponent<MeshCombiner>();
    }

    public float DrawMaze(){
        //checks if maze size is valid
        if(mazeSize.x < 0 || mazeSize.y < 0){
            Debug.LogError("Maze size cannot be negative");

            return 0;
        }
        
        OnStartDrawing?.Invoke();

        //gets the maze as well as the solution from the generator
        solution = null;
        maze = generator.Generate(mazeSize, ref solution);
        
        //make sure if it's currently drawing we first stop drawing before making a new maze
        if(drawRoutine != null)
            StopCoroutine(drawRoutine);
        
        //draw new maze
        drawRoutine = StartCoroutine(DrawMaze(maze, OnEndDrawing));

        //return the size of the new maze (so we can zoom the camera accordingly)
        return mazeSize.magnitude * cellSize;
    }

    private IEnumerator DrawMaze(MazeCell[,] maze, Action onFinished){
        //remove existing maze if necessary and cancel tweens
        if(mazeTransform != null){
            DOTween.KillAll();
            Destroy(mazeTransform.gameObject);
        }

        //create the new maze object
        mazeTransform = new GameObject("Maze").transform;

        //create the maze floor
        Transform mazeFloor = Instantiate(floor, mazeTransform.position, Quaternion.identity).transform;
        mazeFloor.SetParent(mazeTransform, true);
        mazeFloor.localScale = new Vector3(mazeSize.x * cellSize, 1, mazeSize.y * cellSize) / 10;
        
        //floor pop in effect
        mazeFloor.DOPunchScale(Vector3.one/5f, 0.3f, 1, 0.1f);

        //calculate chunk size and chunk counts
        int chunkSize = CalculateChunkSize();

        int xChunkCount = Mathf.CeilToInt(mazeSize.x / (float) chunkSize);
        int yChunkCount = Mathf.CeilToInt(mazeSize.y / (float) chunkSize);
        
        //start drawing the chunks
        for(int x = 0; x < xChunkCount; x++){
            for(int y = 0; y < yChunkCount; y++){
                Transform chunk = DrawMazeChunk(maze, x, y, chunkSize, xChunkCount, yChunkCount, mazeTransform);
                
                //chunk pop-in effect
                chunk.DOPunchScale(Vector3.up * chunkSize/5, 0.3f, 1, 0.1f);
                
                //wait for a bit if there is a draw delay
                if(chunkDrawDelay > 0)
                    yield return new WaitForSeconds(chunkDrawDelay);
            }
        }
        
        onFinished?.Invoke();
    }

    //draws a chunk of maze cells (this way they can be combined into a single mesh)
    private Transform DrawMazeChunk(MazeCell[,] maze, int xChunkIndex, int yChunkIndex, int chunkSize, int xChunkCount, int yChunkCount, Transform mazeParent){
        //for effects like the plop sound
        onDrawChunk?.Invoke();
        
        //create the chunk base game object
        Transform chunkParent = new GameObject("Chunk (" + xChunkIndex + ", " + yChunkIndex + ")").transform;

        //set chunk position and parent it to the maze
        float xChunkPos = -(xChunkCount - 1) / 2f + xChunkIndex;
        float yChunkPos = -(yChunkCount - 1) / 2f + yChunkIndex;
        chunkParent.position = new Vector3(xChunkPos, 0, yChunkPos) * cellSize * chunkSize;
        
        chunkParent.SetParent(mazeParent, true);
        
        //calculate which cells belong to this chunk
        int xStartCell = xChunkIndex * chunkSize;
        int yStartCell = yChunkIndex * chunkSize;

        int xEndCell = Mathf.Min(xStartCell + chunkSize, mazeSize.x);
        int yEndCell = Mathf.Min(yStartCell + chunkSize, mazeSize.y);
        
        //draw the cells
        for(int x = xStartCell; x < xEndCell; x++){
            for(int y = yStartCell; y < yEndCell; y++){
                DrawMazeCell(maze[x, y], x, y, chunkParent);
            }
        }

        //drawer always divides maze into chunks, and if there is a combiner the chunks get combined
        if(meshCombiner != null){
            meshCombiner.CombineMeshes(chunkParent, chunkParent.name);

            //reset the chunk transform position after generating the chunk mesh
            chunkParent.localPosition = Vector3.zero;
        }

        return chunkParent;
    }

    //draws walls for 1 maze cell and parents them to the target parent
    private void DrawMazeCell(MazeCell cell, int x, int y, Transform targetParent){
        Vector3 cellCenter = new Vector3(-(mazeSize.x - 1) / 2f + x, 0, -(mazeSize.y - 1) / 2f + y) * cellSize;
        
        //because the wall origin is halfway and not on the floor, move it up to align with floor
        cellCenter += Vector3.up * wallHeight / 2f;
        
        //cells must always have 4 walls, could get this value from the cell but seems unnecessary
        for(int i = 0; i < 4; i++){
            if(cell.HasWall(i) && !IsDoubleWall(i, x, y, mazeSize.x, mazeSize.y)){
                //make wall
                Transform newWall = Instantiate(wall, cellCenter, Quaternion.identity).transform;
                newWall.localScale = new Vector3(cellSize + wallThickness, wallHeight, wallThickness);
                
                //put top wall, and rotate it around the center to get the other walls
                newWall.position += Vector3.forward * cellSize / 2f;
                newWall.RotateAround(cellCenter, Vector3.up, i * 90);
                
                newWall.SetParent(targetParent, true);
            }
        }        
    }

    //checks if wall is already drawn by other cell
    private bool IsDoubleWall(int wall, int x, int y, int width, int height){
        return (wall == 0 && y < height - 1) || (wall == 1 && x < width - 1);
    }

    //calculates the chunks size based on maze size
    public int CalculateChunkSize(){
        //kind of arbitrary formula, but scales well with larger mazes
        int size = Mathf.CeilToInt(Mathf.Sqrt(mazeSize.x * mazeSize.y) / 10f);

        //override size if user wants to use their own chunk size
        if(useCustomChunkSize)
            size = customChunkSize;
        
        //cap chunk size so it doesn't reach the mesh vertex limit
        size = Mathf.Min(size, maxChunkSize);

        return size;
    }
    
    //returns whether a maze currently exists
    public bool HasMaze(){
        return maze != null;
    }
    
    #region Getters

    public MazeGenerator GetMazeGenerator(){
        return generator;
    }

    public Vector2Int[] GetSolution(){
        return solution;
    }

    public Vector2Int GetSize(){
        return mazeSize;
    }

    public float GetCellSize(){
        return cellSize;
    }
    
    public float GetWallThickness(){
        return wallThickness;
    }

    public float GetWallHeight(){
        return wallHeight;
    }

    public float GetDrawEffect(){
        return chunkDrawDelay;
    }

    public bool GetUseCustomChunkSize(){
        return useCustomChunkSize;
    }

    public float GetCustomChunkSize(){
        return customChunkSize;
    }

    #endregion
    
    #region Setters

    public void SetWidth(int width){
        mazeSize = new Vector2Int(width, mazeSize.y);
    }
    
    public void SetHeight(int height){
        mazeSize = new Vector2Int(mazeSize.x, height);
    }

    public void SetCellSize(float cellSize){
        this.cellSize = cellSize;
    }
    
    public void SetWallThickness(float wallThickness){
        this.wallThickness = wallThickness;
    }

    public void SetWallHeight(float wallHeight){
        this.wallHeight = wallHeight;
    }

    public void SetDrawEffect(float chunkDrawDelay){
        this.chunkDrawDelay = chunkDrawDelay;
    }

    public void SetUseCustomChunkSize(bool useCustomChunkSize){
        this.useCustomChunkSize = useCustomChunkSize;
    }

    public void SetCustomChunkSize(int customChunkSize){
        this.customChunkSize = customChunkSize;
    }

    #endregion
}
