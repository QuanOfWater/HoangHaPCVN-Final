using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HoangHaPCVN.Models.ViewModel;
using System.Configuration;

namespace HoangHaPCVN.Models
{
    public class BankInfo
    {
        public static string BankId => ConfigurationManager.AppSettings["BankId"];
        public static string BankName => ConfigurationManager.AppSettings["BankName"];
        public static string AccountNo => ConfigurationManager.AppSettings["AccountNo"];
        public static string AccountName => ConfigurationManager.AppSettings["AccountName"];

        //Đơn này có đang chờ khách chuyển khoản không (hiện QR khi đúng)
        public static bool ChoChuyenKhoan(Order order)
        {
            return order.PaymentMethod == CheckoutOptions.ChuyenKhoan
                && order.OrderStatus == OrderStatuses.ChoXuLy
                && order.PaymentStatus == PaymentStatuses.ChuaThanhToan;
        }

        //Nội dung chuyển khoản: HHPC + mã đơn, để cửa hàng biết tiền về là của đơn nào
        public static string NoiDungCK(int orderId)
        {
            return "HHPC" + orderId;
        }

        //Link ảnh QR: quét bằng app ngân hàng sẽ điền sẵn STK, số tiền, nội dung
        public static string QrUrl(Order order)
        {
            return "https://img.vietqr.io/image/" + BankId + "-" + AccountNo + "-compact2.png"
                + "?amount=" + (long)order.TotalAmount
                + "&addInfo=" + HttpUtility.UrlEncode(NoiDungCK(order.OrderID))
                + "&accountName=" + HttpUtility.UrlEncode(AccountName);
        }
    }
}