using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Snork.EventBus
{
    public class ConcurrentList<T> : IList<T>
    {
        private readonly List<T> _myList = new List<T>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(T item)
        {
            _myList.Add(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            _myList.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(T item)
        {
            return _myList.Contains(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _myList.CopyTo(array, arrayIndex);
        }

        public int Count => _myList.Count;

        public bool IsReadOnly => false;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Remove(T item)
        {
            return _myList.Remove(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerator<T> GetEnumerator()
        {
            return _myList.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int IndexOf(T item)
        {
            return _myList.IndexOf(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Insert(int index, T item)
        {
            _myList.Insert(index, item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveAt(int index)
        {
            _myList.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _myList[index];
            set => _myList[index] = value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int RemoveAll(Predicate<T> func)
        {
            return _myList.RemoveAll(func);
        }
    }
}