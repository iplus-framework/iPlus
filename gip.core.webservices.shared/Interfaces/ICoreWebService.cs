// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using System;
#if NETFRAMEWORK
using CoreWCF;
using CoreWCF.Web;
#elif NETSTANDARD
using System.Threading.Tasks;
#endif

namespace gip.core.webservices
{
#if NETFRAMEWORK
    [ServiceContract]
#endif
    public partial interface ICoreWebService
    {
#if NETFRAMEWORK
        [OperationContract]
        [WebGet(UriTemplate = CoreWebServiceConst.UriLogin, ResponseFormat = WebMessageFormat.Json)]
        WSResponse<VBUserRights> Login(string userName);
#elif NETSTANDARD
        Task<WSResponse<VBUserRights>> Login(string userName);
#endif

#if NETFRAMEWORK
        [OperationContract]
        [WebGet(UriTemplate = CoreWebServiceConst.UriLogout, ResponseFormat = WebMessageFormat.Json)]
        WSResponse<bool> Logout(string sessionID);
#elif NETSTANDARD
        Task<WSResponse<bool>> Logout(string sessionID);
#endif

#if NETFRAMEWORK
        [OperationContract]
        [WebGet(UriTemplate = CoreWebServiceConst.UriACClass_BarcodeID, ResponseFormat = WebMessageFormat.Json)]
        WSResponse<ACClass> GetACClassByBarcode(string barcodeID);
#elif NETSTANDARD
        Task<WSResponse<ACClass>> GetACClassByBarcodeAsync(string barcodeID);
#endif

#if NETFRAMEWORK
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = CoreWebServiceConst.UriDumpPerfLog, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        WSResponse<bool> DumpPerfLog(string perfLog);
#elif NETSTANDARD
        Task<WSResponse<bool>> DumpPerfLog(string perfLog);
#endif

        // Examples: https://github.com/jaredfaris/WCF-REST-JSON-Examples/blob/master/WcfService/IExampleService.cs
        // https://forums.asp.net/t/2090145.aspx?Is+it+possible+to+use+ApiControllers+from+a+separate+project
        // https://dzone.com/articles/loading-web-api-controllers
        // https://snede.net/re-use-controllers-views-and-tag-helpers-in-asp-net-core/
        // https://restfulapi.net/resource-naming/
        // https://www.vinaysahni.com/best-practices-for-a-pragmatic-restful-api
    }
}
