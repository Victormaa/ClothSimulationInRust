// Builds a Mesh containing a single triangle with uvs.
// Create arrays of vertices, uvs and triangles, and copy them into the mesh.

using UnityEngine;

public class meshTriangles : MonoBehaviour
{
    public Vector3[] verts;
    private Mesh mesh;
    int _x, _y;
    // Use this for initialization
    void Start()
    {
    }
    public void InitiliazedVerts(int x, int y)
    {
        _x = x;
        _y = y;
        verts = new Vector3[(x) * (y)];
    }
    public void Generate(GameObject[] nodesNet)
    {

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Cloth";

        Vector2[] uv = new Vector2[verts.Length];

        for (int i = 0, y = 0; y < _y; y++)
        {
            for (int x = 0; x < _x; i++, x++)
            {
                verts[i] = nodesNet[i].transform.position;
                uv[i] = new Vector2((float)x / _x, (float)y / _y);
            }
        }
        mesh.vertices = verts;
        mesh.uv = uv;
        calcTris();
    }
    public void calcTris()
    {
        int[] tris = new int[(_x-1) * (_y-1) * 6];


        for (int ti = 0, vi = 0, y = 0; y < _y-1; y++, vi++)
        {
            for (int x = 0; x < _x-1; x++, ti += 6, vi++)
            {
                tris[ti] = vi;
                tris[ti + 3] = tris[ti + 2] = vi + 1;
                tris[ti + 4] = tris[ti + 1] = vi + _x - 1 + 1;
                tris[ti + 5] = vi + _x - 1 + 2;
            }
        }

        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }
    public void setMesh(GameObject[] nodesNet)
    {
        for (int i = 0, y = 0; y < _y; y++)
        {
            for (int x = 0; x < _x; i++, x++)
            {
                verts[i] = nodesNet[i].transform.position;
            }
        }
        mesh.vertices = verts;
    }
}