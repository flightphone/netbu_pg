using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace leetSharp
{
    
    //Декартово дерево
    public class Cartesian_item<T> where T : IComparable<T>
    {
        public T key { get; set; }
        public Guid prior { get; set; }
        public long sum = 0;
        public long ordsum = 0;
        public int cnt = 1;
        public int ord = 0;
        public long min = 0;
        public long nmin = 0;

        public long value { get; set; }


        public Cartesian_item<T> l;
        public Cartesian_item<T> r;
        public Cartesian_item()
        {
            prior = Guid.NewGuid();
            l = null;
            r = null;
        }
        public Cartesian_item(T _key, long _val)
        {
            this.key = _key;
            this.value = _val;
            this.sum = _val;
            this.min = _val;
            prior = Guid.NewGuid();
            l = null;
            r = null;
        }

        /*
             public void upd_cnt()
             {
                 cnt = 1;
                 if (l != null)
                     cnt += l.cnt;
                 if (r != null)
                     cnt += r.cnt;
             }

             public void upd_sum()
             {
                 sum = value;
                 if (l != null)
                     sum += l.sum;
                 if (r != null)
                     sum += r.sum;
             }
         */

        public void upd_min()
        {
            min = value;
            cnt = 1;
            sum = value;
            if (l != null)
            {
                min = Math.Max(min, l.min);
                cnt += l.cnt;
                sum += l.sum;

            }
            if (r != null)
            {
                min = Math.Max(min, r.min);
                cnt += r.cnt;
                sum += r.sum;
            }

            /*
            nmin = 0;
            if (l != null && min == l.min)
                nmin += l.nmin;

            if (r != null && min == r.min)
                nmin += r.nmin;

            if (min == value)
                nmin++;
            */

        }
    }

    public class Cartesian_tree<T> where T : IComparable<T>
    {
        public Cartesian_item<T> root = null;
        public void dfs()
        {
            dfs(root);
        }

        public void dfs(Cartesian_item<T> t)
        {
            if (t == null)
                return;
            dfs(t.l);
            Console.Write(t.key + " ");
            dfs(t.r);
        }
        private void split(Cartesian_item<T> t, T key, ref Cartesian_item<T> l, ref Cartesian_item<T> r)
        {
            if (t == null)
            {
                l = null;
                r = null;
                return;
            }
            else if (key.CompareTo(t.key) == -1)
            {
                split(t.l, key, ref l, ref t.l);
                r = t;
            }
            else
            {
                split(t.r, key, ref t.r, ref r);
                l = t;
            }

            //t.upd_cnt();
            //t.upd_sum();
            t.upd_min();
        }
        private void insert(ref Cartesian_item<T> t, Cartesian_item<T> it)
        {
            if (t == null)
            {
                t = it;
            }
            else
            if (it.prior.CompareTo(t.prior) == 1)
            {
                split(t, it.key, ref it.l, ref it.r);
                t = it;
            }
            else
            {

                if (it.key.CompareTo(t.key) == -1)
                {
                    insert(ref t.l, it);
                }
                else
                {
                    insert(ref t.r, it);
                }
            }

            //t.upd_cnt();
            //t.upd_sum();
            t.upd_min();

        }
        private void merge(ref Cartesian_item<T> t, Cartesian_item<T> l, Cartesian_item<T> r)
        {
            if ((l == null) || (r == null))
            {
                t = (l != null) ? l : r;
                return;
            }
            else if (l.prior.CompareTo(r.prior) == 1)
            {
                merge(ref l.r, l.r, r);
                t = l;
            }
            else
            {
                merge(ref r.l, l, r.l);
                t = r;
            }
            //t.upd_cnt();
            //t.upd_sum();
            t.upd_min();

        }
        private void erase(ref Cartesian_item<T> t, T key)
        {
            if (t.key.CompareTo(key) == 0)
                merge(ref t, t.l, t.r);
            else
            if (key.CompareTo(t.key) == -1)
            {
                erase(ref t.l, key);
            }
            else
            {
                erase(ref t.r, key);
            }
            if (t != null)
            {
                //t.upd_cnt();
                //t.upd_sum();
                t.upd_min();
            }
        }

        private void lower_bound(Cartesian_item<T> t, T key, ref Cartesian_item<T> res, int d = 0, long s = 0)
        {
            if (t.key.CompareTo(key) == 0)
            {
                res = t;
                res.ord = d + 1;
                res.ordsum = s + t.value;
                if (t.l != null)
                {
                    res.ord += t.l.cnt;
                    res.ordsum += t.l.sum;
                }
                return;
            }

            if (t.key.CompareTo(key) == 1)
            {
                res = t;
                res.ord = d + 1;
                res.ordsum = s + t.value;

                if (t.l != null)
                {
                    res.ord += t.l.cnt;
                    res.ordsum += t.l.sum;
                    lower_bound(t.l, key, ref res, d, s);
                }
            }
            else
            {
                d++;
                s += t.value;
                if (t.l != null)
                {
                    d += t.l.cnt;
                    s += t.l.sum;
                }


                if (t.r != null)
                    lower_bound(t.r, key, ref res, d, s);
            }

        }

        private void find_by_order(Cartesian_item<T> t, int K, ref Cartesian_item<T> res, int d = 0, long s = 0)
        {
            int key = d + 1;
            long skey = s + t.value;
            if (t.l != null)
            {
                key += t.l.cnt;
                skey += t.l.sum;
            }

            if (K == key)
            {
                res = t;
                res.ord = key;
                res.ordsum = skey;
                return;
            }

            if (K < key)
            {
                if (t.l != null)
                    find_by_order(t.l, K, ref res, d, s);
            }
            else
            {
                d = key;
                s = skey;
                if (t.r != null)
                    find_by_order(t.r, K, ref res, d, s);
            }

        }


        private void edit(ref Cartesian_item<T> t, T key, long v)
        {
            if (t.key.CompareTo(key) == 0)
                t.value = v;
            else
            if (key.CompareTo(t.key) == -1)
                edit(ref t.l, key, v);
            else
                edit(ref t.r, key, v);
            t.upd_min();
        }

        public void edit(T key, long v)
        {
            edit (ref root, key, v);
        }

        

        

        public void add(T key, long value)
        {
            Cartesian_item<T> node = new Cartesian_item<T>(key, value);
            insert(ref root, node);
        }

        public void erase(T key)
        {
            erase(ref root, key);
        }

        public long sum(T key)
        {
            if (root == null)
                return 0;
            Cartesian_item<T> res = null;
            lower_bound(root, key, ref res);
            if (res == null)
                return root.sum;
            else
                return (res.ordsum - res.value);
        }

        public Cartesian_item<T> lower_bound(T key)
        {
            Cartesian_item<T> res = null;
            lower_bound(root, key, ref res);
            return res;
        }

        public Cartesian_item<T> find_by_order(int K)
        {
            Cartesian_item<T> res = null;
            find_by_order(root, K, ref res);
            return res;
        }

        public long prefMin(T key)
        {
            Cartesian_item<T> lt = null;
            Cartesian_item<T> rt = null;
            split(root, key, ref lt, ref rt);
            long val = 0;
            if (lt != null)
                val = lt.min;

            merge(ref root, lt, rt);
            return val;
        }

        public long sufMin(T key)
        {
            Cartesian_item<T> lt = null;
            Cartesian_item<T> rt = null;
            split(root, key, ref lt, ref rt);
            long val = 0;
            if (rt != null)
                val = rt.min;

            merge(ref root, lt, rt);
            return val;
        }

        public long Min(T l, T r)
        {
            Cartesian_item<T> lt = null;
            Cartesian_item<T> rt = null;
            Cartesian_item<T> lt2 = null;
            Cartesian_item<T> rt2 = null;

            long val = 0;
            //Разбираем
            split(root, l, ref lt, ref rt);
            if (rt != null)
            {
                split(rt, r, ref lt2, ref rt2);
                if (lt2 != null)
                    val = lt2.min;
                merge(ref rt, lt2, rt2);
            }
            merge(ref root, lt, rt);
            return val;
        }
    }


}