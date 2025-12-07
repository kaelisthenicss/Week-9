using UnityEngine;

public class Box : MonoBehaviour
{
    private Material mat;
    private float size;
    private Vector2 pos;
    
    public CubeDimensions BoundingBox { get; private set; }

    public void DrawBox(Material material, float zPos, float shapeSize, Vector2 shapePos)
    {
        mat = material;
        size = shapeSize;
        pos = shapePos;
        
        // collision: calculate AABB
        var halfSize = size * 0.5f;
        BoundingBox = new CubeDimensions()
        {
            minX = shapePos.x - halfSize,
            minY = shapePos.y - halfSize,
            minZ = zPos - halfSize,
            maxX = shapePos.x + halfSize,
            maxY = shapePos.y + halfSize,
            maxZ = zPos + halfSize,
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

        var frontSquare = GetSquare(size, pos);
        var backSquare = GetSquare(size, pos);

        // calculate perspective scale only based on the face Z position
        var frontZScale = PerspectiveCamera.Instance.GetPerspective(zPos + halfSize);
        var backZScale = PerspectiveCamera.Instance.GetPerspective(zPos - halfSize);

        var computedFront = RenderSquare(frontSquare, frontZScale);
        var computedBack = RenderSquare(backSquare, backZScale);

        // draw connecting edges
        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(computedFront[i]);
            GL.Vertex(computedBack[i]);
        }

        GL.End();
        GL.PopMatrix();
    }
    
    private Vector2[] GetSquare(float s, Vector2 position)
    {
        var halfS = s * 0.5f;
        var faceArray = new Vector2[]
        {
            new Vector2 (halfS, halfS),
            new Vector2 (-halfS, halfS),
            new Vector2 (-halfS, -halfS),
            new Vector2 (halfS, -halfS),
        };

        for(var i = 0; i < faceArray.Length; i++)
        {
            // apply position translation
            faceArray[i] = new Vector2(position.x + faceArray[i].x, position.y + faceArray[i].y);
        }
        return faceArray;
    }

    private Vector2[] RenderSquare(Vector2[] elements, float perspective)
    {
        var computedShape = new Vector2[elements.Length];
        for(var i = 0; i < elements.Length; i++)
        {
            computedShape[i] = elements[i] * perspective;
            
            // draw face perimeter edges
            GL.Vertex(computedShape[i]);
            GL.Vertex(elements[(i + 1) % elements.Length] * perspective);
        }
        return computedShape;
    }

    public struct CubeDimensions
    {
        public float minX;
        public float minY;
        public float minZ;
        
        public float maxX;
        public float maxY;
        public float maxZ;

        // AABB collision check
        public bool Collide(CubeDimensions otherCube)
        {
            bool overlapX = (minX <= otherCube.maxX && maxX >= otherCube.minX);
            bool overlapY = (minY <= otherCube.maxY && maxY >= otherCube.minY);
            bool overlapZ = (minZ <= otherCube.maxZ && maxZ >= otherCube.minZ);
            
            return overlapX && overlapY && overlapZ;
        }
    }
}