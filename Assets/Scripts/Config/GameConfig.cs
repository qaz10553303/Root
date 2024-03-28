using UnityEngine;

public static class GameConfig
{
    public static readonly int GAME_START_SCROLL_SPD = 3;
    public static readonly int GAME_START_WATER = 100;
    public static readonly int MAX_WATER = 100;
    public static readonly int WATER_DROP_PER_SEC = 5;
    public static readonly float ROOT_PIECE_SPAWN_INTERVAL_LENGTH = 0.35f;
    public static readonly float MOST_LEFT_ROOT_ROTATION_Z = 120;
    public static readonly float MOST_RIGHT_ROOT_ROTATION_Z = 240;
    public static readonly float ROOT_ROTATION_SPD = 150f;
    public static readonly Vector3 CAMERA_INIT_POS = new Vector3(0,-7,-10);
    public static readonly float CAMERA_X_MOVE_SPD = 2f;
    public static readonly float CASE_SPACING = 0.3f;
    public static readonly float CASE_Y_LENGTH = 5f;
    public static readonly float PRE_RENDER_DEPTH = 100f;
    public static readonly float CAMERA_PREP_MOVE_SPEED = 4.5f;
    public static readonly float INITIAL_FOLLOW_DELAY = 1f;

    public static readonly float ROOT_1_SPAWN_X_PERC = 50f;
    public static readonly float ROOT_2_SPAWN_X_PERC = 75f;
    public static readonly float ROOT_3_SPAWN_X_PERC = 25f;
    public static readonly float ROOT_4_SPAWN_X_PERC = 80f;

    public static readonly float ROOT_SPAWN_SPEED_BOOST = 3f;
    public static readonly int ROOT_MAX_COUNT = 4;
}
