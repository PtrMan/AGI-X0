module Engine.Lang.Nullable;

template Nullable(ValueType)
{
   public class Nullable
   {
      private bool     IsNull;
      public ValueType Value;

      this(bool IsNull, ValueType Value)
      {
         this.IsNull = IsNull;
         this.Value  = Value;
      }

      bool isNull()
      {
         return this.IsNull;
      }

      void setNull()
      {
         this.IsNull = true;
      }
   }
}