using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Item
    {
        public int No { get; set; }
        public String Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String Type { get; set; }
        public bool isValid { get; set; }

        public String getSelected( String selectType)
        {
            if (this.Type.Equals(selectType))
            {
                return "selected";
            }
            return "";
        }
    }
}