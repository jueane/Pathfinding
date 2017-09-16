using UnityEngine;
using System.Collections;


//输入：三个顶点，三个节点
public class NavTriangle
{
    //三个顶点
    public Vector3[] verts = new Vector3[3];

    //重心点。
    public Vector3 center;
    //重心标志
    public GameObject centerObj;

    //与三边相邻的三个节点
    public NavTriangle[] nodeArr = new NavTriangle[3];
    
    //三个方向的距离
    public float[] dis = new float[3];

    public NavTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        //三个顶点
        verts[0] = pointA;
        verts[1] = pointB;
        verts[2] = pointC;

        //中心点
        center.x = (verts[0].x + verts[1].x + verts[2].x) / 3;
        center.y = (verts[0].y + verts[1].y + verts[2].y) / 3;
        center.z = (verts[0].z + verts[1].z + verts[2].z) / 3;
    }
    
}
