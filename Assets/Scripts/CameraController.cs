using UnityEngine;

public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Mario's Transform.
    /// </summary>
    public Transform player;

    /// <summary>
    /// GameObject that indicates the end of the map.
    /// </summary>
    public Transform endLimit;

    /// <summary>
    /// Initial x-offset between the camera and Mario.
    /// </summary>
    private float offset;

    /// <summary>
    /// Smallest x-coordinate of the camera.
    /// </summary>
    private float startX;

    /// <summary>
    /// Largest x-coordinate of the camera.
    /// </summary>
    private float endX;

    private float viewportHalfWidth;

    private void Start()
    {
        // Get the coordinate of the bottom left of the viewport.
        // z doesn't matter since the camera is orthographic.
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        viewportHalfWidth = Mathf.Abs(bottomLeft.x - this.transform.position.x);
        offset = this.transform.position.x - player.position.x;
        startX = this.transform.position.x;
        endX = endLimit.transform.position.x - viewportHalfWidth;
    }

    private void Update()
    {
        float desiredX = player.position.x + offset;
        if (desiredX > startX && desiredX < endX)
            this.transform.position = new Vector3(desiredX, this.transform.position.y, this.transform.position.z);
    }
}