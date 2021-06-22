using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using netbu.Models;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using Npgsql;
using System.Text.RegularExpressions;
using WpfBu.Models;


namespace netbu.Controllers
{

    public class FileLoadController : Controller
    {
        private IHostingEnvironment _env;
        public FileLoadController(IHostingEnvironment env)
        {
            _env = env;
        }

        public ActionResult tffile(string nn, List<IFormFile> tfile)
        {
            string[] csvSplit(string csv1, string c, bool opt)
            {
                List<string> res = new List<string>();
                string q = @"""";
                int n2 = csv1.Length;
                int ibegin = 0;
                int iend = 0;
                bool f = false;
                for (int i = 0; i < n2; i++)
                {
                    string a = csv1.Substring(i, 1);
                    if (f)
                    {
                        //открыты кавычки
                        if (a == q)
                        {
                            if (csv1.Substring(i + 1, 1) == q)
                            {
                                //двойная кавычка, ничего не происходит
                                i++;
                            }
                            else
                            {
                                f = false;
                            }
                        }
                    }
                    else
                    {
                        if (a == q)
                        {
                            f = true;
                        }
                        else
                        {
                            if (a == c)
                            {
                                iend = i;
                                int ln = iend - ibegin;
                                if (ln == 0)
                                {
                                    res.Add("");
                                }
                                else
                                {
                                    res.Add(csv1.Substring(ibegin, ln));
                                }
                                ibegin = i + 1;
                            }
                        }
                    }
                }

                if (opt)
                {
                    iend = n2;
                    int ln = iend - ibegin;
                    if (ln == 0)
                    {
                        res.Add("");
                    }
                    else
                    {
                        res.Add(csv1.Substring(ibegin, ln));
                    }
                }
                return res.ToArray();
            }

            IFormFile img = tfile[0];
            string filename = img.FileName;
            int n = (int)img.Length;
            byte[] buf = new byte[n];
            Stream ms = img.OpenReadStream();
            ms.Read(buf, 0, n);
            string csv = Encoding.UTF8.GetString(buf);
            csv = csv.Replace("\r", "");

            string q1 = @"""";
            string q2 = @"""""";

            
            string[] rows = csvSplit(csv, "\n", false);
            string cols = rows[0].Trim().ToLower();
            string[] cols_list = cols.Split(";", StringSplitOptions.None);
            string sql = "select fn_findtable(@cols)";
            var da = new NpgsqlDataAdapter(sql, MainObj.ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("@cols", cols);
            DataTable tabnametab = new DataTable();
            da.Fill(tabnametab);
            if (tabnametab.Rows.Count == 0)
            {
                return Content("Таблица с указанными полями не найдена.");
            }
            string table_name = tabnametab.Rows[0][0].ToString();

            List<string> numobj = new List<string>();
            sql = "select column_name from information_schema.columns where table_name= @table_name and data_type  in ('integer', 'numeric', 'real')";
            da = new NpgsqlDataAdapter(sql, MainObj.ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("@table_name", table_name);
            DataTable rec = new DataTable();
            da.Fill(rec);


            for (int i = 0; i < rec.Rows.Count; i++)
            {
                numobj.Add(rec.Rows[i]["column_name"].ToString());
            }

            string insstr = $"insert into {table_name}({cols.Replace(";", ", ")})";
            sql = $"delete from tariffs_import where nn = {nn.ToString()};\n";
            for (var i = 1; i < rows.Length; i++)
            {
                string[] vals = csvSplit(rows[i], ";", true);
                if (vals.Length != cols_list.Length)
                    continue;

                string valstr = "values (";
                for (int j = 0; j < vals.Length; j++)
                {
                    string vl = "null";
                    if (vals[j] != "")
                    {
                        if (numobj.Contains(cols_list[j]))
                            vl = vals[j].Replace("'", "''").Replace(',', '.');
                        else
                        {
                            vl = vals[j].Replace("'", "''");
                            //Двойные кавычки
                            vl = vl.Replace(q2, q1);
                            if (vl.Substring(0, 1) == q1)
                                vl = vl.Substring(1, vl.Length - 1);
                            if (vl.Substring(vl.Length - 1, 1) == q1)
                                vl = vl.Substring(0, vl.Length - 1);
                            vl = "'" + vl + "'";

                        }
                    }
                    if (j == 0)
                        valstr = valstr + vl;
                    else
                        valstr = valstr + ", " + vl;
                }
                valstr = valstr + ");";
                sql = sql + insstr + '\n' + valstr + '\n';
            }
            NpgsqlConnection cn = new NpgsqlConnection(MainObj.ConnectionString);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, cn);
            string resmes = $"Данные добавлены в таблицу {table_name}. файл: {filename}";
            try
            {
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
            catch (Exception ex)
            {
                resmes = $"Ошибка: {ex.Message}";
            }

            return Content(resmes);

        }


    }
}