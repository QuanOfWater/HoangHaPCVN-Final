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
        public void AddItem(int productId, string productImage, string productName,
            decimal unitPrice, int quantity, string category)
        {
            quantity = Math.Max(1, quantity);
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);

            if (existingItem != null)
            {
                existingItem.Quantity = Math.Min(existingItem.Quantity + quantity, MaxQuantity);
            }
            else
            {
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = Math.Min(quantity, MaxQuantity),
                    Category = category
                });
            }
        }

        //Xóa một sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        // Cập nhật số lượng: Nhỏ hơn một thì xóa sản phẩm khỏi giỏ
        public void UpdateQuantity(int productId, int quantity)
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
            item.Quantity = Math.Min(quantity, MaxQuantity);
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