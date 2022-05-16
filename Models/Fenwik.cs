using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace leetSharp
{
    
	
	public class FenwickTree
    {
        //https://algorithmica.org/ru/fenwick
        private readonly long[] _array;
        public readonly int Count;
        public int logn = 1;

        public FenwickTree(int size)
        {
            _array = new long[size + 1];
            Count = size;
            logn = 1;
            while (logn * 2 < Count) logn *= 2;
        }

        /// <summary>
        /// A[i] добавить n
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        public void Add(int i, long n)
        {
            i++;
            for (; i <= Count; i += i & -i)
            {
                _array[i] += n;
            }
        }

        /// <summary>
        /// [0,r) сумма
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public long Sum(int r)
        {
            long result = 0;
            for (; r > 0; r -= r & -r)
            {
                result += _array[r];
            }

            return result;
        }

        /// <summary>
        /// [0,i) Сумма i больше или равна w
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public int LowerBound(long w)
        {
            if (w <= 0) return 0;
            int x = 0;
            int k = logn;
            for (; k > 0; k /= 2)
            {
                if (x + k < Count && _array[x + k] < w)
                {
                    w -= _array[x + k];
                    x += k;
                }
            }
            return x + 1;
        }

        // [l,r) сумма
        public long Sum(int l, int r) => Sum(r) - Sum(l);
    }





    public class FenwickTree_Max
    {
        //https://e-maxx.ru/algo/fenwick_tree
        private int[] t;
        public readonly int n;
        const int INF = 0; //int.MinValue;

        public FenwickTree_Max(int size)
        {
            t = new int[size];
            n = size;
            for (int i = 0; i < n; i++)
                t[i] = INF;

        }

        public int getMax(int r)
        {
            if (r < 0)
                r = 0;
            if (r > n - 1)    
                r = n - 1;
            int result = INF;
            for (; r >= 0; r = (r & (r + 1)) - 1)
                result = Math.Max(result, t[r]);
            return result;
        }

        public void update(int i, int new_val)
        {
            for (; i < n; i = (i | (i + 1)))
                t[i] = Math.Max(t[i], new_val);
        }


    }

}