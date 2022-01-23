using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Containers
{
    public class PairingHeap<T> : IQueue<T>
    {
        IComparer<T> Comparer;
        IPool<Node> NodePool;
        Node Root;

        public int Count { get; private set; }

        public PairingHeap(IComparer<T> comparer, IPool<Node> nodePool)
        {
            Comparer = comparer;
            NodePool = nodePool;
        }

        public void Enqueue(T item)
        {
            var node = NodePool.Reserve();
            node.Reset(item);
            Root = Merge(Root, node);
            Count++;
        }

        public T Dequeue()
        {
            if (Root == null)
                throw new InvalidOperationException($"Empty.");

            return DequeueUnsafe();
        }

        public bool TryDequeue(out T item)
        {
            if (Root == null)
            {
                item = default(T);
                return false;
            }

            item = DequeueUnsafe();
            return true;
        }


        public T Peek()
        {
            if (Root == null)
                throw new InvalidOperationException($"Empty.");

            return Root.Item;
        }

        public bool TryPeek(out T item)
        {
            if (Root == null)
            {
                item = default(T);
                return false;
            }

            item = Root.Item;
            return true;
        }

        public void Clear()
        {
            Root = null;
            Count = 0;
        }

        private T DequeueUnsafe()
        {
            var root = Root;
            var item = root.Item;

            Root = Extract(root);

            root.Item = default(T);

            NodePool.Release(root);
            Count--;

            return item;
        }


        private Node Merge(Node a, Node b)
        {
            if (a == null) return b;
            if (b == null) return a;

            return Comparer.Compare(a.Item, b.Item) < 0
                ? SetChild(a, b)
                : SetChild(b, a);
        }

        private Node SetChild(Node parent, Node child)
        {
            if (parent.Child != null)
                child.Sibling = parent.Child;

            parent.Child = child;
            return parent;
        }

        private Node Extract(Node node)
        {
            if (node.Child == null)
                return null;

            Node result = null;

            var n = node.Child;
            while (n != null)
            {
                if (n.Sibling == null)
                    return Merge(result, n);

                var pair = n.Sibling;
                n.Sibling = null;

                var next = pair.Sibling;
                pair.Sibling = null;

                result = Merge(result, Merge(n, pair));

                n = next;
            }

            return result;
        }

        public class Node
        {
            // We can't afford to constantly check this for nulls, and we know it will never be null when we use it.
            public T Item { get; set; }

            public Node Child { get; set; }
            public Node Sibling { get; set; }

            public void Reset(T item)
            {
                Item = item;
                Child = null;
                Sibling = null;
            }
        }
    }
}