using UnityEngine;
using UnityEngine.InputSystem;

// Điều khiển dùng Input System mới: bàn phím + tay cầm
public static class GameInput
{
    public static InputAction Move, Jump, Attack, Interact, Down, Pause, Restart, Menu;
    static bool created = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (created) return;
        created = true;

        Move = new InputAction("Move", InputActionType.Value);
        Move.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        Move.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        Move.AddBinding("<Gamepad>/leftStick/x");
        Move.AddCompositeBinding("1DAxis")
            .With("Negative", "<Gamepad>/dpad/left")
            .With("Positive", "<Gamepad>/dpad/right");

        Jump = Button("Jump", "<Keyboard>/upArrow", "<Keyboard>/w", "<Gamepad>/buttonSouth");
        Attack = Button("Attack", "<Keyboard>/space", "<Keyboard>/j", "<Gamepad>/buttonWest");
        Interact = Button("Interact", "<Keyboard>/e", "<Gamepad>/buttonNorth");
        Down = Button("Down", "<Keyboard>/downArrow", "<Keyboard>/s", "<Gamepad>/dpad/down", "<Gamepad>/leftStick/down");
        Pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
        Restart = Button("Restart", "<Keyboard>/r", "<Gamepad>/buttonSouth");
        Menu = Button("Menu", "<Keyboard>/m", "<Gamepad>/buttonEast");
    }

    static InputAction Button(string name, params string[] paths)
    {
        InputAction a = new InputAction(name, InputActionType.Button);
        foreach (string p in paths) a.AddBinding(p);
        a.Enable();
        return a;
    }

    public static float MoveX
    {
        get
        {
            if (!Move.enabled) Move.Enable();
            return Mathf.Clamp(Move.ReadValue<float>(), -1f, 1f);
        }
    }
}
