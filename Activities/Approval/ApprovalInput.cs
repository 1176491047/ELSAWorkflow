using Elsa.Server.Api.Endpoints.Activities;
using System;
using System.Collections.Generic;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.Approval
{
    public class ApprovalInput
    {
        /// <summary>
        /// 单据条码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// json数据
        /// </summary>
        public string Data { get; set; }
        
        /// <summary>
        /// 收件人
        /// </summary>
        public List<string> Receiver { get; set; }


        /// <summary>
        /// 审核结果
        /// </summary>
        public string Result { get; set; }
    }


    /// <summary>
    /// 输出对象
    /// </summary>
    public class ApprovalOutPut
    {
        /// <summary>
       /// 流程实例id
       /// </summary>
        public string WorkflowInstanseId { get; set; }
        /// <summary>
        /// 流程节点Id
        /// </summary>
        public string CurrentNodeId { get; set; }

        /// <summary>
        /// 流程实例名称
        /// </summary>
        public string? CurrentNodeName { get; set; }

        /// <summary>
        /// 当前节点编码
        /// </summary>
        public string CurrentNodeKey { get; set; }

        /// <summary>
        /// 前节点编码
        /// </summary>
        public string PreviousNodeKey { get; set; }


        /// <summary>
        /// 单据条码
        /// </summary>
        public string Code { get; set; }



        /// <summary>
        /// 审核结果
        /// </summary>
        public string PreResult { get; set; }

    }
}
