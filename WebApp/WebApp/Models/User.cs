using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class User
    {
        [Required(ErrorMessage = "ユーザIDは必ず入力してください。")]
        public String UserId {get; set;}
        
        [Required(ErrorMessage = "パスワードは必ず入力してください。")] 
        public String Password { get; set; }

        public bool isValid { get; set; }
    }
}