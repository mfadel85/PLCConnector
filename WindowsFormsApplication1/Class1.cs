using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class Product
    {
        public Product(string name,int quantity,int xPos,int yPos,int bentCount,int unitID, float price)
        {
            this.name = name;
            this.quantity = quantity;
            this.xPos = xPos;
            this.yPos = yPos;
            this.bentCount = bentCount;
            this.unitID = unitID;
            this.price = price;
        }
        public string name { get; set; }
        public int quantity { get; set; }
        public int xPos { get; set; }
        public int yPos { get; set; }
        public int bentCount { get; set; }
        public int unitID { get; set; }
        public float price { get; set; }


    }
}
