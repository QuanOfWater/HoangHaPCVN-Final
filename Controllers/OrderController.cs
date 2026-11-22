using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using HoangHaPCVN.Filters;
using HoangHaPCVN.Models;
using HoangHaPCVN.Models.ViewModel;
using PagedList;
using System.Data.Entity.Infrastructure;

namespace HoangHaPCVN.Controllers
{
    [CustomerAuthorize]
    public class OrderController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        //Lấy các sản phẩm khách đã tick trong giỏ, đọc lại tên/ảnh/giá mới nhất từ DB
        private List<CartItem> LayHangDaChon(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<CartItem>();
            }

            //Chỉ lấy những sản phẩm vừa có trong giỏ vừa được tick
            var daChon = GetCartService().GetCart().Items
                .Where(i => ids.Contains(i.ProductID))
                .ToList();
            var maSP = daChon.Select(i => i.ProductID).ToList();

            var products = db.Products
                .Include(p => p.Category)
                .Where(p => maSP.Contains(p.ProductID))
                .ToList();

            //Ghép giỏ hàng (lấy số lượng) với DB (lấy tên, ảnh, giá)
            //Sản phẩm đã bị Admin xóa khỏi DB sẽ tự bị bỏ qua
            return (from item in daChon
                    join p in products on item.ProductID equals p.ProductID
                    select new CartItem
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        ProductImage = p.ProductImage,
                        UnitPrice = p.ProductPrice,
                        Quantity = item.Quantity,
                        Category = p.Category.CategoryName,
                        Stock = p.Quantity
                    }).ToList();
        }

        //Có món nào khách mua nhiều hơn số còn trong kho không (null = ổn)
        private string KiemTraTonKho(List<CartItem> items)
        {
            var thieu = items.FirstOrDefault(i => i.Quantity > i.Stock);
            if (thieu == null)
            {
                return null;
            }
            return thieu.Stock < 1
                ? "\"" + thieu.ProductName + "\" đã hết hàng, vui lòng bỏ chọn sản phẩm này."
                : "\"" + thieu.ProductName + "\" chỉ còn " + thieu.Stock + " sản phẩm, vui lòng giảm số lượng.";
        }

        //Điền phần hiển thị của trang Thanh toán (sản phẩm, người nhận, tổng tiền)
        private Customer DienThongTinHienThi(CheckoutVM model, List<CartItem> items)
        {
            var customer = db.Customers.Find((int)Session["CustomerID"]);
            model.CartItems = items;
            model.CustomerName = customer.CustomerName;
            model.CustomerPhone = customer.CustomerPhone;
            model.TotalAmount = items.Sum(i => i.TotalPrice);
            return customer;
        }

        //THANH TOÁN
        // GET: Order/Checkout?ids=1&ids=5
        public ActionResult Checkout(int[] ids)
        {
            var items = LayHangDaChon(ids);
            if (!items.Any())
            {
                TempData["CartMessage"] = "Vui lòng chọn ít nhất 1 sản phẩm để mua.";
                return RedirectToAction("Index", "Cart");
            }

            string loiKho = KiemTraTonKho(items);
            if (loiKho != null)
            {
                TempData["CartMessage"] = loiKho;
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutVM
            {
                //Chọn sẵn lựa chọn đầu tiên
                DeliveryMethod = CheckoutOptions.DeliveryMethods[0],
                PaymentMethod = CheckoutOptions.COD
            };
            var customer = DienThongTinHienThi(model, items);
            model.ShippingAddress = customer.CustomerAddress; //Mặc định lấy địa chỉ lúc đăng ký

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model, int[] ids)
        {
            var items = LayHangDaChon(ids);
            if (!items.Any())
            {
                //Giỏ đã trống (ví dụ bấm Đặt hàng 2 lần) thì không tạo đơn nữa
                TempData["CartMessage"] = "Không còn sản phẩm nào để đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }

            string loiKho = KiemTraTonKho(items);
            if (loiKho != null)
            {
                TempData["CartMessage"] = loiKho;
                return RedirectToAction("Index", "Cart");
            }

            //Giá trị radio phải nằm trong danh sách cho phép
            if (!CheckoutOptions.DeliveryMethods.Contains(model.DeliveryMethod))
            {
                ModelState.AddModelError("DeliveryMethod", "Hình thức giao hàng không hợp lệ");
            }
            if (!CheckoutOptions.PaymentMethods.Contains(model.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Hình thức thanh toán không hợp lệ");
            }

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    CustomerID = (int)Session["CustomerID"],
                    OrderDate = DateTime.Now,
                    TotalAmount = items.Sum(i => i.TotalPrice), //Tính ở server từ giá trong DB
                    ShippingAddress = model.ShippingAddress.Trim(),
                    DeliveryMethod = model.DeliveryMethod,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = PaymentStatuses.ChuaThanhToan,
                    OrderStatus = OrderStatuses.ChoXuLy,
                    //Chi tiết đơn: KHÔNG gán TotalPrice vì SQL tự tính (Quantity * UnitPrice)
                    OrderDetails = items.Select(i => new OrderDetail
                    {
                        ProductID = i.ProductID,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                //Trừ tồn kho. Sản phẩm đã được tải ở LayHangDaChon nên Find không hỏi lại DB
                foreach (var item in items)
                {
                    db.Products.Find(item.ProductID).Quantity -= item.Quantity;
                }

                db.Orders.Add(order);
                try
                {
                    //1 lần SaveChanges = 1 giao dịch: lưu đơn + chi tiết + trừ kho, lỗi thì không lưu gì
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    //CHECK (Quantity >= 0) trong SQL chặn lại: có người vừa mua mất hàng
                    TempData["CartMessage"] = "Một số sản phẩm vừa hết hàng trong lúc bạn đặt. Vui lòng kiểm tra lại giỏ hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                //Chỉ bỏ khỏi giỏ những sản phẩm vừa đặt, sản phẩm không tick vẫn còn
                var cart = GetCartService().GetCart();
                foreach (var item in items)
                {
                    cart.RemoveItem(item.ProductID);
                }

                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            //Có lỗi: điền lại phần hiển thị rồi mở lại form
            DienThongTinHienThi(model, items);
            return View(model);
        }

        //XÁC NHẬN ĐƠN HÀNG
        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            int customerId = (int)Session["CustomerID"];

            //Chỉ xem được đơn của chính mình; lấy kèm chi tiết đơn và sản phẩm
            var order = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customerId);

            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        //ĐƠN HÀNG CỦA TÔ
        // GET: Order/MyOrder?status=Đã giao&search=chuột&page=1
        public ActionResult MyOrder(string status, string search, int? page)
        {
            int customerId = (int)Session["CustomerID"];
            var myOrders = db.Orders.Where(o => o.CustomerID == customerId);

            //Tìm kiếm: theo mã đơn (gõ "12" hoặc "#12") hoặc theo tên sản phẩm có trong đơn
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                int maDon;
                if (int.TryParse(search.TrimStart('#'), out maDon))
                {
                    myOrders = myOrders.Where(o => o.OrderID == maDon
                        || o.OrderDetails.Any(d => d.Product.ProductName.Contains(search)));
                }
                else
                {
                    myOrders = myOrders.Where(o => o.OrderDetails.Any(d => d.Product.ProductName.Contains(search)));
                }
            }

            //Đếm số đơn theo từng trạng thái để hiện số trên tab sử dụng LinQ
            var demTheoTrangThai = myOrders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { TrangThai = g.Key, SoDon = g.Count() })
                .ToList();
            var counts = OrderStatuses.All.ToDictionary(
                st => st,
                st => demTheoTrangThai.Where(x => x.TrangThai == st).Select(x => x.SoDon).FirstOrDefault());

            //Lọc theo tab trạng thái (chỉ nhận trạng thái có trong danh sách)
            if (!string.IsNullOrEmpty(status) && OrderStatuses.All.Contains(status))
            {
                myOrders = myOrders.Where(o => o.OrderStatus == status);
            }
            else
            {
                status = null; //Tab "Tất cả đơn"
            }

            var model = new MyOrderVM
            {
                Status = status,
                Search = search,
                StatusCounts = counts,
                TotalCount = demTheoTrangThai.Sum(x => x.SoDon),
                //Lấy kèm chi tiết + sản phẩm, đơn mới nhất lên đầu, 5 đơn/trang
                Orders = myOrders
                    .Include(o => o.OrderDetails.Select(d => d.Product))
                    .OrderByDescending(o => o.OrderDate)
                    .ThenByDescending(o => o.OrderID)
                    .ToPagedList(page ?? 1, 5)
            };
            return View(model);
        }

        // GET: Order/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            int customerId = (int)Session["CustomerID"];
            var order = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customerId);

            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Order/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id, string returnUrl)
        {
            int customerId = (int)Session["CustomerID"];
            var order = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customerId);
            if (order == null)
            {
                return HttpNotFound();
            }

            //Kiểm tra lại ở server: chỉ đơn "Chờ xử lý" mới được hủy
            if (OrderStatuses.CanCancel(order.OrderStatus))
            {
                bool daTraTien = order.PaymentStatus == PaymentStatuses.DaThanhToan;
                OrderStatuses.ChuyenTrangThai(order, OrderStatuses.DaHuy); //Tự trả hàng về kho
                db.SaveChanges();
                TempData["AccountMessage"] = "Đã hủy đơn hàng #" + id + "."
                    + (daTraTien ? " Cửa hàng sẽ hoàn tiền cho bạn trong thời gian sớm nhất." : "");
            }
            else
            {
                TempData["AccountMessage"] = "Đơn hàng #" + id + " đã được giao hoặc đã kết thúc nên không thể hủy.";
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); //Quay lại đúng tab / trang đang xem
            }
            return RedirectToAction("MyOrder");
        }

        // POST: Order/ConfirmReceived/5
        //Khách xác nhận đã nhận được hàng (chỉ khi đơn "Thanh toán xong")
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmReceived(int id, string returnUrl)
        {
            int customerId = (int)Session["CustomerID"];
            var order = db.Orders.SingleOrDefault(o => o.OrderID == id && o.CustomerID == customerId);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (OrderStatuses.CanConfirmReceived(order.OrderStatus))
            {
                OrderStatuses.ChuyenTrangThai(order, OrderStatuses.DaNhanHang);
                db.SaveChanges();
                TempData["AccountMessage"] = "Cảm ơn bạn đã xác nhận nhận đơn hàng #" + id + ".";
            }
            else
            {
                TempData["AccountMessage"] = "Đơn hàng #" + id + " chưa thể xác nhận đã nhận.";
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("MyOrder");
        }

        // POST: Order/BuyAgain/5
        //Thêm lại toàn bộ sản phẩm của đơn cũ vào giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int id)
        {
            int customerId = (int)Session["CustomerID"];
            var order = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product.Category))
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customerId);
            if (order == null)
            {
                return HttpNotFound();
            }

            var cart = GetCartService().GetCart();
            var hetHang = new List<string>();
            foreach (var d in order.OrderDetails)
            {
                var p = d.Product;
                if (p.Quantity < 1)
                {
                    hetHang.Add(p.ProductName); //Hết hàng thì bỏ qua
                    continue;
                }
                //Dùng GIÁ HIỆN TẠI trong DB, số lượng không vượt tồn kho
                cart.AddItem(p.ProductID, p.ProductImage, p.ProductName, p.ProductPrice,
                    d.Quantity, p.Category.CategoryName, p.Quantity);
            }

            TempData["CartMessage"] = hetHang.Any()
                ? "Đã thêm các sản phẩm còn hàng của đơn #" + id + ". Đã hết hàng: " + string.Join(", ", hetHang) + "."
                : "Đã thêm các sản phẩm của đơn #" + id + " vào giỏ hàng (theo giá hiện tại).";
            return RedirectToAction("Index", "Cart");
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