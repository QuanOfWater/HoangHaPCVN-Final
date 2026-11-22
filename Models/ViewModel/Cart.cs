using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace HoangHaPCVN.Models.ViewModel
{
    //Quản lý danh sách các SP trong giỏ
    public class Cart
    {
        //Số lượng tối đa của một SP trong giỏ
        public const int MaxQuantity = 99;
        public List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items => items;

        //Danh sách các sản phẩm trong giỏ hàng đã được nhóm theo tên danh mục
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        //Danh sách các sản phẩm cùng danh mục với các sản phẩm trong giỏ
        public IPagedList<Product> SimilarProducts { get; set; }

        //Thêm sản phẩm: Đã có trong giỏ thì cộng thêm số lượng
        //Stock: Tồn kho hiện tại, số lượng trong giỏ không được vượt quá
        public void AddItem(int productId, string productImage, string productName,
            decimal unitPrice, int quantity, string category, int stock)
        {
            int toiDa = Math.Min(MaxQuantity, stock);
            if (toiDa < 1)
            {
                return; //Hết hàng thì không thêm
            }

            quantity = Math.Max(1, quantity);
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);

            if (existingItem != null)
            {
                existingItem.Quantity = Math.Min(existingItem.Quantity + quantity, toiDa);
                existingItem.Stock = stock;
            }
            else
            {
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = Math.Min(quantity, toiDa),
                    Category = category,
                    Stock = stock
                });
            }
        }

        //Xóa một sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        // Cập nhật số lượng: Nhỏ hơn 1 thì xóa khỏi giỏ, lớn hơn tồn kho thì giữ ở mức tồn kho
        public void UpdateQuantity(int productId, int quantity, int stock)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item == null)
            {
                return;
            }
            if (quantity < 1)
            {
                RemoveItem(productId);
                return;
            }
            int toiDa = Math.Min(MaxQuantity, stock);
            if (toiDa < 1)
            {
                return; //Hết hàng: giữ nguyên, trang giỏ sẽ báo "Hết hàng"
            }
            item.Quantity = Math.Min(quantity, toiDa);
            item.Stock = stock;
        }

        //Tổng giá trị giỏ hàng của đơn hàng
        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        //Tổng số lượng sản phẩm trong giỏ (cái icon đỏ có số trên giỏ hàng ở header)
        public int TotalQuantity()
        {
            return items.Sum(i => i.Quantity);
        }

        //Làm trống giỏ hàng
        public void Clear()
        {
            items.Clear();
        }
    }
}