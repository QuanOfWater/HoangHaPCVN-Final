using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    //Dữ liệu cho trang danh sách SP theo danh mục
    public class ProductListVM
    {
        //Danh mục đang xem hiện tại (null là hiện tất cả SP)
        public int? CategoryID { get; set; }
        public string CategoryName { get; set; }

        //Cách sắp xếp đang chọn
        public string SortOrder { get; set; }

        //Phân trang
        public IPagedList<Product> Products { get; set;}
    }
}