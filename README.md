This Project add MQTT communication to Grasshopper for Rhino3D.

Download the mqtt-component folder and save it inside components folder (Grasshopper -> files -> special folders).
There are three components:
1. mqtt write - Add the broker address(this is the communication server) - it will automaticaly set the port. Add the topic and the QOS (quality of service, default is 0) and you are ready to write messages to the outside world.
2. mqtt read - Add broker address, topic and QOS and you can read messages from outside.
3. mqtt server - will establish a broker on localhost (127.0.0.1) for testing purposes. BUG: This component once called will run in background all the time until Rhino is closed.

Check the example file with an public broker for testing.
