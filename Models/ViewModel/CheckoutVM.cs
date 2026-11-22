using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HoangHaPCVN.Models.ViewModel
{
    //Lưu thông tin form thanh toán
    public class CheckoutVM
    {
        //Phần hiển thị (controller tự điền, không đọc từ form)
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        //Phần customer nhập/chọn trên form
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        [Display(Name = "Hình thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        [Display(Name = "Hình thức thanh toán")]
        public string PaymentMethod { get; set; }
    }

    public static class CheckoutOptions
    {
        public const string COD = "Thanh toán khi nhận hàng (COD)";
        public const string ChuyenKhoan = "Chuyển khoản ngân hàng (Internet Banking)";
        public static readonly string[] PaymentMethods = { COD, ChuyenKhoan };
        public static readonly string[] DeliveryMethods = { "Giao hàng tiêu chuẩn (3 - 5 ngày)", "Giao hàng hỏa tốc (Lấy trong ngày)" };
    }
}