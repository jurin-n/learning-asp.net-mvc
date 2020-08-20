using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using WebApp.Common;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index(string id) //TODO:任意の名前のクエリストリングをパラメータに設定する方法調べる。
        {
            string orderId = id;

            if (orderId == null || orderId.Trim() == "")
            {
                var items = new List<Item>
                    {
                        new Item() { No=1,Id="", Name="",Description="", isValid=false, Type="" },
                        new Item() { No=2,Id="", Name="",Description="", isValid=false, Type="" },
                        new Item() { No=3,Id="", Name="",Description="", isValid=false, Type="" }
                    };
                var order = new Order() { isBulk = true, items = items };
                return View(order);
            } else {

                /* 更新用画面として表示するための処理 */
                //更新データ初期表示
                var items = new List<Models.Item>();
                using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
                {
                    conn.Open();
                    String sqlForList = @"
                        SELECT
                            T1.OrderID
                            ,T1.No
                            ,T1.ItemID
                            ,(CASE WHEN T2.Name IS NULL THEN '' ELSE T2.Name END) AS NAME
                            ,(CASE WHEN T2.Description IS NULL THEN '' ELSE T2.Description END) AS Description
                            ,(CASE WHEN T2.Type IS NULL THEN 'NVARCHAR' ELSE T2.Type END) AS Type
                        FROM Orders_Items T1 LEFT JOIN Items T2
                        ON T1.ItemID = T2.ItemID
                        WHERE T1.OrderID = @OrderId ORDER BY T1.No
                    ";
                    using (SqlCommand command = new SqlCommand(sqlForList))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        command.Connection = conn;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(
                                    new Item() {
                                         No = (int)reader["No"]
                                       , Id = (String)reader["ItemID"]
                                       , Name = (String)reader["Name"]
                                       , Description = (String)reader["Description"]
                                       , Type = (String)reader["Type"]
                                       , isValid = true
                                    }      
                                );
                            }
                        }
                    }
                }
                String BulkRegistration = "";
                foreach (Item i in items)
                {
                    BulkRegistration += (i.Id+"\n");
                }

                string OrderId = "";
                string Description = "";

                using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
                {
                    conn.Open();
                    String sqlForList = @"
                        SELECT
                             OrderID
                            ,OrderDescription
                        FROM Orders
                        WHERE OrderId = @OrderId
                    ";
                    using (SqlCommand command = new SqlCommand(sqlForList))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        command.Connection = conn;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderId = (String)reader["OrderID"];
                                Description = (String)reader["OrderDescription"];
                            }
                        }
                    }
                }

                var order = new Order() { OrderId = OrderId, OrderDescription= Description, BulkRegistration= BulkRegistration, isBulk = true, items = items };
                return View(order);
            }
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            //String SelectedNav = form.GetValues("nav-textarea-or-list")[0];
            //String[] ItemIds = form.GetValues("ItemId");
            String SelectedNav = form.GetValues("nav-textarea-or-list")[0];
            String OrderId = form.GetValues("OrderId")[0];
            String OrderDescription = form.GetValues("OrderDescription")[0];
            if (SelectedNav.Equals("textarea")) 
            {
                String BulkRegistration = form.GetValues("BulkRegistration")[0];
                String[] columns =  BulkRegistration.Replace("\r\n", "\n").Split(new[] { '\n', '\r' });
                var items = new Dictionary<String, Models.Item>();

                using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
                {
                    conn.Open();

                    String sql = @"
                        SELECT
                             ItemID
                            ,(CASE WHEN Name IS NULL THEN '' ELSE Name END) AS NAME
                            ,(CASE WHEN Description IS NULL THEN '' ELSE Description END) AS Description
                            ,(CASE WHEN Type IS NULL THEN 'NVARCHAR' ELSE Type END) AS Type
                        FROM Items WHERE ItemID IN(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19)
                        ";


                    //String sql = "SELECT ItemID FROM Items WHERE ItemID IN('ID001','ID002')";
                    using (SqlCommand command = new SqlCommand(sql))
                    {
                        //command.Parameters.AddWithValue("@ItemIDs", "'ID001','ID002'");
                        for (int i = 0; i < 20; i++)
                        {
                            command.Parameters.AddWithValue("@" + i.ToString()
                                , (columns.ElementAtOrDefault(i) == null ? "" : columns.ElementAtOrDefault(i)));
                        }
                        command.Connection = conn;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(
                                    (String)reader["ItemID"],
                                    new Item() {
                                         No = 1,
                                         Id = (String)reader["ItemID"],
                                         Name = (String)reader["Name"],
                                         Description = (String)reader["Description"],
                                         Type = (String)reader["Type"],
                                         isValid = true
                                    }   
                                );
                            }
                        }
                    }
                }


                var list = new List<Item>();
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i].Trim().Length != 0)
                    {
                        if (items.ContainsKey(columns[i]))
                        {
                            Item item = items[columns[i]];
                            list.Add(
                                new Item() {
                                    No = i + 1,
                                    Id = item.Id,
                                    Name = item.Name,
                                    Description = item.Description,
                                    Type = item.Type,
                                    isValid = item.isValid
                                }
                            );
                        }
                        else
                        {
                            list.Add(
                                new Item() {
                                    No=i + 1,
                                    Id=columns[i],
                                    Name="",
                                    Description="",
                                    Type="",
                                    isValid=false
                                }
                            );
                        }
                    }
                }
                var order = new Order() { OrderId = OrderId, OrderDescription = OrderDescription, BulkRegistration = BulkRegistration, isBulk = false, items = list };
                return View(order);
            }
            return View();
        }
    }
}