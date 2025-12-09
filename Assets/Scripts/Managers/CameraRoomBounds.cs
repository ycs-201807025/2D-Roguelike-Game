using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라를 현재 방 경계 내로 제한
/// </summary>
public class CameraRoomBounds : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float padding = 2f; // 방 경계에서 카메라 여유 공간

    private Camera cam;
    private Room currentRoom;
    private Bounds roomBounds;
    private bool hasBounds = false;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("CameraRoomBounds: Camera component not found!");
        }
    }

    void LateUpdate()
    {
        if (hasBounds && currentRoom != null)
        {
            ClampCameraToRoom();
        }
    }

    /// <summary>
    /// 현재 방 설정
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        currentRoom = room;

        if (room != null && room.RoomData != null)
        {
            // 방 경계 계산
            Vector2 roomSize = room.RoomData.roomSize;
            Vector3 roomCenter = room.transform.position;

            roomBounds = new Bounds(
                roomCenter,
                new Vector3(roomSize.x - padding, roomSize.y - padding, 0)
            );

            hasBounds = true;
            Debug.Log($"[CAMERA] Bounds set for room: {room.RoomData.roomName}");
            Debug.Log($"[CAMERA] Center: {roomBounds.center}, Size: {roomBounds.size}");
        }
        else
        {
            hasBounds = false;
            Debug.LogWarning("[CAMERA] Room or RoomData is null");
        }
    }

    /// <summary>
    /// 카메라를 방 경계 내로 제한
    /// </summary>
    private void ClampCameraToRoom()
    {
        if (cam == null) return;

        float cameraHalfHeight = cam.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * cam.aspect;

        Vector3 pos = transform.position;

        // X축 제한
        float minX = roomBounds.min.x + cameraHalfWidth;
        float maxX = roomBounds.max.x - cameraHalfWidth;

        if (maxX >= minX) // 방이 카메라보다 크면
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }
        else // 방이 카메라보다 작으면 중앙 고정
        {
            pos.x = roomBounds.center.x;
        }

        // Y축 제한
        float minY = roomBounds.min.y + cameraHalfHeight;
        float maxY = roomBounds.max.y - cameraHalfHeight;

        if (maxY >= minY)
        {
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }
        else
        {
            pos.y = roomBounds.center.y;
        }

        transform.position = pos;
    }

    // 디버그: 방 경계 표시
    void OnDrawGizmos()
    {
        if (hasBounds && currentRoom != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(roomBounds.center, roomBounds.size);

            // 카메라 범위 표시
            if (cam != null)
            {
                float h = cam.orthographicSize;
                float w = h * cam.aspect;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, new Vector3(w * 2, h * 2, 0));
            }
        }
    }
}