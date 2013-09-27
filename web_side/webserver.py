import tornado.ioloop
import tornado.web
import tornado.websocket
import liblo
import os, string, random, threading


# Global variables
# Yes, we need some!
SOCKETS       = {}
template_path = os.path.join(os.path.dirname(__file__), "templates")
static_path   = os.path.join(os.path.dirname(__file__), "static")
messagec      = False # For caching.


# Web event handlers.
class MainHandler(tornado.web.RequestHandler):
    '''
    Just serve the pages for now. We may need to do something with them
    in the future, but not yet.
    '''
    def get(self, page_name):
        chash = self.get_cookie("hash")
        if chash == None:
            chash = self.generateID(12)
            SOCKETS[chash] = None
            self.set_cookie("hash", chash)
        self.write(render_template(page_name))

    # Adapted from: <http://stackoverflow.com/questions/2257441/python-random-string-generation-with-upper-case-letters-and-digits>
    def generateID(self, size=6, chars=string.ascii_uppercase + string.ascii_lowercase + string.digits):
        '''
        Create a random alphanumeric ID string.
        '''
        return ''.join(random.choice(chars) for x in range(size))


class HashHandler(tornado.web.RequestHandler):
    '''
    Just diplay the host's IP address.
    ''' 
    def get(self, page_name=None):
      import socket
      self.set_header("Content-Type", 'text/plain')
      self.write(self.get_cookie("hash"))

class WebSocketHandler(tornado.websocket.WebSocketHandler):
    '''
    Get every change from each controller and send it back to all the other
    controllers, if there are any. That way, if other controllers make a
    change, it will include the ones made by other people.
    '''
    allow_draft76 = True
    def open(self):
        print("Websocket connected.")
    def on_message(self, message):
        print("Message: " + message)
        chash, message = message.split("/", 1)
        if chash not in SOCKETS or not SOCKETS[chash]: SOCKETS[chash] = self
        if message == "quit":
            try: del HASHES[chash]
            except KeyError: pass
            OSC.send('/quit', chash)
        if message == "spawn":
            OSC.send('/spawn', chash)
        if ': ' not in message: return
        message, data = message.split(': ', 1)
        if message == "touch":
            OSC.send('/touch', "%s:%s" % (chash, data))
        if message == "color":
          OSC.send('/color', "%s:%s" % (chash, data))
    def on_close(self):
        pass


class StaticFileHandlerExtra(tornado.web.StaticFileHandler):
    '''
    A customized StaticFileHandler that fixes issues with the MIME types
    on Windows.
    '''
    def set_extra_headers(self, path):
        if path.endsWith('.png'): self.set_header("Content-Type", 'image/png')


# Global functions

def file_get_contents(filename):
    with open(filename) as f:
        return f.read()

# So far, there are no index files, but, just in case, plan for them.
def render_template(filename):
    '''
    Find and deliver the right HTML files. Raise the correct error if
    they're not found.
    '''
    # Is a directory.
    if os.path.isdir(os.path.join(template_path, filename)):
        filename = os.path.join(template_path, filename, 'index.html')
    # Is a page.
    else:
        filename = '%s.html' % filename
        filename = os.path.join(template_path, filename)
    filename = os.path.normpath(filename)
    try:
        return file_get_contents(filename)
    except FileNotFoundError:
        raise tornado.web.HTTPError(404)

class OSC:
    '''
    Contains the OSC servers and handlers.
    '''
    @classmethod
    def init(cls):
        # Create the server to listen for messages from the System Manager.
        cls.server = liblo.Server(15309)
        # Assign handlers for the server to listen to.
        cls.server.add_method("/locator", 's', cls.locatorHandler)
        cls.serverThread = OSCThread(cls.server)
        cls.serverThread.start()
        cls.target = liblo.Address(15310)

    @classmethod
    def locatorHandler(cls, path, args):
        print("message: %s" % args[0])
        chash, data = args[0].split(':')
        SOCKETS[chash].write_message("location: " + data)

    @classmethod
    def send(cls, addr, message):
        liblo.send(cls.target, addr, message)

class OSCThread(threading.Thread):
     def __init__(self, server):
         threading.Thread.__init__(self)
         self.server = server

     def run(self):
         while True:
            OSC.server.recv(100)

# Get the whole thing running
def main():
    application = tornado.web.Application(
        [
            (r"/hash",         HashHandler),
            (r"/teamx",        WebSocketHandler),
            (r"/static/(.*)",  StaticFileHandlerExtra, {"path": static_path}),
            (r"/(.*)",         MainHandler),
        ],
        template_path = template_path,
        static_path   = static_path
    )
    OSC.init()
    application.listen(8887)
    print("Starting web server...")
    tornado.ioloop.IOLoop.instance().start()

if __name__ == "__main__":
    main()
