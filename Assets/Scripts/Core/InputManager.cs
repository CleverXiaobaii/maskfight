//csharp Assets/Scripts/Core/InputManager.cs
using System;
using UnityEngine;

/// <summary>
/// 简单的双玩家键盘输入管理器（使用旧 Input API）
///
/// - Player1: WASD 移动, LeftShift 攻击, Z 拾取
/// - Player2: Arrows 移动, RightShift 攻击, / (Slash) 拾取
///
/// 如果你希望改用 Unity 新 Input System，请在此处替换实现或扩展为两套输入实现。
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Player1 Keys")]
    public KeyCode p1_up = KeyCode.W;
    public KeyCode p1_down = KeyCode.S;
    public KeyCode p1_left = KeyCode.A;
    public KeyCode p1_right = KeyCode.D;
    public KeyCode p1_attack = KeyCode.LeftShift;
    public KeyCode p1_pickup = KeyCode.Z;

    [Header("Player2 Keys")]
    public KeyCode p2_up = KeyCode.UpArrow;
    public KeyCode p2_down = KeyCode.DownArrow;
    public KeyCode p2_left = KeyCode.LeftArrow;
    public KeyCode p2_right = KeyCode.RightArrow;
    public KeyCode p2_attack = KeyCode.RightShift;
    // 注意：KeyCode.Slash 在某些键盘布局上可能不同，若有问题可改为 KeyCode.Backslash 或通过扫描键码调整
    public KeyCode p2_pickup = KeyCode.Slash;

    // 每帧读取并缓存，供其它系统查询
    public Vector2 Player1Move { get; private set; }
    public bool Player1AttackPressed { get; private set; }
    public bool Player1PickupPressed { get; private set; }

    public Vector2 Player2Move { get; private set; }
    public bool Player2AttackPressed { get; private set; }
    public bool Player2PickupPressed { get; private set; }

    private void Update()
    {
        UpdatePlayer1();
        UpdatePlayer2();
    }

    private void UpdatePlayer1()
    {
        float x = 0f, y = 0f;
        if (Input.GetKey(p1_left)) x -= 1f;
        if (Input.GetKey(p1_right)) x += 1f;
        if (Input.GetKey(p1_up)) y += 1f;
        if (Input.GetKey(p1_down)) y -= 1f;
        Player1Move = new Vector2(x, y).normalized;

        Player1AttackPressed = Input.GetKeyDown(p1_attack);
        Player1PickupPressed = Input.GetKeyDown(p1_pickup);
    }

    private void UpdatePlayer2()
    {
        float x = 0f, y = 0f;
        if (Input.GetKey(p2_left)) x -= 1f;
        if (Input.GetKey(p2_right)) x += 1f;
        if (Input.GetKey(p2_up)) y += 1f;
        if (Input.GetKey(p2_down)) y -= 1f;
        Player2Move = new Vector2(x, y).normalized;

        Player2AttackPressed = Input.GetKeyDown(p2_attack);
        Player2PickupPressed = Input.GetKeyDown(p2_pickup);
    }
}