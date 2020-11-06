using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OMRON.Compolet.CIP;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using WindowsFormsApplication1;
using System.Net.Http;
using System.Data.SQLite;
using System.Timers;
using System.Drawing.Printing;
using System.Windows.Documents;
using System.IO;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);
        private CJ2Compolet myCJ2;
        private NJCompolet njCompolet;
        private DBOperations dbOp;
        private readonly BackgroundWorker worker;
        string ipAddress ;
        private Font printFont;

        public Form1()
        {
            Form1.SetDefaultPrinter(PrinterName);
            ipAddress = Helper.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            ipAddress = "192.168.1.51";
            InitializeComponent();
            this.myCJ2 = new CJ2Compolet();
            this.njCompolet = new NJCompolet();
            dbOp = new DBOperations();
            SQLiteConnection sqlite_conn;
            sqlite_conn = dbOp.CreateConnetion();
            dbOp.GetOrsersList();
            worker = new BackgroundWorker();
            worker.DoWork += scheudule;
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            //ReadDataDB(sqlite_conn);
        }

        void timer_Elapsed( object sender, ElapsedEventArgs e)
        {
            worker.RunWorkerAsync();
        }

        void scheudule(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Handling Next Order"));
                Order order = dbOp.nextOrder();
                if (order != null && Globals.status == "ToSend")
                {
                    this.handleNextOrder(0);
                }
            }
            catch(Exception ex)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Exception"));

            }

        }

        string PrinterName
        {
            get { return (string)Properties.Settings.Default["PrinterName"]; }
            set
            {
                Properties.Settings.Default["PrinterName"] = value;
                Properties.Settings.Default.Save();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.label3.Text = ipAddress;
            try
            {
                this.myCJ2.UseRoutePath = false;
                this.myCJ2.PeerAddress = "192.168.1.2";
                this.myCJ2.LocalPort = 2;
                this.myCJ2.Active = true;

                this.njCompolet.UseRoutePath = false;
                this.njCompolet.PeerAddress = "192.168.1.2";
                this.njCompolet.LocalPort = 2;
                this.njCompolet.Active = true;
                if(njCompolet.IsConnected)
                    label1.Text = njCompolet.UnitName;

                Thread thred = new Thread(
                    t =>
                    {
                        ExecuteServer();
                    })
                { IsBackground = false };
                thred.Start();
            }
            catch (Exception exp)
            {
                label1.Text = exp.Message;
            }
        }

        public void ExecuteServer()
        {
            //string ipAddress = Helper.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            //string ipAddress = "192.168.250.37";
            IPAddress localAddr = IPAddress.Parse(ipAddress);
            var listener = new TcpListener(localAddr, 11111);
            try
            {
                listener.Start();
                while (true)
                {
                    this.InvokeEx(f => f.listBox3.Items.Add("Start listening to Orders"));
                    TcpClient client = listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(ThreadProc, new object[] { client, listener });
                    ThreadPool.QueueUserWorkItem(HandleNextOrderProc, new object[] {  });

                }
            }
            catch (SocketException e)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Socket Exception: " + e));
            }
            finally
            {
                // Stop listening for new clients.
                listener.Stop();
            }

        }

        public void ThreadProc(object state)
        {
            /// the remaining tasks here is : 
            /// 
            //string ipAddress = Helper.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            ///ipAddress = "192.168.250.37";

            object[] array = state as object[];
            var client = (TcpClient)array[0];
            var listener = (TcpListener)array[1];

            IPAddress localAddr = IPAddress.Parse(ipAddress);
            // Buffer for reading data
            Byte[] bytes = new Byte[1024];
            String data = null;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();
            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {

                this.InvokeEx(f => f.listBox3.Items.Add("Start receiving new order"));

                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                string delimeter = "{" + (char)34 + "OrderID" + (char)34;// handle {"OrderID" as the start of the order data
                string deliveryDelimeter = "{" + (char)34 + "Delivery" + (char)34;// handle {"OrderID" as the start of the order data

                if (data.StartsWith(delimeter))
                {
                    this.HandleOrder(data, stream);
                }
                else if (data.StartsWith(deliveryDelimeter))
                {
                    this.DeliverOrder(data, stream);
                }
            }
            // Shutdown and end connection
            client.Close();
        }



        private void HandleNextOrderProc(object state)
        {
            this.InvokeEx(f => f.listBox3.Items.Add("action 4 ???"));

        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                checkPLCStatus();
                string varName2 = "Order_ID";
                object obj1 = this.njCompolet.ReadVariable(varName2);

                if (obj1 == null)
                {
                    throw new NotSupportedException();
                }
                VariableInfo info1 = this.njCompolet.GetVariableInfo(varName2);
                string str2 = Helper.GetValueOfVariables(obj1);

                this.label1.Text = "PLC is: " + Globals.PLCStaus;
                this.label2.Text = str2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string ReadVariable(string variable)
        {
            try
            {
                object obj = this.njCompolet.ReadVariable(variable);
                if (obj != null)
                {
                    VariableInfo info = this.njCompolet.GetVariableInfo(variable);
                    string str = Helper.GetValueOfVariables(obj);
                    return str;
                }
                this.InvokeEx(f => f.listBox3.Items.Add("Error Nothing Returned from " + variable));

                return "Error nothing returned";
            }
            catch (Exception ex)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Varibale: " + ex.Message));
                return "Error ex.Message";
            }

        }
        private void DeliverOrder(string data, NetworkStream stream)
        {
            try
            {
                OrderDeliver order = JsonConvert.DeserializeObject<OrderDeliver>(data);
                this.InvokeEx(f => f.listBox1.Items.Add("Order To Be delivered Now:"));
                this.InvokeEx(f => f.listBox3.Items.Add(data));
                this.InvokeEx(f => f.listBox1.Items.Add("Order: " + order.OrderID ));
            }
            catch(Exception ex)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Exception Deliver Order now"));

            }
        }
        private void HandleOrder(string data, NetworkStream stream)
        {
            
            try
            {
                
                Order order = JsonConvert.DeserializeObject<Order>(data);
                dbOp.InsertOrder(order);
                this.InvokeEx(f => f.listBox1.Items.Add("Received new order:"));
                this.InvokeEx(f => f.listBox3.Items.Add(data));
                this.InvokeEx(f => f.listBox1.Items.Add("Order: " + order.OrderID + "ProductCount " + order.ProductsCount));

                object sano = new Object();
                EventArgs e = new EventArgs();
                Globals.currentOrder = order;

                this.printOrder(order);
                Globals.ordersList.Add(order);
                checkPLCStatus();
                //Globals.PLCStaus = "Working";
                if (Globals.PLCStaus == "Waiting")
                {
                    this.handleNextOrder(order.OrderID);
                }
                else if (Globals.PLCStaus == "Working")
                {
                    Thread t = new Thread(() => this.scheduleOrder(order.OrderID));
                    t.Start();
                }
                // when receiving a message from the PLC that the order is delivered then ready to send the next order
                string message = "Order Received and will be scheduled!!!";
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
            }
            catch(Exception ex)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Exception not able to send"));

            }

        }

        private void scheduleOrder(int orderID)
        {
            this.InvokeEx(f => f.listBox3.Items.Add("Scheduling: " + orderID)); // to be changed this doesn't reflect the 
            Task.Delay(30000).ContinueWith(t => this.InvokeEx(f =>
            {
                this.handleNextOrder(orderID);
            }));

        }



        private void writeVariable(string name, object value)
        {
            try
            {
                if (this.njCompolet.GetVariableInfo(name).Type == VariableType.STRUCT)
                    value = Helper.ObjectToByteArray(value);
                this.njCompolet.WriteVariable(name, value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void sendOrderToPLC(Order order)
        {

            int[,] allowedPositions = new int[,] {
                {100,1700 },
                {1000,2000 },
                {500,2300 },
                {200, 1800},
                {1000,1800 },
                {320,2110 }
            };
            try
            {
                object orderValue = Helper.RemoveBrackets(order.OrderID.ToString());// test
                this.writeVariable("Order_ID", orderValue);

              
                object productCountValue = Helper.RemoveBrackets(order.ProductsCount.ToString());
                this.writeVariable("ProductCount", productCountValue);
                ///read order.Products
                ///
                for (int i = 0; i < order.Products.Count; i++)
                {
                    int j = i + 1;
                    string productNumber = j.ToString();
                    string xPosVar = "Pos_" + productNumber + "_X";
                    string yPosVar = "Pos_" + productNumber + "_Y";
                    string quantityVar = "Quantity_" + productNumber;
                    string bentCountVar = "BentCount_" + productNumber;
                    string unitVar = "Unit_" + productNumber;
                    string directionVar = "Dir_" + productNumber;

                    int xPos = (order.Products[i].xPos-1) * 15+100;
                    xPos = allowedPositions[i,0];
                    int yPos = (order.Products[i].yPos-1) * 20+1700;// will change based on the physical shelf no : to be checked later VIN
                    yPos = allowedPositions[i, 1];
                    bool direction = (order.Products[i].direction == "Right") ? true : false;
                    object directionVal = Helper.RemoveBrackets(direction.ToString());
                    object xPosVal = Helper.RemoveBrackets(xPos.ToString());
                    object yPosVal = Helper.RemoveBrackets(yPos.ToString());
                    object quantityVal = Helper.RemoveBrackets(order.Products[i].quantity.ToString());
                    object bentCountVal = Helper.RemoveBrackets(order.Products[i].bentCount.ToString());
                    object unitVal = Helper.RemoveBrackets(order.Products[i].unitID.ToString());

                    this.writeVariable(xPosVar, xPosVal);
                    this.writeVariable(yPosVar, yPosVal);
                    this.writeVariable(quantityVar, quantityVal);
                    this.writeVariable(bentCountVar, bentCountVal);
                    this.writeVariable(unitVar, bentCountVal);
                    //this.writeVariable(directionVar, directionVal);

                }

                // 
                object newOrderValue = Helper.RemoveBrackets("True");
                this.writeVariable("newOrder", newOrderValue);
                Globals.status = "Sent";
                ///
                //  this should be dynamic not 30 seconds: waiting this from the PLC
                Task.Delay(30000).ContinueWith(t => this.InvokeEx(async f =>
                {
                    bool delivered = this.lastOrderDelivered();
                    if (delivered)
                    {
                        Globals.status = "ToSend";
                        int orderID = order.OrderID;/// we need to make sure of this : the last delivered from the databae
                        f.listBox1.Items.Add("Order Delivered: " + order.OrderID);
                        Globals.currentOrder = order;
                        // to print the order now
                        this.printOrder(order);
                        Globals.currentOrder = null;
                        newOrderValue = Helper.RemoveBrackets("False");
                        this.writeVariable("newOrder", newOrderValue);
                        object deliveredVar = Helper.RemoveBrackets("False");
                        this.writeVariable("Delivered", deliveredVar);
                        // update from the database
                        dbOp.UpdateOrder(orderID);
                        //Globals.ordersList.Remove(Globals.ordersList[orderID]); // To Be tested
                        // if it gets delivered then we have to send to the PHP server the order_id 
                        // and that is delivered to change its status and change on the stock
                        // write to the database directly or use api?what does OC use for this?
                        HttpClient client = new HttpClient();
                        var content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                            { "order_status_id", "5" }
                        });
                        string mainURL = "http://localhost/store/index.php?route=api/login&key=LA6g3ogGx7lgceCO2uiFZJ4QCwfe93SY54OYi2Pvjnrnxr55sFygOMT1sATi0b7y439oTRZPlM2s9ZY9Qt6tLOYqyDcoVXmhNAChHV2wL3ptKSlaWxMtO5XHhsokshxVyCGiKgMMU775z4IVy549FxY4rTRYb8UVlGNHJBcDIQgkRXdWziUpkzJP6ybm1gUPIIVn5ehCXxQTiRXvqXc6dd0zz4MddwWnQdRMMbdS5wF2IszhxPunqKAYx2If6YZA";
                        var tokenResp = await client.PostAsync(mainURL, content);
                        var tokenString = await tokenResp.Content.ReadAsStringAsync();
                        string token = (string)tokenString;
                        TokenResponse tokenRes = JsonConvert.DeserializeObject<TokenResponse>(token);

                        //f.listBox3.Items.Add(tokenRes.api_token);
                        content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                            { "order_status_id", "5" }
                        });

                        string storeAddress = "http://localhost/store/";
                        string apiToken = tokenRes.api_token;
                        string url = storeAddress + "index.php?route=api/order/history&api_token=" + apiToken + "&store_id=0&order_id=" + orderID.ToString();
                        // another call to remove sold rows from product_to_position
                        var resp = await client.PostAsync(url, content);
                        var repsStr = await resp.Content.ReadAsStringAsync();
                        string finalResponse = (string)repsStr;
                        f.listBox3.Items.Add(finalResponse);
                    }

                    else
                    {
                        f.listBox1.Items.Add("Order " + order.OrderID+ " Not Delivered");
                        // read error state and error number depending on the error number resend the order again
                        /// to handle this
                    }

                }));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // if not all the data of the order has been written : handle
            }
        }
        private void handleNextOrder(int orderID)
        {
            try
            {
                this.InvokeEx(f => f.listBox3.Items.Add("started" + Globals.nextOrderID.ToString()));// next order from the database nextOrder()

                this.checkPLCStatus();

                if (Globals.PLCStaus != "Waiting")
                {
                    this.InvokeEx(f => f.listBox3.Items.Add("PLC is not IDLE, can't send the order to the PLC"));
                    this.scheduleOrder(orderID);
                }
                else if (Globals.ordersList.Count < 1)// to be taken from the database
                {
                    this.InvokeEx(f => f.listBox1.Items.Add("There is no orders at all!!!!"));
                }
                else if (Globals.PLCStaus == "Waiting")
                {
                    Order nextOrder = dbOp.nextOrder();
                    if(nextOrder != null)
                        this.sendOrderToPLC(nextOrder); 
                }
            }
            catch (Exception ex)
            {
                this.InvokeEx(f => f.listBox1.Items.Add("Could Handle next orde "));
            }


        }

        private bool lastOrderDelivered()
        {
            try
            {
                bool result = false;
                string delivered = this.ReadVariable("Delivered");
                if (delivered == "False")
                    result = false;
                else
                    result = true;

                return result;
            }
            catch (Exception ex)
            {
                this.InvokeEx(f => f.listBox1.Items.Add("Could read last delivered "));
                return false;
            }
        }


       

        private void checkPLCStatus()
        {
            string status = this.ReadVariable("PLC_Status");
            this.InvokeEx(f => f.listBox3.Items.Add("PLC Status is" + status));
            Globals.PLCStaus = status == "False" ? "Waiting" : "Working";
        }
        private string checkOrderState()
        {
            try
            {
                string varName2 = "Order_Status";
                object obj1 = this.njCompolet.ReadVariable(varName2);

                if (obj1 == null)
                {
                    throw new NotSupportedException();
                }
                VariableInfo info1 = this.njCompolet.GetVariableInfo(varName2);
                string str2 = Helper.GetValueOfVariables(obj1);
                return str2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return "Exception No Output!!";
            }
        }
        private void readOrderState_Click(object sender, EventArgs e)
        {
            try
            {
                string status = checkOrderState();
                this.label5.Text = "PLC is: " + status;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void getOrders_Click(object sender, EventArgs e)
        {
        }
        // The PrintPage event is raised for each page to be printed.
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            printFont = new Font("Arial", 10);

            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            List<String> invoice = new List<String>();
            invoice.Add("Order ID: "+Globals.currentOrder.OrderID);
            foreach(Product p in Globals.currentOrder.Products)
                invoice.Add(p.name+": $" +p.price );
            
            invoice.Add("Total is "+Globals.currentOrder.Total);
            //invoice.Add("Total: 40TL");
            string timeNow = DateTime.Now.ToString("MM\\/dd\\/yyyy h\\:mm tt");
            invoice.Add(timeNow);

            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

            foreach (string line in invoice)
            {
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
            }
        }
        private void printOrder(Order order)
        {
            try
            {
                // pass the order to it
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                //pd.PrintPage +=   PrintPageEventHandler(this.pd_PrintPage);
                pd.Print();
            }
            catch(Exception ex)
            {
                this.InvokeEx(f => f.listBox1.Items.Add("Could not print the receipt, please check if the printer is connected!!!"));
            }

        }

        private void saveIP_Click(object sender, EventArgs e)
        {
            ipAddress = this.ipTextBox.Text;
        }
    }
}

static class Globals
{
    public static List<Order> ordersList = new List<Order>();
    public static Order currentOrder = new Order();
    public static string PLCStaus = "Waiting";
    public static string status = "ToSend";
    public static int nextOrderID;
}

