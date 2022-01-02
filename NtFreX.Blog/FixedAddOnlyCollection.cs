using System;
using System.Linq;

namespace NtFreX.Blog
{
    public class FixedAddOnlyCollection<T>
    {
        private readonly T[] data;
        private int position = -1;
        private int dataCount = 0;

        public FixedAddOnlyCollection(int size)
        {
            data = new T[size];
        }

        public void Add(T item)
        {
            if(++position >= data.Length)
                position = 0;

            data[position] = item;
            
            if(dataCount < data.Length)
                dataCount++;
        }

        public T[] PeekIncomplete(int count, out int actualCount)
        {
            actualCount = Math.Min(dataCount, count);
            return Peek(actualCount);
        }

        public T[] Peek(int count)
        {
            if (count > data.Length)
            {
                throw new Exception($"The collection has a max size of {data.Length} which means it can never return {count} items");
            }

            if (dataCount < count)
            {
                throw new Exception($"The collection has only {dataCount} items and cannot return {count} items");
            }

            if (position > count - 1)
            {
                return data.Skip(position + 1 - count).Take(count).ToArray();
            }

            var left = position >= 0 ? data.Take(position + 1).ToArray() : Array.Empty<T>();

            var leftOver = count - left.Length;
            var right = data.Skip(data.Length - leftOver).Take(leftOver).ToArray();

            return left.Concat(right).ToArray();
        }

        public bool TryPeek(out T item)
        {
            var success = TryPeek(1, out var items);
            item = success ? items[0] : default;
            return success;
        }

        public bool TryPeek(int count, out T[] items)
        {
            if(count > data.Length)
            {
                throw new Exception($"The collection has a max size of {data.Length} which means it can never return {count} items");
            }

            if (dataCount < count)
            {
                items = default;
                return false;
            }

            items = Peek(count);
            return true;
        }
    }
}