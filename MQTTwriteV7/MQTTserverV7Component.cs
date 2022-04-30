using System;
using System.Diagnostics;
using Grasshopper.Kernel;
using MQTTnet;
using System.Text;
using MQTTnet.Server;


namespace MQTTwriteV7
{
    public class MQTTserverV7 : GH_Component
    {
        public String log = "";
        private Boolean run = false;
        private static Boolean running = false;
        GH_Document doc;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MQTTserverV7()
          : base("MQTTserverV7", "Ms - localhost",
            "MQTT localhost Server for distributing Messages",
            "IOT", "MQTT")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run Broker", "R", "True - run Broker", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "L", "Logging Messages", GH_ParamAccess.list);

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

            if (!DA.GetData(0, ref run)) return;


            //if (!DA.GetData(0, ref port)) return;
            //if (port < 1024 || port > 49151)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "input a valid port number");
            //    return;
            //}
            var option = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithApplicationMessageInterceptor(OnNewMessage)
                .Build();

            // Create a new mqtt server 
            var mqttServer = new MqttFactory().CreateMqttServer();
            if (!running && run)
            {

                Run_Minimal_Server(mqttServer,option);
                running = true;
            }
            else if (running && !run)
            {
                Stop_Minimal_Server(mqttServer, option);
                running = false;

            }

            DA.SetData(0, log);
        }
        async void Run_Minimal_Server(IMqttServer ms, IMqttServerOptions mo)
        {
            /*
             * This sample starts a simple MQTT server which will accept any TCP connection.
             */

            // The port for the default endpoint is 1883.
            // The default endpoint is NOT encrypted!
            // Use the builder classes where possible.
            try
            {
                Debug.WriteLine("start Server / Broker");

                await ms.StartAsync(mo);
            }
            catch (Exception e)

            {
                //String errorstr = "Exception caught." + e;
                String errorstr = "Can't establish Server / Broker";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorstr);
            }
 
        }
        async void Stop_Minimal_Server(IMqttServer ms, IMqttServerOptions mo)
        {
            try
            {
                Debug.WriteLine("stop Server / Broker");

                await ms.StopAsync();
            }
            catch (Exception e)

            {
                //String errorstr = "Exception caught." + e;
                String errorstr = "Can't stop Server / Broker";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorstr);
            }

        }

        void OnNewMessage(MqttApplicationMessageInterceptorContext context)
        {
            // Convert Payload to string
            var payload = context.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);


            log = String.Format("TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",
                DateTime.Now,
                context.ClientId,
                context.ApplicationMessage?.Topic,
                payload,
                context.ApplicationMessage?.QualityOfServiceLevel,
                context.ApplicationMessage?.Retain);

            var calllater = new GH_Document.GH_ScheduleDelegate(UpdateSetData);
            doc.ScheduleSolution(200, calllater);

        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        private void UpdateSetData(GH_Document gh)
        {
            ExpireSolution(false);
            Debug.WriteLine("message1 = " + log);
            // expire solution
            // TODO: Run in main thread.
            //ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.icon3;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2082e449-acc1-451e-aac2-d3d8112c1628"); }
        }

        public override void AddedToDocument(GH_Document document)
        {
            foreach (IGH_DocumentObject obj in Grasshopper.Instances.ActiveCanvas.Document.Objects)
            {
                //Looping through the objects in the active canvas document
                if ((obj.ComponentGuid == this.ComponentGuid) && (obj.InstanceGuid != this.InstanceGuid))
                {
                    Debug.WriteLine("found running instance ");

                    //If the component Guid matches and if the instance Guids are different...
                    Grasshopper.Instances.ActiveCanvas.Document.RemoveObject(obj, true); //True or false - do we want to recompute the canvas? We're removing a component that was unnecessary, so I'm setting this to false - we don't need to recompute the canvas
                    break; //We found and removed our additional instance, we don't need to continue looping (saves computational time and prevents a Grasshopper complaint that the collection (Document.Objects) was modified when you're trying to use it!
                }
            }

            base.AddedToDocument(document);
        }
    }
}
