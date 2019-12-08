using WebApiSample.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StringUtilExtension
    {
        /// <summary>
        /// MyStringUtilをサービスに登録する処理を提供します。
        /// ※Startup.ConfigureServicesで、組み込みのサービスと同様に「services.AddMyStringUtil()」の書式で
        /// 　登録できるようにするためのヘルパーメソッドです。
        /// </summary>
        /// <param name="services"></param>
        public static void AddMyStringUtil(this IServiceCollection services)
        {
            // インスタンスの生成方法（生存期間）に応じたAdd***メソッドを選択する
            // 各Add***メソッドは引数で、DIコンテナから渡される「オブジェクト」または「オブジェクトの生成処理」を渡すこともできる
            // ただし、DIコンテナはIDisposableなインスタンスを自動破棄するが、「オブジェクト」を渡した場合は自動破棄されない
            // （下記の例では何も渡さず、DIコンテナに自動生成させている）

            // シングルトンとして生成する場合
            // ※シングルトンからScopedなサービスを参照するのはNG（後続のサービスが不正な状態になる可能性があるため）
            // ※ファクトリメソッド（実装クラスのコンストラクタ）をスレッドセーフにする必要はない
            services.AddSingleton<IMyStringService, MyStringService>();

            // 接続ごとに生成する場合
            // services.AddScoped<IMyStringUtil,MyStringUtil>();

            // DIコンテナから要求される度に生成する場合
            // services.AddTransient<IMyStringUtil,MyStringUtil>();

            // 複数のインターフェースから単一のインスタンスを解決したい場合は
            // TryAddEnumerableメソッドを使用する
            // services.TryAddEnumerable(ServiceDescriptor.Singleton<IMyStringUtil, MyStringUtil>());
            // services.TryAddEnumerable(ServiceDescriptor.Singleton<IMyStringUtil2, MyStringUtil>());

            // 組み込みのDI機能で不足な場合は、3rdPartyのDIライブラリを使用してもよい
            // https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#default-service-container-replacement
        }
    }
}
