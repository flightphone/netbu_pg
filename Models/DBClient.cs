using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing;
using Npgsql;

namespace netbu.Models
{
    public class DBClient
    {
        //public static string CnStr = @"data source=mssql2.win.agava.net;User ID=bible_user;Password=vultur1;database=biblean5_AntWorldMap";
        public static string CnStr = @"data source=localhost\SQLEXPRESS8;User ID=sa;Password=aA12345678;database=uFlights";
        public string KeyField = "";
        public string DispField = "";
        public string DecName = "";
        public string TableName = "";

        

        [Required]
        [Display(Name="Логин:")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Пароль:")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        /*
        [Required]
        [Display(Name = "Аэропорт(VKO или DME):")]
     
            public string AP_IATA
        {
            get;
            set;
        }
        */


        public static string Account = "Admin";
        public static string Appl = "ALL";
        public enum TypeField { Grid, Select, Edit, List };

        public static string sortCreate(string sort, string order)
        {
            string[] s = sort.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] o = order.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string res = "[" + s[0] + "] " + o[0];
            for (int i = 1; i < s.Length; i++)
            {
                res = res + ", [" + s[i] + "] " + o[i];
            }
            return res;
        }


        
        public static string getFilter(string findValue, string fieldName, string filterRules)
        {
            string flt = "";
            /*
            string addFilter = "";
            if (!string.IsNullOrEmpty(filterRules))
            {
                var Filt = System.Web.Helpers.Json.Decode(filterRules);
                if (Filt.Length > 0)
                {
                    string val = ((string)Filt[0].value).Replace("'", "''");
                    string not = "";
                    if (val.Substring(0, 1) == "!")
                    {
                        not = " not ";
                        val = val.Substring(1);
                    } else
                        not = "";

                    addFilter = not + "([" + Filt[0].field + "] like '%" + val + "%')";
                    for (int i = 1; i < Filt.Length; i++)
                    {
                        val = ((string)Filt[i].value).Replace("'", "''");
                        if (val.Substring(0, 1) == "!")
                        {
                            not = " not ";
                            val = val.Substring(1);
                        }
                        else
                            not = "";
                        addFilter = addFilter + " and " + not +  "([" + Filt[i].field + "] like '%" + val + "%')";
                    }
                }
            }

            if (!string.IsNullOrEmpty(findValue))
            {
                if (!string.IsNullOrEmpty(fieldName))
                { flt = "[" + fieldName + "] like '%" + findValue.Replace("'", "''") + "%'"; }
                else
                { flt = findValue; }
            }
            if (!string.IsNullOrEmpty(addFilter))
            {
                if (string.IsNullOrEmpty(flt))
                { flt = addFilter; }
                else
                { flt = flt + " and " + addFilter; }
            }
            */
            return flt;
        }


        public string CreateFields(int IdDeclare, string Account = "Admin")
        {
            return CreateFields(IdDeclare, TypeField.Grid, "", Account);
        }

