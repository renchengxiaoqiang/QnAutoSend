using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Host;
using SuperSocket.WebSocket.Server;
using Newtonsoft.Json;

namespace QnAutoSend.CDP
{
    public class MyWebSocketServer
    {
        public static MyWebSocketServer WSocketSvrInst = null;
        static MyWebSocketServer()
        {
            if (WSocketSvrInst == null) WSocketSvrInst = new MyWebSocketServer();
        }

        public EventHandler<WSocketNewMessageEventArgs> OnRecieveMessage;

        public async Task Start()
        {
            try
            {
                var host = WebSocketHostBuilder.Create()
                    .UseSessionHandler((session) =>
                    {
                        Task.Run(async () =>
                        {
                            var s = session as WebSocketSession;
                            var cdp = new CDPClient(s);
                            var ver = await cdp.GetVersion();
                            var user = await cdp.GetCurrentUser();
                            QN qn = QN.GetByNick(user.Result);
                            qn.QnVersion = ver.version;
                            qn.CDP = cdp;
                        });
                        return ValueTask.CompletedTask;
                        
                    },  (session, args) => {
                        var s = session as WebSocketSession;  
                        return ValueTask.CompletedTask;
                    })
                    .UseWebSocketMessageHandler(
                         (session, message) =>
                         {
                             Task.Run(() =>
                             {
                                 var wMsg = JsonConvert.DeserializeObject<WSocketMessage>(message.Message);
                                 if (wMsg?.Type == "hi") return;

                                 OnRecieveMessage?.Invoke(session,
                                     new WSocketNewMessageEventArgs(wMsg.Type, wMsg.Response));
                             });
                            return ValueTask.CompletedTask;
                        }
                    )
                    .ConfigureSuperSocket(options =>
                    {
                        options.DefaultTextEncoding = Encoding.UTF8;
                        options.MaxPackageLength = 10 * 1024 * 1024;
                        options.AddListener(new ListenOptions
                        {
                            Ip = "Any",
                            Port = 41010 // Specify your desired port here
                        });
                    }).ConfigureLogging((hostCtx, loggingBuilder) =>
                    {
                        loggingBuilder.AddConsole();
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
        }
    }

    public class WSocketMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }
    }

    public class WSocketNewMessageEventArgs : EventArgs
    {
        public string Type { get; private set; }
        public string Value { get; private set; }

        public WSocketNewMessageEventArgs(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }

}
