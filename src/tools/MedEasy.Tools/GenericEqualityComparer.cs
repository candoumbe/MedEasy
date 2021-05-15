namespace MedEasy.Tools
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Generic equality comparer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericEqualityComparer<T> : EqualityComparer<T>
    {
        private readonly Func<T, int> _hashFunc;
        private readonly Func<T, T, bool> _comparerFunction;

        public GenericEqualityComparer(Func<T, T, bool> comparerFunc, Func<T, int> hashFunc)
        {
            _hashFunc = hashFunc;
            _comparerFunction = comparerFunc;
        }

        public override bool Equals(T x, T y)
        {
            return _comparerFunction(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return _hashFunc(obj);
        }
    }
}
