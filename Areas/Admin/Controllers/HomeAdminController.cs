using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HoangHaPCVN.Areas.Admin.Filters;
using HoangHaPCVN.Models;
using HoangHaPCVN.Models.ViewModel;

namespace HoangHaPCVN.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeAdminController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();

        // GET: Admin/HomeAdmin
        //Trang Home của Admin: Nơi chứa số liệu tổng quan và chart
        public ActionResult Index()
        {
            var model = new DashBoardVM
            {
                //Đếm số dòng có trong bảng
                TongSanPham = db.Products.Count(),
                TongDanhMuc = db.Categories.Count(),
                TongKhachHang = db.Customers.Count(),
                TongDonHang = db.Orders.Count(),

                //Doanh thu = tổng tiền của các đơn hàng có trạng thái "Đã giao"
                DoanhThu = db.Orders
                    .Where(o => o.OrderStatus == "Đã giao")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,
                
                //Mỗi danh mục: Tên và số SP, danh mục có nhiều SP nhất đưa lên đầu
                SanPhamTheoDanhMuc = db.Categories
                .Select(c => new ThongKeDanhMuc
                {
                    TenDanhMuc = c.CategoryName,
                    SoSanPham = c.Products.Count()
                })
                    .OrderByDescending(x => x.SoSanPham)
                    .ToList()
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }; 
 }