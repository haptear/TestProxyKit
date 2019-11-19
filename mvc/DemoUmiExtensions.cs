using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ProxyKit;
using System;
using System.Threading.Tasks;

namespace TestProxyKit
{
    /// <summary>
    /// 
    /// </summary>
    public static class DemoUmiExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmiProxy(this IApplicationBuilder app)
        {
            string path = "/";
            string target = "http://localhost:8000";
            //string target = "http://127.0.0.1:8000"; //is work 

            Console.WriteLine($"start UMI proxy {path} {target}");


            app.Map($"/umi.js", builder =>
            {
                builder.Run(async c => await Task.Run(() => c.Response.Redirect($"{target}/umi.js")));
            });

            app.Map($"/umi.css", builder =>
            {
                builder.Run(async c => await Task.Run(() => c.Response.Redirect($"{target}/umi.css")));
            });

            //umi websocket hot proxy
            app.UseWebSockets();
            app.UseWebSocketProxy(
                context => new Uri(target.ToLower().Replace("http", "ws") + "/"),
                options => options.AddXForwardedHeaders()
            );

            //umi hot proxy
            app.UseWhen(
                content =>
                {
                    bool flag = HttpMethods.IsGet(content.Request.Method)
                    && (content.Request.Path.StartsWithSegments($"/sockjs-node", System.StringComparison.OrdinalIgnoreCase)
                    || content.Request.Path.StartsWithSegments($"/__webpack_dev_server__", System.StringComparison.OrdinalIgnoreCase)
                    || content.Request.Path.Value.EndsWith($"hot-update.json", System.StringComparison.OrdinalIgnoreCase)
                    || content.Request.Path.Value.EndsWith($"hot-update.js", System.StringComparison.OrdinalIgnoreCase)
                    );
                    return flag;
                },
                appInner => appInner.RunProxy(context => context.ForwardTo(target).Send())
            );

            app.Use(async (c, next) =>
            {
                await next();

                //process client route 
                if (c.Response.StatusCode == StatusCodes.Status404NotFound && HttpMethods.IsGet(c.Request.Method)
                && string.IsNullOrEmpty(c.Request.Headers["Referer"].ToString())
                && !string.IsNullOrEmpty(c.Request.Headers["User-Agent"].ToString())
                && c.Request.Path.StartsWithSegments($"{path}", System.StringComparison.OrdinalIgnoreCase))
                {
                    var response = await c.ForwardTo(target).AddXForwardedHeaders().Send();
                    var html = await response.Content.ReadAsStringAsync();
                    c.Response.StatusCode = StatusCodes.Status200OK;
                    await c.Response.WriteAsync(html);
                    await c.Response.Body.FlushAsync();
                    return;
                }
            });
            return app;
        }

    }
}
