using PussInHell.Localization;

namespace PussInHell.Hints
{
    [System.Serializable]
    public class LocalizedMessage
    {
        public string key;
        public string russian;
        public string english;
        public string japanese;
        public string ukrainian;
        public string german;
        public string french;
        public string spanish;
        public string turkish;
        public string italian;
        public string polish;

        public string GetText(GameLanguage language)
        {
            switch (language)
            {
                case GameLanguage.Russian: return russian;
                case GameLanguage.Japanese: return japanese;
                case GameLanguage.Ukrainian: return ukrainian;
                case GameLanguage.German: return german;
                case GameLanguage.French: return french;
                case GameLanguage.Spanish: return spanish;
                case GameLanguage.Turkish: return turkish;
                case GameLanguage.Italian: return italian;
                case GameLanguage.Polish: return polish;
                default: return english;
            }
        }
    }
}
