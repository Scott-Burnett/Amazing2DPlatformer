using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public Variables

    // Movement
    public Vector2 velocity { get; private set; }

    // Collisions
    bool collidingLeft;
    bool collidingRight;
    bool collidingTop;
    bool collidingBottom;

    #endregion

    #region Private Variables

    // Movement
    private Vector3 nextPosition;

    // Collisions
    private BoxCollider2D box;
    private float boxHorizontalExtent;
    private float boxVerticalExtent;
    private Vector2 horizontalEdgeSegment;
    private Vector2 verticalEdgeSegment;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        // Movement
        velocity = Vector2.zero;
        nextPosition = transform.position;

        // Collisions
        collidingLeft = false;
        collidingRight = false;
        collidingTop = false;
        collidingBottom = false;

        box = GetComponent<BoxCollider2D>();
        boxHorizontalExtent = box.size.x / 2.0f;
        boxVerticalExtent = box.size.y / 2.0f;
        horizontalEdgeSegment = new Vector2(0.0f, -box.size.y / (PlayerConstants.collisionSegments - 1));
        verticalEdgeSegment = new Vector2(box.size.x / (PlayerConstants.collisionSegments - 1), 0.0f);
    }

    private void Update()
    {
        // ApplyGravity();
        Move();
        ResolveCollisions();
        UpdatePositionAndVelocity();
    }

    private void OnDrawGizmos()
    {
        
    }

    #endregion

    #region Private Methods

    private void ApplyGravity()
    {
        nextPosition += PlayerConstants.fallspeed * Time.deltaTime * Vector3.down;
    }

    private void Move()
    {
        if (Input.GetKey(PlayerConstants.leftKeyCode))
        {
            nextPosition += PlayerConstants.moveSpeed * Time.deltaTime * Vector3.left;
        }

        if (Input.GetKey(PlayerConstants.rightKeyCode))
        {
            nextPosition += PlayerConstants.moveSpeed * Time.deltaTime * Vector3.right;
        }

        if (Input.GetKey(PlayerConstants.upKeyCode))
        {
            nextPosition += PlayerConstants.moveSpeed * Time.deltaTime * Vector3.up;
        }

        if (Input.GetKey(PlayerConstants.downKeyCode))
        {
            nextPosition += PlayerConstants.moveSpeed * Time.deltaTime * Vector3.down;
        }
    }

    private void ResolveCollisions()
    {
        Vector2 collisionResolution;

        // // Bottom
        // (collidingBottom, collisionResolution) = 
        //     CalculateCollisionOnEdge(NextTopLeft, 
        //                              Vector2.down, 
        //                              box.size.y, 
        //                              PlayerConstants.minimumBottomCollisionResolutionDistance, 
        //                              verticalEdgeSegment);
        // nextPosition += (Vector3) collisionResolution;

        // // Left
        // (collidingLeft, collisionResolution) = 
        //     CalculateCollisionOnEdge(NextTopRight, 
        //                              Vector2.left, 
        //                              box.size.x, 
        //                              PlayerConstants.minimumHorizontalCollisionResolutionDistance, 
        //                              horizontalEdgeSegment);
        // nextPosition += (Vector3) collisionResolution;

        // // Right
        // (collidingRight, collisionResolution) = 
        //     CalculateCollisionOnEdge(NextTopLeft, 
        //                              Vector2.right, 
        //                              box.size.x, 
        //                              PlayerConstants.minimumHorizontalCollisionResolutionDistance, 
        //                              horizontalEdgeSegment);
        // nextPosition += (Vector3) collisionResolution;

        // // Top
        // (collidingTop, collisionResolution) = 
        //     CalculateCollisionOnEdge(NextBottomLeft, 
        //                              Vector2.up, 
        //                              box.size.y, 
        //                              PlayerConstants.minimumTopCollisionResolutionDistance, 
        //                              verticalEdgeSegment);
        // nextPosition += (Vector3) collisionResolution;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Bottom
        (collidingBottom, collisionResolution) = 
            CalculateCollisionOnEdge(BottomLeft, 
                                     NextBottomLeft,
                                     PlayerConstants.minimumBottomCollisionResolutionDistance, 
                                     verticalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;

        // Left
        (collidingLeft, collisionResolution) = 
            CalculateCollisionOnEdge(TopLeft,
                                     NextTopLeft,
                                     PlayerConstants.minimumHorizontalCollisionResolutionDistance, 
                                     horizontalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;

        // Right
        (collidingRight, collisionResolution) = 
            CalculateCollisionOnEdge(TopRight,
                                     NextTopRight,
                                     PlayerConstants.minimumHorizontalCollisionResolutionDistance, 
                                     horizontalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;

        // Top
        (collidingTop, collisionResolution) = 
            CalculateCollisionOnEdge(TopLeft, 
                                     NextTopLeft, 
                                     PlayerConstants.minimumTopCollisionResolutionDistance, 
                                     verticalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;
    }

    // private (bool, Vector2) CalculateCollisionOnEdge(Vector2 edgeStart, 
    //                                                  Vector2 direction, 
    //                                                  float rayDistance,
    //                                                  float minimumResolutionDistance,
    //                                                  Vector2 edgeSegment)
    // {
    //     bool collidingOnEdge = false;
    //     float minimumDistance = rayDistance;

    //     RaycastHit2D hit;
    //     for (int i = 0; i < PlayerConstants.collisionSegments; i++, edgeStart += edgeSegment)
    //     {
    //         hit = Physics2D.Raycast(edgeStart, direction, rayDistance, PlayerConstants.collisionLayerMask);
    //         if (hit.collider &&
    //             hit.distance <= minimumDistance &&
    //             hit.distance > minimumResolutionDistance)
    //         {
    //             minimumDistance = hit.distance;
    //             collidingOnEdge = true;
    //         }
    //     }

    //     Vector2 collisionResolution = (minimumDistance - rayDistance) * PlayerConstants.collisionResolutionOvershoot * direction;
    //     return (collidingOnEdge, collisionResolution);
    // }

    private (bool, Vector2) CalculateCollisionOnEdge(Vector2 edgeStart, 
                                                    //  Vector2 direction,
                                                     Vector2 edgeEnd,
                                                    //  float rayDistance,
                                                     float minimumResolutionDistance,
                                                     Vector2 edgeSegment)
    {
        float rayDistance = Vector2.Distance(edgeStart, edgeEnd);
        Vector2 direction = (edgeEnd - edgeStart).normalized;
        bool collidingOnEdge = false;
        float minimumDistance = rayDistance;

        RaycastHit2D hit;
        for (int i = 0; i < PlayerConstants.collisionSegments; i++, edgeStart += edgeSegment)
        {
            hit = Physics2D.Raycast(edgeStart, direction, rayDistance, PlayerConstants.collisionLayerMask);
            if (hit.collider &&
                hit.distance <= minimumDistance &&
                hit.distance > minimumResolutionDistance)
            {
                minimumDistance = hit.distance;
                collidingOnEdge = true;
            }
        }

        Vector2 collisionResolution = (minimumDistance - rayDistance) * PlayerConstants.collisionResolutionOvershoot * direction;
        return (collidingOnEdge, collisionResolution);
    }

    private void UpdatePositionAndVelocity()
    {
        velocity = nextPosition - transform.position;
        transform.position = nextPosition;
    }

    private Vector2 TopRight => 
        (Vector2) transform.position + new Vector2(boxHorizontalExtent, boxVerticalExtent);

    private Vector2 TopLeft =>
        (Vector2) transform.position + new Vector2(-boxHorizontalExtent, boxVerticalExtent);

    private Vector2 BottomRight =>
        (Vector2) transform.position + new Vector2(boxHorizontalExtent, -boxVerticalExtent);

    private Vector2 BottomLeft =>
        (Vector2) transform.position + new Vector2(-boxHorizontalExtent, -boxVerticalExtent);

    private Vector2 NextTopRight => 
        (Vector2) nextPosition + new Vector2(boxHorizontalExtent, boxVerticalExtent);

    private Vector2 NextTopLeft =>
        (Vector2) nextPosition + new Vector2(-boxHorizontalExtent, boxVerticalExtent);

    private Vector2 NextBottomRight =>
        (Vector2) nextPosition + new Vector2(boxHorizontalExtent, -boxVerticalExtent);

    private Vector2 NextBottomLeft =>
        (Vector2) nextPosition + new Vector2(-boxHorizontalExtent, -boxVerticalExtent);

    #endregion
}
