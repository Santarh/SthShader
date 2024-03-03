using System;
using System.Collections.Generic;

namespace SthShader.Collections
{
    /// <summary>
    /// 優先度付きキュー
    /// Compare して負値を取るものほど優先度が高い
    /// </summary>
    public sealed class PriorityQueue<T> where T : IComparable<T>
    {
        private readonly BinaryHeapTree<T> _binaryHeapTree;

        public int Count => _binaryHeapTree.Count;

        public PriorityQueue(int initialCapacity, IComparer<T> comparer = null)
        {
            _binaryHeapTree = new BinaryHeapTree<T>(initialCapacity, comparer);
        }

        public void Enqueue(in T value)
        {
            _binaryHeapTree.Push(value);
        }

        public bool TryDequeue(out T value)
        {
            return _binaryHeapTree.TryPopRoot(out value);
        }
    }
}