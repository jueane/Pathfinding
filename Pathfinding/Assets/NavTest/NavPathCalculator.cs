using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class NavPathCalculator
{
    //是否显示
    public bool showProcessPath = false;

    MeshBoard mb;

    NavTriangle startVertex;
    NavTriangle endVertex;

    List<PathNode> closeList;

    //最终中心路径
    List<PathNode> pathList = new List<PathNode>();

    public NavPathCalculator(MeshBoard mb)
    {
        this.mb = mb;
    }

    public List<PathNode> FindCenterPointPath(Vector3 start, Vector3 end)
    {
        //标记开始结束点
        startVertex = FindNodeByPoint(start);
        endVertex = FindNodeByPoint(end);

        if (startVertex == null && endVertex == null)
        {
            return null;
        }
        else if (startVertex == null)
        {
            startVertex = FindNearestNode(start);
        }
        else if (endVertex == null)
        {
            endVertex = FindNearestNode(end);
        }

        PathNode startNode = new PathNode();
        startNode.node = startVertex;
        PathNode endNode = new PathNode();
        endNode.node = endVertex;

        //如果起点和终点在同一个三角形中，则返回一个节点
        if (startVertex.center == endVertex.center)
        {
            pathList.Clear();
            pathList.Add(startNode);
            return pathList;
        }
        //如果起点和终点在两个相邻的三角形中，则返回二个节点
        else if (TriangleUtil.GetTwoPointsInTwoPath(startNode, endNode) != null)
        {
            pathList.Clear();
            pathList.Add(startNode);
            pathList.Add(endNode);
            return pathList;
        }

        List<PathNode> openList = new List<PathNode>();

        closeList = new List<PathNode>();


        PathNode path = new PathNode();
        path.node = startVertex;
        path.dis = 0;

        closeList.Add(path);

        FindNextPath(path, endVertex, openList, closeList);

        //记录并显示路径
        if (showProcessPath)
        {
            ShowProcessPath();
        }
        else
        {
            ShowCenterPath();
        }
        return pathList;
    }

    void FindNextPath(PathNode start, NavTriangle endNode, List<PathNode> openList, List<PathNode> closeList)
    {

        for (int i = 0; i < 3; i++)
        {
            if (start == null)
            {
                Debug.Log("是空的");
            }
            if (start.node.nodeArr[i] == null || IsCloseListContains(start.node.nodeArr[i]))
            {
                continue;
            }
            float g = start.dis + start.node.dis[i];
            float h = Vector3.Distance(start.node.nodeArr[i].center, endNode.center);
            float f = g + h;
            PathNode path = new PathNode();
            path.node = start.node.nodeArr[i];
            path.dis = f;
            path.parent = start;

            openList.Add(path);
        }

        //取最小F节点，加入闭合列表
        for (int i = 0; i < openList.Count; i++)
        {
            for (int j = i + 1; j < openList.Count; j++)
            {
                if (openList[i].dis > openList[j].dis)
                {
                    PathNode path = openList[i];
                    openList[i] = openList[j];
                    openList[j] = path;
                }
            }
        }

        //如果最小路径是错误的路径，则标记错误。

        PathNode curPath = null;

        if (openList.Count > 0)
        {
            curPath = openList[0];

            openList.RemoveAt(0);
            closeList.Add(curPath);

            //将结束点加入闭合列表中
            if (openList == null)
            {
                Debug.Log("是空的");
            }
            if (curPath.node.center == endNode.center)
            {
                //PathNode path = new PathNode();
                //path.node = endNode;
                //path.parent = curPath;
                //closeList.Add(path);
                return;
            }
        }

        FindNextPath(curPath, endNode, openList, closeList);

    }

    //找到所在节点
    NavTriangle FindNodeByPoint(Vector3 pos)
    {
        for (int i = 0; i < mb.triangleList.Count; i++)
        {
            bool inside = TriangleUtil.PointInTriangle(mb.triangleList[i].verts, pos);
            if (inside)
            {
                return mb.triangleList[i];
            }
        }
        //没有返回说明没在导航区域内，则找距离最近的三角形
        return null;
    }

    //找到最近的节点
    NavTriangle FindNearestNode(Vector3 pos)
    {
        int iCur = 0;
        float dis;
        if (mb == null)
        {
            Debug.Log("是空的");
        }
        dis = float.MaxValue;

        for (int i = 0; i < mb.triangleList.Count; i++)
        {
            float disTemp = Vector3.Distance(pos, mb.triangleList[i].center);
            if (disTemp < dis)
            {
                iCur = i;
                dis = disTemp;
            }
        }
        return mb.triangleList[iCur];
    }

    //打印中心点路径
    void ShowCenterPath()
    {
        int a = 0;
        Debug.DrawLine(startVertex.center, closeList[0].node.center, Color.green);

        pathList.Clear();
        PathNode tempPath;
        tempPath = closeList[closeList.Count - 1];
        while (tempPath != null)
        {
            pathList.Add(tempPath);
            if (tempPath.parent != null)
            {
                tempPath = tempPath.parent;
            }
            else
            {
                tempPath = null;
            }
            a++;
            if (a > 50)
            {
                break;
            }
        }

        pathList.Reverse();

        for (int i = 0; i < pathList.Count - 1; i++)
        {
            Debug.DrawLine(pathList[i].node.center, pathList[i + 1].node.center, Color.yellow * 0.5f);
        }


    }

    //打印过程路径
    void ShowProcessPath()
    {
        for (int i = 0; i < closeList.Count - 1; i++)
        {
            Debug.DrawLine(closeList[i].node.center, closeList[i + 1].node.center, Color.green);
        }
    }

    bool IsCloseListContains(NavTriangle navNode)
    {
        for (int i = 0; i < closeList.Count; i++)
        {
            if (closeList[i].node.center == navNode.center)
            {
                return true;
            }
        }

        return false;
    }

}