        public string CreateFieldWin(int IdDeclare)
        { 
            StringBuilder sb = new StringBuilder();

            string sql = "select D.* from t_rpDeclare D(nolock) where D.IdDeclare = @IdDeclare";
            DataTable decTab = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@IdDeclare", IdDeclare);
            da.Fill(decTab);
            

            string DecName = decTab.Rows[0]["DecName"].ToString();
            DataTable FieldMapTable = new DataTable();
            string smap = "select * from t_sysFieldMap where DecName = @DecName";
            da = new SqlDataAdapter(smap, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@DecName", DecName);
            da.Fill(FieldMapTable);

            //04/08/2016 Гриды для поиска грузим в своих окошках
            

            DataRow[] fl = FieldMapTable.Select("IdDeclare > 0");
            for (int i = 0; i < fl.Length; i++)
            {
                string ClassName = fl[i]["ClassName"].ToString();
                //21/09/2016 каждый грид грузим в окошко с определенным id
                string IdDec = fl[i]["IdDeclare"].ToString();
                if (ClassName == "Bureau.Finder")
                {
                    sb.AppendLine(string.Format(@"<div id=""w{0}"" class=""easyui-window"" data-options=""modal:true,closed:true,iconCls:'icon-search',collapsible:false,minimizable:false"" style=""height:600px;width:900px;""></div>", IdDec));
                }
                
            }
            return sb.ToString();

        
        
        }

        public string CreateFieldScript(int IdDeclare)
        {
            StringBuilder sb = new StringBuilder();

            string sql = "select D.* from t_rpDeclare D(nolock) where D.IdDeclare = @IdDeclare";
            DataTable decTab = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@IdDeclare", IdDeclare);
            da.Fill(decTab);
            

            string DecName = decTab.Rows[0]["DecName"].ToString();
            DataTable FieldMapTable = new DataTable();
            string smap = "select * from t_sysFieldMap where DecName = @DecName";
            da = new SqlDataAdapter(smap, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@DecName", DecName);
            da.Fill(FieldMapTable);

            //04/08/2016 Гриды для поиска грузим в своих окошках
            //int nFinder = 0;

            DataRow[] fl = FieldMapTable.Select("IdDeclare > 0");
            for (int i = 0; i < fl.Length; i++)
            {
                string ClassName = fl[i]["ClassName"].ToString();
                string dstField = fl[i]["dstField"].ToString();
                string GroupDec = fl[i]["GroupDec"].ToString();
                //21/09/2016 каждый грид грузим в окошко с определенным id
                string IdDec = fl[i]["IdDeclare"].ToString();
                if (ClassName == "Bureau.Check")
                {
                    sb.AppendLine(string.Format("function format_{0}(val, row) ", dstField) + "{");
                    sb.AppendLine(@"var res = '<input type=""checkbox"" ';");
                    sb.AppendLine(@"if (val == 1) {");
                    sb.AppendLine(@"res = res + 'checked=""checked"" ';");
                    sb.AppendLine(@"}");
                    sb.AppendLine(string.Format(@"res = res + ' onchange = ""on_{0}(this.checked);"" />';", dstField));
                    sb.AppendLine(@"return res;");
                    sb.AppendLine(@"}");

                    sb.AppendLine(string.Format("function on_{0}(val)", dstField) + "{");
                    sb.AppendLine(@"$.post(""GetData/echo"", { val: val }, function(data) {");
                    sb.AppendLine(@"flagUpdate = true;");
                    sb.AppendLine(@"var row = $('#MainTabEdit').datagrid('getSelected');");
                    sb.AppendLine(@"if (row){");
                    sb.AppendLine(@"if (data == 'true')");
                    sb.AppendLine(string.Format("row.{0} = 1;", dstField));
                    sb.AppendLine(@"else");
                    sb.AppendLine(string.Format("row.{0} = 0;", dstField));
                    sb.AppendLine(@"}");
                    sb.AppendLine(@"});");
                    sb.AppendLine(@"}");

                }



                if (ClassName == "Bureau.GridCombo")
                {
                    sql = "select D.*  from t_rpDeclare D(nolock) where IdDeclare = @IdDeclare";
                    decTab = new DataTable();
                    da = new SqlDataAdapter(sql, DBClient.CnStr);
                    da.SelectCommand.Parameters.AddWithValue("@IdDeclare", fl[i]["IdDeclare"]);
                    da.Fill(decTab);
                    string MainSQL = decTab.Rows[0]["DecSQL"].ToString();
                    MainSQL = MainSQL.Replace("{Account}", DBClient.Account);

                    da = new SqlDataAdapter(MainSQL, DBClient.CnStr);
                    DataTable resTab = new DataTable();
                    da.Fill(resTab);
                    string res = "";
                    for (int j = 0; j < resTab.Rows.Count; j++)
                    {
                        string ln = @"<option value=""" + resTab.Rows[j][decTab.Rows[0]["KeyField"].ToString()].ToString() + @"""";
                        ln = ln + ">" + resTab.Rows[j][decTab.Rows[0]["DispField"].ToString()].ToString().Replace("<", "&lt;").Replace(">", "&gt;") + "</option>";
                        res = res + " " + ln;
                    }

                    sb.AppendLine(string.Format("function format_{0}(val, row) ", dstField) + "{");
                    sb.AppendLine(string.Format(@"var res = '<select style=""width:100%"" onchange = ""on_{0}(this.value);"" >';", dstField));
                    //sb.AppendLine(@"res = res + '<option value="""">&lt;Не указан&gt;</option>';");
                    sb.AppendLine(string.Format(@"res = res + '{0}'", res));
                    sb.AppendLine("res = res + '</select>';");
                    sb.AppendLine("res = setSelectList(res, val);");
                    sb.AppendLine("return res; }");


                    sb.AppendLine(string.Format("function on_{0}(val)", dstField) + "{");
                    sb.AppendLine("flagUpdate = true;");
                    sb.AppendLine("var row = $('#MainTabEdit').datagrid('getSelected');");
                    sb.AppendLine("if (row)");
                    sb.AppendLine(string.Format("row.{0} = val;", fl[i]["dstField"].ToString()));
                    sb.AppendLine("}");
                }

                if (ClassName == "Bureau.Finder")
                {
                    sb.AppendLine(string.Format("function format_{0}(val, row)",  dstField) + "{");
                    sb.AppendLine(string.Format(@"var res = '<img src=""/Scripts/easyUI/themes/icons/search.png"" height=""20"" weight=""20"" onclick=""on_{0}();""/>';", dstField));
                    sb.AppendLine("return res;");
                    sb.AppendLine("};");

                    sb.AppendLine("var initObject" + IdDec + " = {};"); //21/09/2016
                    sb.AppendLine("var flagRef" + IdDec + "=0;");

                    sb.AppendLine(string.Format("function on_{0}(value)",  dstField) + "{");
                    sb.AppendLine("initObject" + IdDec + " = {");
                    sb.AppendLine("WinID: 'w" + IdDec + "',");
                    sb.AppendLine("onSelect: function (r) {");
                    sb.AppendLine("var rw = $('#MainTabEdit').datagrid('getSelected');");
                    sb.AppendLine("if (rw){");
                    sb.AppendLine("flagUpdate = true;");
                    sb.AppendLine("var rwindex = $('#MainTabEdit').datagrid('getRowIndex', rw);");
                    sb.AppendLine("$('#MainTabEdit').datagrid('endEdit', rwindex);");
                    sb.AppendLine("$('#MainTabEdit').datagrid('updateRow',{");
                    sb.AppendLine("index: rwindex,");
                    sb.AppendLine("row: {");

                    DataRow[] gp = FieldMapTable.Select("GroupDec = '" + GroupDec + "'");
                    for (int j = 0; j < gp.Length-1; j++)
                    {
                        sb.AppendLine(string.Format("{0} : r.{1},", gp[j]["dstField"].ToString(), gp[j]["srcField"].ToString()));
                    }
                    sb.AppendLine(string.Format("{0} : r.{1}", gp[gp.Length - 1]["dstField"].ToString(), gp[gp.Length - 1]["srcField"].ToString()));

                    sb.AppendLine("}");
                    sb.AppendLine("});");
                    
                    sb.AppendLine("$('#MainTabEdit').datagrid('beginEdit', rwindex);");
                    sb.AppendLine("}");
                    sb.AppendLine("}");
                    sb.AppendLine("};");



                    sb.AppendLine("if (flagRef" + IdDec + " == 0) {"); //загружаем окно с поиском только один раз
                    sb.AppendLine("$('#w" + IdDec + "').window({");

                    sql = "select D.*  from t_rpDeclare D(nolock) where IdDeclare = @IdDeclare";
                    decTab = new DataTable();
                    da = new SqlDataAdapter(sql, DBClient.CnStr);
                    da.SelectCommand.Parameters.AddWithValue("@IdDeclare", fl[i]["IdDeclare"]);
                    da.Fill(decTab);
                    string Descr = decTab.Rows[0]["Descr"].ToString();

                    sb.AppendLine(string.Format("title: '{0}'", Descr));
                    sb.AppendLine("});");

                    sb.AppendLine("$('#w" + IdDec + "').window('open');");

                    sb.AppendLine(string.Format("$('#w" + IdDec + "').window('refresh', '/WebFinder/OKCancel?IdDeclare={0}&parent=initObject{1}');", IdDec, IdDec ));

                    sb.AppendLine("flagRef" + IdDec + "=1;}");
                    sb.AppendLine("else"); //Только открываем окошко
                    sb.AppendLine("$('#w" + IdDec + "').window('open');");

                    
                    sb.AppendLine("};");
                    
                    //nFinder++;
                
                }
            }

            return sb.ToString();
        }

        public string CreateFields(int IdDeclare, TypeField TField, string SelectedField, string Account = "Admin")
        {
            string sql = "select D.*, P.ParamValue, W.ParamValue WValue from t_rpDeclare D(nolock) " 
                + " inner join t_sysParams P(nolock) on 'GridFind' + D.DecName = P.ParamName "
                + " left join t_sysParams W(nolock) on D.DecName + '" + Account + "' = W.ParamName " 
                + " where IdDeclare = @IdDeclare";
            DataTable decTab = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@IdDeclare", IdDeclare);
            da.Fill(decTab);
            string s = decTab.Rows[0]["ParamValue"].ToString();
            KeyField = decTab.Rows[0]["KeyField"].ToString();
            DispField = decTab.Rows[0]["DispField"].ToString();
            TableName = decTab.Rows[0]["TableName"].ToString();
            
            string SaveFieldList = decTab.Rows[0]["SaveFieldList"].ToString();
            string WValue = decTab.Rows[0]["WValue"].ToString();
            //31.07.2016 Список размеров
            string[] vars = WValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> FWidths = new Dictionary<string, string>();
            foreach (string v in vars)
            {
                string[] keyval = v.Split(new char[] { '=' });
                FWidths.Add(keyval[0], keyval[1]);
            }
           //===============================

            DecName = decTab.Rows[0]["DecName"].ToString();

            DataTable FieldMapTable = new DataTable();
            if (TField == TypeField.Edit)
            {
                string smap = "select * from t_sysFieldMap where DecName = @DecName";
                da = new SqlDataAdapter(smap, DBClient.CnStr);
                da.SelectCommand.Parameters.AddWithValue("@DecName", DecName);
                da.Fill(FieldMapTable);
            }

            StringBuilder sb = new StringBuilder();
            string ListField = "";
            XmlDocument xm = new XmlDocument();
            XmlElement xRoot;
            xm.LoadXml(s);
            xRoot = xm.DocumentElement;
            bool FlagSelect = false;
            foreach (XmlNode xNod in xRoot.SelectNodes("COLUMN"))
            {

                XmlElement xCol = (XmlElement)xNod;
                string FName = xCol.Attributes["FieldName"].Value;
                string Title = xCol.Attributes["FieldCaption"].Value;
                int Wi = int.Parse(xCol.Attributes["Width"].Value);
                string Vis = xCol.Attributes["Visible"].Value;
                string DisplayFormat = xCol.Attributes["DisplayFormat"].Value;

                //31.07.2016 список размеров
                if (FWidths.ContainsKey(FName))
                    try
                    {
                        Wi = int.Parse(FWidths[FName]);
                    }
                    catch
                    { ;}

                //==========================

                string alignStr = "";
                if (DisplayFormat.IndexOf("#") > -1)
                    alignStr = ",align:'right'";

                if (Vis == "1" & FName.ToLower().IndexOf("_bit") == -1)
                {
                    if (TField == TypeField.Grid)
                        sb.AppendLine(string.Format(@"<th data-options=""field:'{0}',width:{1}{3}"" sortable=""true"">{2}</th>", new object[] { FName, Wi, Title, alignStr }));
                    else
                        if (TField == TypeField.Select)
                        {
                            if (!FlagSelect & (SelectedField == FName | string.IsNullOrEmpty(SelectedField)))
                            {
                                sb.AppendLine(string.Format(@"<option selected value=""{0}"">{1}</option>", new object[] { FName, Title }));
                                FlagSelect = true;
                            }
                            else
                                sb.AppendLine(string.Format(@"<option value=""{0}"">{1}</option>", new object[] { FName, Title }));
                        }

                        else       //31.07.2016
                            if (TField == TypeField.List)
                            {
                                if (String.IsNullOrEmpty(ListField))
                                    ListField = "'" + FName + "'";
                                else
                                    ListField = ListField + ",'" + FName + "'";
                            }
                            else
                    {
                        //Выпадающие списки 21.12.2014
                        DataRow[] ar = FieldMapTable.Select("dstField='" + FName + "' and IdDeclare > 0");
                        string ClassName = "";
                        if (ar.Length > 0)
                            ClassName = ar[0]["ClassName"].ToString();

                        if (FName.ToLower() == "id" | SaveFieldList.ToLower().IndexOf(FName.ToLower()) == -1)
                            sb.AppendLine(string.Format(@"<th data-options=""field:'{0}',width:{1}, sortable:true"">{2}</th>", new object[] { FName, Wi, Title }));
                        else
                        {
                            if (String.IsNullOrEmpty(ClassName) | ClassName == "Bureau.Finder")
                                sb.AppendLine(string.Format(@"<th data-options=""field:'{0}',width:{1},editor:'text', sortable:true"" >{2}</th>", new object[] { FName, Wi, Title }));

                            if (ClassName == "Bureau.GridCombo" | ClassName == "Bureau.Check")
                                sb.AppendLine(string.Format(@"<th data-options=""field:'{0}',width:{1},formatter: format_{0}, sortable:true""  sortable=""true"">{2}</th>", new object[] { FName, Wi, Title }));
                        }

                        if (ClassName == "Bureau.Finder")
                            sb.AppendLine(string.Format(@"<th data-options=""field:'{0}_f', width:25, formatter: format_{0}""></th>", FName));

                    }
                }


            }

            //31/07/2016
            if (TField == TypeField.List)
            {
                //ListField = "[" + ListField + "]";
                return ListField;
            }
            else
                return sb.ToString();
        }

        public string sqlNavigate(string TableName, string flt, string Filed, string order, int id, int mode)
        { 
        //mode - 0 - first
            //   1 - previous
            //   2 - next
            //   3 - last

            StringBuilder sb = new StringBuilder();
            string strorder = Filed + " " + order;
            if (Filed != "id")
                strorder = strorder + ", id asc";

            sb.AppendLine("WITH tmpTab AS (");
            sb.AppendLine("select");
            sb.AppendLine("ROW_NUMBER() OVER (ORDER BY " + strorder + ") AS IDTMPNUM,");
            sb.AppendLine("V.*");
            sb.AppendLine("from ");
            sb.AppendLine(TableName + " V");
            if (!string.IsNullOrEmpty(flt))
                sb.AppendLine("where " + flt);
            sb.AppendLine(")");

            string nrec = "1";
            if (mode == 1)
                nrec = string.Format("(select IDTMPNUM from tmpTab where id = {0}) - 1", id);
            if (mode == 2)
                nrec = string.Format("(select IDTMPNUM from tmpTab where id = {0}) + 1", id);
            if (mode == 3)
                nrec = "(select COUNT(*) from tmpTab)";
            sb.AppendLine("select * from tmpTab where IDTMPNUM = " + nrec);
            return sb.ToString();
        }

        public string CompileSQL(string mSQL, string flt, int N, int LenP)
        {
            StringBuilder sb = new StringBuilder();
            int n = mSQL.ToUpper().IndexOf("ORDER BY");
            if (n == -1)
                throw new Exception("not find ORDER BY section!");
            string decSQL = mSQL.Substring(0, n);
            string OrdField = mSQL.Substring(n + 8);
            sb.AppendLine("WITH tmpWebFind AS (");
            sb.AppendLine(" SELECT TMPA.*, ");
            sb.AppendLine(string.Format(" ROW_NUMBER() OVER (ORDER BY {0}) AS IDTMPNUM", OrdField));
            sb.AppendLine(string.Format(" FROM ({0}) TMPA ", decSQL));
            if (!string.IsNullOrEmpty(flt))
            {
                sb.AppendLine(" WHERE ");
                sb.AppendLine(flt);
            }

            sb.AppendLine(") ");
            sb.AppendLine(" SELECT * FROM tmpWebFind A ");
            sb.AppendLine(string.Format(" WHERE IDTMPNUM BETWEEN {0} AND {1}", (N - 1) * LenP + 1, N * LenP));
            sb.AppendLine(" ORDER BY IDTMPNUM");


            return sb.ToString();
        }

        public string CompileSQLNpage(string mSQL, string flt, int LenP,
            //03.08.2016 итоговые поля
            string SumFields, string LabelText, string LabelField
            )
        {
            StringBuilder sb = new StringBuilder();
            int n = mSQL.ToUpper().IndexOf("ORDER BY");
            if (n == -1)
                throw new Exception("not find ORDER BY section!");
            string decSQL = mSQL.Substring(0, n);
            
            //03.08.2016 итоговые поля
            string ItogStr = "";
            if (!string.IsNullOrEmpty(SumFields))
            {
                ItogStr = ", '" + LabelText + "' " + LabelField;
                string[] fsum = SumFields.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in fsum)
                {
                    ItogStr = ItogStr + ", SUM(" + s + ") " + s;
                }
            }
            sb.AppendLine(string.Format(@"SELECT COUNT(*) / {0}  + CASE WHEN COUNT(*) % {0}  = 0 THEN 0 ELSE 1 END NPage, COUNT(*) NR {2} FROM ({1}) TMPA ", LenP, decSQL, ItogStr));
            
            if (!string.IsNullOrEmpty(flt))
            {
                sb.AppendLine(" WHERE ");
                sb.AppendLine(flt);
            }
            return sb.ToString();
        }

        public string getSQL(int IdDeclare, string Query)
        {
            string sql = "select D.*  from t_rpDeclare D(nolock) where IdDeclare = @IdDeclare";
            DataTable decTab = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@IdDeclare", IdDeclare);
            da.Fill(decTab);
            string MainSQL = decTab.Rows[0]["DecSQL"].ToString();
            MainSQL = MainSQL.Replace("{Account}", DBClient.Account);


            string[] vars = Query.Split(new char[] { '&' });
            Dictionary<string, string> p = new Dictionary<string, string>();
            foreach (string v in vars)
            {
                string[] keyval = v.Split(new char[] { '=' });
                p.Add(keyval[0], keyval[1]);
            }

            string NamePar, ValPar;
            Regex Rx = new Regex(@"\{(.*?)\}");
            MatchCollection Mx = Rx.Matches(MainSQL);
            foreach (Match m in Mx)
            {
                NamePar = m.Groups[1].Value;
                if (p.ContainsKey(NamePar))
                { ValPar = p[NamePar]; }
                else
                { ValPar = "0"; }
                MainSQL = MainSQL.Replace("{" + NamePar + "}", ValPar);
            }

            //Для обратной совместимости
            Rx = new Regex(@"\[(.*?)\]");
            Mx = Rx.Matches(MainSQL);
            foreach (Match m in Mx)
            {
                NamePar = m.Groups[1].Value;
                if (p.ContainsKey(NamePar))
                { ValPar = p[NamePar]; }
                else
                { ValPar = "0"; }
                MainSQL = MainSQL.Replace("[" + NamePar + "]", ValPar);
            }


            return MainSQL;
        }

        public Dictionary<string, string> getFormats(int IdDeclare)
        {
            Dictionary<string, string> formats = new Dictionary<string, string>();


            string sql = "select D.*, P.ParamValue from t_rpDeclare D(nolock) inner join t_sysParams P(nolock) on 'GridFind' + D.DecName = P.ParamName where IdDeclare = @IdDeclare";
            DataTable decTab = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@IdDeclare", IdDeclare);
            da.Fill(decTab);
            string s = decTab.Rows[0]["ParamValue"].ToString();

            XmlDocument xm = new XmlDocument();
            XmlElement xRoot;
            xm.LoadXml(s);
            xRoot = xm.DocumentElement;

            foreach (XmlNode xNod in xRoot.SelectNodes("COLUMN"))
            {

                XmlElement xCol = (XmlElement)xNod;
                string FName = xCol.Attributes["FieldName"].Value;
                string DisplayFormat = xCol.Attributes["DisplayFormat"].Value;
                if (!String.IsNullOrEmpty(DisplayFormat))
                    formats.Add(FName, DisplayFormat);
            }
            return formats;
        }

        public string getJSONData(int IdDeclare, string flt, int N, int LenP, string sort, string order, string Query, Dictionary<string, object> Params = null, string MainSQL = null)
        {
            if (string.IsNullOrEmpty(MainSQL))
                MainSQL = getSQL(IdDeclare, Query);
            if (!string.IsNullOrEmpty(sort) & !string.IsNullOrEmpty(order))
            { 
                //Добавляем свою сортировку
                int n = MainSQL.ToUpper().IndexOf("ORDER BY");
                if (n == -1)
                    throw new Exception("not find ORDER BY section!");
                MainSQL = MainSQL.Substring(0, n) + " order by " + DBClient.sortCreate(sort, order);

            }
            Dictionary<string, string> formats = new Dictionary<string, string>();


            string sql = "select D.*, P.ParamValue from t_rpDeclare D(nolock) inner join t_sysParams P(nolock) on 'GridFind' + D.DecName = P.ParamName where IdDeclare = @IdDeclare";
            DataTable decTab = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            da.SelectCommand.Parameters.AddWithValue("@IdDeclare", IdDeclare);
            da.Fill(decTab);
            string s = decTab.Rows[0]["ParamValue"].ToString();
            
            XmlDocument xm = new XmlDocument();
            XmlElement xRoot;
            xm.LoadXml(s);
            xRoot = xm.DocumentElement;

            foreach (XmlNode xNod in xRoot.SelectNodes("COLUMN"))
            {

                XmlElement xCol = (XmlElement)xNod;
                string FName = xCol.Attributes["FieldName"].Value;
                string DisplayFormat = xCol.Attributes["DisplayFormat"].Value;
                if (!String.IsNullOrEmpty(DisplayFormat))
                    formats.Add(FName, DisplayFormat);
            }

            //Итоги 03.08.2016
            string SumFields = xRoot.GetAttribute("SumFields");
            //string FROZENCOLS = xRoot.GetAttribute("FROZENCOLS");
            string LabelText = xRoot.GetAttribute("LabelText");
            string LabelField = xRoot.GetAttribute("LabelField");



            return getJSONData(MainSQL, flt, N, LenP, formats, Params, SumFields, LabelText, LabelField);
        }

        public static string strEscape(string s)
        {
            return s.Replace(@"\", @"\\").Replace(@"""", @"\""")
                                          .Replace("\n", @"\n")
                                          .Replace("\r", @"\r")
                                          .Replace("\t", @"\t");
        }

        /*
        public string getJSONRow(DataRow rw)
        {
            DataTable res = rw.Table;
            string s = "{";
            for (int j = 0; j < res.Columns.Count; j++)
            {
                string fs = @"""" + res.Columns[j].ColumnName + @""":";
                string val = DBClient.strEscape(rw[j].ToString());

                if (rw[j] != DBNull.Value)
                {
                    if (res.Columns[j].DataType == typeof(System.Double))

                        val = ((Double)rw[j]).ToString("#,##0.00");

                    if (res.Columns[j].DataType == typeof(System.DateTime))
                        val = ((DateTime)rw[j]).ToString("dd.MM.yyyy HH:mm");
                }


                fs = fs + @"""" + val + @"""";


                if (j == res.Columns.Count - 1)
                    s = s + fs + "}";
                else
                    s = s + fs + ",";
            }
            return s;
        }
        */

        public string getJSONRow(DataRow rw, Dictionary<string, string> formats)
        {
            if (formats == null)
                formats = new Dictionary<string, string>();

            DataTable res = rw.Table;
            string s = "{";
            for (int j = 0; j < res.Columns.Count; j++)
            {
                string fs = @"""" + res.Columns[j].ColumnName + @""":";
                string val = DBClient.strEscape(rw[j].ToString());

                //01.08.2016 картинка
                if (res.Columns[j].ColumnName == "image_bmp" & !String.IsNullOrEmpty(val))
                    val = @"<img src='data:image/gif;base64," + val + @"'/>";


                //05.10.2016
                if (res.Columns[j].ColumnName == "Color")
                {
                    if (rw[j] != DBNull.Value)
                    {
                        Color cl = Color.FromArgb((int)rw[j]);
                        val = "#" + ((int)cl.R).ToString("X") + ((int)cl.G).ToString("X") + ((int)cl.B).ToString("X");
                    }
                    else
                        val = "#FFFFFF";
                }

                if (rw[j] != DBNull.Value)
                {
                    if (res.Columns[j].DataType == typeof(System.Double))
                    {
                        if (formats.ContainsKey(res.Columns[j].ColumnName))
                            val = ((Double)rw[j]).ToString(formats[res.Columns[j].ColumnName]);
                        else
                            val = ((Double)rw[j]).ToString("#,##0.00");
                        
                    }
                    if (res.Columns[j].DataType == typeof(System.DateTime))
                    {
                     if (formats.ContainsKey(res.Columns[j].ColumnName))
                         val = ((DateTime)rw[j]).ToString(formats[res.Columns[j].ColumnName]);
                     else
                         val = ((DateTime)rw[j]).ToString("dd.MM.yyyy");
                    }
                }


                fs = fs + @"""" + val + @"""";


                if (j == res.Columns.Count - 1)
                    s = s + fs + "}";
                else
                    s = s + fs + ",";
            }
            return s;
        }

        public string getJSONData(string MainSQL, string flt, int N, int LenP, Dictionary<string, string> formats, Dictionary<string, object> Params,
            //03.08.2016 итоговые поля
            string SumFields, string LabelText, string LabelField
            )
        {

            //Список страниц
            DataTable resP = new DataTable();
            string csql = CompileSQLNpage(MainSQL, flt, LenP, SumFields, LabelText, LabelField);
            SqlDataAdapter da = new SqlDataAdapter(csql, DBClient.CnStr);
            //Здесь можно задать параметры
            //02.08.2016
            if (Params!=null)
                foreach (string s in Params.Keys)
                {
                    da.SelectCommand.Parameters.AddWithValue(s, Params[s]);
                }
            
            da.Fill(resP);
            int total = (int)resP.Rows[0][1];

            //Данные Таблицы
            DataTable res = new DataTable();
            csql = CompileSQL(MainSQL, flt, N, LenP);
            da = new SqlDataAdapter(csql, DBClient.CnStr);
            //Здесь можно задать параметры
            //02.08.2016
            if (Params != null)
                foreach (string s in Params.Keys)
                {
                    da.SelectCommand.Parameters.AddWithValue(s, Params[s]);
                }

            da.Fill(res);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < res.Rows.Count; i++)
            {
                string s = getJSONRow(res.Rows[i], formats);
                
                if (i == res.Rows.Count - 1)
                    sb.AppendLine(s);
                else
                    sb.AppendLine(s + ",");
            }

            //03.08.2016 итоги
            string footer = "";
            if (!string.IsNullOrEmpty(SumFields))
                footer = @",""footer"":[" + getJSONRow(resP.Rows[0], formats) + "]";

            string result = "{" + string.Format(@"""total"":{0},""rows"":[{1}]{2}", total, sb.ToString(), footer)  + "}";
            //+ @",""footer"":[{""Заказчик"":""Итого"", ""Итого"":1000}]"
            return result;
            
            //return Json(rowsCreator.GetRows(), JsonRequestBehavior.AllowGet);
        }



       
        public string getJSONData(string MainSQL, Dictionary<string, string> formats)
        {

            string result = "[]";
            try
            {
                DataTable res = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(MainSQL, DBClient.CnStr);
                da.Fill(res);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < res.Rows.Count; i++)
                {
                    string s = getJSONRow(res.Rows[i], formats);

                    if (i == res.Rows.Count - 1)
                        sb.AppendLine(s);
                    else
                        sb.AppendLine(s + ",");
                }

                result = "[" + sb.ToString() + "]";
            }
            catch 
            {; }
            return result;

        }
        

        private string nspace(string str, int n)
        {
            string r = str;
            for (int i = 0; i < n; i++)
                r = " " + r;
            return r;
        }

        public string CreateMenu()
        {

            DataTable menuTab = new DataTable();
            string sql = "SELECT A.* FROM dbo.fn_MainMenuWeb('" + Appl + "', '" + Account + "') A ORDER BY A.ORDMenu";
            SqlDataAdapter da = new SqlDataAdapter(sql, CnStr);
            da.Fill(menuTab);

            DataTable ImageTab = new DataTable();
            sql = "select * from v_sysMenuImage order by Caption, TypeImg";
            da = new SqlDataAdapter(sql, CnStr);
            da.Fill(ImageTab);

            return CreateItem("Root/", menuTab, ImageTab);

        }

        public string CreateItem(string Root, DataTable Tab, DataTable ImageTab)
        {
            
            return "";
        }

        public string CreateComboOption(string sql, string selectValue)
        {
            SqlDataAdapter da = new SqlDataAdapter(sql, DBClient.CnStr);
            DataTable resTab = new DataTable();
            da.Fill(resTab);
            //StringBuilder sb = new StringBuilder();
            string res = "";
            for (int i = 0; i < resTab.Rows.Count; i++)
            {
                string ln = @"<option value=""" + resTab.Rows[i][0].ToString() + @"""";
                if (selectValue == resTab.Rows[i][0].ToString())
                { ln = ln + @" selected = ""selected"""; }
                ln = ln + ">" + resTab.Rows[i][1].ToString() + "</option>";
                //sb.AppendLine(ln);
                res = res + " " + ln;
            }
            //return sb.ToString();
            return res;
        }


