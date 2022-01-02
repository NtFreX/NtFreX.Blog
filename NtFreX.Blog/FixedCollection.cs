namespace NtFreX.Blog
{
    public class FixedCollection<T>
    {
        private readonly T[] data;
        private int position = -1;

        public FixedCollection(int size)
        {
            data = new T[size];
        }

        public void Add(T item)
        {
            if(++position >= data.Length)
                position = 0;

            data[position] = item;
        }

        public bool TryPeek(out T item)
        {
            if (position < 0)
            {
                item = default;
                return false;
            }

            item = data[position];
            return true;
        }
    }
}