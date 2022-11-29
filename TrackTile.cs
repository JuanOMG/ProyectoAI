using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTile : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
// Dibuja una línea azul desde esta transformación hasta el objetivo.
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 2, 
                transform.position + Vector3.up * 2 + transform.forward * 3
                );
        
    }
}
