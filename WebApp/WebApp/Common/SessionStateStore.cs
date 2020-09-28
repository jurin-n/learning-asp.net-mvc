using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace WebApp.Common
{
    public class SessionStateStore : SessionStateStoreProviderBase
    {
        private string connectionString;
        private string applicationName;
        private SessionStateSection pConfig = null;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            base.Initialize(name, config);

            // セッション管理で使うDBの接続文字列設定
            connectionString = ConfigurationManager.ConnectionStrings[config["connectionStringName"]].ConnectionString;
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            // アプリケーション名設定
            applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;

            //
            // Get <sessionState> configuration element.
            //
            pConfig = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(),
               SessionStateUtility.GetSessionStaticObjects(context),
               timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            //cookielessモード使わないので実装予定なし
            //　参考）https://docs.microsoft.com/en-us/dotnet/api/system.web.sessionstate.sessionstatestoreproviderbase.createuninitializeditem?redirectedfrom=MSDN&view=netframework-4.8#System_Web_SessionState_SessionStateStoreProviderBase_CreateUninitializedItem_System_Web_HttpContext_System_String_System_Int32_
            //throw new NotImplementedException();
            //Cookielessモードにしてないのになぜか呼ばれるので例外スローするのやめてみる。
            //Debug.WriteLine("CreateUninitializedItem");
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void EndRequest(HttpContext context)
        {
            //実装するか迷い中。
            //throw new NotImplementedException();
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            //System.Web.Configuration.PagesSection クラスのプロパティEnableSessionState がread-onlyに
            //セットされたときに呼び出されるメソッドのため実装せず。
            //実行もされない想定のため例外スロー。
            throw new NotImplementedException("System.Web.Configuration.PagesSection クラスのプロパティEnableSessionState がread-onlyにすることを想定しないため実装していません。");
        }


        //DBからセッション取得＆セッション情報ロック
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {

            //return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);

            DateTime now = DateTime.Now;
            // DateTime to check if current session item is expired.
            DateTime expires;
            // String to hold serialized SessionStateItemCollection.
            string serializedItems = null;
            // Timeout value from the data store.
            int timeout = 0;

            locked = false;
            lockAge = TimeSpan.Zero;
            lockId = null;
            actions = SessionStateActions.None;

            //トランザクション開始
            using (TransactionScope scope = new TransactionScope())
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // セッション情報取得
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;

                        //セッション情報取得
                        command.CommandText = @"
SELECT Expires, SessionItems, Locked, LockId, LockDate, Timeout
FROM Sessions
WHERE SessionId = @SessionId
  AND ApplicationName = @ApplicationName
  AND Expires > @Now
";
                        command.Parameters.AddWithValue("@SessionId", id);
                        command.Parameters.AddWithValue("@ApplicationName", applicationName);
                        command.Parameters.AddWithValue("@Now", now);
                        using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if(reader.Read())
                            {
                                expires = reader.GetDateTime(0);
                                serializedItems = reader.GetString(1);
                                locked = reader.GetBoolean(2);
                                lockId = reader.GetInt32(3);
                                lockAge = DateTime.Now.Subtract(reader.GetDateTime(4));
                                timeout = reader.GetInt32(5);
                            }
                        }
                    }

                    //セッション情報が有効期限切れの場合、削除
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = @"
DELETE FROM Sessions
WHERE SessionId = @SessionId
  AND ApplicationName = @ApplicationName
  AND Expires <= @Now
";
                        command.Parameters.AddWithValue("@SessionId", id);
                        command.Parameters.AddWithValue("@ApplicationName", applicationName);
                        command.Parameters.AddWithValue("@Now", now);
                        command.ExecuteNonQuery();
                    }
                }
                scope.Complete();
            }


            if (serializedItems == null)
            {
                return null;
            }
            else
            {
                if (locked)
                {
                    return null;
                }
                else
                {
                    lockId = (int)lockId + 1;

                    //トランザクション開始
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            using (SqlCommand command = new SqlCommand())
                            {

                                command.Connection = conn;
                                command.CommandText = @"
UPDATE Sessions
SET LockId = @LockId
   ,Locked = @Locked
   ,LockDate = @LockDate
WHERE SessionId = @SessionId
  AND ApplicationName = @ApplicationName
";
                                command.Parameters.AddWithValue("@LockId", lockId);
                                command.Parameters.AddWithValue("@Locked", true);
                                command.Parameters.AddWithValue("@LockDate", now);
                                command.Parameters.AddWithValue("@SessionId", id);
                                command.Parameters.AddWithValue("@ApplicationName", applicationName);
                                command.ExecuteNonQuery();
                            }
                        }
                        scope.Complete();
                    }
                    return Deserialize(context, serializedItems, timeout);
                }
            }
        }

        public override void InitializeRequest(HttpContext context)
        {
            //実装せず
        }


        //ロック解除
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            //トランザクション開始
            using (TransactionScope scope = new TransactionScope())
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = @"
UPDATE Sessions
SET Locked = @Locked
   ,Expires = @Expires 
