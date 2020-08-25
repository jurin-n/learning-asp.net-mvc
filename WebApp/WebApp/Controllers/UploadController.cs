using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebApp.Common;
using System.Transactions;

namespace WebApp.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {

            if (file != null && file.ContentLength > 0)
            {
                string fileName = Path.GetFileName(file.FileName);
                //string path = Path.Combine(Server.MapPath("~/js/"), fileName);
                //file.SaveAs(path);

                //トランザクション開始
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
                    {

                        String sql = @"
                        INSERT INTO UploadedFiles(UploadId,UploadBinary,CreatedOn)
                        VALUES (@UploadId,@UploadBinary,@CreatedOn)
                        ";

                        using (SqlCommand command = new SqlCommand(sql))
                        {
                            command.Parameters.AddWithValue("@UploadId", fileName);
                            command.Parameters.Add("@UploadBinary", SqlDbType.Binary, -1).Value = file.InputStream;
                            command.Parameters.AddWithValue("@CreatedOn", "2020-08-25");

                            conn.Open();
                            command.Connection = conn;
                            command.ExecuteNonQuery();
                        }
                    }
                    scope.Complete();
                }
            }

            return View();
        }

        public ActionResult UploadDocument()
        {
            return View();
        }
    }
}