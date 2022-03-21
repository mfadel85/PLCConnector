using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class Product
    {
        public Product(string name,int quantity,int xPos,int yPos,int bentCount,int unitID, float price,string direction,float depth)
        {
            this.name = name;
            this.quantity = quantity;
            this.xPos = xPos;
            this.yPos = yPos;
            this.bentCount = bentCount;
            this.unitID = unitID;
            this.price = price;
            this.direction = direction;
            this.depth = depth;
            this.activeClutch = 1;
        }
        public string name { get; set; }
        public int quantity { get; set; }
        public int xPos { get; set; }
        public int yPos { get; set; }
        public float depth { get; set; }
        public int bentCount { get; set; }
        public int unitID { get; set; }
        public float price { get; set; }
       public string direction { get; set; }

        public int activeClutch { get; set; }


    }
    class ProductJson
    {
        public int id { get; set; }
        public string dir { get; set; }
        public float depth { get; set; }
        public int beltCount { get; set; }
        public int unitNo { get; set; }
        public int beltNo { get; set; }
        public int shelfNo { get; set; }
        public ProductJson(int _id, string _dir, float _depth, int _beltCount, int _unitNo, int _beltNo, int _shelfNo)
        {
            id = _id;
            dir = _dir;
            depth = _depth;
            beltCount = _beltCount;
            unitNo = _unitNo;
            beltNo = _beltNo;
            shelfNo = _shelfNo;
        }

    }
}
