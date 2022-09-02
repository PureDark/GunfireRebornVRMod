using InControl;
using System;
using UnityEngine;
using static VRMod.VRMod;

namespace VRMod.Player.VRInput
{
    public class VRInputManager : MonoBehaviour
    {
        public VRInputManager(IntPtr value) : base(value) { }

        public static VRInputManager Instance { get; private set; }

        public VRInputDevice vrDevice;


        protected VRInputManager() { }


        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            var manager = GetComponent<InControlManager>();
            if (manager == null)
            {
                Log.Error("VR Input Manager component can only be added to the InControl Manager object.");
                DestroyImmediate(this);
                return;
            }

            if (Application.isPlaying)
            {
                InputManager.OnSetup += new Action(Setup);
                InputManager.OnUpdateDevices += new Action<ulong, float>(UpdateDevice);
                InputManager.OnCommitDevices += new Action<ulong, float>(CommitDevice);
            }
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                InputManager.OnSetup -= new Action(Setup);
                InputManager.OnUpdateDevices -= new Action<ulong, float>(UpdateDevice);
                InputManager.OnCommitDevices -= new Action<ulong, float>(CommitDevice);
            }
            InputManager.DetachDevice(vrDevice);
            vrDevice = null;
        }

        void Setup()
        {
            vrDevice = new VRInputDevice();
            InputManager.AttachDevice(vrDevice);
        }

        void UpdateDevice(ulong updateTick, float deltaTime)
        {
            if (vrDevice != null)
                vrDevice.UpdateInternal(updateTick, deltaTime);
        }
        void CommitDevice(ulong updateTick, float deltaTime)
        {
            if (vrDevice != null)
                vrDevice.CommitInternal(updateTick, deltaTime);
        }

        #region Static interface.

        public static InputDevice Device
        {
            get { return Instance.vrDevice; }
        }

        #endregion

        public static implicit operator bool(VRInputManager instance)
        {
            return instance != null;
        }
    }
}
