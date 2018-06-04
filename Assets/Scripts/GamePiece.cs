using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePiece : MonoBehaviour {

    [SerializeField]
    private Vector2 pos;
    public enum pieceColor { RED, GREEN, PURPLE, BLUE, ORANGE };
    public pieceColor pc;

    public enum Bonus { NONE, EXPLOSION, ROW, COL, DESTROYCOLOR };
    public Bonus bonus = (Bonus)0;

    private Image img;

    private GameBoard board;

    [Range (0,1)]
    public float moveSpeed = 0.7f;
    private bool moving = false;

    public Sprite[] sprites;
    public GameObject destroyAnimation;
    public Sprite[] explosionSprites;
    public Sprite[] destroyRowColSprites;
    public Sprite[] destroyColorSprites;

    [SerializeField]
    private bool isMatched = false;

    private List<Vector2> matchedWith = null;

    private bool isDestroyed = false;
    
    private void Awake(){
        img = gameObject.GetComponentInChildren<Image>();
        randomize();
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        if (moving)
        {
            //transform.position = Vector3.Lerp(moveToPos, transform.position, moveSpeed);// * Time.deltaTime);
            transform.position = Vector3.Lerp(board.getLocForPos((int)pos.x, (int)pos.y), transform.position, moveSpeed);// * Time.deltaTime);
        }
        if (moving && Vector3.Distance(board.getLocForPos((int)pos.x, (int)pos.y), transform.position) <= 0.1f)
        {
            moving = false;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    public void randomize() {
        int i = Random.Range(0, 5);
        pc = (pieceColor)i;
        setAlpha(1f);
        img.sprite = sprites[i];
        isMatched = false;
        isDestroyed = false;
        matchedWith = null;
        bonus = (Bonus)0;
    }

    public void moveTo(Vector2 posInArray, float speed) {
        board.getBoard()[(int)posInArray.x, (int)posInArray.y] = this;
        pos = posInArray;
        moveSpeed = speed;
        moving = true;
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }    

    public void setAlpha(float a) {
        Color temp = img.color;
        temp.a = a;
        img.color = temp;
    }

    public void setIsMatched(bool match) {        
        if (match) {
            setAlpha(0.5f);
        }
        isMatched = match;
    }

    public bool getIsMatched() {
        return isMatched;
    }

    public bool checkForBonus() {
        if (matchedWith != null) {
            Vector2Int m = getColRowMatches();
            Debug.Log("Matches: " + m + ". For piece " + pos);
            int rowMatches = m.x;
            int colMatches = m.y;

            if (rowMatches >= 5 || colMatches >= 5) {
                bonus = (Bonus)4;
                return true;
            }

            if (rowMatches >= 3 && colMatches >= 3)
            {
                bonus = (Bonus)1;
                return true;
            }

            if (rowMatches >= 4)
            {
                bonus = (Bonus)2;
                return true;
            }

            if (colMatches >= 4)
            {
                bonus = (Bonus)3;
                return true;
            }
        }
        return false;
    }

    private Vector2Int getColRowMatches() {
        int rowMatches = 0;
        int colMatches = 0;
        foreach (Vector2 v in matchedWith){
            if ((int)pos.x == (int)v.x) {
                rowMatches++;
            }
            if ((int)pos.y == (int)v.y){
                colMatches++;
            }
        }
        return new Vector2Int(rowMatches, colMatches);
    }

    public void destroy(){
        if (!checkForBonus()){
            doDestroyAnim();
            isDestroyed = true;
            setAlpha(0f);
            if ((int)bonus != 0)
            {
                doBonusEffect();
            }
        }
        else {
            turnIntoBonus();
        }
    }

    private void doBonusEffect() {
        Debug.Log("Activating bonus");
        switch (bonus)
        {
            case Bonus.NONE:
                break;
            case Bonus.DESTROYCOLOR:
                destroyMyColors();
                break;
            case Bonus.EXPLOSION:
                explodeBonus();
                break;
            case Bonus.ROW:
                destroyRow();
                break;
            case Bonus.COL:
                destroyCol();
                break;
            default:
                break;
        }
    }

    private void destroyRow() {
        for (int i = 0; i < board.width; i++) {
            if (!board.getBoard()[i, (int)pos.y].getIsDestroyed()){
                board.getBoard()[i, (int)pos.y].destroy();
            }
        }
    }

    private void destroyCol() {
        for (int i = 0; i < board.height; i++){
            if (!board.getBoard()[(int)pos.x, i].getIsDestroyed()) {
                board.getBoard()[(int)pos.x, i].destroy();
            }
        }
    }

    private void destroyMyColors() {
        for (int i = 0; i < board.width; i++){
            for (int j = 0; j < board.height; j++){
                if (!board.getBoard()[i, j].getIsDestroyed() && board.getBoard()[i, j].pc == pc) {
                    board.getBoard()[i, j].destroy();
                }
            }
        }
    }

    private void explodeBonus() {
        //destroy all pieces around this bonus piece
        for (int i = (int)pos.x -1; i < (int)pos.x + 2; i++){
            for (int j = (int)pos.y-1; j < (int)pos.y + 2; j++){
                if ((i >= 0 && i < board.width) && (j >= 0 && j < board.height) && !board.getBoard()[i,j].getIsDestroyed()) {
                    board.getBoard()[i, j].destroy();
                }   
            }
        }
    }

    public void turnIntoBonus() {
        isMatched = false;
        matchedWith = null;
        setAlpha(1f);
        switch (bonus)
        {
            case Bonus.NONE:
                break;
            case Bonus.DESTROYCOLOR:
                img.sprite = destroyColorSprites[(int)pc];
                break;
            case Bonus.EXPLOSION:
                img.sprite = explosionSprites[(int)pc];
                break;
            case Bonus.ROW:
                img.sprite = destroyRowColSprites[(int)pc];
                break;
            case Bonus.COL:
                img.sprite = destroyRowColSprites[(int)pc];
                break;
            default:
                break;
        }
    }

    public void doDestroyAnim() {
        GameObject anim = Instantiate(destroyAnimation, new Vector3(transform.position.x, transform.position.y, transform.position.z + 10), Quaternion.identity);
        anim.transform.SetParent(transform.parent);
        Destroy(anim, 2);
    }

    public bool getIsDestroyed() {
        return isDestroyed;
    }
    
    public void setPos(int w, int h) {
        pos = new Vector2(w,h);
    }

    public void setPos(Vector2 newPos) {
        pos = newPos;
    }

    public Vector2 getPos() {
        return pos;
    } 

    public bool isMoving() {
        return moving;
    }

    public void setMoving(bool moving) {
        this.moving = moving;
    }

    public void setBoard(GameBoard b) {
        board = b;
    }

    public void setMatchedWith(List<Vector2> matches) {
        matchedWith = matches;
        foreach (Vector2 v in matchedWith){
            board.getBoard()[(int)v.x, (int)v.y].setIsMatched(true);
        }
    }

    public List<Vector2> getMatchedWith() {
        return matchedWith;
    }
}