WHERE SessionId = @SessionId
  AND ApplicationName = @ApplicationName
  AND LockId = @LockId
";
                        command.Parameters.AddWithValue("@Locked", false);
                        command.Parameters.AddWithValue("@Expires", DateTime.Now.AddMinutes(pConfig.Timeout.TotalMinutes));
                        command.Parameters.AddWithValue("@SessionId", id);
                        command.Parameters.AddWithValue("@ApplicationName", applicationName);
                        command.Parameters.AddWithValue("@LockId", lockId);

                        command.ExecuteNonQuery();
                    }
                }
                scope.Complete();
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            throw new NotImplementedException();
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            //実装せず
        }


        //セッションをDB登録または更新
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            //System.Diagnostics.Debug.WriteLine(item.Items["UserId"]); //セッションに設定した内容が取れること確認。
            // セッション情報を直列化
            string sessItems = Serialize((SessionStateItemCollection)item.Items);


            if (newItem)
            {
                //トランザクション開始
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        //有効期限切れのセッション削除
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = @"
DELETE FROM Sessions WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND Expires < @Expires
";
                            command.Parameters.AddWithValue("@SessionId", id);
                            command.Parameters.AddWithValue("@ApplicationName", applicationName);
                            command.Parameters.AddWithValue("@Expires", DateTime.Now);

                            command.ExecuteNonQuery();
                        }

                        //新規セッション追加
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = @"
INSERT INTO Sessions (SessionId, ApplicationName, Created, Expires, LockDate, LockId, Timeout, Locked, SessionItems, Flags)
Values(@SessionId, @ApplicationName, @Created, @Expires, @LockDate, @LockId, @Timeout, @Locked, @SessionItems, @Flags)
";
                            command.Parameters.AddWithValue("@SessionId", id);
                            command.Parameters.AddWithValue("@ApplicationName", applicationName);
                            command.Parameters.AddWithValue("@Created", DateTime.Now);
                            command.Parameters.AddWithValue("@Expires", DateTime.Now.AddMinutes((Double)item.Timeout));
                            command.Parameters.AddWithValue("@LockDate", DateTime.Now);
                            command.Parameters.AddWithValue("@LockId", 0);
                            command.Parameters.AddWithValue("@Timeout", item.Timeout);
                            command.Parameters.AddWithValue("@Locked",false);
                            command.Parameters.AddWithValue("@SessionItems", sessItems);
                            command.Parameters.AddWithValue("@Flags", SessionStateActions.None);

                            command.ExecuteNonQuery();
                        }
                    }
                    scope.Complete();
                }
            }
            else
            {
                //トランザクション開始
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        //既存セッション更新
                        using (SqlCommand command = new SqlCommand())
                        {
                            conn.Open();

                            command.Connection = conn;
                            command.CommandText = @"
UPDATE Sessions
SET Expires = @Expires, SessionItems = @SessionItems, Locked = @Locked
WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND LockId = @LockId
";
                            command.Parameters.AddWithValue("@SessionId", id);
                            command.Parameters.AddWithValue("@ApplicationName", applicationName);

                            command.Parameters.AddWithValue("@LockId", lockId);
                            command.Parameters.AddWithValue("@SessionItems", sessItems);
                            command.Parameters.AddWithValue("@Expires", DateTime.Now.AddMinutes((Double)item.Timeout));

                            command.Parameters.AddWithValue("@Locked", false);

                            command.ExecuteNonQuery();
                        }
                    }
                    scope.Complete();
                }
            }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            throw new NotImplementedException();
        }

        // セッション情報をDB登録するため直列化(オブジェクトを文字列に変換)
        private string Serialize(SessionStateItemCollection items)
        {
            byte[] b;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    if (items != null)
                        items.Serialize(writer);
                }
                b = ms.ToArray();
            }

            return Convert.ToBase64String(b);
        }

        // セッション情報を復元(文字列をオブジェクトに戻す)
        private SessionStateStoreData Deserialize(HttpContext context,
          string serializedItems, int timeout)
        {
            MemoryStream ms =
              new MemoryStream(Convert.FromBase64String(serializedItems));

            SessionStateItemCollection sessionItems =
              new SessionStateItemCollection();

            if (ms.Length > 0)
            {
                BinaryReader reader = new BinaryReader(ms);
                sessionItems = SessionStateItemCollection.Deserialize(reader);
            }

            return new SessionStateStoreData(sessionItems,
              SessionStateUtility.GetSessionStaticObjects(context),
              timeout);
        }
    }
}