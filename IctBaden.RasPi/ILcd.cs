namespace IctBaden.RasPi
{
    public interface ILcd
    {
        bool Backlight { get; set; }
        int Lines { get; }
        int Columns { get; }

        void Clear();
        void SetCursor(int col, int row);   // 1..n
        void Print(string text);
    }
}

