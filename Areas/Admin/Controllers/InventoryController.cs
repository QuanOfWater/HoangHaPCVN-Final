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
    public class InventoryController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();

        // GET: Admin/Inventory?searchTerm=chuột
        public ActionResult Index(string searchTerm)
        {
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                                            || p.Category.CategoryName.Contains(searchTerm));
            }

            //Chép ra biến cục bộ
            var daHuy = OrderStatuses.Cancelled;

            //Mỗi sản phẩm 1 dòng: tồn kho + đã bán (tổng số lượng trong các đơn không bị hủy)
            var rows = products
                .Select(p => new StockRowVM
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    ProductImage = p.ProductImage,
                    CategoryName = p.Category.CategoryName,
                    Quantity = p.Quantity,
                    Sold = p.OrderDetails
                        .Where(d => !daHuy.Contains(d.Order.OrderStatus))
                        .Sum(d => (int?)d.Quantity) ?? 0
                })
                .OrderBy(r => r.ProductName)
                .ToList();

            var model = new InventoryVM
            {
                SearchTerm = searchTerm,
                InStock = rows.Where(r => r.Quantity > 0).ToList(),
                OutOfStock = rows.Where(r => r.Quantity == 0).ToList()
            };
            ViewBag.SearchTerm = searchTerm; //Để form nhập hàng trong partial giữ lại từ khóa
            return View(model);
        }

        // POST: Admin/Inventory/Import/5
        //Nhập thêm hàng: Cộng thêm vào tồn kho hiện tại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(int id, int? amount, string searchTerm)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            if (amount == null || amount < 1 || amount > 10000)
            {
                TempData["Error"] = "Số lượng nhập phải từ 1 đến 10.000.";
            }
            else if (product.Quantity + amount.Value > 100000)
            {
                TempData["Error"] = "Tồn kho sau khi nhập không được vượt quá 100.000.";
            }
            else
            {
                product.Quantity += amount.Value;
                db.SaveChanges();
                TempData["Message"] = "Đã nhập thêm " + amount.Value + " \"" + product.ProductName
                                    + "\". Tồn kho hiện tại: " + product.Quantity + ".";
            }

            return RedirectToAction("Index", new { searchTerm = searchTerm }); //Giữ lại từ khóa đang tìm
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}