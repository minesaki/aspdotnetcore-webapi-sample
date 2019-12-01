using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using webapi_sample.models;

namespace webapi_sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly MyOptions myOptions;

        public WeatherForecastController(
            // ILoggerの型引数でログの「カテゴリ」を指定できる。これは下記の形式で出力される。
            // （コンソールの場合）
            // ログレベル: ログカテゴリ[ログイベントID]
            //           ログメッセージ
            // （例）
            // info: webapi_sample.Controllers.WeatherForecastController[0]
            //       これはログメッセージのサンプルです。
            ILogger<WeatherForecastController> logger,
            IOptionsMonitor<MyOptions> optionsAccessor)
        {
            _logger = logger;

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
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "[WeatherForecast]: Exception thrown");
                return Enumerable.Empty<WeatherForecast>();
            }
            finally
            {
                _logger.LogInformation("[WeatherForecast]: End");
            }
        }
    }
}
