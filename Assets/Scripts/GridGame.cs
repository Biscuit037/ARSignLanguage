using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GridGame : MonoBehaviour
{
    [Header("Level: # wall, . floor, S start (facing up), G goal")]
    [TextArea(5, 12)]
    public string level =
        "#####\n" +
        "#..G#\n" +
        "#.###\n" +
        "#S###\n" +
        "#####";

    [Header("Rules")]
    public int maxCommands = 10;
    public float stepTime = 0.35f; // Seconds per move/turn animation.
    public float resultDelay = 1.5f; // How long the result message shows before reset.

    // Directions in (col, row): up, right, down, left. Row grows downward in the level text.
    [Header("UI (optional)")]
    public TMP_Text queueLabel;
    public TMP_Text statusLabel;
    public bool autoFrameCamera = true;

    static readonly Vector2Int[] Dirs =
        { new Vector2Int(0,-1), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(-1,0)};

    char[,] cells; // [col, row]
    int width, height;
    Vector2Int startCell, goalCell, robotCell;
    int robotDir; // Index into Dirs.
    Transform robot;
    readonly List<string> queue = new List<string>();
    bool running;

    [Header("Board camera (optional)")]
    public Camera boardCamera;

    private void OnEnable() => SignConfirmer.OnSignConfirm += HandleCommand;
    private void OnDisable() => SignConfirmer.OnSignConfirm -= HandleCommand;

    private void Start()
    {
        BuildLevel();
        ResetRobot();
        SetStatus("Add command, then Start");
        RefreshQueue(-1);
    }

    private void Update()
    {
        // Keyboard controls for testing without the camera.
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            HandleCommand("Forward");
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            HandleCommand("Left");
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            HandleCommand("Right");
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            HandleCommand("Start");
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveLast();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"RESTART");
            Restart();
        }
    }

    // Single entry point for keyboard now and hand signs later.
    public void HandleCommand(string cmd)
    {
        if (running)
        {
            return;
        }

        if (cmd == "Start")
        {
            if (queue.Count > 0)
            {
                StartCoroutine(RunProgram());
            }
            return;
        }

        if (queue.Count >= maxCommands)
        {
            SetStatus("Program is full");
            return;
        }
        queue.Add(cmd);
        RefreshQueue(-1);
    }

    public void RemoveLast()
    {
        if (running || queue.Count == 0)
        {
            return;
        }
        queue.RemoveAt(queue.Count - 1);
        RefreshQueue(-1);
    }

    public void Restart()
    {
        if (running)
        {
            return;
        }
        queue.Clear();
        ResetRobot();
        SetStatus("Add commands, then Start");
        RefreshQueue(-1);
    }

    IEnumerator RunProgram()
    {
        running = true;
        SetStatus("Running...");
        bool crashed = false;

        for (int i = 0; i < queue.Count; i++)
        {
            RefreshQueue(i);
            string cmd = queue[i];

            if (cmd == "Forward")
            {
                Vector2Int next = robotCell + Dirs[robotDir];
                if (IsWall(next))
                {
                    yield return Bump();
                    crashed = true;
                    break;
                }
                robotCell = next;
                yield return Animate(RobotPosition(robotCell), DirToRotation(robotDir));
            }
            else if (cmd == "Left" || cmd == "Right")
            {
                robotDir = (robotDir + (cmd == "Right" ? 1 : 3)) % 4;
                yield return Animate(robot.position, DirToRotation(robotDir));
            }
        }

        RefreshQueue(-1);
        bool won = !crashed && robotCell == goalCell;
        SetStatus(won ? "Clear!" : crashed ? "Bumped into a wall!" : "Didn't reach the goal");
        yield return new WaitForSeconds(resultDelay);

        queue.Clear();
        ResetRobot();
        RefreshQueue(-1);
        SetStatus(won ? "Clear! Play again" : "Try again");
        running = false;
    }

    //---------------Level Building------------
    private void BuildLevel()
    {
        string[] rows = level.Replace("\r", "").Trim('\n').Split('\n');
        height = rows.Length;
        width = 0;
        foreach (var r in rows)
        {
            width = Mathf.Max(width, r.Length);
        }
        cells = new char[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = x < rows[y].Length ? rows[y][x] : '#';
                if (c == 'S')
                {
                    startCell = new Vector2Int(x, y);
                    c = '.';
                }
                if (c == 'G')
                {
                    goalCell = new Vector2Int(x, y);
                }
                cells[x, y] = c;

                Vector3 p = CellToWorld(new Vector2Int(x, y));
                if (c == '#')
                {
                    MakeCube("Wall", p + Vector3.up * 0.5f, Vector3.one, new Color(0.25f, 0.25f, 0.3f));
                }
                else
                {
                    MakeCube("Floor", p + Vector3.down * 0.05f, new Vector3(0.95f, 0.1f, 0.95f),
                        c == 'G' ? Color.green : new Color(0.85f, 0.85f, 0.85f));
                }
            }
        }

        robot = MakeCube("Robot", Vector3.zero, Vector3.one * 0.6f, new Color(0.2f, 0.5f, 1f)).transform;
        var nose = MakeCube("Nose", Vector3.zero, Vector3.one * 0.4f, Color.yellow, robot);
        nose.transform.localPosition = new Vector3(0f, 0f, 0.55f);
        //if (autoFrameCamera && Camera.main)
        //{
        //    var center = new Vector3((width - 1) / 2f, 0f, -(height - 1) / 2f);
        //    float size = Mathf.Max(width, height);
        //    Camera.main.transform.position = center + new Vector3(0f, size * 1.1f, -size * 0.8f);
        //    Camera.main.transform.LookAt(center);
        //}
        var cam = boardCamera != null ? boardCamera : Camera.main;
        if(autoFrameCamera && cam)
        {
            var center = new Vector3((width - 1) / 2f, 0f, -(height - 1) / 2f);
            float size = Mathf.Max(width, height);
            cam.transform.position = center + new Vector3(0f, size * 1.1f, -size * 0.8f);
            cam.transform.LookAt(center);
        }
    }

    private GameObject MakeCube(string name, Vector3 localPos, Vector3 scale, Color color, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent != null ? parent : transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = color;
        go.layer = gameObject.layer;
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    //-------------- Helpers -------------
    void ResetRobot()
    {
        robotCell = startCell;
        robotDir = 0;
        robot.SetPositionAndRotation(RobotPosition(robotCell), DirToRotation(robotDir));
    }

    bool IsWall(Vector2Int c) => c.x < 0 || c.y < 0 || c.x >= width || c.y >= height || cells[c.x, c.y] == '#';

    static Vector3 CellToWorld(Vector2Int c) => new Vector3(c.x, 0f, -c.y);
    static Vector3 RobotPosition(Vector2Int c) => CellToWorld(c) + Vector3.up * 0.3f;
    static Quaternion DirToRotation(int d) => Quaternion.Euler(0f, d * 90f, 0f);

    IEnumerator Animate(Vector3 toPos, Quaternion toRot)
    {
        Vector3 fromPos = robot.position;
        Quaternion fromRot = robot.rotation;
        for (float t = 0; t < 1f; t += Time.deltaTime / stepTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t);
            robot.SetPositionAndRotation(Vector3.Lerp(fromPos, toPos, s), Quaternion.Slerp(fromRot, toRot, s));
            yield return null;
        }
        robot.SetPositionAndRotation(toPos, toRot);
    }

    IEnumerator Bump()
    {
        Vector3 home = robot.position;
        yield return Animate(home + robot.forward * 0.3f, robot.rotation);
        yield return Animate(home, robot.rotation);
    }

    void RefreshQueue(int current)
    {
        Debug.Log($"current : {current}");
        if (!queueLabel)
        {
            return;
        }

        var sb = new StringBuilder($"Program ({queue.Count}/{maxCommands}): ");
        for (int i = 0; i < queue.Count; i++)
        {
            sb.Append(i == current ? $"<color=yellow><b>{queue[i]}</b></color>" : queue[i]);
            if (i < queue.Count - 1)
            {
                sb.Append(" > ");
            }
        }
        queueLabel.text = sb.ToString();
    }

    void SetStatus(string s)
    {
        if (statusLabel)
        {
            statusLabel.text = s;
        }
        Debug.Log($"[GridGame] {s}");
    }
}

