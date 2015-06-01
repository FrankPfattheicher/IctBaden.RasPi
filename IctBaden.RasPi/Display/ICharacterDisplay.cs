namespace IctBaden.RasPi.Display
{
    public interface ICharacterDisplay
    {
        bool Backlight { get; set; }
        int Lines { get; }
        int Columns { get; }

        void Clear();
        void SetCursor(int col, int row);   // 1..n

        void SetContrast(int contrast);  // 0..100%
        void Print(string text);
    }
}

