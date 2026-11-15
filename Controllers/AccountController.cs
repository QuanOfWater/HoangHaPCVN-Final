using HoangHaPCVN.Filters;
using HoangHaPCVN.Models;
using HoangHaPCVN.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HoangHaPCVN.Controllers
{
    public class AccountController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();
        private const string RoleCustomer = "Customer";

        //Check xem khách đã đăng nhập chưa
        private bool DaDangNhap()
        {
            return Session["UserRole"] as string == RoleCustomer && Session["CustomerID"] != null;
        }

        //Lưu trạng thái đăng nhập vào Session + cookie xác thực
        private void LuuDangNhap(User user, Customer customer)
        {
            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole;
            Session["CustomerID"] = customer.CustomerID;
            Session["CustomerName"] = customer.CustomerName;
            FormsAuthentication.SetAuthCookie(user.Username, false); //False: tắt trình duyệt là mất cookie
        }

        //Quay lại trang trước đó (chỉ trang trong web mình), không có thì về trang chủ
        private ActionResult QuayLai(string returnUrl)
        {   
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        //Đọc thông tin khách đang đăng nhập từ DB
        private ProfileVM LayThongTin()
        {
            int customerId = (int)Session["CustomerID"];
            var c = db.Customers.Find(customerId);
            return new ProfileVM
            {
                Username = c.Username,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                CustomerEmail = c.CustomerEmail,
                CustomerAddress = c.CustomerAddress
            };
        }

        //ĐĂNG KÝ
        // GET: Account/Register
        public ActionResult Register(string returnUrl)
        {
            if (DaDangNhap())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //Kiểm tra tên đăng nhập đã tồn tại chưa (trong cả tài khoản Admin lẫn Customer)
                if (db.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                }
                else
                {
                    //1 bản ghi tài khoản trong bảng User
                    var user = new User
                    {
                        Username = model.Username,
                        Password = model.Password, //Tạm lưu nguyên văn, mã hóa ở phần checklist
                        UserRole = RoleCustomer
                    };
                    //1 bản ghi thông tin khách hàng trong bảng Customer
                    var customer = new Customer
                    {
                        CustomerName = model.CustomerName.Trim(),
                        CustomerPhone = model.CustomerPhone,
                        CustomerEmail = model.CustomerEmail.Trim(),
                        CustomerAddress = model.CustomerAddress.Trim(),
                        Username = model.Username
                    };
                    db.Users.Add(user);
                    db.Customers.Add(customer);
                    //1 lần SaveChanges: EF lưu cả 2 bảng trong cùng 1 giao dịch (lỗi thì không bảng nào bị lưu)
                    db.SaveChanges();

                    //Đăng ký xong chuyển sang trang "Đăng nhập" và điền sẵn tên đăng nhập
                    TempData["AccountMessage"] = "Đăng ký thành công! Vui lòng đăng nhập bằng tài khoản vừa tạo.";
                    TempData["NewUsername"] = user.Username;
                    return RedirectToAction("Login", new { returnUrl = returnUrl });
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //ĐĂNG NHẬP
        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            if (DaDangNhap())
            {
                return QuayLai(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            //Vừa đăng ký xong thì điền sẵn tên đăng nhập
            return View(new LoginVM { Username = TempData["NewUsername"] as string });
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //Chỉ tìm tài khoản Customer theo tên đăng nhập
                var user = db.Users.SingleOrDefault(u =>
                    u.Username == model.Username && u.UserRole == RoleCustomer);
                var customer = user == null ? null
                    : db.Customers.FirstOrDefault(c => c.Username == user.Username);

                //Phân biệt chữ hoa chữ thường
                if (user != null && customer != null && user.Password == model.Password)
                {
                    LuuDangNhap(user, customer);
                    return QuayLai(returnUrl);
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //ĐĂNG XUẤT
        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            //Chỉ xóa thông tin đăng nhập, giữ lại giỏ hàng Session["Cart"]
            Session.Remove("Username");
            Session.Remove("UserRole");
            Session.Remove("CustomerID");
            Session.Remove("CustomerName");
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //Thông tin tài khoản
        // GET: Account/ProfileInfo
        [CustomerAuthorize]
        public ActionResult ProfileInfo()
        {
            return View(LayThongTin());
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            //Luôn lấy khách theo Session, không tin tên đăng nhập gửi lên từ form
            int customerId = (int)Session["CustomerID"];
            var customer = db.Customers.Find(customerId);

            if (ModelState.IsValid)
            {
                customer.CustomerName = model.CustomerName.Trim();
                customer.CustomerPhone = model.CustomerPhone;
                customer.CustomerEmail = model.CustomerEmail.Trim();
                customer.CustomerAddress = model.CustomerAddress.Trim();
                db.SaveChanges();

                Session["CustomerName"] = customer.CustomerName; //Header đổi tên theo
                TempData["AccountMessage"] = "Cập nhật thông tin thành công.";
                return RedirectToAction("ProfileInfo");
            }

            model.Username = customer.Username;
            return View("ProfileInfo", model); //Hiện lại form kèm lỗi
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            var user = db.Users.Find(Session["Username"] as string);

            if (ModelState.IsValid)
            {
                if (user.Password != model.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng");
                }
                else
                {
                    user.Password = model.NewPassword;
                    db.SaveChanges();
                    TempData["AccountMessage"] = "Đổi mật khẩu thành công.";
                    return RedirectToAction("ProfileInfo");
                }
            }

            //Hiện lại trang Thông tin tài khoản, lỗi đổi mật khẩu nằm trong ModelState
            return View("ProfileInfo", LayThongTin());
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