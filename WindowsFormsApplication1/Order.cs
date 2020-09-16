using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class Order
    {
        public int OrderID { get; set; }
        public int ProductsCount { get; set; }
        public string OrderStatus { get; set; } = "waitng";
        public ProductList[] Products { get; set; }
        public Order()
        {

        }

        public Order(int orderId, int count, ProductList[] productsArray)
        {
            OrderID = orderId;
            ProductsCount = count;
            Products = productsArray;
        }
    }
}
