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


    [Authorize]
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

            int nid = Array.IndexOf(cols_list, "nn");
            
            if (nid != -1)
            {
                //return Content("Не указана колонка nn.");
            //Загрузка в любую таблицу    
            string[] vals1 = csvSplit(rows[1], ";", true);
            if (vals1[nid] != nn)
                return Content("В колонке nn указано значение отличное от nn записи о загрузке тарифа.");
            }

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
            if (nid ==-1)
            {
                //Загрузка в любую таблицу
                sql = "";
            }

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

        public IActionResult tariffs(string nn)
        {


            string InsertCreate(string txt, string TableName)
            {

                string fieldValue(string vl, string cname, List<string> nobj)
                {
                    if (string.IsNullOrEmpty(vl))
                        return "null";

                    vl = vl.Replace("'", "''");
                    if (nobj.Contains(cname))
                        vl = vl.Replace(',', '.');

                    vl = "'" + vl + "'";    
                    return vl;
                }

                List<string> numobj = new List<string>();
                numobj.Add("tf_tariffmain");
                numobj.Add("tf_payexec");
                numobj.Add("tf_paymin");
                

                var da1 = new NpgsqlDataAdapter(txt, MainObj.ConnectionString);
                DataTable rec = new DataTable();
                da1.Fill(rec);

                string res = "\r\n";
                var insStr = "insert into " + TableName + "(";
                insStr = insStr + rec.Columns[0].ColumnName;

                for (var i = 1; i < rec.Columns.Count; i++)
                {
                    insStr = insStr + ',' + rec.Columns[i].ColumnName;
                }
                insStr = insStr + ")\r\n";

                for (int i = 0; i < rec.Rows.Count; i++)
                {
                    string valStr = "values (";
                    valStr = valStr + fieldValue(rec.Rows[i][rec.Columns[0].ColumnName].ToString(), rec.Columns[0].ColumnName, numobj);
                    for (var j = 1; j < rec.Columns.Count; j++)
                    {
                        valStr = valStr + ',' + fieldValue(rec.Rows[i][rec.Columns[j].ColumnName].ToString(), rec.Columns[j].ColumnName, numobj);
                    }
                    valStr = valStr + ");\r\n";
                    res = res + insStr + valStr;
                }
                return res;
            }


            if (string.IsNullOrEmpty(nn))
                nn = "-1";
            string sql = "select * from v_Tariffs_ext_import where nn = " + nn + " /*[Tariffs_ext_import]*/";
            if (nn == "-1")
                sql = "select * from v_Tariffs_ext_import_group /*[Tariffs_ext_import]*/";
            var resSQL = "use uFlights\r\ngo\r\n";
            resSQL = resSQL + "truncate table Tariffs_ext_import;\r\n";
            resSQL = resSQL + "\r\n----------------------------------------------------\r\n";
            resSQL = resSQL + InsertCreate(sql, "Tariffs_ext_import"); 

            resSQL = resSQL + "\r\n----------------------------------------------------\r\n";
            resSQL = resSQL + "\r\ndeclare @date datetime, @al uniqueidentifier, @n int, @AP_IATA varchar(3);\r\n";

            var sqlproc = "select * from v_tariffs_ext_load where (is_run = 1 and " + nn + " = -1) or nn = " + nn + ";";
            var da = new NpgsqlDataAdapter(sqlproc, MainObj.ConnectionString);

            DataTable recheck = new DataTable();
            da.Fill(recheck);

            string filename = $"dump_{nn}.sql";
            for (int i = 0; i < recheck.Rows.Count; i++)
            {
                DataRow mi = recheck.Rows[i];
                filename = $"dump_{mi["al_utg"].ToString()}{((DateTime)mi["tf_datebeg"]).ToString("yyyyMMdd")}.sql";

                if (mi["tf_comment"] == DBNull.Value)
                    mi["tf_comment"] = "";
                resSQL = resSQL + "\r\n-----" + mi["al_nameru"].ToString() + "\r\n";
                resSQL = resSQL + "set @date = '" + ((DateTime)mi["tf_datebeg"]).ToString("yyyy-MM-dd") + "';\r\n";
                resSQL = resSQL + "set @n = " + mi["nn"].ToString() + ";\r\n";
                resSQL = resSQL + "set @al = '" + mi["tf_al"].ToString() + "';\r\n";
                if (mi["ap_iata"].ToString() == "DME")
                    resSQL = resSQL + "set @AP_IATA = 'DME'\r\n";
                else
                    resSQL = resSQL + "set @AP_IATA = 'VKO'\r\n";


                resSQL = resSQL + "--exec p_Tariffs_Ext_exists @al, 'VKO', @date, 0 --,'111,8,790' --добавляет по списку услуг;\r\n";


                resSQL = resSQL + "exec uFlights..p_Tariffs_ext_load\r\n";
                resSQL = resSQL + "@NN = @n,\r\n";
                resSQL = resSQL + "@TF_AL =@al,\r\n";
                resSQL = resSQL + "@TF_DateBeg = @date,\r\n";
                resSQL = resSQL + "@TF_Agent = '" + mi["tf_agent"].ToString() + "',\r\n";
                resSQL = resSQL + "@TF_Currency = '" + mi["tf_currency"].ToString() + "',\r\n";
                resSQL = resSQL + "@TF_VAT_str = '" + mi["tf_vat_str"].ToString() + "',\r\n";
                resSQL = resSQL + "@TF_Comment = '" + mi["tf_comment"].ToString() + "',\r\n";
                resSQL = resSQL + "@AP_IATA = @AP_IATA,\r\n";

                if (!string.IsNullOrEmpty(mi["tf_format"].ToString()))
                    resSQL = resSQL + "@NOT_CL = 1";
                else
                    resSQL = resSQL + "@NOT_CL = 0";

                resSQL = resSQL + ";\r\n\r\n";


                resSQL = resSQL + "exec WorkOrders..p_syncTariff\r\n";
                resSQL = resSQL + "@AL_PK = @al,\r\n";
                resSQL = resSQL + "@VT_DateBeg = @date,\r\n";
                resSQL = resSQL + "@AP_IATA = @AP_IATA;\r\n\r\n";


                resSQL = resSQL + "exec WorkOrders..p_Orders_Culc\r\n";
                resSQL = resSQL + "@AL_PK = @al,\r\n";
                resSQL = resSQL + "@VT_DateBeg = @date,\r\n";
                resSQL = resSQL + "@AP_IATA = @AP_IATA;\r\n\r\n";
                resSQL = resSQL + "\r\n----------------------------------------------------\r\n";
                resSQL = resSQL + "--exec p_Tariffs_Ext_compare @al, @n;\r\n";

            }



            string ctype = "application/octet-stream";
            byte[] buf = Encoding.UTF8.GetBytes(resSQL);
            return File(buf, ctype, filename);
        }
    }
}