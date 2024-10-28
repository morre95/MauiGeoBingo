from flask import Flask, request, jsonify
import json

from commons import *

app = Flask(__name__)

# TODO: Kolla i databasen så att inte finns någon som vunnit
# TODO: Kolla i databasen så att det inte är fler än 4 spelare på samma spel

# https://pythonbasics.org/flask-rest-api/
# https://docs.python.org/3/library/sqlite3.html
@app.route('/', methods=['GET'])
def get():
    args = request.args
    game_id = args.get('game_id')
    print(game_id)

    return jsonify({'Name': 'Johan', 'PlayerId': 4, 'Method': 'Get', 'UserId': 2})

@app.route('/get/game/status/<player_id>/<game_id>', methods=['GET'])
def get_game_satus(player_id, game_id):
    try:
        return jsonify({"success": True, 'game_status': fetch_game_status(int(player_id), int(game_id))})
    except ValueError:
        return jsonify({"success": False, 'game_status': []})

@app.route('/get/game/status/all/<player_ids>/<game_id>', methods=['GET'])
def get_game_satus_all(player_ids, game_id):
    try:
        player_ids = list(map(int, player_ids.split(",")))
        return jsonify({"success": True, 'all_game_status': fetch_game_status_all(player_ids, int(game_id)), "winner": get_winner(game_id)})
    except ValueError:
        return jsonify({"success": False, 'game_status': []})

# FIXME: ful lösning för att jag inte hittar varför det inte finns några spelare på android när man går till buttonspage från serverpage
@app.route('/get/game/players/<game_id>', methods=['GET'])
def get_game_players(game_id):
    try:
        return jsonify({"success": True, 'player_ids': fetch_player_ids(int(game_id))})
    except ValueError:
        return jsonify({"success": False})


@app.route('/player/name', methods=['GET'])
def get_name():
    user = request.args
    player_id = user.get('player_id', type=int)
    return jsonify({'player_name': fetch_player_name(player_id)})

@app.route('/update/player', methods=['POST'])
def update_player():
    user = json.loads(request.data)
    #print(user)
    #print(user.get('player_name'))
    #print(user.get('player_id'))
    player_name = user.get('player_name')
    player_id = user.get('player_id')

    if player_id == 0:
        return jsonify(None)

    update_db(player_id, player_name)
    #print(jsonify({'player_id': player_id, 'player_name': player_name}))
    return jsonify({'player_id': player_id, 'player_name': player_name})


@app.route('/new/player', methods=['PUT'])
def new_player():
    user = json.loads(request.data)
    player_id = user.get('player_id')
    if player_id != 0 and player_id_exists(player_id):
        return jsonify(None)

    player_name = 'Player 1'
    if user.get('player_name') != '':
        player_name = user.get('player_name')

    player_id = add_new_player(player_name)
    return jsonify({'player_id': player_id, 'player_name': player_name})

@app.route('/new/game', methods=['PUT'])
def new_game():
    game = json.loads(request.data)
    #print(game)
    game_name = game.get('game_name')
    game_owner = game.get('game_owner')
    latitude = game.get('latitude')
    longitude = game.get('longitude')
    is_map = 1 if game.get('is_map') == True else 0 #game.get('is_map') == True ? 1 : 0

    #print(latitude)
    #print(longitude)

    game_id = create_game(game_name, game_owner, latitude, longitude, is_map)
    return jsonify({'game_id': game_id})


@app.route('/update/game', methods=['POST'])
def update_game():
    game = json.loads(request.data)
    player_id = game.get('player_id')
    game_id = game.get('game_id')
    row = game.get('grid_row')
    col = game.get('grid_col')
    num = game.get('num')
    winning_move = game.get('winning_move')

    print(game)

    update_marker_db(game_id, player_id, row, col, num)

    update_last_played(player_id)

    if winning_move:
        set_winner(game_id, player_id)

    return jsonify({"success": True, "winner": get_winner(game_id)})


@app.route('/edit/game/name', methods=['POST'])
def edit_game_name():
    game = json.loads(request.data)
    game_id = game.get('game_id')
    game_name = game.get('game_name')
    #print("game id": game_id, "game name", game_name)
    return jsonify({"success": update_game_name(game_id, game_name)})

@app.route('/add/player/to/game', methods=['PUT'])
def add_plater_to_game():
    game = json.loads(request.data)
    player_id = game.get('player_id')
    game_id = game.get('game_id')
    #print(f"add_plater_to_game() player id: {player_id}, game id: {game_id}")
    return jsonify({"success": True}) if insert_player_game(game_id, player_id) is not None else jsonify({"success": False})

@app.route("/set/game/as/running", methods=['POST'])
def set_game():
    game = json.loads(request.data)
    game_id = game.get('game_id')

    return jsonify({"success": set_game_as_running(game_id)})

@app.route("/set/game/winner", methods=['POST'])
def set_game_winner():
    game = json.loads(request.data)
    game_id = game.get('game_id')
    player_id = game.get('player_id')

    return jsonify({"success": set_winner(game_id, player_id)})

@app.route('/delete/servers/<game_id>', methods=['DELETE'])
def delete_servers(game_id):
    try:
        return jsonify({"success": True}) if remove_game(int(game_id)) else jsonify({"success": False})
    except ValueError:
        return jsonify({"success": False})

if __name__ == '__main__':
    app.run(debug=True)
