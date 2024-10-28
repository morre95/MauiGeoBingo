import asyncio
from asyncio import CancelledError

import websockets
import json
import secrets

from commons import fetch_player_num, fetch_player_ids, is_game_running, fetch_servers, fetch_game_status_all, get_winner
from datetime import datetime

# Håller reda på anslutna klienter och deras prenumerationer, samt säkerhetsnycklar
clients = {}

# Lagra säkerhetsnycklar kopplade till klienter och ämnen
subscription_keys = {}

background_tasks = {}


def register_client(websocket):
    if websocket not in clients:
        clients[websocket] = set()
        print(f"Client {websocket.remote_address} connected")


def unregister_client(websocket):
    if websocket in clients:
        del clients[websocket]
    # Ta bort säkerhetsnycklar kopplade till klienten
    subscription_keys.pop(websocket, None)
    #print(subscription_keys)
    print(f"Client {websocket.remote_address} disconnected")


async def notify_clients(topic, message):
    for client, subscriptions in clients.items():
        if topic in subscriptions:
            await client.send(message)


def get_new_servers():
    servers = fetch_servers()
    server_list = []
    for server in servers:
        game_id     = server[0]
        game_name   = server[1]
        game_owner  = server[2]
        latitude    = server[3]
        longitude   = server[4]
        is_active   = server[5]
        is_running  = server[6]
        winner      = server[7]
        is_map      = server[8]
        last_modified = server[9]
        created     = server[10]

        player_num = fetch_player_num(game_id)
        player_ids = fetch_player_ids(game_id)
        server_list.append(
            {'game_id': game_id, 'game_name': game_name, 'latitude': latitude, 'longitude': longitude,
             'is_active': is_active, 'is_map': is_map, 'created': created, 'last_modified' : last_modified,
             'game_owner': game_owner, 'number_of_players': player_num, 'player_ids': player_ids})

    message = json.dumps({'servers': server_list, 'type': 'message'})
    return message

async def stream_new_servers(websocket, topic):
    try:
        while True:
            # Generera ett nytt meddelande för varje cykel (kan vara tidsbaserat eller händelsebaserat)
            await notify_clients(topic, get_new_servers())

            # Vänta i 0,3 sekunder innan nästa meddelande skickas
            await asyncio.sleep(0.3)
    except websockets.ConnectionClosed:
        print("Connection closed while streaming")
        unregister_client(websocket)
    except Exception as e:
        print(f"Error while streaming: {e}")
        unregister_client(websocket)


async def stream_waiting_for_players(websocket, topic, game_id):
    try:
        while True:
            count = fetch_player_num(game_id)
            #print(f"player count: {count}")
            await notify_clients(topic, json.dumps({
                "success" : True,
                "player_count": count,
                "player_ids": fetch_player_ids(game_id),
                "is_running": is_game_running(game_id),
                "game_id": game_id,
                "type": "message",
            }))
            await asyncio.sleep(0.2)
    except websockets.ConnectionClosed:
        print("Connection closed while streaming")
        unregister_client(websocket)
    except Exception as e:
        print(f"Error while streaming: {e}")
        unregister_client(websocket)

async def stream_game_status(websocket, topic, game_id, player_ids):
    try:
        while True:
            #print("data skickas inne i stream_game_status", player_ids)

            await notify_clients(topic, json.dumps({"success": True, "type": "message", 'all_game_status': fetch_game_status_all(player_ids, game_id), "winner": get_winner(game_id)}))
            await asyncio.sleep(0.2)
    except websockets.ConnectionClosed:
        print("Connection closed while streaming")
        unregister_client(websocket)
    except Exception as e:
        print(f"Error while streaming: {e}")
        unregister_client(websocket)

async def handle_client(websocket, path):
    register_client(websocket)

    try:
        async for message in websocket:
            data = json.loads(message)
            topic = data["topic"]
            # Prenumeration: Generera och skicka säkerhetsnyckel
            if data["action"] == "subscribe":
                await handle_new_subscribers(topic, websocket)

            # Avprenumeration
            elif data["action"] == "unsubscribe":
                await handle_unsubscription(topic, websocket)

            # Publicering: Kontrollera att klienten skickar rätt säkerhetsnyckel
            elif data["action"] == "publish":
                if topic not in clients[websocket]:
                    await websocket.send(json.dumps({"error": f"Invalid topic {topic}."}))
                    return

                #message_content = data["message"]
                provided_key = data.get("security_key")
                if topic == "new_servers":
                    await handle_new_server_stream(provided_key, topic, websocket)

                elif topic == "waiting_for_server":
                    await handle_waiting_for_server_stream(data, provided_key, topic, websocket)

                elif topic == "stream_game_status":
                    await handle_game_status_stream(data, provided_key, topic, websocket)

    except websockets.ConnectionClosed:
        print("Connection closed while streaming")
    except Exception as e:
        print(f"Error while connecting: {e}")
    finally:
        unregister_client(websocket)


