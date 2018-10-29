using System;
using System.Collections.Generic;
using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.IO
{
    public class DigitalIo
    {
        public static readonly int[] DefaultInputAssignment = { /* GPIO */ 17, 27, 22, 18 };
        public static readonly int[] DefaultOutputAssignment = { /* GPIO */ 7, 8, 9, 10, 11, 23, 24, 25 };

        private int[] _outputAssignment;
        private bool[] _outputValues;

        /// <summary>
        /// GPIO numbers used as digital inputs.
        /// Call Initialize() after changing this.
        /// </summary>
        public int[] InputAssignment { get; set; }

        /// <summary>
        /// GPIO numbers used as digital outputs.
        /// Call Initialize() after changing this.
        /// </summary>
        public int[] OutputAssignment
        {
            get => _outputAssignment;
            set
            {
                _outputAssignment = value;
                _outputValues = new bool[_outputAssignment.Length];
                for (var ix = 0; ix < _outputValues.Length; ix++)
                {
                    _outputValues[ix] = false;
                }
            }
        }

        /// <summary>
        /// GPIO numbers and I/O ALT-mode to use with
        /// Call Initialize() after changing this.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Dictionary<uint, uint> IoMode { get; set; } = new Dictionary<uint, uint>();

        /// <summary>
        /// Count of defined inputs.
        /// </summary>
        public int InputsCount => InputAssignment.Length;

        /// <summary>
        /// Count of defined outputs.
        /// </summary>
        public int OutputsCount => _outputAssignment.Length;

        public DigitalIo()
            : this(DefaultInputAssignment, DefaultOutputAssignment)
        {
        }
        public DigitalIo(int[] inputAssignment, int[] outputAssignment)
        {
            InputAssignment = inputAssignment;
            OutputAssignment = outputAssignment;
        }
        public DigitalIo(int[] inputAssignment, int[] outputAssignment, Dictionary<uint, uint> ioMode)
        {
            InputAssignment = inputAssignment;
            OutputAssignment = outputAssignment;
            IoMode = ioMode;
        }

        /// <summary>
        /// Initialize digital IOs with current assignmends and modes.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                RawGpio.Initialize();
            }
            catch (Exception)
            {
                return false;
            }

            foreach (var mode in IoMode)
            {
                RawGpio.INP_GPIO(mode.Key);
                RawGpio.SET_GPIO_ALT(mode.Key, mode.Value);
            }

            foreach (var input in InputAssignment)
            {
                RawGpio.INP_GPIO((uint)input);
            }
            
            foreach (var output in _outputAssignment)
            {
                RawGpio.INP_GPIO((uint)output); // must use INP_GPIO before we can use OUT_GPIO
                RawGpio.OUT_GPIO((uint)output);
            }
            
            return true;
        }

        /// <summary>
        /// Sets the given output to the given value.
        /// </summary>
        /// <param name="index">Index of the output within the assignments</param>
        /// <param name="value">New output value to be set</param>
        public void SetOutput(int index, bool value)
        {
            if ((index < 0) || (index >= OutputsCount))
            {
                throw new ArgumentException("Output out of range", nameof(index));
            }
            if (!RawGpio.IsInitialized)
            {
                return;
            }
            if (value)
            {
                RawGpio.GPIO_SET = (uint)(1 << _outputAssignment[index]);
            }
            else
            {
                RawGpio.GPIO_CLR = (uint)(1 << _outputAssignment[index]);
            }
            _outputValues[index] = value;
        }

        /// <summary>
        /// Returns the current value of the given output.
        /// </summary>
        /// <param name="index">Index of the output within the assignments</param>
        /// <returns>Current output's value</returns>
        public bool GetOutput(int index)
        {
            if ((index < 0) || (index >= OutputsCount))
            {
                throw new ArgumentException("Output out of range", nameof(index));
            }
            return RawGpio.IsInitialized && _outputValues[index];
        }

        /// <summary>
        /// Returns the current value of the given input.
        /// </summary>
        /// <param name="index">Index of the input within the assignments</param>
        /// <returns>Current input's value</returns>
        public bool GetInput(int index)
        {
            if ((index < 0) || (index >= InputsCount))
            {
                throw new ArgumentException("Input out of range", nameof(index));
            }

            return (RawGpio.GPIO_IN0 & (uint)(1 << InputAssignment [index])) != 0;
        }

        /// <summary>
        /// Retruns the state of all defined inputs within the assignment.
        /// </summary>
        /// <returns>State of all inputs</returns>
        public ulong GetInputs()
        {
            ulong inputs = 0;
            for (var ix = 0; ix < InputsCount; ix++)
            {
                if (GetInput(ix))
                {
                    inputs |= (ulong)1 << ix;
                }
            }
            return inputs;
        }

    }
}
