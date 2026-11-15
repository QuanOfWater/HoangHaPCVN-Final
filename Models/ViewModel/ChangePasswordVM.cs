using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HoangHaPCVN.Models.ViewModel
{
    //Form để đổi mật khẩu
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Mật khẩu không được chứa khoảng trắng")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string ConfirmNewPassword { get; set; }
    }
}