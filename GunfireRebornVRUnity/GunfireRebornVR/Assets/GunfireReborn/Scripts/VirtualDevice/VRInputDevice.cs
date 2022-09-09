	using InControl;
	using UnityEngine;
    using Valve.VR;


// An example of how to map keyboard/mouse input (or anything else) to a virtual device.
//
public class VRInputDevice : InputDevice
{
    public SteamVR_Action_Boolean SnapTurnLeft() => SteamVR_Actions.gameplay_SnapTurnLeft;
    public SteamVR_Action_Boolean SnapTurnRight() => SteamVR_Actions.gameplay_SnapTurnRight;
    public SteamVR_Action_Single LT_WeaponSkill() => SteamVR_Actions.gameplay_LT_WeaponSkill;
    public SteamVR_Action_Single RT_Fire() => SteamVR_Actions.gameplay_RT_Fire;
    public SteamVR_Action_Boolean LB_PrimarySkill() => SteamVR_Actions.gameplay_LB_PrimarySkill;
    public SteamVR_Action_Boolean RB_SecondarySkill() => SteamVR_Actions.gameplay_RB_SecondarySkill;
    public SteamVR_Action_Boolean X_Interact_Reload() => SteamVR_Actions.gameplay_X_Interact_Reload;
    public SteamVR_Action_Boolean Y_SwitchWeapons() => SteamVR_Actions.gameplay_Y_SwitchWeapons;
    public SteamVR_Action_Boolean A_Jump() => SteamVR_Actions.gameplay_A_Jump;
    public SteamVR_Action_Boolean B_Dash_ReturnUI() => SteamVR_Actions.gameplay_B_Dash_ReturnUI;
    public SteamVR_Action_Boolean DPadU_Ping() => SteamVR_Actions.gameplay_DPadU_Ping;
    public SteamVR_Action_Boolean DPadL_InterruptCharging() => SteamVR_Actions.gameplay_DPadL_InterruptCharging;
    public SteamVR_Action_Boolean DPadD_SwitchFireMode() => SteamVR_Actions.gameplay_DPadD_SwitchFireMode;
    public SteamVR_Action_Boolean DPadR_TeamInformation() => SteamVR_Actions.gameplay_DPadR_TeamInformation;
    public SteamVR_Action_Boolean R3_Speak() => SteamVR_Actions.gameplay_R3_Speak;
    public SteamVR_Action_Vector2 LS_Move() => SteamVR_Actions.gameplay_LS_Move;
    public SteamVR_Action_Vector2 RS_Rotate() => SteamVR_Actions.gameplay_RS_Rotate;


    public static bool DisableRT = false;
    const float LowerDeadZone = 0.2f;
    const float UpperDeadZone = 0.9f;


    public VRInputDevice()
        : base("VR Input Device")
    {
        DeviceClass = InputDeviceClass.Controller;
        DeviceStyle = InputDeviceStyle.XboxOne;

        AddControl(InputControlType.LeftStickLeft, "Left Stick Left", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.LeftStickRight, "Left Stick Right", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.LeftStickUp, "Left Stick Up", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.LeftStickDown, "Left Stick Down", LowerDeadZone, UpperDeadZone);

        AddControl(InputControlType.RightStickLeft, "Right Stick Left", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.RightStickRight, "Right Stick Right", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.RightStickUp, "Right Stick Up", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.RightStickDown, "Right Stick Down", LowerDeadZone, UpperDeadZone);

        AddControl(InputControlType.DPadUp, "DPad Up", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.DPadDown, "DPad Down", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.DPadLeft, "DPad Left", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.DPadRight, "DPad Right", LowerDeadZone, UpperDeadZone);

        AddControl(InputControlType.LeftTrigger, "Left Trigger", LowerDeadZone, UpperDeadZone);
        AddControl(InputControlType.RightTrigger, "Right Trigger", LowerDeadZone, UpperDeadZone);

        AddControl(InputControlType.LeftBumper, "LeftBumper");
        AddControl(InputControlType.RightBumper, "RightBumper");

        AddControl(InputControlType.Action1, "A");
        AddControl(InputControlType.Action2, "B");
        AddControl(InputControlType.Action3, "X");
        AddControl(InputControlType.Action4, "Y");

        AddControl(InputControlType.LeftStickButton, "Left Stick Button");
        AddControl(InputControlType.RightStickButton, "Right Stick Button");

        AddControl(InputControlType.View, "View");
        AddControl(InputControlType.Menu, "Menu");


    }

    public override void Update(ulong updateTick, float deltaTime)
    {
        UpdateInternal(updateTick, deltaTime);
    }


    // This method will be called by the input manager every update tick.
    // You are expected to update control states where appropriate passing
    // through the updateTick and deltaTime unmodified.
    //
    public void UpdateInternal(ulong updateTick, float deltaTime)
    {
        var leftStickVector = LS_Move().axis;
        UpdateLeftStickWithValue(leftStickVector, updateTick, deltaTime);

        var rightStickVector = RS_Rotate().axis;
        UpdateRightStickWithValue(rightStickVector, updateTick, deltaTime);

        Debug.Log("rightStickVector " + rightStickVector);
        Debug.Log("RightStick.Vector " + RightStick.Vector);


        // For float values use:
        // UpdateWithValue( InputControlType.LeftStickLeft, floatValue, updateTick, deltaTime );

        // Read from SteamVR actions to submit into action buttons.
        UpdateWithState(InputControlType.Action1, A_Jump().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.Action2, B_Dash_ReturnUI().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.Action3, X_Interact_Reload().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.Action4, Y_SwitchWeapons().state, updateTick, deltaTime);

        UpdateWithState(InputControlType.LeftBumper, LB_PrimarySkill().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.RightBumper, RB_SecondarySkill().state, updateTick, deltaTime);


        UpdateWithState(InputControlType.DPadUp, DPadU_Ping().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.DPadLeft, DPadL_InterruptCharging().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.DPadDown, DPadD_SwitchFireMode().state, updateTick, deltaTime);
        UpdateWithState(InputControlType.DPadRight, DPadR_TeamInformation().state, updateTick, deltaTime);


        UpdateWithValue(InputControlType.LeftTrigger, LT_WeaponSkill().axis, updateTick, deltaTime);

        if (!DisableRT)
            UpdateWithValue(InputControlType.RightTrigger, RT_Fire().axis, updateTick, deltaTime);

        UpdateWithState(InputControlType.RightStickButton, R3_Speak().state, updateTick, deltaTime);

    }

    public void CommitInternal(ulong updateTick, float deltaTime)
    {
        Commit(updateTick, deltaTime);
    }
}
