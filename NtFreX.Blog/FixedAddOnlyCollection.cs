using System;
using System.Collections.Generic;
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

        public void Add(params T[] items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Add(T item)
        {
            if(++position >= data.Length)
                position = 0;

            data[position] = item;
            
            if(dataCount < data.Length)
                dataCount++;
        }

        public IEnumerable<T> PeekIncomplete(int count, out int actualCount)
        {
            actualCount = Math.Min(dataCount, count);
            return Peek(actualCount);
        }

        public IEnumerable<T> Peek(int count)
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
                return SkipAndTake(position + 1 - count, count);
            }

            IEnumerable<T> left = Array.Empty<T>();
            var leftLength = 0;
            if (position >= 0)
            {
                leftLength = position + 1;
                left = SkipAndTake(0, leftLength).Reverse();
            }

            var leftOver = count - leftLength;
            var right = SkipAndTake(data.Length - leftOver, leftOver).Reverse();

            return left.Concat(right);
        }

        public bool TryPeek(out T item)
        {
            var success = TryPeek(1, out var items);
            item = success ? items.First() : default;
            return success;
        }

        public bool TryPeek(int count, out IEnumerable<T> items)
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

        private IEnumerable<T> SkipAndTake(int skip, int take)
        {
            var stop = skip + take;
            for(var i = skip; i < stop; i++)
            {
                yield return data[i];
            }
        }
    }
}