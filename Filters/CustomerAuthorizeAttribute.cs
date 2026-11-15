using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HoangHaPCVN.Filters
{
    //Chỉ customer mới đăng nhập vào được action được gắn [CustomerAuthorize]
    public class CustomerAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session == null)
            {
                return false;
            }
            return httpContext.Session["UserRole"] as string == "Customer" && httpContext.Session["CustomerID"] != null;
        }

        //Chưa đăng nhập thì chuyển về trang đăng nhập của customer, kèm trang muốn vào
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                area = "",
                controller = "Account",
                action = "Login",

                //Chỉ nhớ địa chỉ của trang GET; action POST không mở lại bằng đường link được
                returnUrl = request.HttpMethod == "GET" ? request.RawUrl : null
            }));
        }
    }
}