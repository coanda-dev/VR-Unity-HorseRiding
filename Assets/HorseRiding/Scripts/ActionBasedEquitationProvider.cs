using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace HorseRiding.Scripts
{
    [AddComponentMenu("XR/Locomotion/Equitation Provider (Action-based)")]
    public class ActionBasedEquitationProvider : LocomotionProvider
    {
        // Measurements
        [SerializeField] private int _numberOfMeasurements = 1000;

        private Vector3[] _measurements;

        private int _insertIdx;

        // Log measuremnts
        private string _path = @"C:\Users\raduc\Develop\VR-Unity-HorseRiding\Assets\Developer\Logging\measurements_" +
                              DateTime.Now.ToFileTimeUtc() + ".csv";

        // Velocity
        private float _velocity = 0f;


        // Inputs 
        [SerializeField] private InputActionProperty _hmdPosition;

        public InputActionProperty HmdPosition
        {
            get => _hmdPosition;
            set => SetInputActionProperty(ref _hmdPosition, value);
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        #region Lifecycle Methods

        protected override void Awake()
        {
            base.Awake();

            _insertIdx = 0;
            _measurements = new Vector3[_numberOfMeasurements];
        }

        private void OnEnable()
        {
            using (StreamWriter _measurementsLog = File.CreateText(_path))
            {
                _measurementsLog.WriteLine("Timestamp; Position; ID");    
            }

            
        }

        private void FixedUpdate()
        {
            if (_insertIdx < _measurements.Length)
            {
                Vector3 measurement = _hmdPosition.action?.ReadValue<Vector3>() ?? Vector3.zero;
                using (StreamWriter _measurementsLog = File.AppendText(_path))
                    _measurementsLog.WriteLine(
                        $"{Time.fixedTime.ToString()};{measurement.ToString("F8")};{_insertIdx.ToString()}");
                _measurements[_insertIdx++] = measurement;
            }
        }

        private void Update()
        {
            // 1. compute velocity
            if (_insertIdx >= _measurements.Length * .9f)
            {
                _insertIdx = 0;
            }

            // 2. apply velocity to the XR Rig
        }

        #endregion
    }
}