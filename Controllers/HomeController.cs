using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using netbu.Models;
using Npgsql;
using Microsoft.AspNetCore.Authorization;
using Novell.Directory.Ldap;

namespace netbu.Controllers
{
    public class HomeController : Controller
    {

        //[Authorize]
        public ActionResult Index(string id)
        {
            ViewBag.id = id;
            ViewBag.account = User.Identity.Name;
            return View();
        }

        #region secret
        [Route("ustore/gettree")]
        //[Authorize]
        public JsonResult gettree()
        {
            try
            {
                
                string account = User.Identity.Name;
                if (string.IsNullOrEmpty(account))
                    account = "Admin";
                var tu = new treeutil();

                var data = new DataTable();
                var cnstr = Program.isPostgres ? Program.AppConfig["cns"] : Program.AppConfig["mscns"]; 
                var sql = "select a.* , fn_getmenuimageid(a.caption) idimage from fn_mainmenu('ALL', @Account) a order by a.ordmenu, idmenu";
                if (!Program.isPostgres)
                    //sql = "select a.* , dbo.fn_getmenuimageid(a.caption) idimage from fn_mainmenu('ALL', @Account) a order by a.ordmenu, idmenu";
                    sql = "exec p_fn_getmenuimageid @Account";
                if (Program.isPostgres)
                {
                    var da = new NpgsqlDataAdapter(sql, cnstr);
                    da.SelectCommand.Parameters.AddWithValue("@Account", account);
                    da.Fill(data);
                }
                else
                {
                    var da = new SqlDataAdapter(sql, cnstr);
                    da.SelectCommand.Parameters.AddWithValue("@Account", account);
                    da.Fill(data);
                }

                var rootItem = new treeItem("root");
                rootItem.children = new List<object>();

                tu.CreateItems("Root/", rootItem, data);
                return Json(rootItem.children);
            }
            catch (Exception e)
            {
                return Json(new object[] { new { text = e.Message } });
            }
        }

        

        [Route("/pg/getid/{table_name}")]
        public JsonResult getid(string table_name)
        {
            if (Program.isPostgres)
            {
                var sql = "select column_default, udt_name  from information_schema.columns  where table_name = @table_name and ordinal_position = 1";
                var cnstr = Program.AppConfig["cns"];
                var da = new NpgsqlDataAdapter(sql, cnstr);
                da.SelectCommand.Parameters.AddWithValue("@table_name", table_name);
                var rec = new DataTable();
                da.Fill(rec);

                if (rec.Rows.Count == 0)
                {
                    return Json(new { id = "" });
                };
                if (rec.Rows[0]["column_default"].ToString() == "" && rec.Rows[0]["udt_name"].ToString() != "uuid")
                {
                    return Json(new { id = "" });
                };
                var c_default = rec.Rows[0]["column_default"].ToString();
                if (rec.Rows[0]["udt_name"].ToString() == "uuid")
                    c_default = "uuid_generate_v1()";
                sql = "select " + c_default + " id";
                da = new NpgsqlDataAdapter(sql, cnstr);
                var result = new DataTable();
                da.Fill(result);
                return Json(new { id = result.Rows[0]["id"] });
            }
            else
            {
                var sql = "select c.user_type_id from sys.tables t(nolock) inner join sys.columns c(nolock) on t.object_id = c.object_id where t.name = @tablename and column_id = 1";
                var cnstr = Program.AppConfig["mscns"];
                var da = new SqlDataAdapter(sql, cnstr);
                da.SelectCommand.Parameters.AddWithValue("@tablename", table_name);
                var rec = new DataTable();
                da.Fill(rec);
                if (rec.Rows.Count == 0)
                {
                    return Json(new { id = "" });
                };
                if ((int)rec.Rows[0][0] == 36)
                {
                    return Json(new { id = Guid.NewGuid().ToString() });
                }
                else
                {
                    return Json(new { id = "" });
                }
            }

        }

        #endregion


        private async Task Authenticate(string userName)
        {

            // создаем один claim
            var claims = new List<Claim> {
                new Claim (ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(id),
            new AuthenticationProperties  //запоминает пользователя
            {
                IsPersistent = true
            });
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/Home/Login");
        }
        public ActionResult Login()
        {
            DBClient dc = new DBClient();
            return View(dc);
        }

        [HttpPost]
        public async Task<ActionResult> Login(DBClient model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                DBClient dc = new DBClient();
                bool auth = dc.CheckLogon(model.UserName, model.Password);
                if (!auth)
                {
                    //Пробуем через LDAP
                    string ldap_server = Program.AppConfig["ldap_server"];
                    string ldap_root = Program.AppConfig["ldap_root"];
                    int ldap_port = int.Parse(Program.AppConfig["ldap_port"]);
                    string ldap_user = "cn=" + model.UserName + "," + ldap_root;
                    if (!string.IsNullOrEmpty(Program.AppConfig["domain"]))
                        ldap_user = Program.AppConfig["domain"] + @"\" + model.UserName;
                    try
                    {
                        LdapConnection ldapConn = new LdapConnection();
                        ldapConn.Connect(ldap_server, ldap_port);
                        ldapConn.Bind(ldap_user, model.Password);
                        auth = true;
                    }
                    catch
                    {; }
                }
                if (auth)
                {
                    await Authenticate(model.UserName); // аутентификация
                    return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин или пароль");
                    return View();
                }
            }
            else
            {
                return View();
            }

        }

        

        

    }

}