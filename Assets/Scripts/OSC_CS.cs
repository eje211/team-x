using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class OSC_CS : MonoBehaviour {

	//You can set these variables in the scene because they are public
	public string serverName    = "OSC server";
	public string RemoteIP     = "255.255.255.255";
	public int    SendToPort   = 15309;
	public int    ListenerPort = 15310;
	
	private Osc handler;
	private List<messageStruct> messageArr = new List<messageStruct>();
	// private StreamReader file = null;	
	// private string line = null;
	
	public void Start () {
	}
	
	public void OnDisable() {
		// close OSC UDP socket
		Debug.Log("closing OSC UDP socket in OnDisable");
		handler.Cancel();
		handler = null;
	}
	
	public void Update () {
		// check to see if any messages/events need to be sent up the food chain
		foreach (messageStruct m in messageArr) {
			if (m.sendit) {
				SendMessage(m.func, m.message);
				m.sendit = false;
				m.message = "";
			}
		}
	}
	
	public void StartServer() {
		// Initializes the OSC server to listen for messages and send messages
		// make sure this game object has both UDPPackIO and OSC script attached.
		UDPPacketIO udp = GetComponent<UDPPacketIO>();
		udp.init(RemoteIP, SendToPort, ListenerPort);
		handler = GetComponent<Osc>();
		handler.init(udp);
		Debug.Log("--------------> OSC server started: ("+serverName+")");
	}
	
	public void Send(string addr,  string message) {	
		// send and OSC message with the provided data
		string m = addr + " " + message;
		OscMessage  oscM = Osc.StringToOscMessage(m);
		handler.Send(oscM);
		Debug.Log("--------------> OSC message sent: ("+m+")");
	} 
	
	public void SetHandler(string addr,  string func) {
		// set what message/event should be fired when a osc message with the given addr is received
		handler.SetAddressHandler(addr, ReceivedOSC);
		messageStruct msg = new messageStruct(addr, func);
		messageArr.Add(msg); //.Push(msg);
		Debug.Log("--------------> OSC handler set: ("+addr+")");
	}
	
	//these fucntions are called when messages are received
	void ReceivedOSC(OscMessage oscMessage) {	
		//How to access values: 
		//oscMessage.Values[0], oscMessage.Values[1], etc
		Debug.Log("--------------> OSC message received: ("+Osc.OscMessageToString(oscMessage)+")");
		foreach (messageStruct m in messageArr) {
			if (m.addr == oscMessage.Address) {
				//SendMessage(m.func, Osc.OscMessageToString(oscMessage));		// doesn't work - scope issue?
				m.sendit = true;
				m.message = Osc.OscMessageToString(oscMessage);
			}
		}
	} 
	
	public void CloseServer()	{
		// done using the server so close the OSC UDP socket to free up the port 
		Debug.Log("closing OSC UDP socket:" + serverName);
		handler.Cancel();
		handler = null;
	}
	
	public class messageStruct {
		// class data structure that holds all the handler data
		public string addr;	// the osc address
		public string func;	// the function that chould be called via SendMessage()
		public bool   sendit;	// Is a message ready to be sent
		public string message;	// Store the message data received 
		
		public messageStruct(string s1, string s2) {
			addr = s1;
			func = s2;
			sendit = false;
			message = "";
		}
	}
	
}
