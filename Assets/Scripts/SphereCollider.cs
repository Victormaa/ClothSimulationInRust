using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollider : MonoBehaviour
{
    [SerializeField]
    private float _radius;

    public float radius
    {
        get { return _radius; }
    }

    public Vector3 Pos()
    {
        return this.transform.position;
    }

    private void Start()
    {
        int a = 1;
        int b = 15;
        var c = MyLibrary.add(a, b);
        Debug.Log(c);
    }
}
