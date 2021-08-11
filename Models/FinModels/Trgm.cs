using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Npgsql;


namespace WpfBu.Models
{
    public class Trgm
    {
        //Нечеткий поиск
        public List<char> prepare(string s)
        {
            s = s.ToLower();
            HashSet<char> del = new HashSet<char>() { ' ', '\r', '\n', '\t' };
            List<char> res = new List<char>();
            res.Add(' ');
            bool f = true;
            int n = s.Length;
            for (int i = 0; i < n; i++)
            {
                char c = s[i];
                if (del.Contains(c))
                    if (f)
                    {
                        continue;
                    }
                    else
                    {
                        f = true;
                        res.Add(' ');
                        res.Add(' ');
                    }
                else
                {
                    f = false;
                    res.Add(c);
                }
            }
            if (!f)
                res.Add(' ');
            return res;
        }

        HashSet<string> get_trgm(string s)
        {
            HashSet<string> res = new HashSet<string>();
            List<char> sp = prepare(s);
            s = string.Join("", sp.ToArray());
            int n = s.Length;
            for (int i = 0; i < n - 2; i++)
            {
                if (s[i + 1] == ' ' && s[i + 2] == ' ')
                    continue;
                string trgm = "";
                trgm = s[i].ToString() + s[i + 1].ToString() + s[i + 2].ToString();

                if (!res.Contains(trgm))
                    res.Add(trgm);
            }
            return res;
        }

        double comp(string s1, string s2, bool strong = true)
        {
            HashSet<string> ts1 = get_trgm(s1);
            HashSet<string> ts2 = get_trgm(s2);

            int m = 0;
            foreach (string it in ts1)
            {
                if (ts2.Contains(it))
                    m++;
            }
            double res = 2.0 * m / (ts1.Count + ts2.Count);
            if (!strong)
                res = 1.0 * m / (ts2.Count);
            return res;
        }


        double comp(HashSet<string> ts1, HashSet<string> ts2, bool strong = true)
        {
            int m = 0;
            foreach (string it in ts1)
            {
                if (ts2.Contains(it))
                    m++;
            }
            double res = 2.0 * m / (ts1.Count + ts2.Count);
            if (!strong)
                res = 1.0 * m / (ts2.Count);
            return res;
        }


        public void culc(DataTable data, string code = "code", string usmartname = "usmartname",
        string at_utg = "at_utg", string at_nameru = "at_nameru")
        {
            if (data.Rows.Count == 0)
                return;
            string suf = data.Rows[0]["neyro"].ToString(); //Подключаем разные словари
            string sql = "select * from neyro" + suf;
            var da = new NpgsqlDataAdapter(sql, MainObj.ConnectionString);
            DataTable rec = new DataTable();
            da.Fill(rec);

            sql = "select * from actypes";
            da = new NpgsqlDataAdapter(sql, MainObj.ConnectionString);
            DataTable actypes = new DataTable();
            da.Fill(actypes);




            int n1 = actypes.Rows.Count;
            int n2 = rec.Rows.Count;

            //Посчитаем триграммы один раз, дает выигрыш по скорости в 10 раз
            List<HashSet<string>> recList = new List<HashSet<string>>();
            List<HashSet<string>> actypesList = new List<HashSet<string>>();

            for (int j = 0; j < n1; j++)
            {
                string s2 = actypes.Rows[j]["at_nameru"].ToString();
                HashSet<string> tr2 = get_trgm(s2);
                actypesList.Add(tr2);
            }
            for (int j = 0; j < n2; j++)
            {
                string s2 = rec.Rows[j]["dcname"].ToString();
                HashSet<string> tr2 = get_trgm(s2);
                recList.Add(tr2);
            }

            int n = data.Rows.Count;
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrEmpty(data.Rows[i][at_utg].ToString()))
                {
                    int jmax = 0;
                    double dmax = 0;
                    string s1 = data.Rows[i]["mtow"].ToString();

                    if (string.IsNullOrEmpty(s1))
                        s1 = "space";

                    //Посчитаем триграммы один раз, дает выигрыш по скорости в 10 раз
                    HashSet<string> tr1 = get_trgm(s1);

                    for (int j = 0; j < n1; j++)
                    {
                        //string s2 = actypes.Rows[j]["at_nameru"].ToString();
                        //double d = comp(s1, s2, false);
                        HashSet<string> tr2 = actypesList[j];
                        double d = comp(tr1, tr2, false);
                        if (d > dmax)
                        {
                            dmax = d;
                            jmax = j;
                        }
                    }
                    if (dmax > 0)
                    {
                        data.Rows[i][at_utg] = actypes.Rows[jmax]["at_utg"];
                        if (!string.IsNullOrEmpty(at_nameru))
                            data.Rows[i][at_nameru] = actypes.Rows[jmax]["at_nameru"].ToString();
                    }
                }

                if ((int)data.Rows[i][code] == 0)
                {

                    int jmax = 0;
                    double dmax = 0;
                    string s1 = data.Rows[i]["dc_name"].ToString();
                    HashSet<string> tr1 = get_trgm(s1);

                    for (int j = 0; j < n2; j++)
                    {
                        //string s2 = rec.Rows[j]["dcname"].ToString();
                        //double d = comp(s2, s1, true);
                        HashSet<string> tr2 = recList[j];
                        double d = comp(tr1, tr2, true);
                        if (d > dmax)
                        {
                            dmax = d;
                            jmax = j;
                        }
                    }
                    if (data.Columns.Contains("d"))
                        data.Rows[i]["d"] = dmax;

                    if (dmax > 0)
                    {
                        data.Rows[i][code] = rec.Rows[jmax]["code"];
                        data.Rows[i][usmartname] = rec.Rows[jmax]["usmartname"];
                    }
                    else
                    {
                        data.Rows[i][code] = 0;
                        data.Rows[i][usmartname] = "Не найдено";
                    }
                }
            }

            //сохраняем code
            if (code == "code")
            {
                string sqlu = "";
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sqlu = sqlu+ $"update tariffs_import set save_code = {data.Rows[i]["code"].ToString()} where id = {data.Rows[i]["id"].ToString()};";
                }
                NpgsqlConnection nconn = new NpgsqlConnection(MainObj.ConnectionString);
                NpgsqlCommand nc = new NpgsqlCommand(sqlu, nconn);
                nconn.Open();
                nc.ExecuteNonQuery();
                nconn.Close();

            }
        }


    }
}
