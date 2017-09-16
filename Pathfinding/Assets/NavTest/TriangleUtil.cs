using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleUtil
{
    //判断点是否在三角形内
    public static bool PointInTriangle(Vector3[] triangle, Vector3 point)
    {
        if (triangle.Length != 3)
        {
            return false;
        }

        Vector3 v0 = triangle[2] - triangle[0];
        Vector3 v1 = triangle[1] - triangle[0];
        Vector3 v2 = point - triangle[0];

        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        float inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

        float u = (dot11 * dot02 - dot01 * dot12) * inverDeno;

        if (u < 0 || u > 1)
        {
            return false;
        }

        float v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
        if (v < 0 || v > 1)
        {
            return false;
        }

        return u + v <= 1;
    }

    //取两个三角形的邻边两点
    public static List<Vector3> GetTwoPointsInTwoPath(PathNode p1, PathNode p2)
    {

        NavTriangle n1 = p1.node;
        NavTriangle n2 = p2.node;

        List<Vector3> pointList = new List<Vector3>();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (n1.verts[i] == n2.verts[j])
                {
                    pointList.Add(n1.verts[i]);
                }
            }
        }

        if (pointList.Count == 2)
        {
            //Vector3 v1 = pointList[0] - origin;
            //Vector3 v2 = pointList[1] - origin;

            Vector3 v1 = pointList[0] - p1.node.center;
            Vector3 v2 = pointList[1] - p1.node.center;


            Vector3 m = Vector3.Cross(v1, v2);

            //调成顺时针方向。
            if (m.z > 0)
            {
                Vector3 tempP = pointList[0];
                pointList[0] = pointList[1];
                pointList[1] = tempP;
            }

            //Debug.Log("向量：" + m);


            return pointList;
        }

        return null;
    }

    static void CreateSphere(Vector3 pos, string name)
    {

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.localScale = Vector3.one * 0.3f;
        obj.transform.position = pos;
        obj.transform.parent = GameObject.Find("TurnpointManage").transform;
    }

    public static void ReCreateObj(Vector3 pos, string name)
    {
        GameObject.DestroyImmediate(GameObject.Find(name));
        CreateSphere(pos, name);
    }
}
