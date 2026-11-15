using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Razor.Text;

namespace HoangHaPCVN.Models.ViewModel
{
    //Tương tác với giỏ hàng lưu trong session
    public class CartService
    {
        private readonly HttpSessionStateBase session;

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        //Lấy giỏ hàng từ session, chưa có thì tạo mới
        public Cart GetCart()
        {
            var cart = session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session["Cart"] = cart;
            }
            return cart;
        }

        //Xóa giỏ hàng khỏi session
        public void ClearCart()
        {
            session["Cart"] = null;
        }
    }
}