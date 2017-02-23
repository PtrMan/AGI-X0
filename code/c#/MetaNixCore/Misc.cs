namespace MetaNix {
    class Misc {
        // used for parsing
        static public bool isLetter(char text) {
            return text.ToString().ToLower().IndexOfAny(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }) != -1;
        }
    }
}
