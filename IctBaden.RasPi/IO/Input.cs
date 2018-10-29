namespace IctBaden.RasPi.IO
{
    public class Input
    {
        private readonly DigitalIo _io;
        private readonly int _index;

        public Input(DigitalIo io, int index)
        {
            _io = io;
            _index = index;
        }

        /// <summary>
        /// Returns the input's current value.
        /// </summary>
        /// <param name="input">Input to be queried</param>
        public static implicit operator bool(Input input)
        {
            return input._io.GetInput(input._index);
        }

    }
}
