using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    public class OrderSearchVM
    {
        //Lọc theo khách hàng (khi xem một khách hàng cụ thể)
        public int? CustomerID { get; set; }
        public string CustomerName { get; set; }

        //Lọc theo trạng thái đơn
        public string OrderStatus { get; set; }

        //Phân trang
        public IPagedList<Order> Orders { get; set; }
    }

    //Danh sách trạng thái đơn hàng dùng chung cho cả trang
    public static class OrderStatuses
    {
        public const string ChoXuLy = "Chờ xử lý";
        public const string DaGiao = "Đã giao";
        public const string ThanhToanXong = "Thanh toán xong";
        public const string DaNhanHang = "Đã nhận hàng";
        public const string DaHuy = "Đã hủy";
        public const string HoanTien = "Hoàn tiền";

        //Thứ tự dùng cho tab Đơn hàng của tôi và ô lọc của Admin
        public static readonly string[] All = { ChoXuLy, DaGiao, ThanhToanXong, DaNhanHang, DaHuy, HoanTien };

        //Đơn ở các trạng thái này KHÔNG được tính vào "số lượng đã bán"
        public static readonly string[] Cancelled = { DaHuy, HoanTien };

        //Khách chỉ được hủy khi đơn còn chờ xử lý (chưa giao)
        public static bool CanCancel(string status)
        {
            return status == ChoXuLy;
        }

        //Khách bấm "Đã nhận được hàng" sau khi đã thanh toán xong
        public static bool CanConfirmReceived(string status)
        {
            return status == ThanhToanXong;
        }

        //Các trạng thái Admin được chuyển sang từ trạng thái hiện tại (rỗng = đơn đã kết thúc)
        public static string[] NextForAdmin(Order order)
        {
            switch (order.OrderStatus)
            {
                case ChoXuLy:
                    //Đơn chuyển khoản chưa xác nhận tiền thì chưa được giao, chỉ được hủy
                    bool choXacNhanTien = order.PaymentMethod == CheckoutOptions.ChuyenKhoan
                                       && order.PaymentStatus != PaymentStatuses.DaThanhToan;
                    return choXacNhanTien ? new[] { DaHuy } : new[] { DaGiao, DaHuy };
                case DaGiao:
                    return new[] { ThanhToanXong, DaHuy }; //Đã hủy: khách bom hàng
                case ThanhToanXong:
                    return new[] { DaNhanHang };
                case DaHuy:
                    //Chỉ hoàn tiền khi khách đã trả tiền
                    return order.PaymentStatus == PaymentStatuses.DaThanhToan
                        ? new[] { HoanTien }
                        : new string[0];
                default:
                    return new string[0]; //Đã nhận hàng, Hoàn tiền: kết thúc
            }
        }

        //MỌI lần đổi trạng thái đều gọi hàm này: đổi trạng thái + các việc đi kèm
        //Gọi TRƯỚC db.SaveChanges(). Khi hủy đơn, order phải được Include(o => o.OrderDetails.Select(d => d.Product))
        public static void ChuyenTrangThai(Order order, string trangThaiMoi)
        {
            if (trangThaiMoi == DaHuy)
            {
                //Trả hàng về kho
                foreach (var d in order.OrderDetails)
                {
                    d.Product.Quantity += d.Quantity;
                }
            }
            else if (trangThaiMoi == ThanhToanXong)
            {
                order.PaymentStatus = PaymentStatuses.DaThanhToan; //COD: thu tiền lúc giao
            }
            else if (trangThaiMoi == HoanTien)
            {
                order.PaymentStatus = PaymentStatuses.DaHoanTien;
            }
            order.OrderStatus = trangThaiMoi;
        }
    }

    public static class PaymentStatuses
    {
        public const string ChuaThanhToan = "Chưa thanh toán";
        public const string DaThanhToan = "Đã thanh toán";
        public const string DaHoanTien = "Đã hoàn tiền";
    }
}