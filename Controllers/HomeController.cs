using HoangHaPCVN.Models;
using HoangHaPCVN.Models.ViewModel; //HomeProductVM
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity; //Include
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static HoangHaPCVN.Models.ViewModel.HomeProductVM;
using System.Net;

namespace HoangHaPCVN.Controllers
{
    public class HomeController : Controller
    {
        private HoangHaPCVNEntities db = new HoangHaPCVNEntities();
        public ActionResult Index(string searchTerm, int? page)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = null;
            }
            else
            {
                searchTerm = searchTerm.Trim();
                if (searchTerm.Length > 50)
                {
                    searchTerm = searchTerm.Substring(0, 50);
                }
            }

            var model = new HomeProductVM();

            //Chỉ lấy SP còn hàng; Include đơn hàng để tính "đã bán" (bỏ đơn bị hủy)
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.OrderDetails.Select(d => d.Order))
                .Where(p => p.Quantity > 0);

            //Tìm kiếm dựa trên từ khóa tìm kiếm
            if (searchTerm != null)
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) || p.ProductDescription.Contains(searchTerm) || p.Category.CategoryName.Contains(searchTerm));
                model.SoKetQua = products.Count();
            }

            //Lấy ra top 10 SP bán chạy nhất (nhiều lượt mua)
            model.FeaturedProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .ThenByDescending(p => p.ProductID)
                .Take(10)
                .ToList();

            //Lấy ra 20 SP ít người mua nhất + phân trang (6 SP mỗi trang)
            int pageNumber = page ?? 1;
            int pageSize = 6;
            model.NewProducts = products
                .OrderBy(p => p.ProductID)
                .Take(20)
                .ToPagedList(pageNumber, pageSize);

            //User không tìm kiếm thì hiện thêm mỗi danh mục một khối, 6 SP mới nhất
            if (searchTerm == null)
            {
                var categories = db.Categories.OrderBy(c => c.CategoryID).ToList();
                foreach (var c in categories)
                {
                    var list = db.Products
                        .Include(p => p.OrderDetails.Select(d => d.Order))
                        .Where(p => p.CategoryID == c.CategoryID && p.Quantity > 0)
                        .OrderByDescending(p => p.ProductID)
                        .Take(6)
                        .ToList();

                    //Nếu danh mục chưa có SP thì không hiện khối
                    if (list.Count > 0)
                    {
                        model.CategoriesSections.Add(new CategoryProductsVM
                        {
                            CategoryID = c.CategoryID,
                            CategoryName = c.CategoryName,
                            Products = list

                        });
                    }
                }
            }
            return View(model);
        }

        // Get: Home/Productlist/5
        //Danh sách SP theo danh mục người dùng đã chọn (có sắp xếp + phân trang)
        //Không truyền ID thì hiện tất cả SP
        public ActionResult ProductList(int? id, string sortOrder, int? page)
        {
            var model = new ProductListVM();
            var products = db.Products
                .Include(p => p.OrderDetails.Select(d => d.Order))
                .Where(p => p.Quantity > 0);

            if (id.HasValue)
            {
                //Tìm danh mục, nếu không tìm thấy thì báo lỗi 404
                var category = db.Categories.Find(id.Value);
                if (category == null)
                {
                    return HttpNotFound();
                }
                model.CategoryID = category.CategoryID;
                model.CategoryName = category.CategoryName;

                //Chỉ được lấy SP thuộc danh mục này
                products = products.Where(p => p.CategoryID == id.Value);
            }
            else
            {
                model.CategoryName = "Tất cả sản phẩm";
            }

            //Sort theo lựa chọn của user
            switch (sortOrder)
            {
                case "bestseller": //Bán chạy nhất, nhiều lượt mua nhất
                    products = products.OrderByDescending(p => p.OrderDetails.Count()).ThenByDescending(p => p.ProductID);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                default: //Mặc định: Mới nhất (thêm sau nhưng đứng ở trước)
                    products = products.OrderByDescending(p => p.ProductID);
                    break;
            }
            model.SortOrder = sortOrder;

            //Phân trang: 12 SP một trang theo tỉ lệ 4 ô x 3 hàng
            int pageNumber = page ?? 1;
            int pageSize = 12;
            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Home/ProductDetail/5?quantity=2&page=1
        //Chi tiết một SP, gồm phân trang và sl tạm tính
        //SP tương tự và SP bán chạy cùng danh mục
        public ActionResult ProductDetail(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Lấy SP kèm theo danh mục và chi tiết đơn hàng (để đếm số đã bán)
            Product pro = db.Products
                .Include(p=> p.Category)
                .Include(p => p.OrderDetails.Select(d => d.Order))
                .SingleOrDefault(p => p.ProductID == id);

            if (pro == null)
            {
                return HttpNotFound();
            }

            //Lấy tất cả SP cùng danh mục (trừ SP đang xem hiện tại)
            var products = db.Products
                .Include(p => p.OrderDetails.Select(d => d.Order))
                .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID && p.Quantity > 0);

            var model = new ProductDetailVM();
            model.Product = pro;
            model.SoLuongDaBan = pro.DaBan; //Không tính đơn đã hủy

            //Số lượng muốn mua: từ 1 tới tồn kho (tối đa 99), không nhập thì mặc định 1
            int toiDa = Math.Max(1, Math.Min(Cart.MaxQuantity, pro.Quantity));
            if (quantity.HasValue)
            {
                model.Quantity = Math.Max(1, Math.Min(quantity.Value, toiDa));
            }

            //8 SP cùng một danh mục
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();

            //8 SP bán chạy nhất cùng một danh mục, phân trang 3 SP mỗi trang
            int pageNumber = page ?? 1;
            int pageSize = 3;
            model.TopProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .ThenByDescending(p => p.ProductID)
                .Take(8)
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        //Child Action: Lấy danh sách danh mục để hiện menu danh mục
        //[ChildActionOnly] chỉ gọi từ bên trong view bằng @Html.Action
        //Không mở trực tiếp bằng đường dẫn được (VD: Home/CategoriesMenu
        //Child Action: lấy danh sách danh mục để hiện menu danh mục
        //activeId: mã danh mục đang xem (để tô sáng trong menu), có thể để trống
        [ChildActionOnly]
        public ActionResult CategoriesMenu(int? activeId)
        {
            ViewBag.ActiveId = activeId;
            var categories = db.Categories.OrderBy(c => c.CategoryID).ToList();
            return PartialView("_PVCategories", categories);
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