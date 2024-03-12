using Elsa.Server.Api.Endpoints.Activities;
using System;
using System.Collections.Generic;

namespace ElsaQuickstarts.Server.DashboardAndServer.Model.Bussness
{
    public class VskyWorkflowInstanseResultDto
    {
        public List<VskyWorkflowInstanseInfo> items { get; set; }

        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
    }

    public class VskyWorkflowInstanseInfo
    {
        public string id { get; set; }

        public DateTime createdAt { get; set; }

        public string workflowStatus { get; set; }
    }

    public class BactchDeleteRequest {
        public List<string> workflowInstanceIds { get; set; }
    }

    public class BactchDeleteResponse {
        public int deletedWorkflowCount { get; set; }
    }
}
