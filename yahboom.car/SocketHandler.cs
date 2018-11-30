using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace yahboom.car
{
    public class SocketHandler
    {
        public const int BufferSize = 50;
        WebSocket socket;
        SmartCar smartCar;
        SocketHandler(WebSocket socket)
        {
            this.socket = socket;
            this.smartCar = new SmartCar();
        }
        async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            while (this.socket.State == WebSocketState.Open)
            {
                var incoming = await this.socket.ReceiveAsync(seg, CancellationToken.None);
                var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);
                try
                {
                    var cmd = Encoding.UTF8.GetString(buffer, 0, incoming.Count);
                    this.smartCar.Execute(cmd);
                    await this.socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch {

                }

                
            }
        }
        static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;
            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new SocketHandler(socket);
            await h.EchoLoop();
        }
        /// <summary>
        /// 路由绑定处理
        /// </summary>
        /// <param name="app"></param>
        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }
    }

}
