using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBoard : MonoBehaviour
{

    public int width = 8;
    public int height = 8;
    private float offsetWidth = 0;
    private float offsetHeight = 0;
    private float pieceWidth = 50f;
    private float pieceHeight = 50f;

    public GameObject spawnpoint;
    public GameObject GamePiecePrefab;
    private GamePiece[,] board;

    public float moveDownSpeed = 0.8f;
    public float swapSpeed = 0.5f;

    private bool boardIsUpdating = false;
    
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        offsetWidth = rt.position.x - (rt.rect.width / 2) + pieceWidth;
        offsetHeight= rt.position.y - (rt.rect.height / 2) + pieceHeight;
        board = new GamePiece[width, height];
        setup();
    }

    private void Update(){
        if (Input.GetKeyDown("x")) {
            StartCoroutine(updateBoard());
        }
        
    }

    public GamePiece[,] getBoard() {
        return board;
    }

    private bool boardHasMovingPieces() {
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                if (board[i, j] != null && board[i,j].isMoving()) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool findMatches() {
        bool containsMatches = false;
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                GamePiece piece = board[i,j];
                
                if (!piece.getIsMatched()) {
                    if (hasMatches(piece)) {
                        containsMatches = true;
                    }
                } else {
                    containsMatches = true;
                }
            }
        }
        //Debug.Log("Contains matches: " + containsMatches);
        return containsMatches;
    }

    public bool hasMatches(GamePiece piece) {
        if (piece != null) {
            List<Vector2> rowMatches = checkRow(piece);
            List<Vector2> colMatches = checkCol(piece);
            if (rowMatches.Count >= 2 || colMatches.Count >= 2) {
                List<Vector2> all = new List<Vector2>();
                foreach (Vector2 match in rowMatches){
                    all.Add(match);
                }
                foreach (Vector2 match in colMatches){
                    all.Add(match);
                }
                all.Add(piece.getPos());
                piece.setMatchedWith(all);           
                return true;
            }
        }
        return false;
    }

    private List<Vector2> checkRow(GamePiece piece) {
        Vector2 pos = piece.getPos();
        int x = (int)pos.x;
        int y = (int)pos.y;

        List<Vector2> matches = new List<Vector2>();
        // check row -->
        for (int i = 1; x + i < width && piece.pc == board[x + i, y].pc; i++){
            matches.Add(new Vector2(x + i,y));
        }

        // check row <--
        for (int i = 1; x - i >= 0 && piece.pc == board[x - i, y].pc; i++){
            matches.Add(new Vector2(x - i, y));
        }
        if (matches.Count < 2) { matches.Clear(); }
        return matches;
    }

    private List<Vector2> checkCol(GamePiece piece) {
        Vector2 pos = piece.getPos();
        int x = (int)pos.x;
        int y = (int)pos.y;

        List<Vector2> matches = new List<Vector2>();
        // check col ^
        for (int i = 1; y + i < height &&  piece.pc == board[x, y + i].pc; i++){
            matches.Add(new Vector2(x, y + i));
        }

        // check col v
        for (int i = 1; y - i >= 0 &&  piece.pc == board[x, y - i].pc; i++){
            matches.Add(new Vector2(x, y - i));
        }
        if (matches.Count < 2) { matches.Clear(); }
        return matches;
    }

    public bool moveIsLegal(Vector2 a, Vector2 b)
    {
        Vector2 c = a - b;
        float i = c.x + c.y;
        return ((c.x <= 1 && c.x >= -1) && (c.y <= 1 && c.y >= -1)) && (i == 1 || i == -1);
    }

    IEnumerator updateBoard() {
        Debug.Log("Started updating board");
        boardIsUpdating = true;
        blockRaycasts(false);
        
        while (findMatches()) {
            if (!boardHasMovingPieces()) {
                destroyMatches();
                moveDown();
            }
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Finished updating board");
        boardIsUpdating = false;
        blockRaycasts(true);
    }

    public void swapPieces(GamePiece draggedPiece, GamePiece targetPiece)
    {
        if (!boardIsUpdating && moveIsLegal(draggedPiece.getPos(), targetPiece.getPos()))
        {            
            doSwap(draggedPiece, targetPiece, swapSpeed);
            if(hasMatches(draggedPiece) || hasMatches(targetPiece)){
                Debug.Log("Move makes a match");
                StartCoroutine(updateBoard());
            }else{
                StartCoroutine(returnPieces(draggedPiece, targetPiece));
            }
        }
        else
        {
            // returning
            draggedPiece.moveTo(draggedPiece.getPos(), swapSpeed);
        }
    }

    private void destroyMatches() {
        Debug.Log("Starting destroy");
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                if (board[i,j].getMatchedWith() != null) {
                    destroyPieces(board[i, j].getMatchedWith());
                }
            }
        }
        Debug.Log("Finished destroy");
    }

    private void destroyPieces(List<Vector2> pieces) {
        foreach (Vector2 loc in pieces){
            board[(int)loc.x, (int)loc.y].destroy();
        }
    }

    private void moveDown() {
        Debug.Log("Starting moveDown");

        for (int i = 0; i < width; i++){
            for (int j = height -1; rowHasDestroyedPieces(i); j--) {
                int top = height - 1;
                //check if top piece is destroyed and re-randomize it + make it drop from above
                if (board[i, top].getIsDestroyed()) {
                    board[i, top].randomize();
                    //set transform position to spawnpoint
                    board[i, top].transform.position = new Vector2(board[i, top].transform.position.x, spawnpoint.transform.position.y);
                    board[i, top].setMoving(true);
                }

                //check if i,j piece is destroyed
                if (board[i,j].getIsDestroyed()) {
                    doSwap(board[i, j], board[i, j+1], moveDownSpeed);
                    j = top;
                }
            }
        }
        
        Debug.Log("stopping moveDown");
    }

    private bool rowHasDestroyedPieces(int row) {
        for (int i = 0; i < height; i++){
            if (board[row, i].getIsDestroyed()) {
                return true;
            }
        }
        return false;
    }

    private void doSwap(GamePiece a, GamePiece b, float s){
        Vector3 tempTransformPos = b.getPos();
        b.moveTo(a.getPos(), s);
        a.moveTo(tempTransformPos, s);
    }

    IEnumerator returnPieces(GamePiece a, GamePiece b) {
        while (a.isMoving() && b.isMoving()) {
            yield return new WaitForSeconds(0.05f);
        }
        doSwap(a,b, swapSpeed);
    }

    public Vector3 getLocForPos(int i, int j) {
        return new Vector3(i * pieceWidth + offsetWidth, j * pieceHeight + offsetHeight, transform.position.z);
    }

    private void setup()
    {
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                createPiece(i,j);
            }
        }
        int resetCounter = 0;
        while (findMatches()) {
            resetBoard();
            resetCounter++;
        }
        Debug.Log("Found a match, resetted: " + resetCounter);
    }

    private void resetBoard() {
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++) {
                board[i, j].randomize();
            }
        }
    }

    private void createPiece(int i, int j) {
        Vector2 newPos = getLocForPos(i, j);
        GameObject tile = Instantiate(GamePiecePrefab, new Vector2(newPos.x, spawnpoint.transform.position.y), Quaternion.identity);//gameObject.transform.rotation);
                
        tile.transform.SetParent(gameObject.transform);
        board[i, j] = tile.GetComponent<GamePiece>();
        board[i, j].setBoard(this);
        board[i, j].moveTo(new Vector2(i, j), moveDownSpeed);
    }

    private void blockRaycasts(bool block){
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                board[i,j].GetComponent<CanvasGroup>().blocksRaycasts = block;
            }
        }
    }
}
