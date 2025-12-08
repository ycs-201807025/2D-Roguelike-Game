using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라를 현재 방 경계 내로 제한
/// Cinemachine Confiner 대신 간단한 버전
/// </summary>
public class CameraRoomBounds : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float padding = 5f; // 방 경계에서 카메라 여유 공간

    private Camera cam;
    private Room currentRoom;
    private Bounds roomBounds;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (currentRoom != null)
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

        if (room != null)
        {
            // 방 경계 계산
            Vector2 roomSize = room.RoomData.roomSize;
            Vector3 roomCenter = room.transform.position;

            roomBounds = new Bounds(
                roomCenter,
                new Vector3(roomSize.x - padding, roomSize.y - padding, 0)
            );

            Debug.Log($"Camera bounds set for room: {room.RoomData.roomName}");
        }
    }

    /// <summary>
    /// 카메라를 방 경계 내로 제한
    /// </summary>
    private void ClampCameraToRoom()
    {
        float cameraHalfHeight = cam.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * cam.aspect;

        Vector3 pos = transform.position;

        // X축 제한
        float minX = roomBounds.min.x + cameraHalfWidth;
        float maxX = roomBounds.max.x - cameraHalfWidth;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        // Y축 제한
        float minY = roomBounds.min.y + cameraHalfHeight;
        float maxY = roomBounds.max.y - cameraHalfHeight;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    // 디버그: 방 경계 표시
    void OnDrawGizmos()
    {
        if (currentRoom != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(roomBounds.center, roomBounds.size);
        }
    }
}