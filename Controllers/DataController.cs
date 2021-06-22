using Microsoft.AspNetCore.Mvc;
using WpfBu.Models;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace netbu.Controllers
{
    //[Authorize]
    public class DataController : Controller
    {
        public JsonResult getdata(string id, string mode, string page, string Fc, string TextParams, string SQLParams)
        {
            var F = new Finder();
            F.Account = User.Identity.Name;
            if (string.IsNullOrEmpty(F.Account))
                F.Account = "malkin";

            F.nrows = 30;
            if (!string.IsNullOrEmpty(mode))
                F.Mode = mode;
            else
                F.Mode = "new";

            if (!string.IsNullOrEmpty(page))
                F.page = int.Parse(page);

            if (!string.IsNullOrEmpty(Fc))
            {
                F.Fcols = JsonConvert.DeserializeObject<List<FinderField>>(Fc);
            }

            if (!string.IsNullOrEmpty(TextParams))
            {
                F.TextParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(TextParams);
            }

            if (!string.IsNullOrEmpty(SQLParams))
            {
                F.SQLParams = JsonConvert.DeserializeObject<Dictionary<string, object>>(SQLParams);
                Dictionary<string, object> parseParam = new Dictionary<string, object>();
                foreach (string k in F.SQLParams.Keys)
                {
                    DateTime dval;
                    string val = F.SQLParams[k].ToString();
                    if (DateTime.TryParse(val, out dval))
                    {
                        parseParam.Add(k, dval);
                    }
                    else
                    {
                        parseParam.Add(k, F.SQLParams[k]);
                    }
                }
                F.SQLParams = parseParam;
            }


            try
            {   
                F.not_page = "y";
                F.start(id);
                return Json(F.MainTab);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

    }
}