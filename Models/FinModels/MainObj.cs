using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Npgsql;
using System.Linq;

namespace WpfBu.Models
{
    
    public class MainObj
    {
        public static string ConnectionString { get; set; }
        public static DBUtil Dbutil { get; set; }
        public static bool IsPostgres { get; set; }

        public static string  MSSQLConnection {get; set;}

    }

    public class DBUtil
    {
        public List<string> DataColumn(DataTable data)
        {
            List<string> res = new List<string>();
            DataColumn[] a = new DataColumn[data.Columns.Count];
            data.Columns.CopyTo(a, 0);
            res.AddRange(a.Select((c) => c.ColumnName));
            return res;
        }
        public List<Dictionary<string, object>> DataToJson(DataTable res)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            foreach (DataRow rw in res.Rows)
            {
                var user = new Dictionary<string, object>();
                foreach (DataColumn col in res.Columns)
                {
                    if (col.DataType == typeof(System.DateTime) & rw[col] != DBNull.Value)
                        user.Add(col.ColumnName, ((DateTime)rw[col]).ToString("yyyy-MM-ddTHH:mm:ss.000Z"));
                    else
                        user.Add(col.ColumnName, (rw[col] == DBNull.Value) ? null : rw[col]);
                }
                rows.Add(user);
            }
            return rows;
        }
        public DataTable Runsql(string sql)
        {
            DataTable data = new DataTable();
            if (MainObj.IsPostgres)
            {
                var da = new NpgsqlDataAdapter(sql, MainObj.ConnectionString);
                da.Fill(data);
            }
            else
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, MainObj.ConnectionString);
                da.Fill(data);
            }
            return data;
        }

        public DataTable Runsql(string sql, Dictionary<string, object> par)
        {

            DataTable data = new DataTable();
            if (MainObj.IsPostgres)
            {
                var da = new NpgsqlDataAdapter(sql, MainObj.ConnectionString);
                if (par != null)
                    foreach (string s in par.Keys)
                        da.SelectCommand.Parameters.AddWithValue(s, par[s]);
                da.Fill(data);
            }
            else
            {
                var da = new SqlDataAdapter(sql, MainObj.ConnectionString);
                da.SelectCommand.CommandTimeout = 0;
                if (par != null)
                    foreach (string s in par.Keys)
                        da.SelectCommand.Parameters.AddWithValue(s, par[s]);
                da.Fill(data);
            }
            return data;
        }

        public void ExecSQL(string sql, Dictionary<string, object> par)
        {
            if (MainObj.IsPostgres)
            {
                var cn = new NpgsqlConnection(MainObj.ConnectionString);
                var cmd = new NpgsqlCommand(sql, cn);
                if (par != null)
                    foreach (string s in par.Keys)
                        cmd.Parameters.AddWithValue(s, par[s]);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
            else
            {
                var cn = new SqlConnection(MainObj.ConnectionString);
                var cmd = new SqlCommand(sql, cn);
                if (par != null)
                    foreach (string s in par.Keys)
                        cmd.Parameters.AddWithValue(s, par[s]);
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }
        public object NewID(string tablename)
        {
            object res = DBNull.Value;

            if (MainObj.IsPostgres)
            {
                var sql = "select column_default, udt_name  from information_schema.columns  where table_name = @tablename and ordinal_position = 1";
                var rec = Runsql(sql, new Dictionary<string, object>() { { "@tablename", tablename } });

                if (rec.Rows.Count == 0)
                {
                    return res;
                };

                if (rec.Rows[0]["column_default"].ToString() == "" && rec.Rows[0]["udt_name"].ToString() != "uuid")
                {
                    return res;
                };
                var c_default = rec.Rows[0]["column_default"].ToString();
                if (rec.Rows[0]["udt_name"].ToString() == "uuid")
                    c_default = "uuid_generate_v1()";
                sql = "select " + c_default + " id";
                var result = Runsql(sql);
                return result.Rows[0]["id"];
            }
            else
            {
                var sql = "select c.user_type_id from sys.tables t(nolock) inner join sys.columns c(nolock) on t.object_id = c.object_id where t.name = @tablename and column_id = 1";
                var rec = Runsql(sql, new Dictionary<string, object>() { { "@tablename", tablename } });
                if (rec.Rows.Count == 0)
                {
                    return res;
                };
                if ((int)rec.Rows[0][0] == 36)
                {
                    return Guid.NewGuid();
                }
                else
                {
                    return res;
                }
            }

        }
    }
}
