using UnityEngine;

public class Sphere : MonoBehaviour
{
    private Material mat;
    private float radius;
    private Vector2 pos;
    private float zpos;
    private Vector3 rotation;
    
    public void DrawSphere(Material material, float zPos, float shapeRadius, Vector2 shapePos, Vector3 shapeRot)
    {
        mat = material;
        radius = shapeRadius;
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

        var latitudeSegments = 16;   // horizontal rings
        var longitudeSegments = 16;  // vertical rings

        var center = new Vector3(pos.x, pos.y, zpos);

        // pre-calculate rotation radians
        var xRad = rotation.x * Mathf.Deg2Rad;
        var yRad = rotation.y * Mathf.Deg2Rad;
        var zRad = rotation.z * Mathf.Deg2Rad;

        // loop through latitude (top to bottom)
        for (var lat = 0; lat <= latitudeSegments; lat++)
        {
            var theta = Mathf.PI * lat / latitudeSegments;
            var sinTheta = Mathf.Sin(theta);
            var cosTheta = Mathf.Cos(theta);

            // loop through longitude (around the sphere)
            for (var lon = 0; lon <= longitudeSegments; lon++)
            {
                var phi = 2 * Mathf.PI * lon / longitudeSegments;

                // calculate the initial 3D point relative to the sphere's center (0, 0, zpos)
                var x = radius * sinTheta * Mathf.Cos(phi);
                var y = radius * cosTheta;
                var z = radius * sinTheta * Mathf.Sin(phi);
                
                // apply rotation (relative to center)
                var currentP = RotatePoint(new Vector3(x, y, z), xRad, yRad, zRad);
                
                // apply translation (move to world position)
                currentP += center;

                // apply perspective projection
                var perspective = PerspectiveCamera.Instance.GetPerspective(currentP.z);
                var projected = new Vector3(currentP.x * perspective, currentP.y * perspective, 0);

                // connect longitude lines (horizontal connections)
                if (lon < longitudeSegments)
                {
                    var nextPhi = 2 * Mathf.PI * (lon + 1) / longitudeSegments;
                    
                    var nx = radius * sinTheta * Mathf.Cos(nextPhi);
                    var ny = y;
                    var nz = radius * sinTheta * Mathf.Sin(nextPhi);
                    
                    var nextP = RotatePoint(new Vector3(nx, ny, nz), xRad, yRad, zRad);
                    nextP += center;

                    var nperspective = PerspectiveCamera.Instance.GetPerspective(nextP.z);
                    Vector3 nextProjected = new Vector3(nextP.x * nperspective, nextP.y * nperspective, 0);

                    GL.Vertex(projected);
                    GL.Vertex(nextProjected);
                }

                // connect latitude lines (vertical connections)
                if (lat < latitudeSegments)
                {
                    var nextTheta = Mathf.PI * (lat + 1) / latitudeSegments;
                    var nSinTheta = Mathf.Sin(nextTheta);
                    var nCosTheta = Mathf.Cos(nextTheta);
                    
                    var ny = radius * nCosTheta;
                    var nx2 = radius * nSinTheta * Mathf.Cos(phi);
                    var nz2 = radius * nSinTheta * Mathf.Sin(phi);
                    
                    var nextLatP = RotatePoint(new Vector3(nx2, ny, nz2), xRad, yRad, zRad);
                    nextLatP += center;
                    
                    var nperspective2 = PerspectiveCamera.Instance.GetPerspective(nextLatP.z);
                    var nextLatProj = new Vector3(nextLatP.x * nperspective2, nextLatP.y * nperspective2, 0);

                    GL.Vertex(projected);
                    GL.Vertex(nextLatProj);
                }
            }
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