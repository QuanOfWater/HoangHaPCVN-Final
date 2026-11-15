using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    public class OrderSearchVM
    {
        // Lọc theo khách hàng (khi xem một khách hàng cụ thể)
        public int? CustomerID { get; set; }
        public string CustomerName { get; set; }

        //Lọc theo trạng thái đơn
        public string OrderStatus { get; set; }

        //Phân trang
        public IPagedList<Order> Orders { get; set; }
    }

    // Danh sách trạng thái đơn hàng dùng chung cho cả trang
    public static class OrderStatuses
    {
        public static readonly string[] All =
        {
            "Chờ thanh toán",
            "Đang xử lý",
            "Đang vận chuyển",
            "Đã giao",
            "Đã hủy"
        };
        //Khách chỉ được hủy khi cửa hàng chưa giao đơn cho bên vận chuyển
        public static bool CanCancel(string status)
        {
            return status == "Chờ thanh toán" || status == "Đang xử lý";
        }
    }
}