using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace webapi_sample
{
    public class Program
    {
        // public static void Main(string[] args)
        public static async Task Main(string[] args)
        {
            // 同期的に実行する
            // CreateHostBuilder(args).Build().Run();

            // 非同期に実行し、Ctrl+C/SIGINTまたはSIGTERMを待機する
            await CreateHostBuilder(args).RunConsoleAsync();
        }

        /// <summary>
        /// ホストを構成します。
        /// 
        /// ホストは、アプリのリソース（DI、ログ、構成、IHostedService実装）をカプセル化し、
        /// アプリの起動と有効期間を管理するオブジェクト。
        /// Webアプリの場合、ConfigureWebHostDefaultsを呼ぶことで、HTTPサーバ（APサーバ）を起動する。
        /// .NET Core 3.0以降、（IWebHostBuilderではなく）IHostBuilderを常に使用する。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                // 汎用ホスト（IHostBuilder）の既定の構成を適用する
                // 具体的には以下のような設定が行われる
                // ・DOTNET_ で始まる環境変数、コマンドラインから渡されたキー/値ペアの読み込み
                // ・Kestrelサーバの使用、および既定の構成
                // ・アプリ設定の読み込み（appsettings.json, appsettings.{Environment}.json）
                // https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#default-configuration
                // ※機密性の高い設定の取り扱いについても、上記サイトの [セキュリティ] の記事を参照
                .CreateDefaultBuilder(args)
                // ホストビルダーの構成を指定する場合は使用する
                // IHostEnvironment/IWebHostEnvironmentを初期化するために使用される
                .ConfigureHostConfiguration(config => {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"MemoryCollectionKey1", "value1"},
                        {"MemoryCollectionKey2", "value2"}
                    });
                })
                // CreateDefaultBuilderが既定で設定する以上のアプリ構成をここで設定できる
                // インメモリコレクション、ファイルの読み込み、EntityFrameworkの設定など
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // 例：指定されたプレフィックスを持つ環境変数を読み込む
                    // ※読み込まれた結果のキー名からはプレフィックスは削除される
                    config.AddEnvironmentVariables(prefix: "PREFIX_");
                    
                    // コマンドライン引数でアプリ構成をオーバーライドできるようにする
                    // 例： ※下記のスタイルを混在させないこと
                    //  dotnet run key1=value1
                    //  dotnet run --key1=value1    ※ -- の代わりに / でも可
                    //  dotnet run --key1 value1    ※ -- の代わりに / でも可
                    // config.AddCommandLine(args);
                })
                // ConfigureWebHostDefaultsメソッドによって下記が実施される
                // ・"ASPNETCORE_"で始まる環境変数からホスト構成を読み取る
                // ・KestrelサーバをWebサーバとして構成する
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // ここでWebアプリの設定を行う
                    // （例）詳細なエラーをキャプチャする（この値がtrueであるか、環境がDevelopmentの場合）
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

                    // Startupクラスを使わず、ここでConfigure/ConfigureServiceメソッドを呼ぶこともできる
                    // ConfigureServiceを複数回読んだ場合、全て有効になる
                    // Configureを複数回読んだ場合、最後の１回分のみ有効になる
                    webBuilder.UseStartup<Startup>();
                });
    }
}
