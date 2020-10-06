using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    static class OrderSort
    {
        public static void sort(List<Product> list)
        {
            // product in the first unit then the second unit
            for(int i = 0; i < list.Count-1; i++)
            {
                for(int j = 1; j < list.Count; j++)
                {
                    if (Compare(list[i], list[j]))
                    {
                        Swap(list, i, j);
                    }
                }
            }


        }
        public static void Swap(List<Product> list,int indexA, int indexB)
        {
            Product tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
        private static  bool Compare(Product p1, Product p2)
        {
            // if p2 comes before p1 return true
            if (p2.unitID < p1.unitID)
                return true;
            else if (p2.unitID > p1.unitID)
                return false;
            else
            {
                if (p2.xPos < p1.xPos)
                    return true;
                else if (p2.xPos > p1.xPos)
                    return false;
                else
                {
                    if (p2.yPos < p1.yPos)
                        return true;
                    else
                        return false;
                }
            }
        }

    }
}
