using System.Web;
using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // バンドルの詳細については、https://go.microsoft.com/fwlink/?LinkId=301862 を参照してください
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
                        "~/Scripts/js/jquery-{version}.min.js"));

            /*
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 開発と学習には、Modernizr の開発バージョンを使用します。次に、実稼働の準備が
            // 運用の準備が完了したら、https://modernizr.com のビルド ツールを使用し、必要なテストのみを選択します。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            */

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-bundle").Include(
                      "~/Scripts/js/bootstrap.bundle.min.js"));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap").Include(
                      "~/Content/css/bootstrap.css"));

            bundles.Add(new StyleBundle("~/bundles/css/custom-style").Include(
                      "~/Content/css/custom-style.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
