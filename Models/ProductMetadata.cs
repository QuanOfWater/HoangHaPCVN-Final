using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HoangHaPCVN.Models
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product { }
    public class ProductMetadata
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên sản phẩm từ 3 đến 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
        [Range(typeof(decimal), "1000", "1000000000", ErrorMessage = "Đơn giá từ 1.000 đ đến 1.000.000.000 đ")]
        [Display(Name = "Đơn giá")]
        public decimal ProductPrice { get; set; }
    }
}