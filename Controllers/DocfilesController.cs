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



namespace netbu.Controllers
{
    
    [Authorize]
    public class DocfilesController : Controller
    {
        private IHostingEnvironment _env;
        public DocfilesController(IHostingEnvironment env)
        {
            _env = env;
        }    

        [AllowAnonymous]
        public IActionResult fcheck()
        {
            filecheckAsync();
            return Content("Process started");
        }
        private async void filecheckAsync()
        {
            await Task.Run(() => filecheck());
        }
        private void filecheck()
        {
            try
            {
                string sqlu = "update cntfilehistory set fh_vol = @fh_vol where fh_pk = @fh_pk";
                SqlConnection cn = new SqlConnection(Program.AppConfig["mscns"]);
                SqlCommand cmd = new SqlCommand(sqlu, cn);
                string sql = "select fh_pk,  fh_filename from cntfilehistory(nolock) where fh_vol is null";
                SqlDataAdapter da = new SqlDataAdapter(sql, Program.AppConfig["mscns"]);
                DataTable res = new DataTable();
                da.Fill(res);

                cn.Open();
                foreach (DataRow rw in res.Rows)
                {
                    try
                    {
                        var filename = rw["fh_filename"].ToString();
                        var fi = new FileInfo(filename);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@fh_vol", fi.Length);
                        cmd.Parameters.AddWithValue("@fh_pk", rw["fh_pk"]);
                        cmd.ExecuteNonQuery();
                    }
                    catch {; }
                }
                cn.Close();
            }
            catch {; }
        }

