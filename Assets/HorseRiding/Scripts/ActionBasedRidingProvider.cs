using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace HorseRiding.Scripts
{
    [AddComponentMenu("XR/Locomotion/Riding Provider (Action-based)")]
    public class ActionBasedRidingProvider : LocomotionProvider
    {
        #region Inputs

        [SerializeField] private InputActionProperty _hmdPos;

        public InputActionProperty HmdPosition
        {
            get => _hmdPos;
            set => SetInputActionProperty(ref _hmdPos, value);
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        #endregion

        #region Lifecycle Methods

        protected override void Awake()
        {
            base.Awake();

            _measurementsRingBuffer = new float[_measurementsCount];
        }

        private void FixedUpdate()
        {
            RecordHeadPos();
        }

        private void Update()
        {
            float magnitude = ComputeVelocityMagnitude();
            Debug.Log($"Magnitude: {magnitude.ToString(CultureInfo.InvariantCulture)}");
        }

        #endregion

        #region Locomotion Logic

        [SerializeField, Range(50, 500)] private uint _measurementsCount = 100;
        [SerializeField, Range(5, 20)] private uint _windowSize = 5;
        [SerializeField, Range(1, 50)] private float _speedFactor = 1.0f;
        [SerializeField, Range(1, 100)] private int _allowedErrorInCm = 1;

        public float AllowedErrorInMeters => (float) _allowedErrorInCm / 1000;

        private float[] _measurementsRingBuffer;
        private uint _insertPtr = 0;

        private void RecordHeadPos()
        {
            Vector3 headPos = _hmdPos.action?.ReadValue<Vector3>() ?? Vector3.zero;
            _measurementsRingBuffer[_insertPtr % _measurementsCount] = headPos.y;
            _insertPtr++;
        }

        private float ComputeVelocityMagnitude()
        {
            uint k = _windowSize / 2;
            uint changesCount = 0;
            List<float> amplitudes = new List<float>();
            
            // Detect peak or trough
            for (uint i = k; i < _measurementsCount - k; i++)
            {
                uint idx = (i + _insertPtr) % _measurementsCount;
                float min = float.MaxValue;
                float max = float.MinValue;

                // Find the min/max values in a neighbourhood around current idx
                for (uint j = 0; j < _windowSize; j++)
                {
                    uint nIdx = (j + idx - k) % _measurementsCount;
                    if (nIdx == idx) continue;

                    if (_measurementsRingBuffer[nIdx] < min) min = _measurementsRingBuffer[nIdx];
                    if (_measurementsRingBuffer[nIdx] > max) max = _measurementsRingBuffer[nIdx];
                }
                
                // Check whether value at idx is at peak or at trough
                if (min - _measurementsRingBuffer[idx] > AllowedErrorInMeters ||
                    _measurementsRingBuffer[idx] - max > AllowedErrorInMeters)
                {
                    changesCount++;
                    amplitudes.Add(_measurementsRingBuffer[idx]);
                }
            }

            float amplitude = 0.0f;
            if (amplitudes.Count > 0)
                amplitude = amplitudes.Average();
            
            return changesCount * amplitude * _speedFactor;
        }

        #endregion
    }
}