using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnPointCalculator
{

    public List<PathNode> pathList;

    //拐点集合
    List<Vector3> turnPointList = new List<Vector3>();

    public Vector3 origin;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public Vector3 p4;

    //计算拐点路径
    public List<Vector3> CalculateBestPath(List<PathNode> originPathList, Vector3 start, Vector3 end)
    {
        if (originPathList == null)
        {
            return null;
        }

        turnPointList.Clear();

        pathList = originPathList;

        //节点小于3则不需要计算。
        if (pathList.Count < 3)
        {
            return null;
        }


        List<Vector3> twoPoint =TriangleUtil.GetTwoPointsInTwoPath(pathList[0], pathList[1]);
        p1 = twoPoint[0];
        p2 = twoPoint[1];

        origin = start;

        turnPointList.Add(origin);

        for (int i = 0; i < pathList.Count - 2; i++)
        {
            //取下一组路过边的两点
            List<Vector3> twoPoint2 = TriangleUtil.GetTwoPointsInTwoPath(pathList[i + 1], pathList[i + 2]);
            p3 = twoPoint2[0];
            p4 = twoPoint2[1];

            CalculateTurnPoint();

            //计算最后一个三角形是否存在拐点
            if (i == pathList.Count - 3)
            {
                CalculateFinalTurnPoint(end);
            }
        }

        return turnPointList;
    }

    //计算拐点
    void CalculateTurnPoint()
    {
        Vector3 v1 = p1 - origin;
        Vector3 v2 = p2 - origin;
        Vector3 v3 = p3 - origin;
        Vector3 v4 = p4 - origin;

        Vector3 c13 = Vector3.Cross(v1, v3);
        Vector3 c14 = Vector3.Cross(v1, v4);

        Vector3 c23 = Vector3.Cross(v2, v3);
        Vector3 c24 = Vector3.Cross(v2, v4);

        Vector3 c12 = Vector3.Cross(v1, v2);
        Vector3 c34 = Vector3.Cross(v3, v4);



        //v4超过v1
        if (c14.z > 0)
        {
            //Debug.Log("左超：" + turnPointList.Count);

            turnPointList.Add(p1);
            origin = p1;

            //根据当前origin取v1,v2;
            List<Vector3> twoP = GetV1V2();
            p1 = twoP[0];
            p2 = twoP[1];

            CalculateTurnPoint();
            return;
        }

        //v3超过v2
        if (c23.z < 0)
        {
            //Debug.Log("右超：" + turnPointList.Count);

            turnPointList.Add(p2);
            origin = p2;

            //根据当前origin取v1,v2;
            List<Vector3> twoP = GetV1V2();
            p1 = twoP[0];
            p2 = twoP[1];

            CalculateTurnPoint();
            return;
        }

        //左不超
        if (c13.z < 0)
        {
            //Debug.Log("左不超");
            p1 = p3;
        }

        if (c24.z > 0)
        {
            //Debug.Log("右不超");
            p2 = p4;
        }

    }

    //计算最后一个三角形是否存在拐点
    void CalculateFinalTurnPoint(Vector3 end)
    {
        //判断最后一个拐点是否最后一个三角形的顶点。
        for (int i = 0; i < 3; i++)
        {

            //Debug.Log("三个位置：" + pathList[pathList.Count - 1].node.verts[i]);
            if (turnPointList[turnPointList.Count - 1] == pathList[pathList.Count - 1].node.verts[i])
            {
                turnPointList.Add(end);
                return;
            }
        }

        //如果不是，则说明还存在一个拐点

        Vector3 v1 = p1 - origin;
        Vector3 v2 = p2 - origin;
        Vector3 v3 = p3 - origin;
        Vector3 v4 = p4 - origin;
        Vector3 vE = end - origin;

        Vector3 c13 = Vector3.Cross(v1, v3);
        Vector3 c14 = Vector3.Cross(v1, v4);

        Vector3 c23 = Vector3.Cross(v2, v3);
        Vector3 c24 = Vector3.Cross(v2, v4);

        Vector3 c12 = Vector3.Cross(v1, v2);
        Vector3 c34 = Vector3.Cross(v3, v4);

        Vector3 c1E = Vector3.Cross(v1, vE);
        Vector3 c2E = Vector3.Cross(v2, vE);


        //Debug.Log("两边情况：" + c1E + "," + c2E);

        if (c1E.z > 0)
        {
            //Debug.Log("左边");
            turnPointList.Add(p1);
            turnPointList.Add(end);
        }
        else if (c2E.z < 0)
        {
            //Debug.Log("右边");
            turnPointList.Add(p2);
            turnPointList.Add(end);
        }
        else
        {
            //Debug.Log("中间");
            turnPointList.Add(end);
        }
    }

    //添加拐点后，取距离拐点最近的v1v2
    List<Vector3> GetV1V2()
    {
        //原理：遍历当前origin所在的最后一个三角形的三个顶点。即可取出需要计算的下一对v1v2.
        List<Vector3> ptwo = new List<Vector3>();
        for (int i = 0; i < pathList.Count; i++)
        {
            int cur = pathList.Count - 1 - i;
            Vector3[] verts = pathList[cur].node.verts;
            for (int j = 0; j < 3; j++)
            {
                if (verts[j] == origin)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (origin != verts[k])
                        {
                            ptwo.Add(verts[k]);
                        }
                    }

                    //排序
                    Vector3 m1 = ptwo[0] - origin;
                    Vector3 m2 = ptwo[1] - origin;

                    if (Vector3.Cross(m1, m2).z > 0)
                    {
                        Vector3 pTemp = ptwo[0];
                        ptwo[0] = ptwo[1];
                        ptwo[1] = pTemp;
                    }
                    return ptwo;
                }

            }

        }

        return null;
    }

}
