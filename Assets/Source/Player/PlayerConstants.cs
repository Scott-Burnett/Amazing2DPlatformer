using UnityEngine;

public static class PlayerConstants
{
    #region KeyCodes

    public const KeyCode upKeyCode = KeyCode.W;
    public const KeyCode leftKeyCode = KeyCode.A;
    public const KeyCode downKeyCode = KeyCode.S;
    public const KeyCode rightKeyCode = KeyCode.D;
    public const KeyCode jumpKeyCode = KeyCode.Space;
    public const KeyCode dashKeyCode = KeyCode.LeftShift;

    #endregion

    #region Movement

    public const float moveSpeed = 10.0f;

    #endregion

    #region Jumping

    public const float fallspeed = 20.0f;

    #endregion

    #region Collisions

    public const int collisionSegments = 3;
    public const int collisionLayerMask = 1;
    public const float collisionResolutionOvershoot = 1.001f;
    // public const float minimumHorizontalCollisionResolutionDistance = 0.75f;
    // public const float minimumBottomCollisionResolutionDistance = 0.75f;
    // public const float minimumTopCollisionResolutionDistance = 0.0f;
    public const float minimumHorizontalCollisionResolutionDistance = 1.0f - 0.75f;
    public const float minimumBottomCollisionResolutionDistance = 1.0f - 0.75f;
    public const float minimumTopCollisionResolutionDistance = 1.0f - 0.0f;
    public const float maximumHorizontalCollisionResolutionDistance = 0.0f;
    public const float maximumBottomCollisionResolutionDistance = 0.0f;
    public const float maximumTopCollisionResolutionDistance = 0.0f;

    #endregion
}
