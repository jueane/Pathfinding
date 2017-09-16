using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(MeshBoard))]
public class MeshEditor : Editor
{
    //public List<Vector3> PList = new List<Vector3>();

    public GameObject meshObj;

    private MeshBoard mb;

    public static bool isEditing;

    private Transform cur;

    bool isHandle;
    Vector3 hd;
    List<GameObject> selectedList = new List<GameObject>();

    //选中重叠范围
    public static float minRadis = 0.15f;

    void OnEnable()
    {
        mb = (MeshBoard)target;
    }

    void OnDisable()
    {
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        GUI.color = Color.yellow;
        if (isEditing && GUILayout.Button("Edit:On"))
        {
            isEditing = false;
        }

        GUI.color = Color.white;
        if (isEditing == false && GUILayout.Button("Edit:Off"))
        {
            GUI.color = Color.white;
            isEditing = true;
        }

    }

    void OnSceneGUI()
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);


        Event e = Event.current;
        //if (e.type == EventType.mouseUp && e.button == 0)
        //{
        //    if (e.control)
        //    {
        //        Debug.Log(1);
        //    }
        //}
        //if (e.control && e.type == EventType.keyUp && e.keyCode == KeyCode.Q)
        //{
        //    Debug.Log(2);
        //}

        //if (e.type == EventType.mouseUp && e.shift && e.button == 1)
        //{
        //    Debug.Log("redraw");
        //}

        mb.Init();

        //增加顶点（必须先取到所有顶点）
        if (isEditing && e.type == EventType.mouseDown && e.button == 0)
        {
            Ray r = UnityEditor.HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 posRes = new Vector3(r.origin.x, r.origin.y, 0);

            posRes = GetOverlapPoint(posRes);


            //设置当前点
            if (e.control || cur == null)
            {
                cur = new GameObject().transform;
                cur.transform.parent = mb.transform;
                cur.name = "MeshChild";
            }
            //新建顶点
            GameObject obj = new GameObject();
            obj.name = "p" + cur.transform.childCount;
            obj.transform.position = posRes;
            obj.transform.parent = cur;
        }

        //调整位置
        if (isEditing == false && e.control && e.type == EventType.mouseDown && e.button == 0)
        {
            //Debug.Log("选中");
            Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 posRes = new Vector3(r.origin.x, r.origin.y, 0);
            Vector3 overPos = GetOverlapPoint(posRes);
            hd = overPos;
            if (overPos == posRes)
            {
                isHandle = false;
                Tools.current = Tool.None;
            }
            else
            {
                isHandle = true;
            }
        }
        if (isHandle)
        {
            Vector3 diff = Handles.PositionHandle(hd, Quaternion.identity) - hd;
            List<GameObject> objList = GetOverlapPoints(hd);
            hd += diff;            
            AdjustObjsPosition(objList, diff);
        }

        //绘制网格
        RedrawNavMesh();

        //if (isEditing)
        //{
        //    Tools.current = Tool.None;
        //}

        //取消选中锁定
        //if (!isEditing)
        //{
        //    if (e.type == EventType.mouseUp && e.button == 0)
        //    {
        //        Selection.activeObject = null;
        //    }
        //}

        HandleUtility.AddDefaultControl(-1);
        HandleUtility.Repaint();
    }

    //绘制网格
    private void RedrawNavMesh()
    {
        //遍历顶点
        GameObject obj = mb.gameObject;
        int count = obj.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = obj.transform.GetChild(i);

            int countG = child.childCount;
            Vector3[] verts = new Vector3[countG];

            List<Vector3> posList = new List<Vector3>();
            for (int j = 0; j < countG; j++)
            {
                Vector3 pos = child.GetChild(j).transform.position;
                posList.Add(pos);

                //标记顶点
                Handles.color = Color.red * 0.7f;
                Handles.CircleCap(0, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * minRadis);
                Handles.color = Color.white;
            }

            //创建网格
            CreateMesh(child, posList);
        }
    }

    //创建网格
    private void CreateMesh(Transform obj, List<Vector3> posList)
    {

        MeshFilter mf = obj.gameObject.GetComponent<MeshFilter>();
        if (mf == null)
        {
            obj.gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer mr = obj.gameObject.GetComponent<MeshRenderer>();
        if (mf == null)
        {
            obj.gameObject.AddComponent<MeshRenderer>();
        }

        for (int i = 0; i < posList.Count; i++)
        {
            posList[i] = obj.InverseTransformPoint(posList[i]);
        }

        Mesh mesh = new Mesh();

        //顶点必须是localPosition;
        mesh.vertices = posList.ToArray();

        List<int> intList = new List<int>();
        for (int i = 0; i < mesh.vertices.Length - 2; i++)
        {
            intList.Add(0);
            intList.Add(i + 1);
            intList.Add(i + 2);
        }

        if (intList.Count >= 3)
        {
            int[] intArr = new int[intList.Count];
            for (int i = 0; i < intArr.Length; i++)
            {
                intArr[i] = intList[i];
            }

            mesh.triangles = intArr;

        }

        obj.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        obj.gameObject.GetComponent<MeshRenderer>().material = mb.mat;
    }

    //取重合点
    private Vector3 GetOverlapPoint(Vector3 src)
    {
        List<Vector3> vertsList = mb.vertsList;

        for (int i = 0; i < vertsList.Count; i++)
        {
            if (Vector3.Distance(vertsList[i], src) < HandleUtility.GetHandleSize(src) * minRadis)
            {
                return vertsList[i];
            }
        }
        return src;
    }

    //取重合点对象列表
    private List<GameObject> GetOverlapPoints(Vector3 src)
    {
        List<GameObject> overlapList = new List<GameObject>();
        //遍历顶点
        GameObject obj = mb.gameObject;
        int count = obj.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = obj.transform.GetChild(i);

            int countG = child.childCount;
            for (int j = 0; j < countG; j++)
            {
                Vector3 pos = child.GetChild(j).transform.position;
                if (Vector3.Distance(pos, src) < HandleUtility.GetHandleSize(src) * minRadis)
                {
                    overlapList.Add(child.GetChild(j).gameObject);
                }
            }
        }
        return overlapList;
    }

    private void AdjustObjsPosition(List<GameObject> pointsList, Vector3 diff)
    {
        for (int i = 0; i < pointsList.Count; i++)
        {
            pointsList[i].transform.position += diff;
        }
    }
}
