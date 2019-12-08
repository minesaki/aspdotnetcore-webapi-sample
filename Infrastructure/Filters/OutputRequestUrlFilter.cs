
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using WebApiSample.Infrastructure.Middleware;

namespace WebApiSample.Infrastructure.Filters
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
}
