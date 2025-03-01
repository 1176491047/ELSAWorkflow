﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elsa;
using Elsa.Activities.Email;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Providers.WorkflowStorage;
using Elsa.Serialization;
using Elsa.Services;
using Elsa.Services.Models;
using ElsaQuickstarts.Server.DashboardAndServer.Common;
using ElsaQuickstarts.Server.DashboardAndServer.Common.MailKit;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace ElsaQuickstarts.Server.DashboardAndServer.Activities.SendMail
{
    [Action(
        Category = "消息推送",
        DisplayName = "发送邮件(支持单独配置)",
        Description = "可在流程节点内部配置SMTP信息",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class SendMailWithSMTPConfig : Activity
    {
        private Logger aLogger;
        private readonly ISmtpService _smtpService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IContentSerializer _contentSerializer;
        private readonly SmtpOptions _options;

        public SendMailWithSMTPConfig(ISmtpService smtpService, IOptions<SmtpOptions> options, IHttpClientFactory httpClientFactory, IContentSerializer contentSerializer)
        {
            _smtpService = smtpService;
            aLogger = Logger.Default;
            _httpClientFactory = httpClientFactory;
            _contentSerializer = contentSerializer;
            _options = options.Value;
        }

        #region 入参


        #region SMTP配置信息


        /// <summary>
        /// SmtpPort
        /// </summary>
        [ActivityInput(Hint = "", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public int SmtpPort { get; set; }

        ///// <summary>
        ///// IsSsl
        ///// </summary>
        //[ActivityInput(Hint = "IsSsl", UIHint = ActivityInputUIHints.Dropdown,
        //    Options = new[] { "True", "Flase" },
        //    DefaultValue = "True")]
        //public bool IsSsl { get; set; }

        /// <summary>
        /// SenderAccount
        /// </summary>
        [ActivityInput(Hint = "", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string SenderAccount { get; set; }

        /// <summary>
        /// SmtpHost
        /// </summary>
        [ActivityInput(Hint = "", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string SmtpHost { get; set; }


        /// <summary>
        /// SenderPassword
        /// </summary>
        [ActivityInput(Hint = "", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string SenderPassword { get; set; }


        #endregion


        /// <summary>
        /// 发件人
        /// </summary>
        [ActivityInput(Hint = "The sender's email address.", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string? From { get; set; }



        /// <summary>
        /// 收件人
        /// </summary>
        [ActivityInput(Hint = "The recipients email addresses.", UIHint = ActivityInputUIHints.MultiText, DefaultSyntax = SyntaxNames.Json, SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript })]
        public ICollection<string> To { get; set; } = new List<string>();

        /// <summary>
        /// 抄送
        /// </summary>
        [ActivityInput(
            Hint = "The cc recipients email addresses.",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript },
            Category = "More")]
        public ICollection<string> Cc { get; set; } = new List<string>();


        /// <summary>
        /// 秘抄
        /// </summary>
        [ActivityInput(
            Hint = "The Bcc recipients email addresses.",
            UIHint = ActivityInputUIHints.MultiText,
            DefaultSyntax = SyntaxNames.Json,
            SupportedSyntaxes = new[] { SyntaxNames.Json, SyntaxNames.JavaScript },
            Category = "More")]
        public ICollection<string> Bcc { get; set; } = new List<string>();


        /// <summary>
        /// 主题
        /// </summary>
        [ActivityInput(Hint = "The subject of the email message.", SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string? Subject { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        [ActivityInput(
            Hint = "The attachments to send with the email message. Can be (an array of) a fully-qualified file path, URL, stream, byte array or instances of EmailAttachment.",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName
        )]
        public object? Attachments { get; set; }


        /// <summary>
        /// 附件主体
        /// </summary>
        [ActivityInput(
            Hint = "when Base64String is not support and ”ImageIsAttachement“ Is ture",
            UIHint = ActivityInputUIHints.MultiLine,
            SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid },
            DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName
        )]
        //当ImageIsAttachment为true表示附件转换为图片放置body部分 那么该变量应该赋值为上个节点(FileHadnling)传入的imageBytes字段
        public object? AttachmentsBody { get; set; }


        /// <summary>
        /// 邮件主体
        /// </summary>
        [ActivityInput(Hint = "The body of the email message.", UIHint = ActivityInputUIHints.MultiLine, SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string? Body { get; set; }

        /// <summary>
        /// 后缀名
        /// </summary>
        [ActivityInput(Hint = "The lastname of the attachments file.", UIHint = ActivityInputUIHints.MultiLine, SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string? FileLastName { get; set; }

        /// <summary>
        /// 附件名称
        /// </summary>
        [ActivityInput(Hint = "The name of the attachments file.", UIHint = ActivityInputUIHints.MultiLine, SupportedSyntaxes = new[] { SyntaxNames.JavaScript, SyntaxNames.Liquid })]
        public string? FileName { get; set; }

        /// <summary>
        ///图片为附件
        /// </summary>
        [ActivityInput(Hint = "Image Is Attachment(when Base64String is not support)", UIHint = ActivityInputUIHints.Dropdown,
            Options = new[] { "True", "Flase" },
            DefaultValue = "True")]
        //表示图片内容为附件 注:通常邮件中的图片为base64字符串 由于某些邮箱不支持比如outlool2016 那么需要cid的方式进行图片赋值 即"ImageIsAttachment"
        public string ImageIsAttachment { get; set; }

        #endregion

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var cancellationToken = context.CancellationToken;

            var message = new MimeMessage();
            var from = string.IsNullOrWhiteSpace(From) ? _options.DefaultSender : From;

            message.Sender = MailboxAddress.Parse(from);
            message.From.Add(MailboxAddress.Parse(from));
            //message.Subject = Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = Body };

            //如果图片为附件 表示需要链接到附件 即cid
            if (ImageIsAttachment == "True")
            {
                if (AttachmentsBody != null && !string.IsNullOrEmpty(AttachmentsBody.ToString()))
                    await AddAttachmentsAsync_BodyIsAttachments(bodyBuilder, cancellationToken, "Png");
                string imgList = "";
                int i = 0;
                foreach (var item in bodyBuilder.Attachments)
                {
                    i++;
                    item.ContentId = $"att{i}";
                    imgList += $"<img src='cid:att{i}'/>";
                }

                string messageHtml = $"<html>{imgList}</html>";
                bodyBuilder.HtmlBody += messageHtml;

                if (Attachments != null && !string.IsNullOrEmpty(Attachments.ToString()))
                    await AddAttachmentsAsync(bodyBuilder, cancellationToken, FileLastName);
            }
            //如果附件不需要连接到body
            else
            {
                if (Attachments != null && !string.IsNullOrEmpty(Attachments.ToString()))
                    await AddAttachmentsAsync(bodyBuilder, cancellationToken, FileLastName);
            }

            message.Body = bodyBuilder.ToMessageBody();

            SetRecipientsEmailAddresses(message.To, To);
            SetRecipientsEmailAddresses(message.Cc, Cc);
            SetRecipientsEmailAddresses(message.Bcc, Bcc);


            var mailServerConfig = new SendServerConfigurationEntity()
            {
                SmtpPort = SmtpPort,                           //SMTP端口
                IsSsl = false,                           //启用Ssl
                SenderAccount = SenderAccount,     //发件人
                SmtpHost = SmtpHost,               //邮件Smtp服务，QQ：smtp.qq.com
                SenderPassword = SenderPassword,    //邮箱授权码
            };
            var mailBodyEntity = new MailBodyEntity()
            {
                Subject = Subject,
                Body = Body,
                Recipients = To.ToList(),      //收件人
                SenderAddress = from,     //发件人
            };

            aLogger.Info($"mailBodyEntity：{JsonConvert.SerializeObject(mailBodyEntity)}", "邮件发送日志");
            aLogger.Info($"mailServerConfig：{JsonConvert.SerializeObject(mailServerConfig)}", "邮件发送日志");
            var result = MailHelper.SendMail(mailBodyEntity, mailServerConfig);
            aLogger.Info($"发送结果：{JsonConvert.SerializeObject(result)}", "邮件发送日志");

            

            //await _smtpService.SendAsync(context, message, context.CancellationToken);
            if (result.ResultStatus)
            {
                return Done();
            }
            else
            {
                return Fault(new Exception(result.ResultInformation));
            }
        }

        private async Task AddAttachmentsAsync(BodyBuilder bodyBuilder, CancellationToken cancellationToken, string fileLastName)
        {
            var attachments = Attachments;

            if (attachments != null)
            {
                var index = 0;
                var attachmentObjects = InterpretAttachmentsModel(attachments);


                foreach (var attachmentObject in attachmentObjects)
                {
                    switch (attachmentObject)
                    {
                        case Uri url:
                            await AttachOnlineFileAsync(bodyBuilder, url, cancellationToken);
                            break;
                        case string path when path.Contains("://"):
                            await AttachOnlineFileAsync(bodyBuilder, new Uri(path), cancellationToken);
                            break;
                        case string path:
                            await AttachLocalFileAsync(bodyBuilder, path, cancellationToken);
                            break;
                        case byte[] bytes:
                            {
                                var fileName = $"{(string.IsNullOrEmpty(FileName) ? "Attach" : FileName)}.{fileLastName}";
                                var contentType = "application/binary";
                                bodyBuilder.Attachments.Add(fileName, bytes, ContentType.Parse(contentType));
                                break;
                            }
                        case Stream stream:
                            {
                                var fileName = $"Attachment-{++index}.{fileLastName}";
                                var contentType = "application/binary";
                                await bodyBuilder.Attachments.AddAsync(fileName, stream, ContentType.Parse(contentType), cancellationToken);
                                break;
                            }
                        case EmailAttachment emailAttachment:
                            {
                                var fileName = emailAttachment.FileName ?? $"Attachment-{++index}";
                                var contentType = emailAttachment.ContentType ?? "application/binary";
                                var parsedContentType = ContentType.Parse(contentType);

                                if (emailAttachment.Content is byte[] bytes)
                                    bodyBuilder.Attachments.Add(fileName, bytes, parsedContentType);

                                else if (emailAttachment.Content is Stream stream)
                                    await bodyBuilder.Attachments.AddAsync(fileName, stream, parsedContentType, cancellationToken);

                                break;
                            }
                        default:
                            {
                                var json = _contentSerializer.Serialize(attachmentObject);
                                var fileName = $"Attachment-{++index}";
                                var contentType = "application/json";
                                bodyBuilder.Attachments.Add(fileName, Encoding.UTF8.GetBytes(json), ContentType.Parse(contentType));
                                break;
                            }
                    }
                }
            }
        }


        private async Task AddAttachmentsAsync_BodyIsAttachments(BodyBuilder bodyBuilder, CancellationToken cancellationToken, string fileLastName)
        {
            var attachments = AttachmentsBody;

            if (attachments != null)
            {
                var index = 0;
                var attachmentObjects = InterpretAttachmentsModel(attachments);


                foreach (var attachmentObject in attachmentObjects)
                {
                    switch (attachmentObject)
                    {
                        case Uri url:
                            await AttachOnlineFileAsync(bodyBuilder, url, cancellationToken);
                            break;
                        case string path when path.Contains("://"):
                            await AttachOnlineFileAsync(bodyBuilder, new Uri(path), cancellationToken);
                            break;
                        case string path:
                            await AttachLocalFileAsync(bodyBuilder, path, cancellationToken);
                            break;
                        case byte[] bytes:
                            {
                                var fileName = $"Att{++index}.{fileLastName}";
                                var contentType = "application/binary";
                                bodyBuilder.Attachments.Add(fileName, bytes, ContentType.Parse(contentType));
                                break;
                            }
                        case Stream stream:
                            {
                                var fileName = $"Attachment-{++index}.{fileLastName}";
                                var contentType = "application/binary";
                                await bodyBuilder.Attachments.AddAsync(fileName, stream, ContentType.Parse(contentType), cancellationToken);
                                break;
                            }
                        case EmailAttachment emailAttachment:
                            {
                                var fileName = emailAttachment.FileName ?? $"Attachment-{++index}";
                                var contentType = emailAttachment.ContentType ?? "application/binary";
                                var parsedContentType = ContentType.Parse(contentType);

                                if (emailAttachment.Content is byte[] bytes)
                                    bodyBuilder.Attachments.Add(fileName, bytes, parsedContentType);

                                else if (emailAttachment.Content is Stream stream)
                                    await bodyBuilder.Attachments.AddAsync(fileName, stream, parsedContentType, cancellationToken);

                                break;
                            }
                        default:
                            {
                                var json = _contentSerializer.Serialize(attachmentObject);
                                var fileName = $"Attachment-{++index}";
                                var contentType = "application/json";
                                bodyBuilder.Attachments.Add(fileName, Encoding.UTF8.GetBytes(json), ContentType.Parse(contentType));
                                break;
                            }
                    }
                }
            }
        }

        private async Task AttachLocalFileAsync(BodyBuilder bodyBuilder, string path, CancellationToken cancellationToken) => await bodyBuilder.Attachments.AddAsync(path, cancellationToken);

        private async Task AttachOnlineFileAsync(BodyBuilder bodyBuilder, Uri url, CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileName(url.LocalPath);
            var response = await DownloadUrlAsync(url);
            var contentStream = await response.Content.ReadAsStreamAsync();
            var contentType = response.Content.Headers.ContentType.MediaType;
            await bodyBuilder.Attachments.AddAsync(fileName, contentStream, ContentType.Parse(contentType), cancellationToken);
        }

        //private async Task AttachOnlineFileAsync(BodyBuilder bodyBuilder, Stream stream,string fileName, CancellationToken cancellationToken)
        //{
        //    var response = await DownloadUrlAsync(url);
        //    var contentStream = stream;
        //    var contentType = response.Content.Headers.ContentType.MediaType;
        //    await bodyBuilder.Attachments.AddAsync(fileName, contentStream, ContentType.Parse(contentType), cancellationToken);
        //}

        private IEnumerable InterpretAttachmentsModel(object attachments) => attachments is string text ? new[] { text } : attachments is IEnumerable enumerable ? enumerable : new[] { attachments };

        private void SetRecipientsEmailAddresses(InternetAddressList list, IEnumerable<string>? addresses)
        {
            if (addresses == null)
                return;

            list.AddRange(addresses.Select(MailboxAddress.Parse));
        }

        private async Task<HttpResponseMessage> DownloadUrlAsync(Uri url)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            return response;
        }
    }
}
