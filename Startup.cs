using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using webapi_sample.filters;
using webapi_sample.models;

namespace webapi_sample
{
    /// <summary>
    /// アプリケーション全体の初期化処理を記述します。
    /// 具体的には、要求処理パイプライン（ミドルウェア）とサービスを構成します。
    /// 
    /// ※Startup{環境名}というクラスを用意すると、実行時の環境に応じたStartupが呼ばれます。
    /// 　該当するクラスが見つからない場合は、既定のStartupクラスが使用されます。
    /// ※環境名は、環境変数「ASPNETCORE_ENVIRONMENT」で指定します。
    /// 　ただし、launchSettings.jsonで環境変数をオーバーライドできます。
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startupコンストラクタ
        /// 引数としてIConfigurationの他に、IHostEnvironment, IWebHostEnvironmentが取得可能
        /// 各オブジェクトから下記情報の取得が可能
        /// ・IConfiguration：アプリ設定(appsettings.json)
        /// ・IHostEnvironment：アプリ名、環境名、コンテントルートパス
        /// ・IWebHostEnvironment：上記＋Webルートパス
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // appsettings.jsonの情報を利用
            System.Console.WriteLine($"[Configuration] SampleSetting.key1: {configuration.GetValue<string>("SampleSetting:key1")}");
            System.Console.WriteLine($"[Configuration] SampleSetting.key2: {configuration.GetValue<string>("SampleSetting:key2")}");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// サービス（アプリ機能を提供する再利用可能なコンポーネント）を構成します。
        /// ここで登録されたサービスは、DIまたはIApplicationBuilder.ApplicationServicesより利用できます。
        /// 
        /// ※Configure{環境名}Serviceというメソッドを用意すると、実行時の環境に応じたConfigureServiceが呼ばれます。
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // 組み込みのサービス
            // https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1#built-in-middleware
            // Controllerサービス（MVC機能：ビューやページの処理を含まない）
            services.AddControllers();
            // 圧縮サービス（BrotliとGzipをサポート。Brotliが優先される）
            services.AddResponseCompression();
            // アプリのシャットダウンタイムアウトの設定例（既定は5秒）
            services.Configure<HostOptions>(option =>
            {
                option.ShutdownTimeout = System.TimeSpan.FromSeconds(20);
            });

            // 自作のサービス
            // MyStringUtilサービス（詳細はAddStringUtilメソッドを参照）
            services.AddMyStringUtil();
            // 自作のフィルタを登録（ヘルパーメソッドを作らず、ここで直接登録する場合）
            services.AddTransient<IStartupFilter, OutputRequestUrlFilter>();

            // オプション
            // 関連する設定値をグループ化する。DIで受け取って使用する。
            // https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1
            services.Configure<MyOptions>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// アプリの要求処理パイプラインを構成します。
        /// パイプラインは、リクエストからレスポンスまでの一連の処理を指します。
        /// ここで登録されたミドルウェア（User***メソッドで登録される）が
        /// リクエスト受信時の前処理、またはレスポンス送信時の後処理を行います。
        /// 記述順にパイプラインに登録され、この順に処理されることに注意します。
        /// 
        /// ※ユーザーコードは、パイプラインの終端（折り返し地点）で実行されるイメージ
        /// ※Configure{環境名}というメソッドを用意すると、実行時の環境に応じたConfigureが呼ばれます。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            // ルーティングに依存しない処理は、UseRoutingより前に書く

            // レスポンスの圧縮処理
            app.UseResponseCompression();

            // 開発時の場合
            if (env.IsDevelopment())
            {
                // Exceptionが発生したら詳細なエラーページを返す
                app.UseDeveloperExceptionPage();
            }
            // その他の環境の判定
            // 環境変数 ASPNETCORE_ENVIRONMENT に基づく判定ができる
            // デバッグ時の値は launchSettings.json で変更可能
            // （注）dotnet runでアプリを起動した場合、.vscode/launch.jsonは読み込まれない。
            string envMode =
              env.IsProduction() || env.IsStaging() || env.IsEnvironment("UAT") ? "Production"
              : env.IsDevelopment() ? "Development"
              : "Unknown";
            System.Console.WriteLine($"[Startup][Configure] Application is running in {envMode} Mode");

            // HTTPのアクセスをHTTPSにリダイレクトする場合
            // app.UseHttpsRedirection();

            // ルートのマッチング処理（ルーティング処理）
            app.UseRouting();

            // 認証・認可・CORSはルーティングの結果が必要なので、UseRoutingの後に書く
            // app.UseCors();
            // app.UseAuthentication();
            // app.UseAuthorization();

            // マッチング結果に従って各処理に振り分け
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // ルート先をここで指定する場合はここに書く
                // （ここで指定しない場合は、各Controllerの属性で指定する）
                // endpoints.MapControllerRoute(
                //     name: "default",
                //     pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // IHostApplicationLifetimeを使って、ライフタイムイベントをハンドルできる
            // ※似た名前のIHostLifetimeというものもあるが、こちらは何に使うのか不明・・・
            appLifetime.ApplicationStarted.Register(() => Console.WriteLine($"{DateTime.Now}: ApplicationStarted called"));
            appLifetime.ApplicationStopping.Register(() => Console.WriteLine($"{DateTime.Now}: ApplicationStopping called"));
            appLifetime.ApplicationStopped.Register(() => Console.WriteLine($"{DateTime.Now}: ApplicationStopped called"));
        }
    }
}
