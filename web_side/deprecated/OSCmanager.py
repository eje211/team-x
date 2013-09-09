#!/usr/bin/python
"""
Two classes for controlling the ETC space bridge network messaging.

ETC Space Bridge 2011

Always drink and pilot responsibly. The authors of this software 
are not responsible for the accidental loss of life or limbs which
may result from unforeseen warp core failures. 

This code conforms to Convention 15 of the Shadow Proclamation 
and is legal on all level 5 planets in the outer spiral galaxies.

Updated by Jerry Fu on April 5, 2011
-- Added timer function for OSC sender to see when the last OSC code was sent.

========================
Example Usage - Receiver
========================
import OSCmanager

# Create a receiver server
server = OSCmanager.Receiver( ('127.0.0.1', 9000) ) # tupple with the (ip, port)

# Assign a handler to the server
server.addHandler("/helmcontrol/all/open", open_handler)

# define a handler
def open_handler(addr, tags, data, source):

========================
Example Usage - Sender
========================
import OSCmanager

# Create a client 
client = OSCmanager.Sender( ('127.0.0.1', 9000), ('127.0.0.1', 9000) ) # pair of tupples with the (ip, port).  first is UDP, second is TCP

# Send a message
client.send("/print", ["testing, testing, is this on?", 2011, 10.5])


========================
  Commands - Receiver   
========================
              close() - closes any connection
        getHandlers() - prints out a list of addresses that the server is listening to 
addHandler(addr,func) - assigns an address to listen to and the callback function

========================
  Commands - Sender     
========================
              close() - closes any connection
         bundleSend() - sends out the bundle of messages 
 bundleAdd(addr,list) - adds a address and message list to the bundle
         bundleInit() - create a new bundle
      send(addr,list) - send a single address and message out

"""

import myOSC
import time, threading

class Receiver(object):   
    def __init__ (self, addr):
        # addr is a tupple with the (ip, port) ex. ('127.0.0.1', 9000)
        # use ('127.0.0.1', 9000) for local communications 
        # use ('', 9000) for broadcast communications
        self.receive_address = addr
        ##self.server = myOSC.OSCServer(self.receive_address) # basic
        self.server = myOSC.ThreadingOSCServer(self.receive_address) # threading
        ##self.server = myOSC.ForkingOSCServer(self.receive_address) # forking
        
        # this registers a 'default' handler (for unmatched messages), 
        # an /'error' handler, an '/info' handler.
        #self.server.addDefaultHandlers()
        
        # define a message-handler function for the server to call.
        def printing_handler(addr, tags, data, source):
            print("---")
            print("received new osc msg from %s" % myOSC.getUrlStr(source))
            print("with addr : %s" % addr)
            print("typetags %s" % tags)
            print("data %s" % data)
            print("---")
        # adding our function
        self.server.addMsgHandler("/print", printing_handler) 
        
        # Start OSCServer
        print("Starting OSCServer.")
        self.serverThread = threading.Thread( target = self.server.serve_forever )
        self.serverThread.start()
        
    def addHandler(self, address, function):
        # add the provided handler function callback to the server with the provided tag address
        self.server.addMsgHandler(address, function)
        
    def getHandlers(self):
        # print out a list of addresses that the server is listening to 
        print("Registered Callback-functions are :")
        for addr in self.server.getOSCAddressSpace():
            print(addr)
            
    def close(self):
        # close out the server
        print("Closing OSCServer.")
        self.server.close()
        print("Waiting for Server-thread to finish")
        self.serverThread.join() ##!!!
        print("Done")
    
    def __del__(self):
        self.close()
        
        
class Sender:   
    def __init__ (self, addr, TCPaddr=None):
        # addr is a tupple with the (ip, port) ex. ('127.0.0.1', 9000)
        # use ('127.0.0.1', 9000) for local communications 
        # use ('255.255.255.255', 9000) or ('<broadcast>', 9000) for broadcast communications 
        self.send_address = addr
        self.TCPsend_address = TCPaddr
        self.bundle = None
        self.client = myOSC.OSCClient(None, addr, TCPaddr)
        self.client.connect( self.send_address ) # note that the argument is a tupple of (ip,port)
        self.lastSend = -1 #time in seconds that the last OSC message was sent

        
    def send(self, address, message_list = []):
        # send out a message with the address and message 
        msg = myOSC.OSCMessage()  
        msg.setAddress(address)
        for i in message_list:
            msg.append(i)
        self.client.send(msg)  
        
    def bundleInit(self):
        # init a bundle that addresses and messages can be add to
        self.bundle = myOSC.OSCBundle()
        
    def bundleAdd(self, address, message_list = []):
        # add an address and a list of messages to the bundle
        if self.bundle != None:
            self.bundle.append( {'addr': address, 'args': message_list} )
        
    def bundleSend(self):
        # send out the bundle
        if self.bundle != None:
            self.client.send(self.bundle)
            self.bundle = None
            self.lastSend = time.clock()
            
    def bundleCancel(self):
        self.bundle = None
    
    def close(self):
        # close out the connection
        self.client.close()
        
    #Jerry
    def timeSinceLastSend(self):
        return time.clock() - self.lastSend
