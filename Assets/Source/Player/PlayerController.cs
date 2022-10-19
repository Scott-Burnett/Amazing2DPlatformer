using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Public Variables

    // Direction Facing
    public DirectionFacing directionFacing { get; private set; }

    // Movement
    public Vector2 velocity { get; private set; }

    #endregion

    #region Private Variables

    // Dashing
    private bool dashing;
    private float timeStartedDash;

    // Jump
    private bool jumping;
    private float timeStartedJump;

    // Collisions
    private BoxCollider2D box;
    private float boxHorizontalExtent;
    private float boxVerticalExtent;
    private Vector2 horizontalEdgeSegment;
    private Vector2 verticalEdgeSegment;

    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 bottomRight;
    private Vector2 bottomLeft;

    private Edge bottomEdge;
    private Edge leftEdge;
    private Edge rightEdge;
    private Edge topEdge;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        // Dashing
        timeStartedDash = 0.0f;

        // Jumping
        timeStartedJump = 0.0f;

        // Movement
        velocity = Vector2.zero;

        // Collisions
        box = GetComponent<BoxCollider2D>();
        boxHorizontalExtent = box.size.x / 2.0f;
        boxVerticalExtent = box.size.y / 2.0f;
        horizontalEdgeSegment = new Vector2(0.0f, -box.size.y / (PlayerConstants.collisionSegments - 1));
        verticalEdgeSegment = new Vector2(box.size.x / (PlayerConstants.collisionSegments - 1), 0.0f);

        topLeft = TopLeft;
        topRight = TopRight;
        bottomRight = BottomRight;
        bottomLeft = BottomLeft;

        bottomEdge = new Edge
        {
            start = bottomLeft,
            end = bottomRight,
            normal = Vector2.down,
            colliding = false
        };

        leftEdge = new Edge
        {
            start = topLeft,
            end = bottomLeft,
            normal = Vector2.left,
            colliding = false
        };

        rightEdge = new Edge
        {
            start = topRight,
            end = bottomRight,
            normal = Vector2.right,
            colliding = false
        };

        topEdge = new Edge
        {
            start = topRight,
            end = topLeft,
            normal = Vector2.up,
            colliding = false
        };
    }

    private void Update()
    {
        velocity = Vector2.zero;

        DetermineDirectionFacing();
        ApplyGravity();
        Dash();
        Jump();
        Move();
        ResolveCollisions();
        UpdatePositionAndVelocity();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        const float sphereRadius = 0.1f;

        if (topEdge.colliding)
        {
            Gizmos.DrawLine(TopLeft, TopRight);
            Gizmos.DrawSphere(TopLeft, sphereRadius);
            Gizmos.DrawSphere(TopRight, sphereRadius);
        }

        if (bottomEdge.colliding)
        {
            Gizmos.DrawLine(BottomLeft, BottomRight);
            Gizmos.DrawSphere(BottomLeft, sphereRadius);
            Gizmos.DrawSphere(BottomRight, sphereRadius);
        }

        if (leftEdge.colliding)
        {
            Gizmos.DrawLine(TopLeft, BottomLeft);
            Gizmos.DrawSphere(TopLeft, sphereRadius);
            Gizmos.DrawSphere(BottomLeft, sphereRadius);
        }

        if (rightEdge.colliding)
        {
            Gizmos.DrawLine(TopRight, BottomRight);
            Gizmos.DrawSphere(TopRight, sphereRadius);
            Gizmos.DrawSphere(BottomRight, sphereRadius);
        }
    }

    #endregion

    #region Private Methods

    private void DetermineDirectionFacing()
    {
        Vector2 mousePosition = (Vector2) Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        float angleToMouse = Vector2.Angle(mousePosition - (Vector2) transform.position, transform.right);

        directionFacing = (DirectionFacing) (int) (angleToMouse % 45);

        Debug.Log(directionFacing);
    }

    private void Dash()
    {
        if (Input.GetKeyDown(PlayerConstants.dashKeyCode))
        {
            dashing = true;
            timeStartedDash = Time.time;
        }

        if (Time.time - timeStartedDash > PlayerConstants.dashDuration)
        {
            dashing = false;
        }

        if (dashing)
        {
            if (Input.GetKey(PlayerConstants.leftKeyCode))
            {
                velocity += PlayerConstants.dashSpeed * Time.deltaTime * Vector2.left;
            }

            if (Input.GetKey(PlayerConstants.rightKeyCode))
            {
                velocity += PlayerConstants.dashSpeed * Time.deltaTime * Vector2.right;
            }
        }
    }

    private void Jump()
    {
        if (bottomEdge.colliding &&
            Input.GetKeyDown(PlayerConstants.jumpKeyCode))
        {
            jumping = true;
            timeStartedJump = Time.time;
        }

        if (Time.time - timeStartedJump > PlayerConstants.jumpDuration ||
            !Input.GetKey(PlayerConstants.jumpKeyCode))
        {
            jumping = false;
        }

        if (jumping)
        {
            const float jumpDurationCoefficient = 1.0f / PlayerConstants.jumpDuration;
            float jumpDurationFactor = 1.0f - ((Time.time - timeStartedJump) * jumpDurationCoefficient);
            jumpDurationFactor = Mathf.Pow(jumpDurationFactor, PlayerConstants.jumpSpeedCurve);
            velocity += PlayerConstants.fallspeed * jumpDurationFactor * Time.deltaTime * Vector2.up;
        }
    }

    private void ApplyGravity()
    {
        if (jumping ||
            dashing)
        {
            return;
        }

        velocity += PlayerConstants.fallspeed * Time.deltaTime * Vector2.down;
    }

    private void Move()
    {
        if (Input.GetKey(PlayerConstants.leftKeyCode))
        {
            velocity += PlayerConstants.moveSpeed * Time.deltaTime * Vector2.left;
        }

        if (Input.GetKey(PlayerConstants.rightKeyCode))
        {
            velocity += PlayerConstants.moveSpeed * Time.deltaTime * Vector2.right;
        }
    }

    private void ResolveCollisions()
    {
        // Bottom
        velocity += SweepForCollision(ref bottomEdge);

        // Left
        velocity += SweepForCollision(ref leftEdge);

        // Right
        velocity += SweepForCollision(ref rightEdge);

        // Top
        velocity += SweepForCollision(ref topEdge);
    }

    private Vector2 SweepForCollision(ref Edge edge)
    {
        edge.colliding = false;

        float distance = velocity.magnitude;
        Vector2 direction = velocity.normalized;

        Vector2 edgeSegment = (edge.end - edge.start) / (PlayerConstants.collisionSegments - 1);

        Vector2 origin = edge.start;
        RaycastHit2D hit;
        float minimumHitDistance = distance;
        for (int i = 0; i < PlayerConstants.collisionSegments; i++, origin += edgeSegment)
        {
            hit = Physics2D.Raycast(origin, direction, distance, PlayerConstants.collisionLayerMask);

            if (hit.collider &&
                hit.distance <= minimumHitDistance &&
                Vector2.Dot(edge.normal, hit.normal) < 0.0f)
            {
                minimumHitDistance = hit.distance;
                edge.colliding = true;
            }
        }

        return (minimumHitDistance - distance) * PlayerConstants.collisionResolutionOvershoot * edge.normal;
    }

    private void UpdatePositionAndVelocity()
    {
        transform.position += (Vector3) velocity;

        topLeft = TopLeft;
        topRight = TopRight;
        bottomRight = BottomRight;
        bottomLeft = BottomLeft;

        bottomEdge.start = bottomLeft;
        bottomEdge.end = bottomRight;

        leftEdge.start = topLeft;
        leftEdge.end = bottomLeft;

        rightEdge.start = topRight;
        rightEdge.end = bottomRight;

        topEdge.start = topRight;
        topEdge.end = topLeft;
    }

    private Vector2 TopRight => 
        (Vector2) transform.position + new Vector2(boxHorizontalExtent, boxVerticalExtent);

    private Vector2 TopLeft =>
        (Vector2) transform.position + new Vector2(-boxHorizontalExtent, boxVerticalExtent);

    private Vector2 BottomRight =>
        (Vector2) transform.position + new Vector2(boxHorizontalExtent, -boxVerticalExtent);

    private Vector2 BottomLeft =>
        (Vector2) transform.position + new Vector2(-boxHorizontalExtent, -boxVerticalExtent);

    #endregion
}
