using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class LineGenCollision : MonoBehaviour
{
    [Header("Shape Settings")]
    public Material defaultMaterial;
    public float shapeSize = 1f;
    public float zPos = 0f;
    public Vector2 shapePos = new Vector2(0, 0);
    
    [Header("References")]
    public Box box;
    public Platform platform; 
    
    [Header("Platform Settings")]
    public float platformWidth = 10f;
    public float platformHeight = 1f;
    public float platformDepth = 2f;
    public Vector2 platformPos = new Vector2(0, -3f);
    public float platformZPos = 0f;

    [Header("Collision Settings")]
    public float jumpHeight = 5f;
    public Material collisionMaterial;
    public Material originalMaterial;
    private bool isJumpTimerActive = false;
    
    [Header("Physics Settings")]
    public float gravity = -9.8f;      
    public float initialYVelocity = 0f; 
    private float currentYVelocity;

    private void Start()
    {
        originalMaterial =  defaultMaterial;
        currentYVelocity = initialYVelocity;
    }

    private void OnPostRender()
    {
        if (box != null)
            box.DrawBox(defaultMaterial, zPos, shapeSize, shapePos);
        else
            Debug.LogError("Box Reference Missing");

        if (platform != null)
            platform.DrawPlatform(originalMaterial, platformWidth, platformHeight, platformDepth, platformPos, platformZPos);
        else
            Debug.LogError("Platform Reference Missing");


        if (box != null && platform != null)
        {
            var boxAABB = box.BoundingBox;
            var platformAABB = platform.BoundingBox;
            
            var currentlyColliding = boxAABB.Collide(platformAABB);
            var deltaTime = Time.deltaTime;

            // check collision
            if (currentlyColliding)
            {
                if (currentYVelocity < 0) // only stop if falling down
                {
                    currentYVelocity = 0; // stop vertical motion
                    
                    // prevents overlapping of shapes
                    var overlap = platformAABB.maxY - boxAABB.minY;
                    shapePos.y += overlap; 
                }
            }
            else
            {
                // apply gravity to box
                currentYVelocity += gravity * deltaTime;
            }
            
            // update position
            shapePos.y += currentYVelocity * deltaTime;

            // if collided, jump after a few seconds
            if (currentlyColliding && !isJumpTimerActive)
            {
                // collision started
                isJumpTimerActive = true;
                defaultMaterial = collisionMaterial;
                Debug.Log("Box Collided with Platform");

                StartCoroutine(JumpAfterDelay(2.5f));
            }
            else if (!currentlyColliding && isJumpTimerActive)
            {
                // collision ended
                isJumpTimerActive = false;
                StopAllCoroutines();
                defaultMaterial = originalMaterial;
            }
        }
    }
    
    private IEnumerator JumpAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // check collision before jumping
        if (box.BoundingBox.Collide(platform.BoundingBox))
        {
            currentYVelocity = jumpHeight; 
            Debug.Log($"Box jumped.");
            
            // reset shape material and collision 
            defaultMaterial = originalMaterial;
        }
        isJumpTimerActive = false;
    }
}