using System;
using System.Web;
using System.Web.SessionState;

namespace WebApp.Common
{
    public class SessionStateStore : SessionStateStoreProviderBase
    {
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            throw new NotImplementedException();
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            throw new NotImplementedException();
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

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            //TODO:正しい値で設定する。
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = true;
            actions = 0;

            int timeout = 1000;
            return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout );
        }

        public override void InitializeRequest(HttpContext context)
        {
            //実装せず
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            //実装するか迷い中。
            //throw new NotImplementedException();
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            throw new NotImplementedException();
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            throw new NotImplementedException();
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            System.Diagnostics.Debug.WriteLine(item.Items["UserId"]); //セッションに設定した内容が取れること確認。
            throw new NotImplementedException();
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            throw new NotImplementedException();
        }
    }
}