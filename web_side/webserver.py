import tornado.ioloop
import tornado.web
import tornado.websocket
import os


# Global variables
# Yes, we need some!
CARS          = []
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
        self.write(render_template(page_name))


class IPHandler(tornado.web.RequestHandler):
    '''
    Just diplay the host's IP address.
    ''' 
    def get(self, page_name=None):
      import socket
      self.set_header("Content-Type", 'text/plain')
      self.write(socket.gethostbyname(socket.gethostname()))

class WebSocketHandler(tornado.websocket.WebSocketHandler):
    '''
    Get every change from each controller and send it back to all the other
    controllers, if there are any. That way, if other controllers make a
    change, it will include the ones made by other people.
    '''
    def open(self):
        CONTROLERS.append(self)
        if messagec: self.write_message(messagec)
        else: self.write_message(
            '{"e": 50, "a": 50, "k": 50, "s": 50}')
    def on_message(self, message):
        global messagec
        messagec = message
        for chart in CHARTS: chart.write_message(message)
        for cont in filter(lambda x: x != self, CONTROLERS):
            cont.write_message(message)
    def on_close(self):
        try: CONTROLERS.remove(self)
        except ValueError as e: print('Could not remove controller handler:', e)

class StaticFileHandlerExtra(tornado.web.StaticFileHandler):
    '''
    A customized StaticFileHandler that fixes issues with the MIME types
    on Windows.
    '''
    def set_extra_headers(self, path):
        if path.endsWith('.png'): self.set_header("Content-Type", 'image/png')


# Global functions

# Adapted from: <http://stackoverflow.com/questions/2257441/python-random-string-generation-with-upper-case-letters-and-digits>
def id_generator(size=6, chars=string.ascii_uppercase + string.ascii_lowercase + string.digits):
    return ''.join(random.choice(chars) for x in range(size))

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

def OSCInit(self):
    '''
////////////////////////////////////////////////////////
// Initialize the OSC components that will be necessary
// for buttons and touch.
////////////////////////////////////////////////////////
'''
    #create the server to listen for messages from the System Manager
    self.server = OSCmanager.Receiver( ('', 15309) )# addr is a tupple with the (ip, port)  -- broadcast
    # assign handlers for the server to listen to 
    self.server.addHandler("/posutionUpdate", positionUpdate)

def positionUpdate(): pass

# Get the whole thing running
def main():
    application = tornado.web.Application(
        [
            (r"/ip",           IPHandler),
            (r"/teamx",        WebSocketHandler),
            (r"/static/(.*)",  StaticFileHandlerExtra, {"path": static_path}),
            (r"/(.*)",         MainHandler),
        ],
        template_path = template_path,
        static_path   = static_path
    )
    application.listen(8887)
    tornado.ioloop.IOLoop.instance().start()

if __name__ == "__main__":
    main()