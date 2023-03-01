//#define NotUserust

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class SpringForce : MonoBehaviour
{
    #region private Parameter
    public GameObject node;
    private GameObject[] nodesNet;
    private Vector3[] lastPos;
    private Vector3[] curVel;
    private int springNodesNum;
    private float L1;
    private float L2;
    private int _calNumChunks = 1;
    private int change = 0;
    #endregion

    #region public region
    [Range(0, 6)]
    public float WF = 5.0f;
    [Range(0, 1)]
    public float dampValRate = 0.995f;
    public Vector2Int xandy;
    public float G = 9.8f;
    public float K0 = 12;
    public float L0 = 2f;
    public meshTriangles mt;
    public int calNumChunks;
    public SphereCollider sphereCollider;
    public Wind wind;
    #endregion

    void Awake()
    {
        springNodesNum = xandy.x * xandy.y;
        lastPos = new Vector3[springNodesNum];
        curVel = new Vector3[springNodesNum];
        nodesNet = new GameObject[xandy.x * xandy.y];
        int i = 0;
        int j = 0;
        int index = 0;
        float LeftUpY = 6.0f;
        float LeftUpX = -6.0f;
        L1 = L0 * 1.414f;
        L2 = L0 * 2f;

        while (i < xandy.y)
        {
            var parent = new GameObject();
            parent.name = "object"+i.ToString();
            while (j < xandy.x)
            {
                nodesNet[index] = Instantiate(node,
                new Vector3(LeftUpX + j * 2.0f, LeftUpY - (i * 2.0f), 0),
                Quaternion.identity);

                nodesNet[index].transform.parent = parent.transform;

                nodesNet[index].GetComponent<MeshRenderer>().enabled = false;

                lastPos[index] = nodesNet[index].transform.position;

                j += 1;
                index+=1;
            }
            
            i += 1;
            j = 0; 
        }
        mt.InitiliazedVerts(xandy.x, xandy.y);
        mt.Generate(nodesNet);
    }
    private void Update()
    {
        L1 = L0 * Mathf.Sqrt(2);
        L2 = L0 * 2f;
        Profiler.BeginSample("ClothOutLine");
        ClothSimulationCal(xandy.x, xandy.y, _calNumChunks, 
            dampValRate, Time.deltaTime, WF, G, sphereCollider.radius,
            sphereCollider.Pos(), ref lastPos, ref curVel);
        Profiler.EndSample();
        mt.setMesh(nodesNet);
        if (change < 10)
        {
            _calNumChunks = 1;
            change += 1;
        }
        else
        {
            _calNumChunks = calNumChunks;
        }
    }

    private void ClothSimulationCal(int xNodesNum, int yNodesNum, int calNumChunks, 
        float dampRate, float timeStep, float windForce, float gravity, float sphereRadius,
        Vector3 spherePos, ref Vector3[] lastPosition, ref Vector3[] velocities)
    {
        int i = 0;
        while (i < xNodesNum * yNodesNum)
        {
            if (i == 0 || i == xNodesNum - 1)
            {
                //lastPosition[i] = nodesNet[i].transform.position;
            }
            else
            {
                int count = 0;

                while (count < calNumChunks)
                {
                    Profiler.BeginSample("ForceCal");

                    var deltaS = ForceCal(xNodesNum, yNodesNum, 
                        dampRate, timeStep, windForce, gravity, sphereRadius, 
                        spherePos, lastPosition, velocities, i);

                    Profiler.EndSample();

                    lastPosition[i] = nodesNet[i].transform.position;

                    nodesNet[i].transform.position += deltaS;

                    count += 1;
                }
            }
            i += 1;
        }
        
    }
    private Vector3 ForceCal(int xNodesNum, int yNodesNum, 
        float dampRate, float timeStep, float windForce, float gravity, float sphereRadius, 
        Vector3 spherePos, Vector3[] lastPosition, Vector3[] velocities, int i)
    {
        Vector3 deltaS;

        //var Forcenormal =  isInsideSphereCollider(lastPosition[i], spherePos, sphereRadius);
        var Forcenormal = MyLibrary.is_inside_sphere_collider(lastPosition[i], spherePos, sphereRadius);

        //Vector3 F = GravityForce(gravity) +
        //            WindForceNew(lastPosition[i],
        //            wind.Pos().x, 
        //            wind.Width(),
        //            wind.Height(),
        //            wind.Pos().y,                      
        //            windForce);
        Vector3 F = MyLibrary.wind_force(lastPosition[i],
                    wind.Pos().x,
                    wind.Width(),
                    wind.Height(),
                    wind.Pos().y,
                    windForce) + MyLibrary.gravity_force(gravity);

        Profiler.BeginSample("structral force cost");

#if NotUserust
        F = StructralForceCal(xNodesNum, yNodesNum, lastPosition, i, F);
#else
        F = StructralForceCal_Rust(xNodesNum, yNodesNum, lastPosition, i, F);
#endif

        Profiler.EndSample();
        var accel = F + Forcenormal * F.magnitude;

        velocities[i] += accel * timeStep;

        velocities[i] *= dampRate;

        deltaS = velocities[i] * timeStep;

        return deltaS;
    }

    private Vector3 StructralForceCal(int xNodesNum, int yNodesNum, Vector3[] lastPosition, int i, Vector3 F)
    {
#region Structural
        float tempK = K0;
        float tempL = L0;
        // upper
        if (i >= xNodesNum)
        {

            F += InnerForce(lastPosition[i], lastPosition[i - xNodesNum], tempK, tempL);
        }
        //// left
        if (i % xNodesNum != 0)
        {
            F += InnerForce(lastPosition[i], lastPosition[i - 1], tempK, tempL);
        }
        //right
        if (i % xNodesNum != xNodesNum - 1)
        {
            F += InnerForce(lastPosition[i], lastPosition[i + 1], tempK, tempL);
        }
        //down
        if (i < xNodesNum * yNodesNum - xNodesNum)
        {
            F += InnerForce(lastPosition[i], lastPosition[i + xNodesNum], tempK, tempL);
        }
#endregion
#region Shear
        tempL = L1;
        // upperleft
        if (i >= xNodesNum && i % xNodesNum != 0)
        {
            F += InnerForce(lastPosition[i], lastPosition[i - xNodesNum - 1], tempK, tempL);
        }
        // upperright
        if (i >= xNodesNum && i % xNodesNum != xNodesNum - 1)
        {
            F += InnerForce(lastPosition[i], lastPosition[i - xNodesNum + 1], tempK, tempL);
        }
        // downright
        if (i < xNodesNum * yNodesNum - xNodesNum && i % xNodesNum != xNodesNum - 1)
        {
            F += InnerForce(lastPosition[i], lastPosition[i + xNodesNum + 1], tempK, tempL);
        }
        // downleft
        if (i < xNodesNum * yNodesNum - xNodesNum && i % xNodesNum != 0)
        {
            F += InnerForce(lastPosition[i], lastPosition[i + xNodesNum - 1], tempK, tempL);
        }
#endregion
#region Flexion
        tempL = L2;
        // down + 2
        if (i < (xNodesNum * yNodesNum) - (xNodesNum * 2))
        {
            F += InnerForce(lastPosition[i], lastPosition[i + 2 * xNodesNum], tempK, tempL);
        }
        // upper + 2
        if (i >= (xNodesNum * 2))
        {
            F += InnerForce(lastPosition[i], lastPosition[i - 2 * xNodesNum], tempK, tempL);
        }
        // left + 2
        if (i % xNodesNum != 0 && i % xNodesNum != 1)
        {
            F += InnerForce(lastPosition[i], lastPosition[i - 2], tempK, tempL);
        }
        //right + 2
        if (i % xNodesNum != xNodesNum - 1 && i % xNodesNum != xNodesNum - 2)
        {
            F += InnerForce(lastPosition[i], lastPosition[i + 2], tempK, tempL);
        }
#endregion
        return F;
    }
    private Vector3 StructralForceCal_Rust(int xNodesNum, int yNodesNum, Vector3[] lastPosition, int i, Vector3 F)
    {
        #region Structural
        float tempK = K0;
        float tempL = L0;
        // upper
        if (i >= xNodesNum)
        {

            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i - xNodesNum], tempK, tempL);
        }
        //// left
        if (i % xNodesNum != 0)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i - 1], tempK, tempL);
        }
        //right
        if (i % xNodesNum != xNodesNum - 1)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i + 1], tempK, tempL);
        }
        //down
        if (i < xNodesNum * yNodesNum - xNodesNum)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i + xNodesNum], tempK, tempL);
        }
        #endregion
        #region Shear
        tempL = L1;
        // upperleft
        if (i >= xNodesNum && i % xNodesNum != 0)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i - xNodesNum - 1], tempK, tempL);
        }
        // upperright
        if (i >= xNodesNum && i % xNodesNum != xNodesNum - 1)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i - xNodesNum + 1], tempK, tempL);
        }
        // downright
        if (i < xNodesNum * yNodesNum - xNodesNum && i % xNodesNum != xNodesNum - 1)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i + xNodesNum + 1], tempK, tempL);
        }
        // downleft
        if (i < xNodesNum * yNodesNum - xNodesNum && i % xNodesNum != 0)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i + xNodesNum - 1], tempK, tempL);
        }
        #endregion
        #region Flexion
        tempL = L2;
        // down + 2
        if (i < (xNodesNum * yNodesNum) - (xNodesNum * 2))
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i + 2 * xNodesNum], tempK, tempL);
        }
        // upper + 2
        if (i >= (xNodesNum * 2))
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i - 2 * xNodesNum], tempK, tempL);
        }
        // left + 2
        if (i % xNodesNum != 0 && i % xNodesNum != 1)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i - 2], tempK, tempL);
        }
        //right + 2
        if (i % xNodesNum != xNodesNum - 1 && i % xNodesNum != xNodesNum - 2)
        {
            F += MyLibrary.inner_force(lastPosition[i], lastPosition[i + 2], tempK, tempL);
        }
        #endregion
        return F;
    }
    Vector3 isInsideSphereCollider(Vector3 point, Vector3 spherePos, float sphereRadius)
    {
        Vector3 Forcenormal = new Vector3();

        var vecPtoS = -(spherePos - point);

        if (vecPtoS.magnitude <= sphereRadius)
        {
            Forcenormal = vecPtoS.normalized;
        }
        return Forcenormal;
    }
    Vector3 InnerForce(Vector3 a, Vector3 b, float K, float L)
    {
        Vector3 bToa = b - a;

        var F = -K *
            (L - bToa.magnitude) *
            bToa.normalized;

        return F;
    }
    Vector3 GravityForce(float gravity)
    {
        return new Vector3(0, -gravity, 0);
    }
    Vector3 ViscousFluidForce()
    {
        return new Vector3();
    }
    Vector3 WindForce(Vector3 a, int xNodesNum, int yNodesNum, float windForce)
    {
        // there is a wind area at where cloth will be blowed up;
        // middle is 3 - xandy.x, xandy.y - 3 ___ end is 6 - 2 * xandy.x, 2*xandy.y - 6;
        if ((a.x > xNodesNum - 1 - 3
            && a.x < 2 * (xNodesNum - 1) - 6)
            && (a.y > 6 - 2 * (yNodesNum - 1)
            && a.y < yNodesNum - 1 - 3))
        {
            return new Vector3(0, 0, windForce);
        }
        return new Vector3();
    }
    Vector3 WindForceNew(Vector3 point, 
        float windMinX, float windMaxX,
        float windMinY, float windMaxY,
        float windForce)
    {
        // there is a wind area at where cloth will be blowed up;
        // middle is 3 - xandy.x, xandy.y - 3 ___ end is 6 - 2 * xandy.x, 2*xandy.y - 6;
        if ((point.x > windMinX
            && point.x < windMaxX)
            && (point.y > windMinY
            && point.y < windMaxY))
        {
            return new Vector3(0, 0, windForce);
        }
        return new Vector3();
    }
}
