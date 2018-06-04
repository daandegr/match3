using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
        
    public static bool moveIsLegal(Vector2 a, Vector2 b) {
        Vector2 c = a-b;
        float i = c.x + c.y;
        return ((c.x <= 1 && c.x >= -1) && (c.y <= 1 && c.y >= -1)) && (i == 1 || i == -1);
    }

}
