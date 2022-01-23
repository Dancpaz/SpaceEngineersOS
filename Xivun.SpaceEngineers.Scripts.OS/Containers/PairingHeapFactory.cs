using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Containers
{
    public class PairingHeapFactory
    {
        public PairingHeap<T> Create<T>(IComparer<T> comparer) =>
            new PairingHeap<T>(comparer, CreatePool<T>());

        public PairingHeap<T> Create<T>(IComparer<T> comparer, IPool<PairingHeap<T>.Node> nodePool) =>
            new PairingHeap<T>(comparer, nodePool);

        public virtual IPool<PairingHeap<T>.Node> CreatePool<T>() =>
            new ObjectPool<PairingHeap<T>.Node>(() => new PairingHeap<T>.Node());
    }
}