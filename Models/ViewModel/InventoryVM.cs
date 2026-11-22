using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HoangHaPCVN.Models.ViewModel
{
    public class InventoryVM
    {
        public string SearchTerm { get; set; }
        public List<StockRowVM> InStock { get; set; } //Hàng tồn
        public List<StockRowVM> OutOfStock { get; set; } //Hàng cần nhập
    }

    public class StockRowVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string CategoryName { get; set; }
        public int Quantity { get; set; } //Tồn kho hiện tại
        public int Sold { get; set; } //Đã bán
    }
}