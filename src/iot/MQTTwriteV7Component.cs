using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace iot
{
    public class MQTTwriteV7 : GH_Component
    {
        private string topic = "com";
        private string lastTopic = "";
        private string data = "";
        private string lastData = "";
        private int qos = 2;

        private string broker = "mqtt.eclipse.org";
        private string lastBroker = "";
        private int counter = 0;
        private Boolean published = false;
        GH_Document doc;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MQTTwriteV7()
          : base("MQTTwrite", "Mw",
            "write data to a MQTT topic.",
            "IOT", "MQTT")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Broker", "B", "Broker Address", GH_ParamAccess.item, "mqtt.eclipse.org");
            pManager.AddTextParameter("Topic", "T", "Topic Address", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Data", "D", "Data to write", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("QoS", "Q", "Quality of Service", GH_ParamAccess.item, 0);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Published", "P", "Confirmation of Publishing", GH_ParamAccess.tree);
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


            if (!DA.GetData(0, ref broker)) return;
            if (!DA.GetData(1, ref topic)) return;
            if (!DA.GetData(2, ref data)) return;
            if (!DA.GetData(3, ref qos)) return;


            // We should now validate the data and warn the user if invalid data is supplied.
            if (broker == "")
            {
                broker = "mqtt.eclipse.org";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "broker set to default");
                return;
            }

            if (lastBroker != broker || lastTopic != topic)
            {

                lastBroker = broker;
                lastTopic = topic;
            }
            if (qos < 0 || qos > 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "qos can be 0,1 or 2");
                qos = 2;
                return;
            }
            if (lastData != data)
            {
                Publish_Application_Message();

            }
            if (published)
            {
                counter += 1;
            }
            Debug.WriteLine("counter = " + counter);

            DA.SetData(0, published);
            DA.SetData(1, counter);

        }

        private async void Publish_Application_Message()
        {
            var mqttFactory = new MQTTnet.MqttFactory();

            var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(broker)
                    .Build();
                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                }
                catch (Exception e)
                {
                    //String errorstr = "Exception caught." + e;
                    String errorstr = "Can't connect to broker - check connection or address";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorstr);
                    published=false;
                    return;
                }

                var applicationMessageBuilder = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(data);
            switch (qos)
            { case 0:
                    applicationMessageBuilder.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce);
                    break;
                case 1:
                    applicationMessageBuilder.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);
                    break;
                case 2:
                    applicationMessageBuilder.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce);
                    break;
                default:
                    applicationMessageBuilder.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce);
                    break;
            }

            var applicationMessage = applicationMessageBuilder.Build();

            if (mqttClient.IsConnected)
            {
                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                Console.WriteLine("MQTT application message is published.");
                published = true;
            }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        ///protected override System.Drawing.Bitmap Icon => null;
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return Resources.icon1;
                //return null;
            }
        }
        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("FE34ED33-49BA-4DA8-A211-418AE381D566");
    }
}