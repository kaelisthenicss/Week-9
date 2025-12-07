using UnityEngine;

public class Cylinder : MonoBehaviour
{
    private Material mat;
    private float radius;
    private float height;
    private Vector2 pos;
    private float zpos;
    private Vector3 rotation;

    public void DrawCylinder(Material material, float zPos, float shapeRadius, float shapeHeight, Vector2 shapePos, Vector3 shapeRot, int segments = 32)
    {
        mat = material;
        radius = shapeRadius;
        height = shapeHeight;
        pos = shapePos;
        rotation = shapeRot;
        zpos = zPos;

        if (mat == null)
        {
            Debug.LogError("Missing material");
            return;
        }

        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.LINES);

        var halfHeight = height / 2f;
        var center = new Vector3(pos.x, pos.y, zpos);

        // pre-calculate rotation radians
        var xRad = rotation.x * Mathf.Deg2Rad;
        var yRad = rotation.y * Mathf.Deg2Rad;
        var zRad = rotation.z * Mathf.Deg2Rad;

        // draw top and bottom circles
        DrawCircle(center, halfHeight, segments, xRad, yRad, zRad);

        // connect edges between top and bottom
        for (var i = 0; i < segments; i++)
        {
            var angle = (i / (float)segments) * Mathf.PI * 2f;

            // calculate the initial 3D points relative to the center (0, 0, 0)
            Vector3 bottomP = new Vector3(Mathf.Cos(angle) * radius, -halfHeight, Mathf.Sin(angle) * radius);
            Vector3 topP = new Vector3(Mathf.Cos(angle) * radius, halfHeight, Mathf.Sin(angle) * radius);

            // apply rotation (relative to center)
            bottomP = RotatePoint(bottomP, xRad, yRad, zRad);
            topP = RotatePoint(topP, xRad, yRad, zRad);

            // apply translation (move to world position)
            bottomP += center;
            topP += center;

            // apply perspective projection
            var scaleBottom = PerspectiveCamera.Instance.GetPerspective(bottomP.z);
            var scaleTop = PerspectiveCamera.Instance.GetPerspective(topP.z);

            // apply scaling to x and y coordinates (projection)
            bottomP.x *= scaleBottom;
            bottomP.y *= scaleBottom;

            topP.x *= scaleTop;
            topP.y *= scaleTop;

            // draw the vertical line segment
            GL.Vertex(bottomP);
            GL.Vertex(topP);
        }

        GL.End();
        GL.PopMatrix();
    }

    private void DrawCircle(Vector3 center, float halfHeight, int segments, float xRad, float yRad, float zRad)
    {
        for (var i = 0; i < segments; i++)
        {
            var angle1 = (i / (float)segments) * Mathf.PI * 2f;
            var angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2f;

            var p1_base = new Vector3(Mathf.Cos(angle1) * radius, halfHeight, Mathf.Sin(angle1) * radius);
            var p2_base = new Vector3(Mathf.Cos(angle2) * radius, halfHeight, Mathf.Sin(angle2) * radius);
            
            var p3_base = new Vector3(Mathf.Cos(angle1) * radius, -halfHeight, Mathf.Sin(angle1) * radius);
            var p4_base = new Vector3(Mathf.Cos(angle2) * radius, -halfHeight, Mathf.Sin(angle2) * radius);
            
            // top face
            DrawProjectedLine(p1_base, p2_base, center, xRad, yRad, zRad);
            
            // bot face
            DrawProjectedLine(p3_base, p4_base, center, xRad, yRad, zRad);
        }
    }

    private void DrawProjectedLine(Vector3 p1_base, Vector3 p2_base, Vector3 center, float xRad, float yRad, float zRad)
    {
        // rotate
        var p1 = RotatePoint(p1_base, xRad, yRad, zRad);
        var p2 = RotatePoint(p2_base, xRad, yRad, zRad);

        // translate
        p1 += center;
        p2 += center;

        // project (Get scale and apply to x/y)
        var scale1 = PerspectiveCamera.Instance.GetPerspective(p1.z);
        var scale2 = PerspectiveCamera.Instance.GetPerspective(p2.z);

        p1.x *= scale1;
        p1.y *= scale1;

        p2.x *= scale2;
        p2.y *= scale2;
        
        // draw
        GL.Vertex(p1);
        GL.Vertex(p2);
    }
    
    private Vector3 RotatePoint(Vector3 p, float xRad, float yRad, float zRad)
    {
        // z rotation
        var x = p.x * Mathf.Cos(zRad) - p.y * Mathf.Sin(zRad);
        var y = p.y * Mathf.Cos(zRad) + p.x * Mathf.Sin(zRad);
        var z = p.z;
        p = new Vector3(x, y, z);

        // x rotation
        y = p.y * Mathf.Cos(xRad) - p.z * Mathf.Sin(xRad);
        z = p.y * Mathf.Sin(xRad) + p.z * Mathf.Cos(xRad);
        p = new Vector3(p.x, y, z);

        // y rotation
        x = p.x * Mathf.Cos(yRad) + p.z * Mathf.Sin(yRad);
        z = -p.x * Mathf.Sin(yRad) + p.z * Mathf.Cos(yRad);
        p = new Vector3(x, p.y, z);

        return p;
    }
}