using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    private GameBoard gb;

    void Start() {
        gb = gameObject.GetComponentInParent<GameBoard>();
    }

    public void OnDrop(PointerEventData eventData) {
        Draggable dragged = eventData.pointerDrag.GetComponent<Draggable>();
        
        GamePiece targetPiece = gameObject.GetComponent<GamePiece>();
        GamePiece draggedPiece = eventData.pointerDrag.GetComponent<GamePiece>();
        if ((targetPiece != null && draggedPiece != null))
        {
            gb.swapPieces(draggedPiece, targetPiece);
            dragged.setHit(true);
        }
        else {
            dragged.setHit(false);
        }
    }
}
