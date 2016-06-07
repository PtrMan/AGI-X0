module misc.IQueue;

interface IQueue(DataType) {
   public void insert(DataType Data);
   
   public void peek(out bool Success, out DataType element);

   public bool isEmpty();

   public void flush();
}
