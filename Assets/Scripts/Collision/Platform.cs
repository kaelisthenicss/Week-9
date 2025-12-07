using UnityEngine;

public class Platform : MonoBehaviour
{
    public Box.CubeDimensions BoundingBox { get; private set; }
    
    private Material mat;
    private float width;
    private float height;
    private float depth; 

    public void DrawPlatform(Material material, float shapeWidth, float shapeHeight, float shapeDepth, Vector2 shapePos, float zPos)
    {
        mat = material;
        width = shapeWidth;
        height = shapeHeight;
        depth = shapeDepth; 

        // collision: calculate AABB
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;
        var halfDepth = depth * 0.5f;
        var center = new Vector3(shapePos.x, shapePos.y, zPos);

        BoundingBox = new Box.CubeDimensions()
        {
            minX = shapePos.x - halfWidth,
            minY = shapePos.y - halfHeight,
            minZ = zPos - halfDepth,
            maxX = shapePos.x + halfWidth,
            maxY = shapePos.y + halfHeight,
            maxZ = zPos + halfDepth,
        };

        if (mat == null)
        {
            Debug.LogError("Missing material");
            return;
        }
        
        // start draw
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        mat.SetPass(0);

        // define the 8 vertices relative to its center (0, 0, 0)
        var vertices = new Vector3[]
        {
            // front face
            new Vector3( halfWidth,  halfHeight,  halfDepth), // 0
            new Vector3(-halfWidth,  halfHeight,  halfDepth), // 1
            new Vector3(-halfWidth, -halfHeight,  halfDepth), // 2
            new Vector3( halfWidth, -halfHeight,  halfDepth), // 3
            
            // back face 
            new Vector3( halfWidth,  halfHeight, -halfDepth), // 4
            new Vector3(-halfWidth,  halfHeight, -halfDepth), // 5
            new Vector3(-halfWidth, -halfHeight, -halfDepth), // 6
            new Vector3( halfWidth, -halfHeight, -halfDepth)  // 7
        };

        // array to store the final projected 2D points
        var computedVertices = new Vector2[8];
        
        // translation and perspective projection
        for (var i = 0; i < 8; i++)
        {
            // translate to world position
            var p = vertices[i] + center;

            // get perspective scale from z-depth
            var scale = PerspectiveCamera.Instance.GetPerspective(p.z);
            
            // project x and y
            computedVertices[i] = new Vector2(p.x * scale, p.y * scale);
        }
        
        // draw 4 edges of the front face (0-1-2-3-0)
        DrawFace(computedVertices, 0, 1, 2, 3);
        
        // draw 4 edges of the back face (4-5-6-7-4)
        DrawFace(computedVertices, 4, 5, 6, 7);
        
        // draw 4 connecting edges (0-4, 1-5, 2-6, 3-7)
        for (var i = 0; i < 4; i++)
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
}