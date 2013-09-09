team-x
======

A demo for a crowd-player game concept in Unity and HTML 5.

Any number of players can participate to a common experience on a common screen simply by logging onto a web site on their mobile device.

There are three parts to this system:

* A Unity game
* A central webserver
* A client script

The Unity game uses an implementation of OSC that uses a specific DLL. As such, **Unity Pro** is needed for now.

The webserver is in Python 3.3 and uses Tornado. Earlier versions of Python should also wok but have not been tested. The webserver requires the **Tornado web server libraries** to be installed, as well as the OSC library **Pyliblo**, which in turn requires the C libary liblo.

The web client includes jQuery.

The car models and their textures in the Unity side of the game are by Albert Gea.

To reiterate, the requirements for this service for now are:

* Unity Pro
* Python 3
* Pyliblo
* The Tornado web server
