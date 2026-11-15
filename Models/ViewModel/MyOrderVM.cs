using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    //Dữ liệu trang "Đơn hàng của tôi"
    public class MyOrderVM
    {
        public string Status { get; set; } //Tab đang chọn hiện tại (Tất cả các đơn = null)
        public string Search {  get; set; } //Từ khóa tìm kiếm
        public Dictionary<string, int> StatusCounts { get; set; } //Số đơn của từng trạng thái (hiện ở trên tab)
        public int TotalCount { get; set; } //Tổng số đơn (tab tất cả)
        public IPagedList<Order> Orders { get; set; } //Phân trang
    }
}