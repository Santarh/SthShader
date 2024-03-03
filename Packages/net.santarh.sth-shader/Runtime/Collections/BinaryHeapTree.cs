using System;
using System.Collections.Generic;

namespace SthShader.Collections
{
    /// <summary>
    /// 二分ヒープ木
    /// Compare して負値を取るものほどルートに近づく
    /// </summary>
    public sealed class BinaryHeapTree<T> where T : IComparable<T>
    {
        private readonly IComparer<T> _comparer;
        private T[] _array;

        public int Count { get; private set; }

        public BinaryHeapTree(int initialCapacity, IComparer<T> comparer = null)
        {
            _array = new T[initialCapacity];
            _comparer = comparer;
        }

        public void Push(in T value)
        {
            if (_array.Length == Count)
            {
                Array.Resize(ref _array, _array.Length * 2);
            }

            var nextIndex = Count;
            Count += 1;
            _array[nextIndex] = value;
            SiftUp(nextIndex);
        }

        public bool TryPopRoot(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            value = _array[0];
            Count -= 1;
            _array[0] = _array[Count];
            SiftDown(0);
            return true;
        }

        private void SiftUp(int index)
        {
            var parentIndex = (index - 1) / 2;
            while (index > 0 && Compare(_array[index], _array[parentIndex]) < 0)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                var leftChildIndex = index * 2 + 1;
                var rightChildIndex = index * 2 + 2;
                var minIndex = index;

                if (leftChildIndex < Count && Compare(_array[leftChildIndex], _array[minIndex]) < 0)
                {
                    minIndex = leftChildIndex;
                }
                if (rightChildIndex < Count && Compare(_array[rightChildIndex], _array[minIndex]) < 0)
                {
                    minIndex = rightChildIndex;
                }

                if (minIndex == index)
                {
                    break;
                }

                Swap(index, minIndex);
                index = minIndex;
            }
        }

        private int Compare(in T value0, in T value1)
        {
            if (_comparer != null)
            {
                return _comparer.Compare(value0, value1);
            }
            else
            {
                return value0.CompareTo(value1);
            }
        }

        private void Swap(int index0, int index1)
        {
            var temp = _array[index0];
            _array[index0] = _array[index1];
            _array[index1] = temp;
        }
    }
}