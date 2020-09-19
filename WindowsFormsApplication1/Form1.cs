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

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        private CJ2Compolet myCJ2;
        private NJCompolet njCompolet;
        public Form1()
        {
            InitializeComponent();
            this.myCJ2 = new CJ2Compolet();
            this.njCompolet = new NJCompolet();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string ipAddress = Helper.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            this.label3.Text = "The IP of this PC is:"+ipAddress;
            try
            {
                this.myCJ2.UseRoutePath = false;
                this.myCJ2.PeerAddress = "192.168.250.1";
                this.myCJ2.LocalPort = 2;
                this.myCJ2.Active = true;
                
                this.njCompolet.UseRoutePath = false;
                this.njCompolet.PeerAddress = "192.168.250.1";
                this.njCompolet.LocalPort = 2;
                this.njCompolet.Active = true;

                label1.Text = njCompolet.UnitName;

                Thread thred = new Thread(
                    t =>
                    {
                        ExecuteServer();
                    })
                { IsBackground = false };
                thred.Start();
            }
            catch(Exception exp)
            {
                label1.Text = exp.Message;
            }
        }

        public  void ExecuteServer()
        {
            string ipAddress = Helper.GetLocalIPv4(NetworkInterfaceType.Ethernet);
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
                }
            }
            catch (SocketException e)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Socket Exception: "+e));
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
            string ipAddress = Helper.GetLocalIPv4(NetworkInterfaceType.Ethernet);
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

                if (data.StartsWith(delimeter))
                {
                    this.handleOrder(data, stream);
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

                if (obj1== null)
                {
                    throw new NotSupportedException();
                }
                VariableInfo info1 = this.njCompolet.GetVariableInfo(varName2);
                string str2 = Helper.GetValueOfVariables(obj1);

                this.label1.Text ="PLC is: "+ Globals.PLCStaus;
                this.label2.Text = str2;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string readVariable(string variable)
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
                this.InvokeEx(f => f.listBox3.Items.Add("Error Nothing Returned from "+variable));

                return "Error nothing returned";
            }
            catch(Exception ex)
            {
                this.InvokeEx(f => f.listBox3.Items.Add("Varibale: "+ex.Message));
                return "Error ex.Message";
            }

        }
        private void readData()
        {
            try
            {
                string varName1 = "Delivered";
                string varName2 = "PLC_Error";
                object obj = this.njCompolet.ReadVariable(varName1);
                object obj1 = this.njCompolet.ReadVariable(varName2);

                if (obj == null || obj1 == null)
                {
                    throw new NotSupportedException();
                }
                VariableInfo info = this.njCompolet.GetVariableInfo(varName1);
                VariableInfo info1 = this.njCompolet.GetVariableInfo(varName2);
                string str = Helper.GetValueOfVariables(obj);
                string str2 = Helper.GetValueOfVariables(obj1);

               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        private void handleOrder(string data, NetworkStream stream)
        {
            Order order = JsonConvert.DeserializeObject<Order>(data);

            this.InvokeEx(f => f.listBox1.Items.Add("Received new order:"));
            this.InvokeEx(f => f.listBox3.Items.Add(data));
            this.InvokeEx(f => f.listBox1.Items.Add("Order: " + order.OrderID + "ProductCount " + order.ProductsCount));


            Globals.orderList.Add(order);
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

        private void scheduleOrder(int orderID)
        {
            this.InvokeEx(f => f.listBox3.Items.Add("Scheduling: "+ orderID)); // to be changed this doesn't reflect the 
            Task.Delay(30000).ContinueWith(t => this.InvokeEx(f => {
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
            try
            {
                object orderValue = Helper.RemoveBrackets(order.OrderID.ToString());
                this.writeVariable("Order_ID", orderValue);

                object newOrderValue = Helper.RemoveBrackets("True");
                this.writeVariable("newOrder", newOrderValue);

                object productCountValue = Helper.RemoveBrackets(order.ProductsCount.ToString());
                this.writeVariable("ProductCount", productCountValue);
                ///read order.Products
                ///
                for (int i = 0; i < order.Products.Length; i++)
                {
                    string productNumber = i.ToString();
                    string xPosVar = "Pos_" + productNumber+ "_X";
                    string yPosVar = "Pos_" + productNumber + "_Y";
                    string quantityVar = "Quantity_" + productNumber;
                    string bentCountVar = "BentCount_" + productNumber;
                    string unitVar = "Unit_" + productNumber;

                    object xPosVal = Helper.RemoveBrackets(order.Products[i].xPos.ToString());
                    object yPosVal = Helper.RemoveBrackets(order.Products[i].yPos.ToString());
                    object quantityVal = Helper.RemoveBrackets(order.Products[i].quantity.ToString());
                    object bentCountVal = Helper.RemoveBrackets(order.Products[i].bentCount.ToString());
                    object unitVal = Helper.RemoveBrackets(order.Products[i].unitID.ToString());

                    this.writeVariable(xPosVar, xPosVal);
                    this.writeVariable(yPosVar, yPosVal);
                    this.writeVariable(quantityVar, quantityVal);
                    this.writeVariable(bentCountVar, bentCountVal);
                    this.writeVariable(unitVar, bentCountVal);
                }
                //  this should be dynamic not 30 seconds
                Task.Delay(30000).ContinueWith(t => this.InvokeEx(async  f => 
                {
                    bool delivered = this.lastOrderDelivered();
                    if (delivered)
                    {
                        int orderID = Globals.orderList[0].OrderID;
                        f.listBox1.Items.Add("Order Delivered: "+ Globals.orderList[0].OrderID);
                        newOrderValue = Helper.RemoveBrackets("False");
                        this.writeVariable("newOrder", newOrderValue);
                        Globals.orderList.Remove(Globals.orderList[0]);
                        // if it gets delivered then we have to send to the PHP server the order_id 
                        // and that is delivered to change its status and change on the stock
                        // write to the database directly or use api?what does OC use for this?
                        HttpClient client = new HttpClient();
                        var content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                            { "order_status_id", "5" }
                        });
                        string mainURL = "http://localhost/store/index.php?route=api/login&key=LA6g3ogGx7lgceCO2uiFZJ4QCwfe93SY54OYi2Pvjnrnxr55sFygOMT1sATi0b7y439oTRZPlM2s9ZY9Qt6tLOYqyDcoVXmhNAChHV2wL3ptKSlaWxMtO5XHhsokshxVyCGiKgMMU775z4IVy549FxY4rTRYb8UVlGNHJBcDIQgkRXdWziUpkzJP6ybm1gUPIIVn5ehCXxQTiRXvqXc6dd0zz4MddwWnQdRMMbdS5wF2IszhxPunqKAYx2If6YZA";
                        var tokenResp = await client.PostAsync(mainURL,content);
                        var tokenString = await tokenResp.Content.ReadAsStringAsync();
                        string token = (string)tokenString;
                        TokenResponse tokenRes = JsonConvert.DeserializeObject<TokenResponse>(token);

                        //f.listBox3.Items.Add(tokenRes.api_token);
                        content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                            { "order_status_id", "5" }
                        });

                        string storeAddress = "http://localhost/store/";
                        string apiToken = tokenRes.api_token;
                        string url = storeAddress  + "index.php?route=api/order/history&api_token=" + apiToken + "&store_id=0&order_id=" + orderID.ToString();

                        var resp = await client.PostAsync(url, content);
                        var repsStr = await resp.Content.ReadAsStringAsync();
                        string finalResponse = (string)repsStr;
                        f.listBox3.Items.Add(finalResponse);
                    }

                    else
                    {
                        f.listBox1.Items.Add("Order Not Delivered");
                        // read error state and error number depending on the error number resend the order again
                        /// to handle this
                    }

                }));
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private  void handleNextOrder(int orderID)
        {
            this.InvokeEx(f => f.listBox3.Items.Add("started" + Globals.orderList[0].OrderID));

            this.checkPLCStatus();

            if (Globals.PLCStaus != "Waiting")
            {
                this.InvokeEx(f => f.listBox3.Items.Add("PLC is not IDLE, can't send the order to the PLC"));
                this.scheduleOrder(orderID);

            }
            else if (Globals.orderList.Count < 1)
            {
                this.InvokeEx(f => f.listBox1.Items.Add("There is no orders at all!!!!"));
            }
            else if (Globals.PLCStaus == "Waiting")
            {
                Order nextOrder = pickNextOrder(Globals.orderList);
                this.sendOrderToPLC(nextOrder);
            }
        }

        private bool lastOrderDelivered()
        {
            bool result = false;
            string delivered = this.readVariable("Delivered");
            if (delivered == "False")
                result = false;
            else
                result = true;

            return result;
        }

        private Order pickNextOrder(List<Order> orderList)
        {
            // an algorithm to handle picking up orders for the list 
            // if the order is delivered then move it from this list or flag it as delivered
            // what if the order has been started for a product whose quanitiy is one: 
            // then if the order has been started and not finished and no stock

            return orderList[0];
        }

        private void checkPLCStatus()
        {
            string status = this.readVariable("PLC_Status");
            this.InvokeEx(f => f.listBox3.Items.Add("PLC Status is"+status));
            Globals.PLCStaus =  status == "False" ? "Waiting" : "Working";
        }
    }
}

static class Globals
{
    public static List<Order> orderList = new List<Order>();

    public static string PLCStaus = "Waiting";
}

