using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public Variables

    // Movement
    public Vector2 velocity { get; private set; }

    // Collisions
    public bool collidingLeft { get; private set; }
    public bool collidingRight { get; private set; }
    public bool collidingTop { get; private set; }
    public bool collidingBottom { get; private set; }

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
        ApplyGravity();
        Move();
        ResolveCollisions();
        UpdatePositionAndVelocity();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        const float sphereRadius = 0.1f;

        if (collidingTop)
        {
            Gizmos.DrawLine(TopLeft, TopRight);
            Gizmos.DrawSphere(TopLeft, sphereRadius);
            Gizmos.DrawSphere(TopRight, sphereRadius);
        }

        if (collidingBottom)
        {
            Gizmos.DrawLine(BottomLeft, BottomRight);
            Gizmos.DrawSphere(BottomLeft, sphereRadius);
            Gizmos.DrawSphere(BottomRight, sphereRadius);
        }

        if (collidingLeft)
        {
            Gizmos.DrawLine(TopLeft, BottomLeft);
            Gizmos.DrawSphere(TopLeft, sphereRadius);
            Gizmos.DrawSphere(BottomLeft, sphereRadius);
        }

        if (collidingRight)
        {
            Gizmos.DrawLine(TopRight, BottomRight);
            Gizmos.DrawSphere(TopRight, sphereRadius);
            Gizmos.DrawSphere(BottomRight, sphereRadius);
        }
    }

    #endregion

    #region Private Methods

    private void ApplyGravity()
    {
        if (Input.GetKey(PlayerConstants.jumpKeyCode))
        {
            nextPosition += PlayerConstants.fallspeed * Time.deltaTime * Vector3.up;
            return;
        }

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

        // Bottom
        (collidingBottom, collisionResolution) = 
            CalculateCollisionOnEdge(BottomLeft, 
                                     NextBottomLeft,
                                    //  Vector2.up,
                                     PlayerConstants.minimumBottomCollisionResolutionDistance, 
                                     PlayerConstants.maximumBottomCollisionResolutionDistance, 
                                     verticalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;

        // Left
        (collidingLeft, collisionResolution) = 
            CalculateCollisionOnEdge(TopLeft,
                                     NextTopLeft,
                                    //  Vector2.right,
                                     PlayerConstants.minimumHorizontalCollisionResolutionDistance, 
                                     PlayerConstants.maximumHorizontalCollisionResolutionDistance, 
                                     horizontalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;

        // Right
        (collidingRight, collisionResolution) = 
            CalculateCollisionOnEdge(TopRight,
                                     NextTopRight,
                                    //  Vector2.left,
                                     PlayerConstants.minimumHorizontalCollisionResolutionDistance, 
                                     PlayerConstants.maximumHorizontalCollisionResolutionDistance, 
                                     horizontalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;

        // Top
        (collidingTop, collisionResolution) = 
            CalculateCollisionOnEdge(TopLeft, 
                                     NextTopLeft,
                                    //  Vector2.down,
                                     PlayerConstants.minimumTopCollisionResolutionDistance, 
                                     PlayerConstants.maximumTopCollisionResolutionDistance, 
                                     verticalEdgeSegment);
        nextPosition += (Vector3) collisionResolution;
    }

    private (bool, Vector2) CalculateCollisionOnEdge(Vector2 edgeStart, 
                                                     Vector2 edgeEnd,
                                                    //  Vector2 resolutionNormal,
                                                     float maximumResolutionDistance,
                                                     float minimumResolutionDistance,
                                                     Vector2 edgeSegment)
    {
        float rayDistance = Vector2.Distance(edgeStart, edgeEnd);
        Vector2 direction = (edgeEnd - edgeStart).normalized;
        Vector2 resolutionNormal = Vector2.zero;
        bool collidingOnEdge = false;
        float minimumDistance = rayDistance;

        RaycastHit2D hit;
        for (int i = 0; i < PlayerConstants.collisionSegments; i++, edgeStart += edgeSegment)
        {
            hit = Physics2D.Raycast(edgeStart, direction, rayDistance, PlayerConstants.collisionLayerMask);
            if (hit.collider &&
                hit.distance <= minimumDistance /*&&
                hit.distance > minimumResolutionDistance*/)
            {
                minimumDistance = hit.distance;
                resolutionNormal = hit.normal;
                collidingOnEdge = true;
            }
        }

        // Vector2 collisionResolution = (minimumDistance - rayDistance) * PlayerConstants.collisionResolutionOvershoot * direction;
        Vector2 collisionResolution = (rayDistance - minimumDistance) * PlayerConstants.collisionResolutionOvershoot * resolutionNormal;

        // if (collisionResolution.magnitude > maximumResolutionDistance ||
        //     collisionResolution.magnitude < minimumResolutionDistance)
        // {
        //     return (false, Vector2.zero);
        // }

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
