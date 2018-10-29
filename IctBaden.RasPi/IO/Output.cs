namespace IctBaden.RasPi.IO
{
    public class Output
    {
        private readonly DigitalIo _io;
        private readonly int _index;

        public Output(DigitalIo io, int index)
        {
            _io = io;
            _index = index;
        }

        /// <summary>
        /// Returns output's current value.
        /// </summary>
        /// <param name="output">Output to be queried</param>
        public static implicit operator bool(Output output)
        {
            return output._io.GetOutput(output._index);
        }

        /// <summary>
        /// Sets the output to the given value.
        /// </summary>
        /// <param name="value">True to set output</param>
        public void Set(bool value)
        {
            _io.SetOutput(_index, value);
        }

    }
}
