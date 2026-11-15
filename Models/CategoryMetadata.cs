using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HoangHaPCVN.Models
{
    //Gắn lớp metadata và class Category do EF sinh ra 
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category { }

    //Chỉ chứa các thuộc tính cần kiểm tra, tên phải trùng với thuộc tính trong Category.cs
    public class CategoryMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2 đến 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [StringLength(100, ErrorMessage = "Biểu tượng tối đa 100 ký tự")]
        [RegularExpression(@"^fa-[a-z]+ fa-[a-z0-9-]+$",
            ErrorMessage = "Nhập đúng dạng class Font Awesome, ví dụ: fa-solid fa-fan")]
        [Display(Name = "Biểu tượng")]
        public string CategoryIcon { get; set; }
    }
}