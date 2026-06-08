using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RuntimeBoxFrame : MonoBehaviour
{
    public BoxCollider2D box;
    private LineRenderer line;

    void Start()
    {

        line = GetComponent<LineRenderer>();


        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 4;

        UpdateFrame();
    }

    void UpdateFrame()
    {
        Vector2 size = box.size * 0.5f;
        Vector2 offset = box.offset;


        Vector3[] points = new Vector3[4];
        points[0] = new Vector3(offset.x - size.x, offset.y - size.y, 0);
        points[1] = new Vector3(offset.x - size.x, offset.y + size.y, 0);
        points[2] = new Vector3(offset.x + size.x, offset.y + size.y, 0);
        points[3] = new Vector3(offset.x + size.x, offset.y - size.y, 0);

        line.SetPositions(points);
    }
}
