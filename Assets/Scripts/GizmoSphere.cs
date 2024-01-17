using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Añade un gizmo en su pivote para visualizar y seleccionar mejor un gameobject en el editor
public class GizmoSphere : MonoBehaviour
{
    public float sphereRadius = 0.05f;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;


    private void OnDrawGizmos()
    {
        Gizmos.color = normalColor;
        Gizmos.DrawSphere(transform.position, sphereRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = selectedColor;
        Gizmos.DrawSphere(transform.position, sphereRadius);
    }
}
