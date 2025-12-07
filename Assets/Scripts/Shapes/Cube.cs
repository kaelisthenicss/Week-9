using UnityEngine;

public class Cube : MonoBehaviour
{
    private Material mat;
    private float size;
    private Vector2 pos;
    private float zpos;
    private Vector3 rotation;
    
    public void DrawCube(Material material, float zPos,float shapeSize, Vector2 shapePos, Vector3 shapeRot)
    {
        mat = material;
        size = shapeSize;
        pos = shapePos;
        rotation = shapeRot;
        zpos = zPos;
        
        if (mat == null)
        {
            Debug.LogError("Missing material");
            return;
        }
        GL.PushMatrix();

        GL.Begin(GL.LINES);
        mat.SetPass(0);

        var frontSquare = GetCube();
        var frontZ = zpos + size * .5f;
        var backSquare = GetCube();
        var backZ = zpos - size * .5f;
        
        var zFrontValues = new float[4];
        var zBackValues  = new float[4];

        for (var i = 0; i < 4; i++)
        {
            zFrontValues[i] = frontZ;  
            zBackValues[i]  = backZ; 
        }
        
        RotateShape(ref frontSquare, zFrontValues);
        RotateShape(ref backSquare, zBackValues);

        var computedFront = RenderSquare(frontSquare, zFrontValues);
        var computedBack = RenderSquare(backSquare, zBackValues);

        for (var i = 0; i < 4; i++)
        {
            GL.Vertex(computedFront[i]);
            GL.Vertex(computedBack[i]);
        }

        GL.End();
        GL.PopMatrix();
    }

    private Vector2[] GetCube()
    {
        var halfSize = size * 0.5f;

        var faceArray = new Vector2[]
        {
            new Vector2 (halfSize, halfSize),
            new Vector2 (-halfSize, halfSize),
            new Vector2 (-halfSize, -halfSize),
            new Vector2 (halfSize, -halfSize),
        };

        return faceArray;
    }

    private Vector2[] RenderSquare(Vector2[] pts, float[] zs)
    {
        var computed = new Vector2[pts.Length];

        for (var i = 0; i < pts.Length; i++)
        {
            var p = PerspectiveCamera.Instance.GetPerspective(zs[i]);
            computed[i] = pts[i] * p;

            var next = (i + 1) % pts.Length;

            var pNext = PerspectiveCamera.Instance.GetPerspective(zs[next]);

            GL.Vertex(computed[i]);
            GL.Vertex(pts[next] * pNext);
        }

        return computed;
    }

    private void RotateShape(ref Vector2[] pts, float[] zs)
    {
        var xRad = rotation.x * Mathf.Deg2Rad;
        var yRad = rotation.y * Mathf.Deg2Rad;
        var zRad = rotation.z * Mathf.Deg2Rad;
        
        var center = new Vector3(pos.x, pos.y, zpos);
        
        for (var i = 0; i < pts.Length; i++)
        {
            // temporary Vector3 for rotation
            var p = new Vector3(
                pts[i].x, 
                pts[i].y, 
                zs[i] - center.z     
            );

            // z rot
            var x = p.x * Mathf.Cos(zRad) - p.y * Mathf.Sin(zRad);
            var y = p.y * Mathf.Cos(zRad) + p.x * Mathf.Sin(zRad);
            var z = p.z;
            p = new Vector3(x, y, z);

            // x rot
            y = p.y * Mathf.Cos(xRad) - p.z * Mathf.Sin(xRad);
            z = p.y * Mathf.Sin(xRad) + p.z * Mathf.Cos(xRad);
            p = new Vector3(p.x, y, z);

            // y rot
            x = p.x * Mathf.Cos(yRad) + p.z * Mathf.Sin(yRad);
            z = -p.x * Mathf.Sin(yRad) + p.z * Mathf.Cos(yRad);
            p = new Vector3(x, p.y, z);

            // convert back
            pts[i] = new Vector2(p.x + center.x, p.y + center.y);
            zs[i] = p.z + center.z; 
        }
    }
}
