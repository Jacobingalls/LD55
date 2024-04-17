using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private Camera _camera;
    private Vector3 _lastMousePosition;

    public bool CameraIsPanning = false;
    public bool CameraIsZooming = false;

    private bool _lookingForPanGesture = false;

    private bool _canZoom = true;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        DragCamera();
        ZoomCamera();
    }

    void DragCamera()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _lookingForPanGesture = true;
        }

        if (!_lookingForPanGesture && !CameraIsPanning)
        {
            return;
        }

        if (!Input.GetMouseButton(1) && !Input.GetMouseButtonUp(1))
        {
            _lookingForPanGesture = false;
            CameraIsPanning = false;
            return;
        }
        
        if (_lookingForPanGesture)
        {
            _lastMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _lookingForPanGesture = false;
            CameraIsPanning = true;
        }

        if (CameraIsPanning)
        {
            Vector3 delta = _camera.ScreenToWorldPoint(Input.mousePosition) - _lastMousePosition;

            _camera.transform.position = _camera.transform.position - new Vector3(delta.x, delta.y, 0);

            _lastMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public float MaxOrthographicSize = 10;
    public float MinOrthographicSize = 2;

    void ZoomCamera()
    {
        if (!_canZoom)
        {
            return;
        }

        var scrollDelta = Input.mouseScrollDelta;

        _camera.orthographicSize = Mathf.Clamp(
            _camera.orthographicSize - scrollDelta.y,
            MinOrthographicSize,
            MaxOrthographicSize);
    }

    public void EnableZooming()
    {
        _canZoom = true;
    }

    public void DisableZooming()
    {
        _canZoom = false;
    }
}
