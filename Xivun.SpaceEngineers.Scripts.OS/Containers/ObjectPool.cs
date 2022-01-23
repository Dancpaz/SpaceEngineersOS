using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Containers
{
    public class ObjectPool<T> : IPool<T>
    {
        private Stack<T> Items { get; }
        private Func<T> ItemFactory { get; }

        public ObjectPool(Func<T> itemFactory)
        {
            Items = new Stack<T>();
            ItemFactory = itemFactory;
        }

        public T Reserve() =>
            Items.Count > 0
                ? Items.Pop()
                : ItemFactory();

        public void Release(T item)
        {
            Items.Push(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

    }
}