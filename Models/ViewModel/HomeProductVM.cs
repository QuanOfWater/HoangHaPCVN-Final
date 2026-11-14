using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    public class HomeProductVM
    {
        //Từ khóa tìm kiếm
        public string SearchTerm { get; set; }

        //Tổng số sản phẩm tìm được (khi đang tìm kiếm)
        public int SoKetQua { get; set; }

        //Danh sách 10 SP bán chạy nhất
        public List<Product> FeaturedProducts { get; set; }

        //Danh sách 20 SP ít người mua nhất + phân trang
        public IPagedList<Product> NewProducts { get; set; }

        //Giao diện HoangHaPC, mỗi danh mục một khối sản phẩm
        public List<CategoryProductsVM> CategoriesSections { get; set; } = new List<CategoryProductsVM>();

        //Một khối SP theo danh mục trên trang chủ
        public class CategoryProductsVM
        {
            public int CategoryID { get; set; }
            public string CategoryName { get; set; }
            public List<Product> Products { get; set; }
        }
    }
}