        [AllowAnonymous]
        public ActionResult photo(string id)
        {
            string path = Program.AppConfig["photofiles"] + @"\" + id;
            string ctype = "image/jpeg";
            if (System.IO.File.Exists(path))
            {
                byte[] buf = System.IO.File.ReadAllBytes(path);
                return File(buf, ctype);
            }
            else
                return NotFound();
        }
        public IActionResult errorlog()
        {
            var webRoot = _env.WebRootPath;
            var path = webRoot +  @"\..\netbu_error.log";
            string ctype = "application/octet-stream";
            return PhysicalFile(path, ctype, "netbu_error.log");
        }
        public IActionResult file(string id, string id64)
        {
            try
            {
                if (!string.IsNullOrEmpty(id64))
                {
                    id64 = id64.Replace(" ", "+");
                    id = Encoding.UTF8.GetString(Convert.FromBase64String(id64));
                }
                else
                    id = WebUtility.HtmlDecode(id);


                string idf = id.Replace("/", @"\");
                string path = Program.AppConfig["docfiles"] + @"\" + idf;
                //Ищем на альтернативном сервере 01.12.2020
                //if (!System.IO.File.Exists(path))
                //    path = Program.AppConfig["docfiles2"] + @"\" + idf;

                string ext = Path.GetExtension(path).ToLower().Replace(".", "");
                string ctype = "application/octet-stream";
                /*
                if (ext == "pdf")
                    ctype = "application/pdf";
                */
                //async log 17/10/2020
                filelogAsync(path, "get");

                if (ext == "gif" || ext == "bmp" || ext == "jpg" || ext == "jpeg" || ext == "png")
                    ctype = "image/jpeg";
                if (ext == "tiff")
                    ctype = "image/tiff";

                //17.10.2020
                return PhysicalFile(path, ctype, Path.GetFileName(path));
                /*
                if (ctype == "application/octet-stream")
                    return PhysicalFile(path, ctype, Path.GetFileName(path));
                else
                {
                    byte[] buf = System.IO.File.ReadAllBytes(path);
                    return File(buf, ctype);
                }
                */

            }
            catch (Exception ex)
            {
                string mes = "Ошибка приложения. " + ex.Message;
                return Content(mes);
            }

        }

        //async log 17/10/2020
        private async void filelogAsync(string path, string action)
        {
            await Task.Run(() => filelog(path, action));
        }

        private void filelog(string path, string action)
        {
            //лог 18.12.2019
            try
            {
                String sql = "p_cntfilehistory_add";
                SqlDataAdapter da = new SqlDataAdapter(sql, Program.AppConfig["mscns"]);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@fh_filename", path);
                da.SelectCommand.Parameters.AddWithValue("@fh_account", User.Identity.Name);
                da.SelectCommand.Parameters.AddWithValue("@fh_action", action);  //19/02/2020
                DataTable head = new DataTable();
                da.Fill(head);
            }
            catch
            {; }
            //лог 18.12.2019

        }
        public JsonResult delete_file(string id, string mode)
        {
            id = WebUtility.HtmlDecode(id);
            string res = "";
            try
            {
                string idf = id.Replace("/", @"\");
                string path = Program.AppConfig["docfiles"] + @"\" + idf;
                 //Ищем на альтернативном сервере 01.12.2020
                //if (!System.IO.File.Exists(path))
                //    path = Program.AppConfig["docfiles2"] + @"\" + idf;

                //лог 19.02.2020
                try
                {
                    
                }
                catch
                {; }
                //лог 19.02.2020


                if (mode == "file")
                    System.IO.File.Delete(path);
                else
                    Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                res = e.Message;
            }
            return Json(new { error = res });
        }

        public JsonResult newdir(string id, string dir)
        {
            id = WebUtility.HtmlDecode(id);
            string res = "";
            try
            {
                string idf = id.Replace("/", @"\");
                string path = Program.AppConfig["docfiles"] + @"\" + idf; 
                //Ищем на альтернативном сервере 01.12.2020
                //if (!Directory.Exists(path))
                //    path = Program.AppConfig["docfiles2"] + @"\" + idf;
                
                path = path + @"\" + dir;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                res = e.Message;
            }
            return Json(new { error = res });
        }
        public JsonResult upload(string id, List<IFormFile> files)
        {

            string res = "";
            id = WebUtility.HtmlDecode(id);
            string idf = id.Replace("/", @"\");
            string path = Program.AppConfig["docfiles"] + @"\" + idf;  //  / заменили на \
            //Ищем на альтернативном сервере 01.12.2020
            //if (!Directory.Exists(path))
            //    path = Program.AppConfig["docfiles2"] + @"\" + idf;

            try
            {


                if (files != null)
                    foreach (IFormFile img in files)
                    {

                        //лог 19.02.2020
                        try
                        {
                            
                        }
                        catch
                        {; }
                        //лог 19.02.2020

                        string FileName = img.FileName;
                        int n = (int)img.Length;
                        byte[] buf = new byte[n];
                        Stream ms = img.OpenReadStream();
                        ms.Read(buf, 0, n);
                        System.IO.File.WriteAllBytes(path + img.FileName, buf);
                    }
            }
            catch (Exception e)
            {
                res = e.Message;
            }

            return Json(new { error = res });

        }

        
        public IActionResult dir(string id, string id64, string caption)
        {
            if (string.IsNullOrEmpty(caption))
                caption="";
            try
            {

                if (!string.IsNullOrEmpty(id64))
                {
                    id64 = id64.Replace(" ", "+");
                    id = Encoding.UTF8.GetString(Convert.FromBase64String(id64));

                    caption = caption.Replace(" ", "+");
                    caption = Encoding.UTF8.GetString(Convert.FromBase64String(caption));
                }
                else
                {
                    id = WebUtility.HtmlDecode(id);
                    caption = WebUtility.HtmlDecode(caption);
                }    

                string idf = id.Replace("/", @"\");
                string[] paths = id.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string parent = "";
                if (paths.Length > 1)
                    parent = string.Join("/", paths, 0, paths.Length - 1) + "/";
                string path = Program.AppConfig["docfiles"] + @"\" + idf;

                //Ищем на альтернативном сервере 01.12.2020
                //if (!Directory.Exists(path))
                //    path = Program.AppConfig["docfiles2"] + @"\" + idf;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                DirectoryInfo di = new DirectoryInfo(path);
                //DirectoryInfo[] dirs = di.GetDirectories();
                //FileInfo[] files = di.GetFiles();
                //Доступ
                int fileacc = 15;
                ViewBag.fileacc = fileacc;

                string pagetitle = id;
                if (!string.IsNullOrEmpty(caption))
                pagetitle = pagetitle.Replace(paths[0], caption);


                ViewBag.di = di;
                //ViewBag.dirs = dirs;
                //ViewBag.files = files;
                ViewBag.id = id;
                ViewBag.parent = parent;
                ViewBag.caption = caption;
                ViewBag.pagetitle = pagetitle;

                return View();
            }
            catch (Exception ex)
            {
                string mes = "Ошибка приложения. " + ex.Message;
                return Content(mes);
            }
        }
    }
}