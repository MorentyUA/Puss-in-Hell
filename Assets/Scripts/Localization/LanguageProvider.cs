using System;

namespace PussInHell.Localization
{
    public static class LanguageProvider
    {
        private static GameLanguage current = GameLanguage.English;

        public static GameLanguage Current
        {
            get => current;
            set
            {
                if (current == value) return;
                current = value;
                Changed?.Invoke(current);
            }
        }

        public static event Action<GameLanguage> Changed;

        public static void Refresh()
        {
            Changed?.Invoke(current);
        }
    }
}
