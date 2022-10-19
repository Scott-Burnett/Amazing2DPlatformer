using UnityEngine;

public class CameraTransformFollow : MonoBehaviour
{
    #region Editor Fields

    [SerializeField]
    private Transform transformToFollow = null;

    #endregion

    void Start()
    {
    }

    void Update()
    {
        Vector2 difference = (Vector2) transformToFollow.position - (Vector2) transform.position;

        float distanceToTransformToTrack = difference.magnitude;

        if (distanceToTransformToTrack > CameraConstants.playerFollowRadius ||
            distanceToTransformToTrack < -CameraConstants.playerFollowRadius)
        {
            float lerpRate = Mathf.Clamp01(CameraConstants.playerFollowRate * Time.deltaTime);
            transform.position += (Vector3) (difference.normalized * (distanceToTransformToTrack - CameraConstants.playerFollowRadius) * lerpRate);
        }

        if (transform.position.y > transformToFollow.position.y)
        {
            transform.position = new Vector3(transform.position.x,
                                             transformToFollow.position.y,
                                             transform.position.z);
        }
    }
}
