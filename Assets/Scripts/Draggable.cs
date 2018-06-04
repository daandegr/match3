using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool hit;

    private GamePiece gamePiece;
    
    void Start() {
        gamePiece = gameObject.GetComponent<GamePiece>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        hit = false;
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        gameObject.transform.SetSiblingIndex(gameObject.transform.parent.childCount);
    }
    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = eventData.position;
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 50));
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (!hit) {
            gamePiece.moveTo(gamePiece.getPos(), 0.5f);
        }
    }

    public void setHit(bool h) {
        hit = h;
    }
}
