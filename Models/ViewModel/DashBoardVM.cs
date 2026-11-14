using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HoangHaPCVN.Models.ViewModel
{
    //Dữ liệu cho trang Home của Admin
    public class DashBoardVM
    {
        public int TongSanPham {  get; set; }
        public int TongDanhMuc { get; set; }
        public int TongKhachHang { get; set; }
        public int TongDonHang { get; set; }

        //Doanh thu
        public decimal DoanhThu { get; set; }

        //Dữ liệu cho chart, mỗi danh mục có bao nhiêu sản phẩm
        public List<ThongKeDanhMuc> SanPhamTheoDanhMuc { get; set; }
    }

    //Dòng thống kê: Tên danh mục và số sản phẩm của danh mục đó
    public class ThongKeDanhMuc //Mỗi ThongKeDanhMuc ứng với một miếng của chart
    {
        public string TenDanhMuc {  get; set; }
        public int SoSanPham { get; set; }
    }
}