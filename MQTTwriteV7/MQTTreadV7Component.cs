using System;
using System.Diagnostics;
using Grasshopper.Kernel;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text;

namespace MQTTwriteV7
{
    public class MQTTreadV7 : GH_Component
    {

        private string topic = "com";
        private string lastTopic = "";
        private string data = "";
        private int qos = 2;
        private static IMqttClient _Client;
        private static IMqttClientOptions _options;

        private string broker = "broker.mqttdashboard.com";
        private string lastBroker = "";
        private int counter = 0;
        GH_Document doc;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MQTTreadV7()
          : base("MQTTread", "Mr",
            "Receive data from an MQTT topic.",
            "IOT", "MQTT")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Broker", "B", "Broker Address", GH_ParamAccess.item, "broker.mqttdashboard.com");
            pManager.AddTextParameter("Topic", "T", "Topic Address", GH_ParamAccess.item, "com");
            pManager.AddIntegerParameter("QoS", "Q", "Quality of Service", GH_ParamAccess.item, 2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "D", "Data", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Counter", "C", "C", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (doc == null)
            {
                doc = OnPingDocument();
            }
            doc.ScheduleSolution(500);
            Debug.WriteLine("start making inputs");

            if (!DA.GetData(0, ref broker)) return;
            if (!DA.GetData(1, ref topic)) return;
            if (!DA.GetData(2, ref qos)) return;

            Debug.WriteLine("checking topics");

            String[] inTopic = { "-" };

            if (topic != "-")
            {
                inTopic[0] = topic;
            }
            if (topic == "")
            {
                inTopic[0] = "-";
            }
            Debug.WriteLine("checking qos");

            if (qos < 0 || qos > 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "qos can be only 0,1 or 2");
                return;
            }

            // We should now validate the data and warn the user if invalid data is supplied.

            if (lastBroker != broker || lastTopic != topic)
            {
                Debug.WriteLine("subscribe topic");
                Subscribe_Topic();
                lastBroker = broker;
                lastTopic = topic;
            }
            DA.SetData(0, data);

            //}else if (data != lastData)
            //{
            //    DA.SetData(0, data);
            //    lastData = data;
            //}


            counter += 1;
            Debug.WriteLine("counter = " + counter);

            DA.SetData(1, counter);
        }

        async void Subscribe_Topic()
        {
            try
            {
                if (broker == "")
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "broker has to be set");
                    //broker = "broker.hivemq.com";
                    broker = "broker.mqttdashboard.com";
                }
                if (topic == "")
                {
                    topic = "test";
                }
                //var uri = new Uri("tcp://"+broker);
                var factory = new MqttFactory();


                var uri = new Uri("tcp://"+broker);

                _Client = factory.CreateMqttClient();


                _options = new MqttClientOptionsBuilder()
                    .WithTcpServer(uri.Host)
                    .Build();

                //Handlers
                _Client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected successfully with MQTT Brokers.");

                //Subscribe to topic
                await _Client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            });

            _Client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from MQTT Brokers.");
            });

                _Client.UseApplicationMessageReceivedHandler(e =>
                {
                    data = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var calllater = new GH_Document.GH_ScheduleDelegate(UpdateSetData);
                    doc.ScheduleSolution(200, calllater);

                });

                //actaully connect
                try
                {
                    _Client.ConnectAsync(_options).Wait();
                }
                catch (Exception e)
                {
                    //String errorstr = "Exception caught." + e;
                    String errorstr = "Can't connect to broker - check connection or address";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorstr);
                }
                Debug.WriteLine("Press Key to Exit");
            }
                catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private void UpdateSetData(GH_Document gh)
        {
            ExpireSolution(false);
            Debug.WriteLine("message1 = " + data);
            // expire solution
            // TODO: Run in main thread.
            //ExpireSolution(true);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.icon2;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9cc3dc01-83cc-4067-bf61-c31c35da2276"); }
        }

        public object DA { get; private set; }
    }
}
