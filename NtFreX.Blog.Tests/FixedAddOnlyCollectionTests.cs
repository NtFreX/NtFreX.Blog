using System.Linq;
using Xunit;

namespace NtFreX.Blog.Tests
{
    public class FixedAddOnlyCollectionTests
    {
        [Fact]
        public void CanReadFromCollectionWhenOverflown()
        {
            var size = 5;
            var collection = new FixedAddOnlyCollection<int>(size);
            collection.Add(1, 2, 3, 4, 5, 6, 7);
            var items = collection.Peek(4);

            Assert.True(Enumerable.SequenceEqual(items, new[] { 7, 6, 5, 4 }));
        }
    }
}