using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HoangHaPCVN.Models.ViewModel;

namespace HoangHaPCVN.Models.ViewModel
{
    //Thông tin sau khi đăng nhập (trạng thái đã đăng nhập chưa, tên user, có mấy SP trong giỏ)
    public class PersonalMenuVM
    {
        public bool IsLoggedIn { get; set; }
        public string Username {  get; set; }
        public string CustomerName { get; set; } //Thêm CustomerName để header chào bằng họ tên thay vì tên đăng nhập
        public int CartCount { get; set; }
    }
}