async def handle_unsubscription(topic, websocket):
    if topic in clients[websocket]:
        clients[websocket].remove(topic)
        print(f"Unsubscribed from {topic}")
        await websocket.send(json.dumps({
            "message": f"Unsubscribed from {topic}"
        }))


async def handle_new_subscribers(topic, websocket):
    # TODO: Den här if-satsen borde inte behövas.
    #  Vet inte varför det kommer in en ny uppkoppling direkt efter att man lämnat server sidan
    if websocket not in clients:
        register_client(websocket)
    clients[websocket].add(topic)
    # Skapa en säkerhetsnyckel för denna prenumeration
    security_key = secrets.token_urlsafe(32)
    if websocket not in subscription_keys:
        subscription_keys[websocket] = {}
    subscription_keys[websocket][topic] = security_key
    # Skicka tillbaka säkerhetsnyckeln till klienten
    await websocket.send(json.dumps({
        "message": f"Subscribed to {topic}",
        "type": "sub_auth",
        "topic": topic,
        "security_key": security_key
    }))


async def handle_game_status_stream(data, provided_key, topic, websocket):
    if websocket in subscription_keys and subscription_keys[websocket].get(topic) == provided_key:
        game_id = data["game_id"]
        player_ids = fetch_player_ids(game_id)

        task = asyncio.create_task(stream_game_status(websocket, topic, game_id, player_ids))
        await register_task(task, topic, websocket)
    else:
        print("Invalid security key for publishing.")
        await websocket.send(json.dumps({"error": "Invalid security key for publishing."}))
        clients[websocket].remove(topic)
        await websocket.send(json.dumps({
            "message": f"Unsubscribed from {topic}"
        }))


async def handle_waiting_for_server_stream(data, provided_key, topic, websocket):
    if websocket in subscription_keys and subscription_keys[websocket].get(topic) == provided_key:
        game_id = data["game_id"]

        count = fetch_player_num(game_id)
        await notify_clients(topic, json.dumps({
            "success": True,
            "player_count": count,
            "player_ids": fetch_player_ids(game_id),
            "is_running": is_game_running(game_id),
            "game_id": game_id,
            "type": "message",
        }))
        await asyncio.sleep(2)
        task = asyncio.create_task(stream_waiting_for_players(websocket, topic, game_id))

        await register_task(task, topic, websocket)
    else:
        print("Invalid security key for publishing.")
        await websocket.send(json.dumps({"error": "Invalid security key for publishing."}))
        clients[websocket].remove(topic)
        await websocket.send(json.dumps({
            "message": f"Unsubscribed from {topic}"
        }))


async def handle_new_server_stream(provided_key, topic, websocket):
    # Kontrollera om nyckeln matchar
    if websocket in subscription_keys and subscription_keys[websocket].get(topic) == provided_key:
        # Starta en ström av meddelanden till prenumeranter
        #await notify_clients(topic, get_new_servers())
        #await asyncio.sleep(2)
        task = asyncio.create_task(stream_new_servers(websocket, topic))

        await register_task(task, topic, websocket)
    else:
        print("Invalid security key for publishing.")
        await websocket.send(json.dumps({"error": "Invalid security key for publishing."}))
        clients[websocket].remove(topic)
        await websocket.send(json.dumps({
            "message": f"Unsubscribed from {topic}"
        }))


async def register_task(task, topic, websocket):
    if websocket not in background_tasks:
        background_tasks[websocket] = {}
    background_tasks[websocket][topic] = task
    try:
        await task
    except CancelledError:
        print(f"Task {topic} has been cancelled.")
    finally:
        if not task.done():
            task.cancel()
            print(f"Task {topic} has been cancelled.")


async def main():
    async with websockets.serve(handle_client, "127.0.0.1", 8765):
        try:
            await asyncio.Future()  # Körs för evigt
        except asyncio.CancelledError:
            pass


if __name__ == "__main__":
    asyncio.run(main())

