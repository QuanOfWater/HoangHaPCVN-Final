using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HoangHaPCVN.Models;
using HoangHaPCVN.Models.ViewModel;
using PagedList;
using System.Data.Entity;

namespace HoangHaPCVN.Controllers
{
    public class CartController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();

        //Hàm lấy dịch vụ giỏ hàng
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Cart
        //Hiển thị giỏ hàng đã gom nhóm theo danh mục, kèm sản phẩm tương tự
        public ActionResult Index(int? page)
        {
            var cart = GetCartService().GetCart();

            //Tên các danh mục và mã các sản phẩm đang có trong giỏ
            var categoryNames = cart.Items.Select(i => i.Category).Distinct().ToList();
            var productIds = cart.Items.Select(i => i.ProductID).ToList();

            //Sản phẩm tương tự: Cùng danh mục với sản phẩm trong giỏ, nhưng chưa có trong giỏ
            var similarProducts = db.Products
                .Include(p => p.Category)
                .Include(p => p.OrderDetails)
                .Where(p => categoryNames.Contains(p.Category.CategoryName)
                         && !productIds.Contains(p.ProductID))
                .OrderByDescending(p => p.ProductID);

            //Phân trang
            int pageNumber = page ?? 1;
            int pageSize = 6;
            cart.SimilarProducts = similarProducts.ToPagedList(pageNumber, pageSize);

            return View(cart);
        }

        // POST: Cart/AddToCart
        //Thêm sản phẩm vào giỏ (khi tương tác nút "+" ở ô sản phẩm và trang chi tiết)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = db.Products
                .Include(p => p.Category)
                .SingleOrDefault(p => p.ProductID == productId);

            if (product == null)
            {
                return HttpNotFound();
            }

            GetCartService().GetCart().AddItem(product.ProductID, product.ProductImage,
                product.ProductName, product.ProductPrice, quantity, product.Category.CategoryName);

            TempData["CartMessage"] = "Đã thêm \"" + product.ProductName + "\" vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        //Change = -1 (nút -), +1 (nút +), hoặc không có (gõ số trực tiếp)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int id, int quantity, int? change)
        {
            int soLuongMoi = quantity + (change ?? 0);
            GetCartService().GetCart().UpdateQuantity(id, soLuongMoi);
            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveFromCart
        //Xóa 1 sản phẩm khỏi giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int id)
        {
            GetCartService().GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        // POST: Cart/ClearCart
        //Làm trống giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
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