        public string CreateComboOption(string[] vals, string selectValue)
        {
            string res = "";
            for (int i = 0; i < vals.Length; i++)
            {
                string ln = @"<option value=""" + vals[i] + @"""";
                if (selectValue == vals[i])
                { ln = ln + @" selected = ""selected"""; }
                ln = ln + ">" + vals[i] + "</option>";
                res = res + " " + ln;
            }
            //
            return res;
        }

        public bool checkAccess(string account, string password)
        {
            var cnstr = Program.isPostgres ? Program.AppConfig["cns"] : Program.AppConfig["mscns"];
            var sqlcheck = "select username from t_ntusers where username = @account and pass = @password";
            var res = new DataTable();
            if (Program.isPostgres)
            {
                var da = new NpgsqlDataAdapter(sqlcheck, cnstr);
                da.SelectCommand.Parameters.AddWithValue("@account", account);
                da.SelectCommand.Parameters.AddWithValue("@password", password);
                da.Fill(res);
            }
            else
            {
                var da = new SqlDataAdapter(sqlcheck, cnstr);
                da.SelectCommand.Parameters.AddWithValue("@account", account);
                da.SelectCommand.Parameters.AddWithValue("@password", password);
                da.Fill(res);
            }
            return (res.Rows.Count > 0);
        }
        public bool CheckLogon(string UserName, string Password)
        {
            return checkAccess(UserName, Password);
        }
        
    }


    public class DBAdapter
    {

        public void LoadFromRow(object mod, DataRow rw, List<string> FieldList)
        {
            Type t = mod.GetType();
            foreach (string fld in FieldList)
            {
                PropertyInfo pi = t.GetProperty(fld);
                if (rw[fld] == DBNull.Value)
                    pi.SetValue(mod, null, null);
                else
                    pi.SetValue(mod, rw[fld], null);

            }
        }

        public void setParameters(object mod, SqlCommand cmd, List<string> FieldList)
        {
            Type t = mod.GetType();
            foreach (string fld in FieldList)
            {
                PropertyInfo pi = t.GetProperty(fld);
                object val = pi.GetValue(mod, null);
                if (val == null)
                    val = DBNull.Value;
                else
                    if (val.GetType() == typeof(System.DateTime))
                        if ((DateTime)val == new DateTime(1, 1, 1))
                            val = DBNull.Value;

                cmd.Parameters.AddWithValue("@" + fld, val);
            }
        }


        public void saveDB(object mod, string procName, List<string> FieldList)
        {
            SqlConnection cn = new SqlConnection(DBClient.CnStr);
            SqlCommand cmd = new SqlCommand(procName, cn);
            cmd.CommandType = CommandType.StoredProcedure;
            setParameters(mod, cmd, FieldList);
            cn.Open();
            cmd.ExecuteNonQuery();
            cn.Close();
        }

        protected object GetFieldValue(String FieldName, DataRow _WorkRow)
        {
            Object r = _WorkRow[FieldName];

            if (r == DBNull.Value | r == null)
            {
                if (_WorkRow.Table.Columns[FieldName].DataType == Type.GetType("System.String"))
                    r = "";

                if (_WorkRow.Table.Columns[FieldName].DataType == Type.GetType("System.Int32"))
                    r = 0;

                if (_WorkRow.Table.Columns[FieldName].DataType == Type.GetType("System.DateTime"))
                    r = DateTime.MinValue;


                if (_WorkRow.Table.Columns[FieldName].DataType == typeof(System.Double))
                    r = 0.0;

                if (_WorkRow.Table.Columns[FieldName].DataType == typeof(System.Guid))
                    r = Guid.Empty;


            }

            return r;
        }

        

    }
}