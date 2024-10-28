import asyncio
import json
from websockets.asyncio.client import connect

async def hello():
    uri = "ws://localhost:6789"
    """async for websocket in connect(uri):
        try:
            what = input("1 = plus 0 = minus: ")

            if what == "1":
                action = "plus"
            else:
                action = "minus"

            message = json.dumps({"action": action})
            await websocket.send(message)
            print(f">>> {message}")

            greeting = await websocket.recv()
            print(f"<<< {greeting}")
        except websocket.ConnectionClosed:
            continue"""
    async with connect(uri, additional_headers={"Authentication": "123456"}) as websocket:
        message = json.dumps({"action": "add me"})
        await websocket.send(message)
        print(f">>> {message}")
        recv_json = await websocket.recv()
        event = json.loads(recv_json)
        security_key = ""
        if "security_key" in event:
            security_key = event["security_key"]

        print(f">>> {json.loads(recv_json)}")
        while True:
            what = input("1 = plus 0 = minus: ")

            if what == "1":
                action = "plus"
            else:
                action = "minus"

            message = json.dumps({"action": action, "security_key": security_key})
            await websocket.send(message)
            print(f">>> {message}")

            result = await websocket.recv()
            print(f"<<< {result}")

if __name__ == "__main__":
    asyncio.run(hello())