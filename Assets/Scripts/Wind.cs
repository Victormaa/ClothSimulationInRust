using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField]
    private  int width;
    [SerializeField]
    private int height;
    public Vector3 Pos()
    {
        return this.transform.position;
    }
    public float Width()
    {
        return Pos().x + width;
    }
    public float Height()
    {
        return Pos().y - height;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(Pos(), new Vector3(Width(), Height(), Pos().z));
    }
}
