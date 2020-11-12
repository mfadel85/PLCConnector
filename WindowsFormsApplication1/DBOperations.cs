using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Timers;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    class DBOperations
    {
        public SQLiteConnection CreateConnetion()
        {
            SQLiteConnection sqliteConn = new SQLiteConnection("Data Source=orderDB.db");
            try
            {
                sqliteConn.Open();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MessageBox.Show(ex.Message);


            }
            return sqliteConn;
        }
        public List<Order> GetOrsersList()
        {
            string sql = "SELECT * FROM Orders O join Products P on o.order_id = p.order_id and O.status ='Waiting'"; // add price to the query

            var orderList = new List<Order>();

            try
            {
                using (var con = new SQLiteConnection("Data Source=orderDB.db"))
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Product> products = new List<Product>();
                        Order order = new Order();
                        int i = 0;
                        int j = 1;
                        int productsCount = -1;
                        while (reader.Read())
                        {
                            productsCount = int.Parse(reader["product_count"].ToString());

                            /// we have three cases: if this is the first line of the order then open a new order
                            /// and add its products
                            /// if this is seconde line of the order then only add the new product
                            /// if this is last line then add the product and close and init variables again
                            if (j == 1 && productsCount > 1)
                            {
                                products = new List<Product>();
                                order.OrderID = int.Parse(reader["order_id"].ToString());
                                order.OrderStatus = reader["status"].ToString();
                                order.ProductsCount = productsCount;
                                Product p = new Product(reader["name"].ToString(), int.Parse(reader["quantity"].ToString()), int.Parse(reader["xPos"].ToString()),
                                    int.Parse(reader["yPos"].ToString()), int.Parse(reader["bentCount"].ToString()), int.Parse(reader["unit_id"].ToString()),
                                    float.Parse(reader["price"].ToString()),
                                    reader["direction"].ToString()
                                    ) ;
                                products.Add(p);
                            }
                            else if (j == 1 && productsCount == 1)
                            {
                                products = new List<Product>();
                                order.OrderID = int.Parse(reader["order_id"].ToString());
                                order.OrderStatus = reader["status"].ToString();
                                order.ProductsCount = productsCount;
                                Product p = new Product(reader["name"].ToString(), int.Parse(reader["quantity"].ToString()), int.Parse(reader["xPos"].ToString()),
                                    int.Parse(reader["yPos"].ToString()), int.Parse(reader["bentCount"].ToString()), int.Parse(reader["unit_id"].ToString()),
                                    float.Parse(reader["price"].ToString()),
                                    reader["direction"].ToString());
                                products.Add(p);
                                order.Products = products;
                                Globals.ordersList.Add(order);
                            }
                            else if (j > 1 && productsCount > 1)
                            {
                                Product p = new Product(reader["name"].ToString(), int.Parse(reader["quantity"].ToString()), int.Parse(reader["xPos"].ToString()),
                                   int.Parse(reader["yPos"].ToString()), int.Parse(reader["bentCount"].ToString()),
                                   int.Parse(reader["unit_id"].ToString()), float.Parse(reader["price"].ToString()),
                                    reader["direction"].ToString());
                                products.Add(p);
                            }
                            if (j > 1 && productsCount == j)
                            {
                                order.Products = products;
                                Globals.ordersList.Add(order);
                                j = 0;
                                //MessageBox.Show(order.Products[0].quantity.ToString());

                            }
                            j++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return orderList;
        }

        public void InsertOrder(Order order)
        {
            try
            {
                using (var con = new SQLiteConnection("Data Source=orderDB.db"))
                {
                    con.Open();
                    SQLiteCommand insertSQL = new SQLiteCommand(con);
                    insertSQL.CommandText = "INSERT INTO Orders(order_id, status, product_count,total) VALUES(" + order.OrderID.ToString() + ",'Waiting'," + order.ProductsCount.ToString() + ","+ order.Total+")";
                    insertSQL.ExecuteNonQuery();
                    foreach (Product p in order.Products)
                    {
                        SQLiteCommand insertSQLDetails = new SQLiteCommand(con);
                        string sqlStatement = "INSERT INTO Products(product_id,order_id,quantity,name,xPos,yPos,bentCount,unit_id,price,direction) VALUES(1," + order.OrderID.ToString() + "," + p.quantity.ToString()
                            + ",'" + p.name.ToString() + "'," + p.xPos.ToString() + "," + p.yPos.ToString() + "," + p.bentCount.ToString() + "," + p.unitID.ToString() + "," + p.price.ToString() + "," + p.direction.ToString() + "')";
                        insertSQLDetails.CommandText = "INSERT INTO Products(product_id,order_id,quantity,name,xPos,yPos,bentCount,unit_id,price,direction) VALUES(1," + order.OrderID.ToString() + "," + p.quantity.ToString()
                            + ",'" + p.name.ToString() + "'," + p.xPos.ToString() + "," + p.yPos.ToString() + "," + p.bentCount.ToString() + "," + p.unitID.ToString() + "," + p.price.ToString()+ ",'" + p.direction.ToString() + "')";
                        insertSQLDetails.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //throw new Exception(ex.Message);
            }
        }

        public void UpdateOrder(int orderID)
        {
            try
            {
                using (var con = new SQLiteConnection("Data Source=orderDB.db"))
                {
                    con.Open();
                    SQLiteCommand updateOrder = new SQLiteCommand(con);
                    updateOrder.CommandText = "UPDATE Orders set status = 'Delivered' where order_id=" + orderID.ToString();
                    updateOrder.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //throw new Exception(ex.Message);
            }
        }
        public float[] lastOrderDetails()
        {
            float[] result = new float[3] { -1,-1,-1};
            string sql = "select  order_id,product_count,total from ORDERs where status = 'Waiting' limit 1";
            try
            {
                using (var con = new SQLiteConnection("Data Source=orderDB.db"))
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            
                            return result;
                        }
                        while(reader.Read())
                        {
                            result[0] = float.Parse(Convert.ToString(reader["order_id"]));
                            result[1] = float.Parse(Convert.ToString(reader["product_count"]));
                            result[2] = float.Parse(Convert.ToString(reader["total"]));
                            return result;
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return result;
            }
            return result;
        }
        public Order nextOrder()
        {
            // we have to set the total here also :D
            int orderID = (int)lastOrderDetails()[0];
            float total = lastOrderDetails()[2];
            string sql = "SELECT * FROM  Products  where order_id = "+ orderID;
            Order order = new Order();
            try
            {
                using (var con = new SQLiteConnection("Data Source=orderDB.db"))
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            order = null;
                            return order;
                        }

                        List<Product> products = new List<Product>();
                        int i = 0;
                        int j = 1;
                        int productsCount = -1;
                        while (reader.Read())
                        {
                            productsCount = (int)lastOrderDetails()[1];
                            order.Total = total;

                            /// we have three cases: if this is the first line of the order then open a new order
                            /// and add its products
                            /// if this is seconde line of the order then only add the new product
                            /// if this is last line then add the product and close and init variables again
                            if (j == 1 && productsCount > 1)
                            {
                                products = new List<Product>();
                                order.OrderID = int.Parse(reader["order_id"].ToString());
                                order.OrderStatus = "Waiting";
                                order.ProductsCount = productsCount;
                                Product p = new Product(reader["name"].ToString(), int.Parse(reader["quantity"].ToString()), int.Parse(reader["xPos"].ToString()),
                                    int.Parse(reader["yPos"].ToString()), int.Parse(reader["bentCount"].ToString()), int.Parse(reader["unit_id"].ToString()),
                                    float.Parse(reader["price"].ToString()),
                                    reader["direction"].ToString());
                                products.Add(p);
                            }
                            else if (j == 1 && productsCount == 1)
                            {
                                products = new List<Product>();
                                order.OrderID = int.Parse(reader["order_id"].ToString());
                                //order.OrderStatus = reader["status"].ToString();
                                order.ProductsCount = productsCount;
                                Product p = new Product(reader["name"].ToString(), int.Parse(reader["quantity"].ToString()), int.Parse(reader["xPos"].ToString()),
                                    int.Parse(reader["yPos"].ToString()), int.Parse(reader["bentCount"].ToString()), int.Parse(reader["unit_id"].ToString()),
                                    float.Parse(reader["price"].ToString()),
                                    reader["direction"].ToString());
                                products.Add(p);
                                order.Products = products;
                                Globals.ordersList.Add(order);
                            }
                            
                            else if (j > 1 && productsCount > 1)
                            {
                                Product p = new Product(reader["name"].ToString(), int.Parse(reader["quantity"].ToString()), int.Parse(reader["xPos"].ToString()),
                                   int.Parse(reader["yPos"].ToString()), int.Parse(reader["bentCount"].ToString()), int.Parse(reader["unit_id"].ToString()),
                                   float.Parse(reader["price"].ToString()),
                                    reader["direction"].ToString());
                                products.Add(p);
                            }
                            if (j > 1 && productsCount == j)
                            {
                                OrderSort.sort(products);
                                order.Products = products;
                            }
                            j++;
                        }
                    }
                }
                if (order != null)
                {
                    Globals.nextOrderID = order.OrderID;
                }
                return order;
            }
            catch (Exception ex)
            {
                //return order;
                MessageBox.Show(ex.Message);
                return order;
            }

        }
    }
}
