# IoT

DOI 10.5281/zenodo.11535308

![icons](/Users/admin/gitlab/gh_mqtt/icons.png)

This open-source project adds IoT communication to Grasshopper for Rhino3D. It is part of a series of diverse digital projects developed since 2020 at the Department for Digital Design Methods at the Institute for Architecture at BTU Cottbus-Senftenberg. We use it for controlling robots, managing various microservices in combination with simulation, visualization, and building processes on Rhino.

Initially, it was planned only for the MQTT protocol. The UDP protocol (minimal, connectionless) will be incorporated soon.

To install, download the folders from the dist-folder and save them inside the components folder (Grasshopper -> files -> special folders).

The MQTT component works on Rhino7. The IoT component is an updated version for Rhino8, offering more reliability.

There are three components:

​	1.	**MQTT Write** - Add the broker address (this is the communication server), and it will automatically set the port to 1883. Add the topic and the QOS (quality of service, default is 0), and you are ready to write messages to the outside world.

​	2.	**MQTT Read** - Add the broker address, topic, and QOS, and you can read messages from the outside.

​	3.	**MQTT Server** - Establishes a broker on localhost (127.0.0.1) for testing purposes. **BUG:** This component, once called, will run in the background until Rhino is closed.



Check the example file with a public broker for testing.