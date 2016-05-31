module Engine.Lang.EscapedString;

// NOTE< struct because it have to be stack managed >
struct EscapedString
{
   class EscapedChar
   {
      this(char Char, bool Escaped)
      {
         this.Char = Char;
         this.Escaped = Escaped;
      }

      public bool Escaped;
      public char Char;

   }

   private EscapedChar []Content;

   public void append(char Char, bool Escaped)
   {
      this.Content ~= new EscapedChar(Char, Escaped);
   }

   public string convertToString()
   {
      string Return;

      foreach (EscapedChar Escaped; this.Content)
      {
         Return ~= Escaped.Char;
      }

      return Return;
   }

   public EscapedChar[] getContent()
   {
      return this.Content;
   }
}
