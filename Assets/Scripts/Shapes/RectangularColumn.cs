using UnityEngine;

public class RectangularColumn : MonoBehaviour
{
    private Material mat;
    private float width;  
    private float height; 
    private Vector2 pos;
    private float zpos;
    private Vector3 rotation;
    
    public void DrawRectangularColumn(Material material, float shapeWidth, float shapeHeight, Vector2 shapePos, float zPos, Vector3 shapeRot)
    {
        mat = material;
        width = shapeWidth; 
        height = shapeHeight; 
        pos = shapePos;
        zpos = zPos;
        rotation = shapeRot;
        
        if (mat == null)
        {
            Debug.LogError("Missing material");
            return;
        }
        
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        mat.SetPass(0);

        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;
        var center = new Vector3(pos.x, pos.y, zpos);

        // pre-calculate rotation radians
        var xRad = rotation.x * Mathf.Deg2Rad;
        var yRad = rotation.y * Mathf.Deg2Rad;
        var zRad = rotation.z * Mathf.Deg2Rad;

        // define the 8 vertices of the column in 3D space, relative to its center (0, 0, 0)
        Vector3[] vertices = new Vector3[]
        {
            // front face 
            new Vector3( halfWidth,  halfHeight,  halfWidth), 
            new Vector3(-halfWidth,  halfHeight,  halfWidth), 
            new Vector3(-halfWidth, -halfHeight,  halfWidth), 
            new Vector3( halfWidth, -halfHeight,  halfWidth), 
            
            // back face 
            new Vector3( halfWidth,  halfHeight, -halfWidth), 
            new Vector3(-halfWidth,  halfHeight, -halfWidth), 
            new Vector3(-halfWidth, -halfHeight, -halfWidth), 
            new Vector3( halfWidth, -halfHeight, -halfWidth)  
        };

        // array to store the final projected 2D points
        Vector2[] computedVertices = new Vector2[8];

        // process all 8 vertices
        for (int i = 0; i < 8; i++)
        {
            // rotate the point
            Vector3 p = RotatePoint(vertices[i], xRad, yRad, zRad);
            
            // translate the point to world position
            p += center;

            // project (get scale from final z and apply to x/y)
            var scale = PerspectiveCamera.Instance.GetPerspective(p.z);
            computedVertices[i] = new Vector2(p.x * scale, p.y * scale);
        }
        
        // draw 4 edges of the front face (0-1-2-3-0)
        DrawFace(computedVertices, 0, 1, 2, 3);
        
        // draw 4 edges of the back face (4-5-6-7-4)
        DrawFace(computedVertices, 4, 5, 6, 7);
        
        // draw 4 connecting edges (0-4, 1-5, 2-6, 3-7)
        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(computedVertices[i]);
            GL.Vertex(computedVertices[i + 4]);
        }

        GL.End();
        GL.PopMatrix();
    }
    
    private void DrawFace(Vector2[] points, int i0, int i1, int i2, int i3)
    {
        GL.Vertex(points[i0]); GL.Vertex(points[i1]);
        GL.Vertex(points[i1]); GL.Vertex(points[i2]);
        GL.Vertex(points[i2]); GL.Vertex(points[i3]);
        GL.Vertex(points[i3]); GL.Vertex(points[i0]);
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