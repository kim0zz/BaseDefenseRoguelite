using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerCharacter — ruch + aim + wejście ataku.
/// P1: WASD + mysz (punkt na XZ). Pad: lewy stick ruch, prawy stick look.
/// Atak: Space / LMB / Pad West.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private int _playerIndex;
    private PlayerInputMode _inputMode;
    private Gamepad _assignedGamepad;
    private Renderer _renderer;
    private Camera _aimCamera;
    private bool _combatEnabled = true;
    private Vector3 _facingDirection = Vector3.forward;
    private Vector3 _aimDirection = Vector3.forward;
    private Vector3 _currentMoveDirection;
    private Vector3 _lastMoveDirection = Vector3.forward;
    private float _configuredMoveSpeed = -1f;
    private float _attackMoveMultiplier = 1f;
    private bool _facingLocked;
    private StatusEffectReceiver _statusReceiver;

    private static readonly Color[] PlayerColors =
    {
        new(1f,  0.2f, 0.2f, 1f),
        new(0.2f, 0.4f, 1f,  1f),
        new(0.2f, 0.9f, 0.3f, 1f),
        new(1f,  0.9f, 0.1f, 1f)
    };

    public int PlayerIndex => _playerIndex;
    public PlayerInputMode InputMode => _inputMode;
    public Gamepad AssignedGamepad => _assignedGamepad;
    public bool IsCombatEnabled => _combatEnabled;
    public Vector3 FacingDirection => _facingDirection;
    public Vector3 AimDirection => _aimDirection;
    public Vector3 CurrentMoveDirection => _currentMoveDirection;
    public Vector3 LastMoveDirection => _lastMoveDirection;
    public Camera AimCamera => ResolveAimCamera();

    public void ApplyMoveSpeed(float speed)
    {
        _configuredMoveSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetAttackMoveMultiplier(float multiplier)
    {
        _attackMoveMultiplier = Mathf.Clamp(multiplier, 0f, 1f);
    }

    public void SetFacingLock(bool locked, Vector3 facing)
    {
        _facingLocked = locked;
        facing.y = 0f;
        if (facing.sqrMagnitude > 0.01f)
            _facingDirection = facing.normalized;
        ApplyFacingRotation();
    }

    public bool TryReadWeaponSwapInput()
    {
        if (!_combatEnabled || !LootLoopConfig.LootEnabledInRun) return false;

        if (_inputMode == PlayerInputMode.KeyboardMouse)
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
        }

        if (_assignedGamepad == null || !_assignedGamepad.enabled) return false;
        return _assignedGamepad.leftShoulder.wasPressedThisFrame ||
               _assignedGamepad.rightShoulder.wasPressedThisFrame;
    }

    /// <summary>Umiejętność aktywna — slot 0=Q/East, 1=E/North, 2=R/LB (M7.6-T3 DRAFT).</summary>
    public bool TryReadSkillInput(int slotIndex = 0)
    {
        if (!_combatEnabled) return false;
        if (slotIndex < 0 || slotIndex > SkillLoadout.UltimateSlot) return false;

        if (_inputMode == PlayerInputMode.KeyboardMouse)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            return slotIndex switch
            {
                0 => keyboard.qKey.wasPressedThisFrame,
                1 => keyboard.eKey.wasPressedThisFrame,
                2 => keyboard.rKey.wasPressedThisFrame,
                3 => keyboard.fKey.wasPressedThisFrame,
                _ => false
            };
        }

        if (_assignedGamepad == null || !_assignedGamepad.enabled) return false;
        return slotIndex switch
        {
            0 => _assignedGamepad.buttonEast.wasPressedThisFrame,
            1 => _assignedGamepad.buttonNorth.wasPressedThisFrame,
            2 => _assignedGamepad.leftShoulder.wasPressedThisFrame,
            3 => _assignedGamepad.rightShoulder.wasPressedThisFrame,
            _ => false
        };
    }

    public void RequestGamepadRumble(float low, float high, float seconds)
    {
        if (_inputMode != PlayerInputMode.Gamepad || _assignedGamepad == null) return;
        _assignedGamepad.SetMotorSpeeds(low, high);
        CancelInvoke(nameof(StopGamepadRumble));
        Invoke(nameof(StopGamepadRumble), seconds);
    }

    private void StopGamepadRumble()
    {
        _assignedGamepad?.SetMotorSpeeds(0f, 0f);
    }

    private float CurrentMoveSpeed => _configuredMoveSpeed > 0f ? _configuredMoveSpeed : moveSpeed;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _statusReceiver = GetComponent<StatusEffectReceiver>();
    }

    public void Initialize(int playerIndex, PlayerInputMode mode, Gamepad gamepad = null)
    {
        _playerIndex = playerIndex;
        SetInputMode(mode, gamepad);
        ApplyPlayerColor(playerIndex);

        Debug.Log($"[PlayerCharacter] Gracz {playerIndex + 1} gotowy " +
                  $"(tryb: {mode}, urządzenie: {GetDeviceLabel()}).");
    }

    public void SetInputMode(PlayerInputMode mode, Gamepad gamepad = null)
    {
        if (_playerIndex > 0 && mode == PlayerInputMode.KeyboardMouse)
        {
            Debug.LogWarning($"[PlayerCharacter] Gracz {_playerIndex + 1} nie może używać klawiatury.");
            return;
        }

        _inputMode = mode;
        _assignedGamepad = mode == PlayerInputMode.Gamepad ? gamepad : null;
    }

    public void SetCombatEnabled(bool enabled)
    {
        _combatEnabled = enabled;
        if (enabled) return;
        _attackMoveMultiplier = 1f;
        _facingLocked = false;
    }

    public bool TryReadAttackInput()
    {
        if (!_combatEnabled) return false;

        if (_inputMode == PlayerInputMode.KeyboardMouse)
            return ReadKeyboardAttack();

        if (_assignedGamepad == null || !_assignedGamepad.enabled)
            return false;

        return _assignedGamepad.buttonWest.wasPressedThisFrame;
    }

    public bool TryReadAttackHeld()
    {
        if (!_combatEnabled) return false;

        if (_inputMode == PlayerInputMode.KeyboardMouse)
            return ReadKeyboardAttackHeld();

        if (_assignedGamepad == null || !_assignedGamepad.enabled)
            return false;

        return _assignedGamepad.buttonWest.isPressed;
    }

    private void Update()
    {
        if (!_combatEnabled) return;

        var moveInput = ReadMoveInput();
        var moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        _currentMoveDirection = moveDirection;
        if (moveInput.sqrMagnitude >= 0.01f)
            _lastMoveDirection = moveDirection.normalized;

        var hasAim = TryReadAimDirection(out var aim);
        _aimDirection = AimMath.ResolveFacing(_aimDirection, moveDirection, hasAim, aim);

        if (!_facingLocked)
            _facingDirection = _aimDirection;

        if (moveInput.sqrMagnitude >= 0.01f)
        {
            var statusSpeed = _statusReceiver != null ? _statusReceiver.MoveSpeedMultiplier : 1f;
            var move = new Vector3(moveInput.x, 0f, moveInput.y) *
                       (CurrentMoveSpeed * _attackMoveMultiplier * statusSpeed * Time.deltaTime);
            var cc = GetComponent<CharacterController>();
            if (cc != null && cc.enabled)
                cc.Move(move);
            else
                transform.Translate(move, Space.World);
        }

        ApplyFacingRotation();
    }

    private bool TryReadAimDirection(out Vector3 aim)
    {
        aim = default;
        var cam = ResolveAimCamera();

        if (_inputMode == PlayerInputMode.KeyboardMouse)
        {
            var mouse = Mouse.current;
            if (mouse == null || cam == null) return false;
            var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            return AimMath.TryAimOnGround(ray, transform.position.y, transform.position, out aim);
        }

        if (_assignedGamepad == null || !_assignedGamepad.enabled) return false;

        var stick = _assignedGamepad.rightStick.ReadValue();
        var camRight = cam != null ? cam.transform.right : Vector3.right;
        var camForward = cam != null ? cam.transform.forward : Vector3.forward;
        var camUp = cam != null ? cam.transform.up : Vector3.up;
        return AimMath.TryWorldAimFromStick(stick, camRight, camForward, camUp, out aim);
    }

    private Camera ResolveAimCamera()
    {
        if (_aimCamera != null) return _aimCamera;

        var shared = FindAnyObjectByType<SharedCamera>();
        if (shared != null)
            _aimCamera = shared.GetComponent<Camera>();
        if (_aimCamera == null)
            _aimCamera = Camera.main;
        return _aimCamera;
    }

    private void ApplyFacingRotation()
    {
        if (_facingDirection.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.LookRotation(_facingDirection, Vector3.up);
    }

    private Vector2 ReadMoveInput()
    {
        if (_inputMode == PlayerInputMode.KeyboardMouse)
            return ReadKeyboardMove();

        if (_assignedGamepad == null || !_assignedGamepad.enabled)
            return Vector2.zero;

        return _assignedGamepad.leftStick.ReadValue();
    }

    private static Vector2 ReadKeyboardMove()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;

        var x = 0f;
        var y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

        var input = new Vector2(x, y);
        return input.sqrMagnitude > 1f ? input.normalized : input;
    }

    private static bool ReadKeyboardAttack()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) return true;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private static bool ReadKeyboardAttackHeld()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard != null && keyboard.spaceKey.isPressed) return true;
        return mouse != null && mouse.leftButton.isPressed;
    }

    private void ApplyPlayerColor(int playerIndex)
    {
        if (_renderer == null || playerIndex >= PlayerColors.Length) return;

        var mat = new Material(_renderer.material);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", PlayerColors[playerIndex]);
        else
            mat.color = PlayerColors[playerIndex];
        _renderer.material = mat;
    }

    private string GetDeviceLabel()
    {
        if (_inputMode == PlayerInputMode.KeyboardMouse)
            return "Keyboard&Mouse";

        return _assignedGamepad != null ? _assignedGamepad.displayName : "brak pada";
    }

    /// <summary>UI level-up — wybór opcji (M7 co-op).</summary>
    public bool TryReadLevelUpOptionInput(int optionCount, ref int selectedIndex)
    {
        if (optionCount <= 0) return false;
        var changed = false;

        if (_inputMode == PlayerInputMode.KeyboardMouse && _playerIndex == 0)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (keyboard.digit1Key.wasPressedThisFrame && optionCount >= 1)
            {
                selectedIndex = 0;
                changed = true;
            }

            if (keyboard.digit2Key.wasPressedThisFrame && optionCount >= 2)
            {
                selectedIndex = 1;
                changed = true;
            }

            if (keyboard.digit3Key.wasPressedThisFrame && optionCount >= 3)
            {
                selectedIndex = 2;
                changed = true;
            }

            if (keyboard.digit4Key.wasPressedThisFrame && optionCount >= 4)
            {
                selectedIndex = 3;
                changed = true;
            }

            if (keyboard.digit5Key.wasPressedThisFrame && optionCount >= 5)
            {
                selectedIndex = 4;
                changed = true;
            }

            if (keyboard.digit6Key.wasPressedThisFrame && optionCount >= 6)
            {
                selectedIndex = 5;
                changed = true;
            }
        }
        else if (_assignedGamepad != null && _assignedGamepad.enabled)
        {
            if (_assignedGamepad.dpad.left.wasPressedThisFrame ||
                _assignedGamepad.buttonWest.wasPressedThisFrame)
            {
                selectedIndex = (selectedIndex - 1 + optionCount) % optionCount;
                changed = true;
            }

            if (_assignedGamepad.dpad.right.wasPressedThisFrame ||
                _assignedGamepad.buttonEast.wasPressedThisFrame)
            {
                selectedIndex = (selectedIndex + 1) % optionCount;
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>UI level-up — zatwierdzenie wyboru (M7 co-op).</summary>
    public bool TryReadLevelUpConfirmInput()
    {
        if (_inputMode == PlayerInputMode.KeyboardMouse && _playerIndex == 0)
        {
            var keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
        }

        if (_assignedGamepad != null && _assignedGamepad.enabled)
            return _assignedGamepad.buttonSouth.wasPressedThisFrame;

        return false;
    }
}
