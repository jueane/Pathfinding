using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBoard : MonoBehaviour
{
    public Material mat;

    public List<Vector3> vertsList = new List<Vector3>();

    public List<NavTriangle> triangleList = new List<NavTriangle>();

    public Transform a;
    public Transform b;

    NavPathCalculator pathCalc;

    TurnPointCalculator turnPointCalc;

    // Use this for initialization
    void Start()
    {
        pathCalc = new NavPathCalculator(this);
        turnPointCalc = new TurnPointCalculator();
    }

    // Update is called once per frame
    void Update()
    {
        Init();
        FindPath(a.position, b.position);
    }

    public void Init()
    {
        //设置所有顶点
        SetVertexList();

        //初始化所有三角形
        InitTriangleList();
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        //寻找路径
        List<PathNode> pathList = pathCalc.FindCenterPointPath(start, end);

        List<Vector3> bestPathList = null;
        //节点小于3不计算
        if (pathList == null || pathList.Count < 3)
        {
            bestPathList = new List<Vector3>();
            bestPathList.Add(start);
            bestPathList.Add(end);
        }
        else
        {
            //计算拐点
            bestPathList = turnPointCalc.CalculateBestPath(pathList, start, end);
        }

        //Debug.Log("拐点：" + turnPointList.Count);
        if (bestPathList.Count >= 2)
        {
            for (int i = 0; i < bestPathList.Count - 1; i++)
            {
                Debug.DrawLine(bestPathList[i], bestPathList[i + 1], Color.green * 0.8f);
            }
        }

        return bestPathList;
    }

    //初始化顶点列表
    void SetVertexList()
    {
        vertsList.Clear();
        triangleList.Clear();

        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3[] verts = new Vector3[child.childCount];
            int countG = child.childCount;

            List<Vector3> posList = new List<Vector3>();
            for (int j = 0; j < countG; j++)
            {
                Vector3 pos = child.GetChild(j).transform.position;
                posList.Add(pos);
            }

            vertsList.AddRange(posList);
            //设置各节点的顶点

            for (int k = 0; k < posList.Count - 2; k++)
            {
                //保存节点信息
                NavTriangle node = new NavTriangle(posList[0], posList[k + 1], posList[k + 2]);

                node.verts = new Vector3[] { posList[0], posList[k + 1], posList[k + 2] };

                if (node == null)
                {
                    print("空节点");
                }
                triangleList.Add(node);
            }

        }
    }

    //初始化三角形相邻关系
    void InitTriangleList()
    {
        for (int i = 0; i < triangleList.Count; i++)
        {
            NavTriangle node = triangleList[i];
            //设置相邻三角形
            Vector3[] verts = node.verts;
            node.nodeArr[0] = GetLinkNode(new Vector3[] { verts[0], verts[1] }, node);
            node.nodeArr[1] = GetLinkNode(new Vector3[] { verts[0], verts[2] }, node);
            node.nodeArr[2] = GetLinkNode(new Vector3[] { verts[1], verts[2] }, node);

            //设置相邻三角形的距离
            for (int j = 0; j < 3; j++)
            {
                if (node.nodeArr[j] != null)
                {
                    node.dis[j] = Vector3.Distance(node.center, node.nodeArr[j].center);
                }
            }
        }
    }

    //是否有共边节点，是的话返回该节点，否则返回空。参数为边
    NavTriangle GetLinkNode(Vector3[] rim, NavTriangle except)
    {
        for (int i = 0; i < triangleList.Count; i++)
        {
            if (triangleList[i].Equals(except))
            {
                continue;
            }
            Vector3[] verts = triangleList[i].verts;

            //共顶点数量
            int count = 0;

            for (int j = 0; j < 3; j++)
            {
                if (verts[j] == rim[0])
                {
                    count++;
                }
                if (verts[j] == rim[1])
                {
                    count++;
                }
            }
            if (count == 2)
            {
                return triangleList[i];
            }
        }
        return null;
    }


}
