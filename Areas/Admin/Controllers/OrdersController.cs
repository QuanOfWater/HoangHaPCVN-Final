using HoangHaPCVN.Models;
using HoangHaPCVN.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HoangHaPCVN.Areas.Admin.Filters;

namespace HoangHaPCVN.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();

        // GET: Admin/Order
        // GET: Admin/Orders?customerId=1 là đơn hàng của một khách hàng cụ thể
        // GET: Admin/Orders?orderStatus=Đã giao lọc theo trạng thái đơn hàng
        public ActionResult Index(int? customerId, string orderStatus, int? page)
        {
            var model = new OrderSearchVM();
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            //Lọc đơn hàng của một khách hàng cụ thể
            if (customerId.HasValue)
            {
                model.CustomerID = customerId;
                var customer = db.Customers.Find(customerId);
                model.CustomerName = customer != null ? customer.CustomerName : null;
                orders = orders.Where(o => o.CustomerID == customerId.Value);
            }

            //Lọc theo trạng thái đơn
            if (!string.IsNullOrEmpty(orderStatus))
            {
                model.OrderStatus = orderStatus;
                orders = orders.Where(o => o.OrderStatus == orderStatus);
            }

            //Đơn mới nhất lên đầu, một trang có 10 đơn
            int pageNumber = page ?? 1;
            int pageSize = 10;
            model.Orders = orders.OrderByDescending(o => o.OrderDate).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Admin/Orders/Details/5
        //Chi tiết một đơn hàng: thông tin khách và các sản phẩm trong đơn
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        //Đổi trạng thái đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus)
        {
            Order order = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            //Chỉ cho chuyển sang bước hợp lệ (chặn "Đã hủy" → "Đã giao"...)
            if (!OrderStatuses.NextForAdmin(order).Contains(orderStatus))
            {
                TempData["Error"] = "Không thể chuyển đơn từ \"" + order.OrderStatus + "\" sang \"" + orderStatus + "\".";
                return RedirectToAction("Details", new { id = id });
            }

            OrderStatuses.ChuyenTrangThai(order, orderStatus); //Tự lo kho + trạng thái thanh toán
            db.SaveChanges();
            TempData["Message"] = "Đã cập nhật trạng thái đơn hàng thành \"" + orderStatus + "\".";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: Admin/Orders/ConfirmPayment/5
        //Xác nhận đã nhận tiền chuyển khoản (đơn đang Chờ xử lý)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.PaymentMethod == CheckoutOptions.ChuyenKhoan
                && order.OrderStatus == OrderStatuses.ChoXuLy
                && order.PaymentStatus != PaymentStatuses.DaThanhToan)
            {
                order.PaymentStatus = PaymentStatuses.DaThanhToan;
                db.SaveChanges();
                TempData["Message"] = "Đã xác nhận nhận tiền cho đơn #" + id + ". Bây giờ có thể chuyển sang \"Đã giao\".";
            }
            else
            {
                TempData["Error"] = "Đơn này không ở trạng thái chờ xác nhận tiền.";
            }
            return RedirectToAction("Details", new { id = id });
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