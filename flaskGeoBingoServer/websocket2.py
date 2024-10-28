import asyncio
import json
import logging
from websockets.asyncio.server import broadcast, serve

from http import HTTPStatus
import secrets

logging.basicConfig()

CLIENTS = set()
SUB_KEYS = {}

VALUE = 0

def new_user_event(security_key):
    return json.dumps({"type": "users", "count": len(CLIENTS), "security_key": security_key})

def users_event():
    return json.dumps({"type": "users", "count": len(CLIENTS)})

def value_event():
    return json.dumps({"type": "value", "value": VALUE})

async def counter(websocket):
    global CLIENTS, VALUE
    try:
        # Register user
        if websocket not in CLIENTS:
            security_key = secrets.token_urlsafe(32)
            if websocket not in SUB_KEYS:
                SUB_KEYS[websocket] = {}
            SUB_KEYS[websocket]["security_key"] = security_key

            CLIENTS.add(websocket)
            await websocket.send(new_user_event(security_key))
            broadcast(CLIENTS, users_event())
        # Send current state to user
        #await websocket.send(value_event())
        # Manage state changes
        async for message in websocket:
            event = json.loads(message)

            if event["security_key"] != SUB_KEYS[websocket]["security_key"]:
                break

            if event["action"] == "minus":
                VALUE -= 1
                broadcast(CLIENTS, value_event())
            elif event["action"] == "plus":
                VALUE += 1
                broadcast(CLIENTS, value_event())
            else:
                logging.error("unsupported event: %s", event)
    finally:
        # Unregister user
        print(f"Unregister user: {websocket}")
        CLIENTS.remove(websocket)
        SUB_KEYS.pop(websocket, None)
        broadcast(CLIENTS, users_event())

def process_request(connection, request):
    authentication = request.headers["Authentication"]
    if "123456" != authentication:
        return connection.respond(HTTPStatus.BAD_REQUEST, b"Not authorized\n")

    return None

async def main():
    async with serve(counter, "localhost", 6789, process_request=process_request):
        await asyncio.get_running_loop().create_future()  # run forever

if __name__ == "__main__":
    #asyncio.run(main())
    security_key = secrets.token_urlsafe(32)
    print(security_key)





"""import asyncio
from http import HTTPStatus
from websockets.asyncio.server import serve

class PubSub:
    def __init__(self):
        self.waiter = asyncio.get_event_loop().create_future()

    def publish(self, value):
        waiter = self.waiter
        self.waiter = asyncio.get_event_loop().create_future()
        waiter.set_result((value, self.waiter))

    async def subscribe(self):
        waiter = self.waiter
        while True:
            value, waiter = await waiter
            yield value

    __aiter__ = subscribe

PUBSUB = PubSub()

def broadcast(message):
    PUBSUB.publish(message)

async def handler(websocket):
    print(await websocket.recv())
    async for message in PUBSUB:
        await websocket.send(message)

def ping_pong(connection, request):
    if request.path == "/ping":
        return connection.respond(HTTPStatus.OK, b"pong\n")

async def broadcast_messages():
    while True:
        await asyncio.sleep(1)
        message = "foo bar"
        broadcast(message)

async def main():
    async with serve(handler, "localhost", 8765, process_request=ping_pong):
        await broadcast_messages()  # run forever

if __name__ == "__main__":
    asyncio.run(main())"""


