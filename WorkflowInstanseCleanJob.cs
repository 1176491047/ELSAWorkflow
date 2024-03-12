using Elsa.Builders;
using NodaTime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using ElsaQuickstarts.Server.DashboardAndServer.Common;
using Microsoft.Extensions.Configuration;
using Elsa.Activities.Temporal;
using Elsa.Activities.Console;
using System.Net.Http;
using Storage.Net;
using ElsaQuickstarts.Server.DashboardAndServer.Model.Bussness;
using Newtonsoft.Json;
using System.Drawing.Printing;
using System.Threading.Tasks;
using Nancy.Json;
using System.Text;
using System.Net.Http.Headers;

namespace ElsaQuickstarts.Server.DashboardAndServer
{
    public class WorkflowInstanseCleanJob : IWorkflow
    {
        private Logger aLogger;
        private IConfiguration _configuration;
        private IHttpClientFactory _httpClientFactory;
        string getHistoryURL = "";
        string batchDeleteURL = "";
        string TargetDeleteCount = "";
        string DeleteDuration = "";

        public WorkflowInstanseCleanJob(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            aLogger = Logger.Default;
            _httpClientFactory=httpClientFactory;

            getHistoryURL = _configuration["GetHistoryURL"];
            batchDeleteURL = _configuration["BatchDeleteURL"];
            TargetDeleteCount = _configuration["DeleteCount"];
            DeleteDuration = _configuration["DeleteDuration"];
        }

        //每23小时执行一次图片删除任务
        public void Build(IWorkflowBuilder builder) =>
            builder
                .Timer(Duration.FromMinutes(string.IsNullOrEmpty(DeleteDuration) ? 5 : Convert.ToInt32(DeleteDuration)))//默认五分钟
                .WriteLine(() => $"{DateTime.Now} ：开始执行删除 结果：{CelanHistory().Result}");



        private async Task<string> CelanHistory()
        {
            aLogger.Info($"执行删除 \r\n 查询地址：{getHistoryURL}\r\n 删除地址：{batchDeleteURL}\r\n 目标数量：{TargetDeleteCount}", "历史任务删除");
            try
            {
                //删除基数
                int baseCount =string.IsNullOrEmpty(TargetDeleteCount) ?  500:Convert.ToInt32(TargetDeleteCount);
                int deleteCount = 0;
                //查询总数
                int totalCount =await GetTotalCount();

                //查询页数 向上取整
                decimal pageSize = Math.Ceiling(Convert.ToDecimal(Convert.ToDecimal(totalCount) / Convert.ToDecimal(baseCount)));

                aLogger.Info($"当前总数：{totalCount} \r\n 页数：{pageSize}", "历史任务删除");
                List<string> idList = new List<string>();
                 //索引-1 查询最后一页
                 int deleteIndex = Convert.ToInt32(pageSize)-1;
                //再次查询 查询第deleteIndex页的baseCount行数据
                VskyWorkflowInstanseResultDto targetDeleteInfos =await GetTargetDeleteInfos(deleteIndex, baseCount);
                if (targetDeleteInfos != null && targetDeleteInfos.items != null && targetDeleteInfos.items.Count > 0)
                {
                    //根据时间过滤 保留24小时的历史数据
                    List<VskyWorkflowInstanseInfo> items = targetDeleteInfos.items.Where(x=>x.createdAt<=DateTime.Now.AddHours(-24)).ToList();
                    idList.AddRange(items.Select(x=>x.id).ToList());
                }

                //页数大于两页 （前边已经-1）
                if (deleteIndex>=1)
                {
                    //查询倒数第二页
                   int secondindex = deleteIndex - 1;

                    VskyWorkflowInstanseResultDto targetDeleteInfos_second = await GetTargetDeleteInfos(secondindex, baseCount);
                    if (targetDeleteInfos_second != null && targetDeleteInfos_second.items != null && targetDeleteInfos_second.items.Count > 0)
                    {
                        //根据时间过滤 保留24小时的历史数据
                        List<VskyWorkflowInstanseInfo> items = targetDeleteInfos_second.items.Where(x => x.createdAt <= DateTime.Now.AddHours(-24)).ToList();
                        idList.AddRange(items.Select(x => x.id).ToList());
                    }
                }

                if (idList.Count>0)
                {    //调用接口批量删除
                    BactchDeleteRequest bactchDeleteRequest = new BactchDeleteRequest()
                    {
                        workflowInstanceIds = idList
                    };
                    aLogger.Info($"发起删除 行数：{idList.Count}", "历史任务删除");
                    BactchDeleteResponse bactchDeleteResponse = await SendBactchDeleteRequest(bactchDeleteRequest);

                    if (bactchDeleteResponse != null)
                    {
                        aLogger.Info($"成功删除{bactchDeleteResponse.deletedWorkflowCount}行数据", "历史任务删除");
                        return $"删除{bactchDeleteResponse.deletedWorkflowCount}行数据";
                    }
                }
                else
                {
                    return "无数据";
                }


                aLogger.Info($"成功删除{deleteCount}行", "历史任务删除");
                return $"成功删除{deleteCount}行";
            }
            catch (Exception ex)
            {
                aLogger.Info($"历史任务删除发生异常{ex}", "历史任务删除");
                Console.WriteLine("历史任务删除发生错误: " + ex.Message);
                return $"发生异常";
            }
        }


