using UnityEngine;

public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Mario's Transform.
    /// </summary>
    public Transform player;

    /// <summary>
    /// GameObject indicating the right end of the scrollable map.
    /// </summary>
    public Transform endLimit;

    /// <summary>
    /// Desired horizontal offset of the camera from the player.
    /// 0 means the player is centered horizontally.
    /// Positive values shift the camera right (player appears left).
    /// Negative values shift the camera left (player appears right).
    /// </summary>
    public float desiredOffsetX = 0f;

    /// <summary>
    /// Smallest x-coordinate of the camera.
    /// </summary>
    private float _minCameraX;

    /// <summary>
    /// Largest x-coordinate of the camera.
    /// </summary>
    private float _maxCameraX;

    /// <summary>
    /// Cached half-width of the camera's viewport in world units.
    /// </summary>
    private float _viewportHalfWidth;

    private void Start()
    {
        // Get the coordinate of the bottom left of the viewport.
        // z doesn't matter since the camera is orthographic.
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        _viewportHalfWidth = Mathf.Abs(bottomLeft.x - this.transform.position.x);

        // -- Calculate boundaries --
        _minCameraX = this.transform.position.x;
        _maxCameraX = endLimit.position.x - _viewportHalfWidth;

        float initialDesiredX = player.position.x + desiredOffsetX;
        float clampedInitialX = Mathf.Clamp(initialDesiredX, _minCameraX, _maxCameraX);
        this.transform.position = new Vector3(clampedInitialX, this.transform.position.y, this.transform.position.z);
    }

    // Using LateUpdate to run after all Update and physics calculations are done for the frame.
    private void LateUpdate()
    {
        float desiredX = player.position.x + desiredOffsetX;
        float clampedX = Mathf.Clamp(desiredX, _minCameraX, _maxCameraX);
        this.transform.position = new Vector3(clampedX, this.transform.position.y, this.transform.position.z);
    }
}