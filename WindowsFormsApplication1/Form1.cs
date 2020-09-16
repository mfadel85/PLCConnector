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
            this.label3.Text = ipAddress;
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
                        //this.label3.Text = "I like you";
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
            this.label3.Text = ipAddress;

            IPAddress localAddr = IPAddress.Parse(ipAddress);
            var listener = new TcpListener(localAddr, 11111);
            try
            {
                listener.Start();
                while (true)
                {
                    //this.label4.Text = "Waiting for an order or event from the PLC... ";
                    TcpClient client = listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(ThreadProc, new object[] { client, listener });
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                listener.Stop();
            }
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string varName1 = "Finish";
                string varName2 = "Order_ID";
                object obj = this.njCompolet.ReadVariable(varName1);
                object obj1 = this.njCompolet.ReadVariable(varName2);

                if (obj == null || obj1== null)
                {
                    throw new NotSupportedException();
                }
                VariableInfo info = this.njCompolet.GetVariableInfo(varName1);
                VariableInfo info1 = this.njCompolet.GetVariableInfo(varName2);
                string str = Helper.GetValueOfVariables(obj);
                string str2 = Helper.GetValueOfVariables(obj1);

                this.label1.Text = str;
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
                    //return str;
                }
                this.InvokeEx(f => f.listBox3.Items.Add("Nothing Returned Error"));

                return "Error nothing returned";
            }
            catch(Exception ex)
            {
    
                this.InvokeEx(f => f.listBox3.Items.Add(ex.Message));
                return "Error ex.Message";
            }

        }
        private void readData()
        {
            try
            {
                string varName1 = "Finish";
                string varName2 = "Order_ID";
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

            this.InvokeEx(f => f.listBox3.Items.Add("Got an order"));
            this.InvokeEx(f => f.listBox3.Items.Add("Received new order:"));
            this.InvokeEx(f => f.listBox3.Items.Add(data));
            this.InvokeEx(f => f.listBox3.Items.Add("Order: " + order.OrderID + "ProductCount " + order.ProductsCount));
            string message = "Order Received and will be scheduled!!!";
            this.InvokeEx(f => f.listBox3.Items.Add(message));

            Globals.orderList.Add(order);
            string plcStatus = checkPLCStatus();
            //Globals.PLCStaus = "Working";
            if (Globals.PLCStaus == "Waiting")
            {
                this.handleNextOrder(Globals.orderList);
            }
            else if (Globals.PLCStaus == "Working")
            {
                Thread t = new Thread(new ThreadStart(this.scheduleOrder));
                t.Start();
                //this.scheduleOrder();
            }
                // when receiving a message from the PLC that the order is delivered then ready to send the next order
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

            // Send back a response.
            stream.Write(msg, 0, msg.Length);
        }

        private void scheduleOrder()
        {
            this.InvokeEx(f => f.listBox3.Items.Add("scheduling"));
            Task.Delay(10000).ContinueWith(t => this.InvokeEx(f => { f.listBox3.Items.Add("started"); }));


            /// wait for thirty seconds and then reread the PLC status and see if it is time to work
        }

        public void ThreadProc(object state)
        {
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
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                string delimeter = "{" + (char)34 + "OrderID" + (char)34;// handle {"OrderID" as the start of the order data

                if (data.StartsWith(delimeter))
                {
                    this.handleOrder(data,stream);
                }
                else if (data.StartsWith("PLC-NOTIFY"))
                {
                    Console.WriteLine("");
                    Console.WriteLine("Signal from PLC");
                    String message = "Notification is received and we will see what we will do!!!";
                    // Process the data sent by the client.

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: PLC Notification Received and will be handled!!!");
                    Console.WriteLine("------------------------------------------------------");
                    Console.WriteLine("");
                }

            }
            // Shutdown and end connection
            client.Close();
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

                object productCountValue = Helper.RemoveBrackets(order.ProductsCount.ToString());
                this.writeVariable("Count_Item", productCountValue);
                ///read order.Products
                ///
                for (int i = 0; i < order.Products.Length; i++)
                {
                    string productNumber = i.ToString();
                    string xPosVar = "X_POS_" + productNumber;
                    string yPosVar = "Y_POS_" + productNumber;
                    string quantityVar = "Quantity_" + productNumber;
                    string bentCountVar = "Bent_Count_" + productNumber;
                    string unitVar = "Unit_" + productNumber;

                    object xPosVal = Helper.RemoveBrackets(order.Products[i].xPos.ToString());
                    object yPosVal = Helper.RemoveBrackets(order.Products[i].yPos.ToString());
                    object quantityVal = Helper.RemoveBrackets(order.Products[i].quantity.ToString());
                    object bentCountVal = Helper.RemoveBrackets(order.Products[i].bentCount.ToString());
                    object unitVal = Helper.RemoveBrackets(order.Products[i].unitID.ToString());
                    /// here will be 5 this.writeVariable();
                    this.writeVariable("X_Postion", xPosVal);
                    this.writeVariable("Y_Postion", yPosVal);
                    this.writeVariable("Number_OF_Band", bentCountVal);

                }


                string valWrite = "X_Postion";
                if (valWrite.StartsWith("_"))
                {
                    MessageBox.Show("The SystemVariable can not write!");
                    return;
                }
                object val = Helper.RemoveBrackets(order.Products[0].xPos.ToString());
                if (this.njCompolet.GetVariableInfo(valWrite).Type == VariableType.STRUCT)
                {
                    val = Helper.ObjectToByteArray(val);
                }
                this.njCompolet.WriteVariable(valWrite, val);

                // rea
                //this.(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private  void handleNextOrder(List<Order> orderList)
        {
            this.readData();

            if (Globals.PLCStaus != "Waiting")
            {
                this.readData();
                Console.WriteLine("PLC is not IDLE, can't send the order to the PLC");
                //return;
            }
            else if (orderList.Count < 1)
            {
                Console.WriteLine("There is no orders at all!!!!");
            }
            else if (Globals.PLCStaus == "Waiting")
            {
                Order nextOrder = pickNextOrder(orderList);
                this.sendOrderToPLC(nextOrder);
            }
        }

        private Order pickNextOrder(List<Order> orderList)
        {
            return orderList[0];
        }

        private string checkPLCStatus()
        {
            string status = this.readVariable("Finish");
            this.InvokeEx(f => f.listBox3.Items.Add(status));
            return "waiting";
        }
    }
}

static class Globals
{
    public static List<Order> orderList = new List<Order>();

    public static string PLCStaus = "Waiting";
}

