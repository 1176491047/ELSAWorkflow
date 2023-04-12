using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using Namotion.Reflection;
using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.Business
{
    [Activity(
       Category = "Message Push",
       Description = "Enterprise WeChat robot settings",
       Outcomes = new[] { OutcomeNames.Done })]
    public class WipAndOutputDataConvert : Activity
    {


        [ActivityInput(Hint = "Source data type", UIHint = ActivityInputUIHints.Dropdown,
Options = new[] { "Wip", "OutPut"})]
        public string DataType
        {
            get; set;
        }
        protected override IActivityExecutionResult OnExecute(ActivityExecutionContext context)
        {
            var sourceActivity = context.WorkflowExecutionContext.WorkflowBlueprint.Connections.Where(x => x.Target.Activity.Id == context.ActivityId).FirstOrDefault().Source.Activity;

             if (sourceActivity.Type == "SendHttpRequest")
            {
                //获取当前节点的源节点
                var perActivityId = context.WorkflowExecutionContext.WorkflowBlueprint.Connections.Where(x => x.Target.Activity.Id == context.ActivityId).FirstOrDefault().Source.Activity.Id;

                var value = context.WorkflowInstance.ActivityData[perActivityId]["ResponseContent"].ToString();
                 if (DataType == "Wip")
                {
                    return Done();
                }
                else
                {
                    return Suspend();
                }
            }
            else
            {
                return Suspend();
            }
        }

        /// <summary>
        /// 恢复时
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override IActivityExecutionResult OnResume(ActivityExecutionContext context)
        {
            // Read received input.
            // Instruct workflow runner that we're done.
            return Done();
        }
    }
}
