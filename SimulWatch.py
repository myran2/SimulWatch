#!/usr/bin/env python3

# Adapted from https://github.com/realpython/materials/blob/master/python-sockets-tutorial/multiconn-server.py
# Modified to echo messages to all connected sockets instead of just the socket that sent the message

import sys
import socket
import selectors
import types

sel = selectors.DefaultSelector()

HOST = ''
PORT = 22000

def accept_wrapper(sock):
    conn, addr = sock.accept()  # Should be ready to read
    print("accepted connection from", addr)
    conn.setblocking(False)
    data = types.SimpleNamespace(addr=addr, inb=b"", outb=b"")
    events = selectors.EVENT_READ | selectors.EVENT_WRITE
    sel.register(conn, events, data=data)


def service_connection(key, mask):
    sock = key.fileobj
    data = key.data
    if mask & selectors.EVENT_READ:
        try:
            recv_data = sock.recv(1024)  # Should be ready to read
        except:
            print("closing connection to", data.addr)
            sel.unregister(sock)
            sock.close()
            return

        if recv_data:
            data.outb += recv_data
        else:
            print("closing connection to", data.addr)
            sel.unregister(sock)
            sock.close()
    if mask & selectors.EVENT_WRITE:
        if data.outb:
            print("echoing", repr(data.outb), "to:")
            for key, mask in sel.select(timeout=None):
                print(key.data.addr)
                sent = key.fileobj.send(data.outb)  # Should be ready to write
                key.data.outb = key.data.outb[sent:]

host, port = HOST, PORT
lsock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
lsock.bind((host, port))
lsock.listen()
print("listening on", (host, port))
lsock.setblocking(False)
sel.register(lsock, selectors.EVENT_READ, data=None)

try:
    while True:
        events = sel.select(timeout=None)
        for key, mask in events:
            if key.data is None:
                accept_wrapper(key.fileobj)
            else:
                service_connection(key, mask)
except KeyboardInterrupt:
    print("caught keyboard interrupt, exiting")
finally:
    sel.close()
