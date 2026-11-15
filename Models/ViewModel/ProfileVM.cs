using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HoangHaPCVN.Models.ViewModel
{
    //Hiển thị thông tin cá nhân và cập nhập ở trang "Thông tin tài khoản"
    public class ProfileVM
    {
        //Chỉ hiển thị, controller không đọc giá trị này từ form
        public string Username { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }
    }
}