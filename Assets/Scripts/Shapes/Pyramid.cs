using UnityEngine;

public class Pyramid : MonoBehaviour
{
    private Material mat;
    private float size; 
    private Vector2 pos;
    private float zpos;
    private Vector3 rotation;
    
    public void DrawPyramid(Material material, float shapeSize, Vector2 shapePos, float zPos, Vector3 shapeRot)
    {
        mat = material;
        size = shapeSize;
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

        var halfSize = size * 0.5f;
        var center = new Vector3(pos.x, pos.y, zpos);

        // pre-calculate rotation radians
        var xRad = rotation.x * Mathf.Deg2Rad;
        var yRad = rotation.y * Mathf.Deg2Rad;
        var zRad = rotation.z * Mathf.Deg2Rad;

        // define 3D vertices relative to the pyramid's center (0, 0, 0)
        // apex is at (0, halfSize, 0)
        var apex = new Vector3(0, halfSize, 0); 
        
        // base points are on the xz plane at y = -halfSize
        var basePoints = new Vector3[]
        {
            new Vector3( halfSize, -halfSize,  halfSize), 
            new Vector3(-halfSize, -halfSize,  halfSize), 
            new Vector3(-halfSize, -halfSize, -halfSize), 
            new Vector3( halfSize, -halfSize, -halfSize)  
        };
        
        // process and project the apex
        var rotatedApex = RotatePoint(apex, xRad, yRad, zRad);
        rotatedApex += center;
        var apexScale = PerspectiveCamera.Instance.GetPerspective(rotatedApex.z);
        var computedApex = new Vector2(rotatedApex.x * apexScale, rotatedApex.y * apexScale);


        // process and project the base and side edges ---
        var computedBase = new Vector2[4];
        
        for (var i = 0; i < 4; i++)
        {
            // rotate and translate the current base point
            var p1 = RotatePoint(basePoints[i], xRad, yRad, zRad);
            p1 += center;

            // project current base point
            var scale1 = PerspectiveCamera.Instance.GetPerspective(p1.z);
            var p1_projected = new Vector2(p1.x * scale1, p1.y * scale1);
            computedBase[i] = p1_projected;

            // side edges 
            GL.Vertex(computedApex);
            GL.Vertex(p1_projected);
            
            // base edges
            var nextIndex = (i + 1) % 4;
            
            // get the next base point, rotate, translate
            var p2 = RotatePoint(basePoints[nextIndex], xRad, yRad, zRad);
            p2 += center;

            // project the next base point
            var scale2 = PerspectiveCamera.Instance.GetPerspective(p2.z);
            var p2_projected = new Vector2(p2.x * scale2, p2.y * scale2);

            GL.Vertex(p1_projected);
            GL.Vertex(p2_projected);
        }

        GL.End();
        GL.PopMatrix();
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