using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elsa.Activities.Console;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using NodaTime;


namespace ElsaQuickstarts.Server.DashboardAndServer
{
    public class HeartbeatWorkflow : IWorkflow
    {
        private readonly IClock _clock;
        public HeartbeatWorkflow(IClock clock) => _clock = clock;
        public void Build(IWorkflowBuilder builder)
        {
            //builder
            //    //每十秒执行一次该工作流
            //    .Timer(Duration.FromSeconds(10))
            //    //当前时间写入标准输出  (它使用采用委托而不是字符串文字的重载。 这允许在运行时提供动态的属性值)
            //    .WriteLine(context => $"Heartbeat at {_clock.GetCurrentInstant()}");
            ////另请注意，HeartbeatWorkflow 类可以接受构造函数注入的服务，就像将向 DI 系统注册的任何其他类型一样。
        }
    }
}
