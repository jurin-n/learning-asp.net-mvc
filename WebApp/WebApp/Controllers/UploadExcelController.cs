﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Packaging;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using A = DocumentFormat.OpenXml.OpenXmlAttribute;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Transactions;
using System.Data.SqlClient;
using WebApp.Common;
using WebApp.Models;
using Item = WebApp.Models.Item;
using WebApp.Filters;

namespace WebApp.Controllers
{
    [CustomAuthenticationFilter]
    public class UploadExcelController : Controller
    {
        // GET: UploadExcel
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form, HttpPostedFileBase file)
        {
            String Id = form.GetValues("Id")[0];
            String Description = form.GetValues("Description")[0];

            //System.Diagnostics.Debug.WriteLine(Id);
            //System.Diagnostics.Debug.WriteLine(Description);

            if (file != null && file.ContentLength > 0)
            {

                var confirmItem = new ConfirmItem() { items = new List<Item>()};

                // Open file as read-only.
                using (SpreadsheetDocument SpreadSheet = SpreadsheetDocument.Open(file.InputStream, false))
                {
                    //Read the first Sheets 
                    Sheet sheet = SpreadSheet.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                    Worksheet worksheet = (SpreadSheet.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                    IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                    bool firstRow = true;
                    int index = 1;
                    foreach (Row row in rows)
                    {
                        if (firstRow)
                        {
                            firstRow = false;
                        }
                        else
                        {
                            if (row.Descendants<Cell>().ElementAt(0).CellValue == null) 
                            {
                                break;
                            }
                            confirmItem.items.Add(
                                new Item() {
                                    No = index,
                                    Id = GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(0)),
                                    Name = GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(1)),
                                    Description = GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(2)),
                                    isValid = true,
                                    Type = GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(3))
                                }
                            );
                            index++;
                        }
                    }

                    /*
                    bool firstRow = true;
                    //トランザクション開始
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
                        {
                            String sql = @"
                            INSERT INTO ExcelSheetData(cell01,cell02,cell03,cell04,cell05)
                            VALUES (@01,@02,@03,@04,@05)
                            ";

                            conn.Open();

                            foreach (Row row in rows)
                            {
                                if (firstRow)
                                {
                                    firstRow = false;
                                }
                                else
                                {
                                    using (SqlCommand command = new SqlCommand(sql))
                                    {

                                        command.Connection = conn;
                                        //System.Diagnostics.Debug.WriteLine("----------------");
                                        //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(0)));
                                        //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(1)));
                                        //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(2)));
                                        //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(3)));
                                        //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(4)));
                                        command.Parameters.AddWithValue("@01", GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(0)));
                                        command.Parameters.AddWithValue("@02", GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(1)));
                                        command.Parameters.AddWithValue("@03", GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(2)));
                                        command.Parameters.AddWithValue("@04", GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(3)));
                                        command.Parameters.AddWithValue("@05", GetCellValue(SpreadSheet, row.Descendants<Cell>().ElementAt(4)));
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        scope.Complete();
                    }
                    */
                }
                return View("confirm", confirmItem);
            }
            return View();
        }
        private string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).FirstChild.InnerText;
            }
            return value;
        }

        [HttpPost]
        public ActionResult Confirm(FormCollection form)
        {

            var confirmItem = new ConfirmItem() { items = new List<Item>() };
            for (int i = 0; i < form.GetValues("ItemId").Length; i++)
            {
                //return View("ErrorResult");
                confirmItem.items.Add(
                    new Item()
                    {
                        No = int.Parse(form.GetValues("ItemNo")[i]),
                        Id = form.GetValues("ItemId")[i],
                        Name = form.GetValues("ItemName")[i],
                        Description = form.GetValues("ItemDescription")[i],
                        isValid = false,
                        Type = form.GetValues("ItemType")[i]
                    }
                );
            }

            //デモ用に適当にエラーになるロジック動くようにする
            if (form.GetValues("ItemId").Length == 2) { 
                return View("confirm", confirmItem);
            }

            //トランザクション開始
            using (TransactionScope scope = new TransactionScope())
            {
                using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
                {
                    String sql = @"
                            INSERT INTO ExcelSheetData(cell01,cell02,cell03,cell04)
                            VALUES (@01,@02,@03,@04)
                            ";

                    conn.Open();

                    for (int i = 0; i < form.GetValues("ItemId").Length; i++)
                    {
                        using (SqlCommand command = new SqlCommand(sql))
                        {

                            command.Connection = conn;
                            //System.Diagnostics.Debug.WriteLine("----------------");
                            //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(0)));
                            //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(1)));
                            //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(2)));
                            //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(3)));
                            //System.Diagnostics.Debug.WriteLine(GetCellValue(SpreadSheet,row.Descendants<Cell>().ElementAt(4)));
                            command.Parameters.AddWithValue("@01", form.GetValues("ItemId")[i]);
                            command.Parameters.AddWithValue("@02", form.GetValues("ItemName")[i]);
                            command.Parameters.AddWithValue("@03", form.GetValues("ItemDescription")[i]);
                            command.Parameters.AddWithValue("@04", form.GetValues("ItemType")[i]);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                scope.Complete();
            }
            ViewData["uploadedRecordCount"] = form.GetValues("ItemId").Length;
            return View("result");
        }
    }
}