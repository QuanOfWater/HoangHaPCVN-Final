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

            var tonKho = db.Products
                .Where(p => productIds.Contains(p.ProductID))
                .Select(p => new { p.ProductID, p.Quantity })
                .ToDictionary(x => x.ProductID, x => x.Quantity);

            bool daGiam = false;
            foreach (var item in cart.Items)
            {
                item.Stock = tonKho.ContainsKey(item.ProductID) ? tonKho[item.ProductID] : 0;
                if (item.Stock > 0 && item.Quantity > item.Stock)
                {
                    item.Quantity = item.Stock; //Giảm về đúng số còn trong kho
                    daGiam = true;
                }
            }
            if (daGiam)
            {
                ViewBag.StockMessage = "Một số sản phẩm trong giỏ đã được giảm số lượng cho khớp với tồn kho.";
            }

            //Sản phẩm tương tự: Cùng danh mục, chưa có trong giỏ, CÒN HÀNG
            var similarProducts = db.Products
                .Include(p => p.Category)
                .Include(p => p.OrderDetails.Select(d => d.Order))
                .Where(p => categoryNames.Contains(p.Category.CategoryName)
                         && !productIds.Contains(p.ProductID)
                         && p.Quantity > 0)
                .OrderByDescending(p => p.ProductID);

            //Phân trang
            int pageNumber = Math.Max(1, page ?? 1);
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

            if (product.Quantity < 1)
            {
                TempData["CartMessage"] = "\"" + product.ProductName + "\" đã hết hàng.";
                return RedirectToAction("Index");
            }

            var cart = GetCartService().GetCart();
            var daCo = cart.Items.FirstOrDefault(i => i.ProductID == productId);
            int truocKhiThem = daCo == null ? 0 : daCo.Quantity;

            cart.AddItem(product.ProductID, product.ProductImage, product.ProductName,
                product.ProductPrice, quantity, product.Category.CategoryName, product.Quantity);

            int sauKhiThem = cart.Items.First(i => i.ProductID == productId).Quantity;
            if (sauKhiThem < truocKhiThem + Math.Max(1, quantity))
            {
                //Không thêm đủ vì chạm giới hạn tồn kho (hoặc 99)
                TempData["CartMessage"] = "Số lượng \"" + product.ProductName + "\" trong giỏ đã đạt tối đa ("
                                        + sauKhiThem + "). Kho còn " + product.Quantity + " sản phẩm.";
            }
            else
            {
                TempData["CartMessage"] = "Đã thêm \"" + product.ProductName + "\" vào giỏ hàng.";
            }
            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        //Change = -1 (nút -), +1 (nút +), hoặc không có (gõ số trực tiếp)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int id, int quantity, int? change)
        {
            int soLuongMoi = quantity + (change ?? 0);
            int tonKho = db.Products.Where(p => p.ProductID == id)
                                    .Select(p => p.Quantity).FirstOrDefault();

            GetCartService().GetCart().UpdateQuantity(id, soLuongMoi, tonKho);

            if (tonKho > 0 && soLuongMoi > Math.Min(Cart.MaxQuantity, tonKho))
            {
                TempData["CartMessage"] = "Kho chỉ còn " + tonKho + " sản phẩm này.";
            }
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