using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    public class ProductDetailVM
    {
        //SP đang xem hiện tại
        public Product Product { get; set; }

        //Số lượng đặt mua (mặc định là 1)
        public int Quantity { get; set; } = 1;

        //Tính giá trị tạm thời = sl x đơn giá
        public decimal EstimatedValue
        {
            get { return Quantity * Product.ProductPrice; }
        }

        //Tổng số lượng đã bán của SP
        public int SoLuongDaBan { get; set; }

        //List 8 SP cùng danh mục
        public List<Product> RelatedProducts { get; set; }

        //List 8 SP bán chạy nhất ở cùng danh mục + phân trang
        public IPagedList<Product> TopProducts { get; set; }
    }
}