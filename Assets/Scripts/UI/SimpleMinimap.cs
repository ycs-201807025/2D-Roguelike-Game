using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 미니맵 UI
/// </summary>
public class SimpleMinimap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private int roomIconSize = 35;
    [SerializeField] private int roomSpacing = 45;

    [Header("Colors")]
    [SerializeField] private Color currentRoomColor = Color.green;
    [SerializeField] private Color clearedRoomColor = Color.gray;
    [SerializeField] private Color unvisitedRoomColor = Color.white;

    private Rect minimapRect;
    private GUIStyle boxStyle;
    private GUIStyle roomStyle;
    private bool stylesInitialized = false;

    void Start()
    {
        // 미니맵 위치 (화면 우측 상단)
        minimapRect = new Rect(Screen.width - 270, 10, 260, 70);

        if (dungeonManager == null)
        {
            dungeonManager = FindObjectOfType<DungeonManager>();
        }

        // 스타일은 OnGUI에서 초기화
    }

    void InitStyles()
    {
        if (stylesInitialized) return;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.normal.textColor = Color.white;

        roomStyle = new GUIStyle(GUI.skin.box);
        roomStyle.fontSize = 14;
        roomStyle.alignment = TextAnchor.MiddleCenter;
        roomStyle.fontStyle = FontStyle.Bold;

        stylesInitialized = true;
    }

    void OnGUI()
    {
        // 스타일 초기화 (OnGUI에서만 가능)
        if (!stylesInitialized)
        {
            InitStyles();
        }

        if (dungeonManager == null) return;

        // 배경
        GUI.Box(minimapRect, "MINIMAP", boxStyle);

        // 방들 표시
        int totalRooms = dungeonManager.TotalRooms;
        int currentIndex = dungeonManager.CurrentRoomIndex;

        float startX = minimapRect.x + 10;
        float startY = minimapRect.y + 30;

        for (int i = 0; i < totalRooms; i++)
        {
            Rect roomRect = new Rect(
                startX + i * roomSpacing,
                startY,
                roomIconSize,
                roomIconSize
            );

            // 방 상태에 따라 색상 결정
            Color color;
            if (i == currentIndex)
            {
                color = currentRoomColor; // 현재 방
            }
            else if (i < currentIndex)
            {
                color = clearedRoomColor; // 클리어한 방
            }
            else
            {
                color = unvisitedRoomColor; // 아직 안 간 방
            }

            // 방 그리기
            GUI.backgroundColor = color;
            GUI.Box(roomRect, (i + 1).ToString(), roomStyle);

            // 연결선 그리기 (다음 방이 있으면)
            if (i < totalRooms - 1)
            {
                DrawLine(
                    new Vector2(roomRect.xMax, roomRect.center.y),
                    new Vector2(roomRect.xMax + (roomSpacing - roomIconSize), roomRect.center.y),
                    Color.white,
                    2f
                );
            }
        }

        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// 선 그리기
    /// </summary>
    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        GUI.color = color;

        float distance = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - thickness / 2, distance, thickness), Texture2D.whiteTexture);
        GUI.matrix = matrixBackup;

        GUI.color = Color.white;
    }
}
