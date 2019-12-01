using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using webapi_sample.models;

namespace webapi_sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // 特定のホストであることを指定する場合（合致しないとルーティングされない）
    // [Host("localhost", "*.hoge.com")]
    public class ApiSampleController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ApiSampleController> _logger;
        private readonly MyOptions myOptions;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly GitHubService gitHubService;

        public ApiSampleController(
            // ILoggerの型引数でログの「カテゴリ」を指定できる。これは下記の形式で出力される。
            // （コンソールの場合）
            // ログレベル: ログカテゴリ[ログイベントID]
            //           ログメッセージ
            // （例）
            // info: webapi_sample.Controllers.WeatherForecastController[0]
            //       これはログメッセージのサンプルです。
            ILogger<ApiSampleController> logger,
            IOptionsMonitor<MyOptions> optionsAccessor,
            IHttpClientFactory httpClientFactory,
            GitHubService gitHubService)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.gitHubService = gitHubService;

            // DIでMyOptionsオプション（Startup.ConfigureServices参照）の現在値を取得
            // IOptionsMonitorを取得すると、シングルトンサービスのため、常に最新値を取得できる
            // IOptionsSnapshotを取得すると、スコープサービスのため、当インスタンスが生成された時点の値を取得できる
            this.myOptions = optionsAccessor.CurrentValue;
            System.Console.WriteLine($"[WeatherForecastController][ctor] Option1: {myOptions.Option1}");
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // ログレベルに応じたメソッドを使用する。
            // Critical: 即時の対応が必要な、アプリ全体の停止につながるエラー（例：ディスク容量不足）
            // Error: 処理できないエラー・例外のうち、アプリ全体でなく、現在の処理におけるエラー（例：一意キー違反）
            // Warning: 異常・予期しないイベントが発生した場合。アプリ停止には至らないが調査が必要な可能性があるデータ（ハンドルされた例外など）を含める。
            // Information: アプリの一般的なフローを追跡する。通常、このログには生存期間が比較的長期のデータ（URLパスなど）を含める。
            // Debug: 開発とデバッグで役立つ可能性のある情報。ログサイズが大きくなるため、トラブルシューティングを除き運用環境では有効にしない。
            // Trace: デバッグでのみ役に立つ情報。運用環境で有効にすべきでない。（既定で無効）
            // ※運用環境では、Critical〜Warningを記録する。必要に応じ、Information〜Traceを低コストの大容量データストアに記録する。
            // ※開発環境では、Critical〜Warningをコンソール出力する。トラブルシューティング時のみ、Information〜Traceを追加出力する。
            // ※メソッドのオーバーロードで、イベントID（このアプリ用に検討する必要がある）の指定が可能
            _logger.LogInformation("[WeatherForecast]: Start");

            try
            {
                var rng = new Random();
                _logger.LogTrace("[WeatherForecast]: Random created.");
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WeatherForecast]: Exception thrown");
                return Enumerable.Empty<WeatherForecast>();
            }
            finally
            {
                _logger.LogInformation("[WeatherForecast]: End");
            }
        }

        // ルート名やパラメータ名として使用できないキーワード
        // action, area, controller, handler, page
        [HttpGet("random/{count:int?}")]
        // もしくは、下記もアリ。ただし、パターンに該当しないと400 BadRequestではなく404 NotFoundになるので、入力検証としては適さない。
        // https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/routing?view=aspnetcore-3.1#route-constraint-reference
        // [HttpGet("random/{count:int:range(1,10)})]    // 1〜10の整数値のみ(int制約は無くても同義)
        public string GetRandom(int count = 1)
        {
            if (count < 1) count = 1;
            if (count > 10) count = 10;
            var rng = new Random();
            return Enumerable.Range(1, count).Select(c => rng.Next().ToString()).Aggregate((a, b) => $"{a}, {b}");
        }

        // パラメータに正規表現を使用するパターン（量指定子の波括弧は二度続けてエスケープする必要あり）
        [HttpGet(@"tel/{telNo:regex(^\d{{2,5}}-\d{{1,4}}-\d{{4}}$)}")]
        public object GetTelNo(string telNo)
        {
            var ret = new { telNo };
            return ret;
        }

        // パラメータに独自の制約を指定するパターン
        // ここでは、"male"か"female"しかマッチしない制約 myGender を指定している（GenderConstraintクラス参照）
        [HttpGet("gender/{gender:myGender}")]
        public string GetJapaneseGenderName(string gender)
        {
            return gender == "male" ? "男性" : "女性";
        }

        [HttpGet("exception")]
        public int GetException()
        {
            // レスポンスは 500 Internal Server Error になる。
            // Startup.ConfigureでUseDeveloperExceptionPageの呼び出しがあれば
            // BODYにエラー情報（スタックトレースなど）を含めて返却される。
            // 上記メソッドの呼び出しが無い場合、BODYは空になる。
            throw new ApplicationException("アプリケーションでエラーが発生しました。");
        }

        [HttpGet("weather1")]
        public async Task<string> GetWeather1()
        {
            // アドホックにHTTPクライアントを生成してリクエストする例
            var osakaCityCode = "270000";
            var weatherUrl = $"http://weather.livedoor.com/forecast/webservice/json/v1?city={osakaCityCode}";
            using var client = this.httpClientFactory.CreateClient();
            System.Console.WriteLine($"[GetWeather]URL: {weatherUrl}");
            return await client.GetStringAsync(weatherUrl);
        }

        [HttpGet("weather2")]
        public async Task<string> GetWeather2()
        {
            // 名前付きHTTPクライアントを使用してリクエストする例
            using var client = this.httpClientFactory.CreateClient("weather");
            return await client.GetStringAsync(null as Uri);
        }

        [HttpGet("doc")]
        public async Task<IEnumerable<GitHubIssue>> GetDotnetDocFromGithub()
        {
            // 型指定されたHTTPクライアントを使用してリクエストする例
            return await this.gitHubService.GetAspNetDocsIssues();
        }
    }
}
