using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using webapi_sample.services;

namespace webapi_sample.filters
{
    /*
     * 自作のミドルウェア・フィルタの例
     * ※クラスやソースファイル、配置するディレクトリの構成がこれで良いか不明・・・
     */

    /// <summary>
    /// 自作フィルタの例
    /// 自作ミドルウェア OutputRequestUrlMiddeleware を構成します。
    /// </summary>
    public class OutputRequestUrlFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<OutputRequestUrlMiddeleware>("before");
                next(builder);
                // ここに書いても機能しない
                // builder.UseMiddleware<OutputRequestUrlMiddeleware>("after");
            };
        }
    }

    public class OutputRequestUrlMiddeleware
    {
        private readonly RequestDelegate _next;
        private readonly string title;
        private readonly IMyStringUtil stringUtil;

        /// <summary>
        /// OutputRequestUrlMiddelewareコンストラクタ
        /// ※ミドルウェアは起動時に一度だけ生成されるため、Scopedなサービスをコンストラクタで取得しても
        /// 　シングルトンなサービスであるかのような挙動をしてしまう。このため、Scopedなサービスは
        /// 　コンストラクタではなくInvokeメソッドのパラメータとして受け取る必要がある。
        /// </summary>
        /// <param name="next"></param>
        /// <param name="stringUtil"></param>
        /// <param name="title"></param>
        public OutputRequestUrlMiddeleware(RequestDelegate next, IMyStringUtil stringUtil, string title)
        {
            _next = next;
            this.stringUtil = stringUtil;
            if (!string.IsNullOrWhiteSpace(title))
            {
                this.title = title;
            }
            else
            {
                this.title = "(no title)";
            }
        }

        // Test with https://localhost:5001/Privacy/?option=Hello
        public async Task Invoke(HttpContext httpContext)
        {
            // リクエスト時の処理（Responseの書き込み・変更は禁止）
            var path = httpContext.Request.Path.ToString();
            if (!stringUtil.IsEmpty(path))
            {
                System.Console.WriteLine($"[OutputRequestUrl][{title}][start]: {path}");
            }

            // 後続のパイプライン処理を実行
            // ※エラーチェックを満たさないなど、ここで処理を短絡（中断）したい場合は
            // 　この呼び出しを実施しないことで実現できる
            await _next(httpContext);

            // レスポンス時の処理（Responseの書き込み・変更は禁止）
            if (!stringUtil.IsEmpty(path))
            {
                System.Console.WriteLine($"[OutputRequestUrl][{title}][end]: {path}");
            }
        }
    }
}