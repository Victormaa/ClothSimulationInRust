using System.Runtime.InteropServices;
using UnityEngine;
public static class MyLibrary
{
    [DllImport("my_library")]
    public static extern int add(int a, int b);

    [DllImport("my_library")]
    public static extern Vector3 is_inside_sphere_collider(Vector3 point, Vector3 sphere_pos, float sphere_radius);
    [DllImport("my_library")]
    public static extern Vector3 gravity_force(float gravity); 
     [DllImport("my_library")]
    public static extern Vector3 wind_force(Vector3 point,
        float wind_min_x, float wind_max_x, float wind_min_y, float wind_max_y,
        float wind_force);
    [DllImport("my_library")]
    public static extern Vector3 inner_force(Vector3 point1, Vector3 point2, float k, float l);
}
