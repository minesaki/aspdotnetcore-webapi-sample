using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApiSample
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
                // ログプロバイダを構成する
                // 組み込みのログプロバイダ： https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#built-in-logging-providers
                // サードパーティのログプロバイダ： https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#third-party-logging-providers
                .ConfigureLogging(logging =>
                {
                    // CreateDefaultBuilderにより「コンソール」「デバッグ」「EventSource」「イベントログ（Windowsのみ）」が追加されている
                    logging.ClearProviders();
                    logging.AddConsole();
                    // ログのフィルタリングをカスタマイズ
                    // logging.AddFilter("System", LogLevel.Debug);
                    // logging.AddFilter((provider, category, logLevel) => category == "webapi_sample.Controllers.WeatherForecastController");
                })
                // ホストビルダーの構成を指定する場合は使用する
                // IHostEnvironment/IWebHostEnvironmentを初期化するために使用される
                .ConfigureHostConfiguration(config =>
                {
                    // ほとんどの設定はConfigureAppConfigurationで実施可能
                    // ここでしか設定できないものは、下記のようなものが考えられる（未確認）
                    // ・アプリ名
                    // ・環境名
                    // ・コンテントルート
                    // ・Kestrel
                })
                // CreateDefaultBuilderが既定で設定する以上のアプリ構成をここで設定できる
                // インメモリコレクション、ファイルの読み込み、DB(EntityFramework)からの設定値取得など
                // 読み込まれた設定は、IConfiguration.Get***で取得したり、IConfiguration.GetSection.Bindでオブジェクトとして取得可能
                // https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#getvalue
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // 例：指定されたプレフィックスを持つ環境変数を読み込む
                    // ※読み込まれた結果のキー名からはプレフィックスは削除される
                    config.AddEnvironmentVariables(prefix: "PREFIX_");

                    // 設定ファイルはJSON,INI,XMLの読み込みに対応
                    // config.AddJsonFile("config.json", optional: true, reloadOnChange: true);

                    // ディレクトリのファイル群からキー/値のペア群を生成
                    // config.AddKeyPerFile(directoryPath: path, optional: true);

                    // コマンドライン引数でアプリ構成をオーバーライドできるようにする
                    // 例： ※下記のスタイルを混在させないこと
                    //  dotnet run key1=value1
                    //  dotnet run --key1=value1    ※ -- の代わりに / でも可
                    //  dotnet run --key1 value1    ※ -- の代わりに / でも可
                    config.AddCommandLine(args);

                    // メモリ（オブジェクト）から構成を追加する
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"MemoryCollectionKey1", "value1"},
                        {"MemoryCollectionKey2", "value2"}
                    });
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
