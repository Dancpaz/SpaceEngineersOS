using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Containers
{
    public interface IQueue<T>
    {
        int Count { get; }

        void Enqueue(T item);
        T Peek();
        T Dequeue();
        
        bool TryDequeue(out T item);
        bool TryPeek(out T item);
        
        void Clear();
    }
}