        private async Task<int> GetTotalCount() {
            try
            {
                //调用接口获取总数
                var client = _httpClientFactory.CreateClient();
                var getresult =await client.GetAsync(getHistoryURL);
                VskyWorkflowInstanseResultDto workflowinstanseResult = JsonConvert.DeserializeObject<VskyWorkflowInstanseResultDto>(getresult.Content.ReadAsStringAsync().Result);
                if (workflowinstanseResult.totalCount == 0)
                    return 0;
                else
                    return workflowinstanseResult.totalCount;
            }
            catch (Exception ex)
            {

                aLogger.Info($"查询总数发生异常{ex}", "历史任务删除");
                Console.WriteLine("查询总数发生异常: " + ex.Message);
                return 0;
            }

        }

        //http://10.21.18.191:5098/v1/workflow-instances?page=1&pageSize=500

        private async Task<VskyWorkflowInstanseResultDto> GetTargetDeleteInfos(int pageIndex,int pageSize) {
            try
            {  //调用接口获取需要删除的目标集合
                var client = _httpClientFactory.CreateClient();
                var getresult =await client.GetAsync($"{getHistoryURL}?orderBy=Started&page={pageIndex}&pageSize={pageSize}");
                VskyWorkflowInstanseResultDto workflowinstanseResult = JsonConvert.DeserializeObject<VskyWorkflowInstanseResultDto>(getresult.Content.ReadAsStringAsync().Result);
                return workflowinstanseResult;

            }
            catch (Exception ex)
            {
                aLogger.Info($"查询第{pageIndex}页的{pageSize}行数据发生异常{ex}", "历史任务删除");
                Console.WriteLine($"查询第{pageIndex}页的{pageSize}行数据发生异常: " + ex.Message);
                return new VskyWorkflowInstanseResultDto();
            }
        }

        private async Task<BactchDeleteResponse> SendBactchDeleteRequest(BactchDeleteRequest bactchDeleteRequest) {

            try
            {
                //调用接口执行批量删除
                var client = _httpClientFactory.CreateClient();
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, batchDeleteURL);
                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(bactchDeleteRequest),encoding:null, mediaType: "application/json-patch+json");
                var deleteResult =await client.SendAsync(httpRequestMessage);
                BactchDeleteResponse workflowinstanseResult = JsonConvert.DeserializeObject<BactchDeleteResponse>(deleteResult.Content.ReadAsStringAsync().Result);
                return workflowinstanseResult;
            }
            catch (Exception ex)
            {
                aLogger.Info($"查批量删除发生异常{ex}", "历史任务删除");
                Console.WriteLine($"查批量删除发生异常: " + ex.Message);
                return new BactchDeleteResponse();
            }


        }
    }
}
