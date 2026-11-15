using HoangHaPCVN.Areas.Admin.Filters;
using HoangHaPCVN.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace HoangHaPCVN.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CategoriesController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities(); //Vào DB

        // GET: Admin/Categories
        //Nơi hiển thị danh sách danh mục
        public ActionResult Index()
        {
            return View(db.Categories.ToList()); //Lấy tất cả dòng của table Category thành một List, rồi gửi sang View
        }

        // GET: Admin/Categories/Details/5
        //Details: Lấy chi tiết một bản ghi có CategoryID = id
        public ActionResult Details(int? id) //? nghĩa là id được phép không có, mở mà thiếu số ở cuối thì id sẽ là null. Trả về lỗi 400 tránh crash
        {
            if (id == null)
            {
                //Lỗi 404: Thiếu giá trị truyền vào
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id); //Tìm một dòng theo khóa chính (primary key), nếu không tìm được thì trả về null và action trả về lỗi 404
            if (category == null) //Không tìm thấy bản ghi
            {
                return HttpNotFound(); //Lỗi 404
            }
            return View(category); //Mở view và gửi dữ liệu từ Category sang. Bên view dữ liệu này gọi là Model
        }

        //Kiểm tra thêm ngoài Data Annotation: bỏ khoảng trắng thừa, không cho trùng tên danh mục
        private void KiemTraDanhMuc(Category category)
        {
            if (!string.IsNullOrWhiteSpace(category.CategoryName))
            {
                category.CategoryName = category.CategoryName.Trim();
                bool trungTen = db.Categories.Any(c => c.CategoryName == category.CategoryName
                                                    && c.CategoryID != category.CategoryID);
                if (trungTen)
                {
                    ModelState.AddModelError("CategoryName", "Đã có danh mục tên này");
                }
            }
            if (category.CategoryIcon != null)
            {
                category.CategoryIcon = category.CategoryIcon.Trim();
            }
        }

        //Trả về lý do không được xóa danh mục (null = được xóa)
        private string LyDoKhongXoa(Category category)
        {
            int soSanPham = db.Products.Count(p => p.CategoryID == category.CategoryID);
            if (soSanPham > 0)
            {
                return "Danh mục đang có " + soSanPham + " sản phẩm. Hãy chuyển các sản phẩm sang danh mục khác hoặc xóa chúng trước.";
            }
            return null;
        }

        // GET: Admin/Categories/Create
        //Hiện ra form thêm danh mục (trống)
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //Nhận dữ liệu từ form và lưu danh mục mới vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName,CategoryIcon")] Category category)
        {
            KiemTraDanhMuc(category);
            if (ModelState.IsValid)
            {
                db.Categories.Add(category); //Đưa vào danh sách chờ thêm
                db.SaveChanges(); //Ghi nhận thật vào DB
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        //Edit giống Details nên không cần viết lại nên gọi lại Details
        public ActionResult Edit(int? id)
        {
            return Details(id);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //Nhận dữ liệu đã sửa và cập nhập lại vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName,CategoryIcon")] Category category)
        {
            KiemTraDanhMuc(category);
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified; //Đánh dấu bản ghi đã sửa
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.LyDoKhongXoa = LyDoKhongXoa(category);
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            //Kiểm tra lại ở server: Còn sản phẩm thì quay về trang Delete để hiện lý do
            if (LyDoKhongXoa(category) != null)
            {
                return RedirectToAction("Delete", new { id = id });
            }
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) //Đóng kết nối tới DB khi controller không còn dùng nữa
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
