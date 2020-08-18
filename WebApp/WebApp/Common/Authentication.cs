using System;
using System.Data.SqlClient;

namespace WebApp.Common
{
    public class Authentication
    {
        public static int IdAndPasswordSignIn(String UserId, String Password)
        {
            using (SqlConnection conn = new SqlConnection(DbHelper.getConnectionString()))
            {
                String sql = "SELECT COUNT(*) FROM Users WHERE UserID=@UserId AND Password=@Password";
                using (SqlCommand command = new SqlCommand(sql))
                {
                    command.Parameters.AddWithValue("@UserId", UserId);
                    command.Parameters.AddWithValue("@Password", Encrypt(Password));

                    conn.Open();
                    command.Connection = conn;
                    if ((int)command.ExecuteScalar() == 1)
                    {
                        //UserIdとPasswordにマッチするユーザ情報があったためSignIn成功。
                        return 0;
                    }
                    //SignIn失敗
                    return -1;
                }
            }   
        }

        private static object Encrypt(string password)
        {
            //TODO:暗号化ロジック実装
            return password;
        }
    }
}