import sqlite3

import datetime

def add_new_player(player_name):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "INSERT INTO players(player_name) VALUES(?)"
    cur.execute(sql, (player_name,))
    con.commit()

    return cur.lastrowid

def update_db(player_id, player_name):
    con = sqlite3.connect("database.db")
    cur = con.cursor()

    sql = "UPDATE players SET player_name = ? WHERE player_id = ?"
    cur.execute(sql, (player_name, player_id))
    con.commit()

def update_last_played(player_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()

    sql = "UPDATE players SET last_played = CURRENT_TIMESTAMP WHERE player_id = ?"
    cur.execute(sql, (player_id,))
    con.commit()

def player_id_exists(player_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()

    sql = "SELECT count(*) as num FROM players WHERE player_id = ?"
    res = cur.execute(sql, (player_id,))
    num = res.fetchone()

    return num == 1


def create_game(game_name, game_owner, latitude, longitude, is_map):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "INSERT INTO games (game_name, game_owner, latitude, longitude, is_map) VALUES(?, ?, ?, ?, ?)"

    cur.execute(sql, (game_name, game_owner, latitude, longitude, is_map))
    con.commit()
    return cur.lastrowid

def fetch_servers():
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT * FROM games WHERE is_active = 1"
    res = cur.execute(sql)
    return res.fetchall()

def fetch_servers_earlier(date = None):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT * FROM games WHERE is_active = 1 AND last_modified > ?"

    if date is None: date = datetime.date.min

    res = cur.execute(sql, (date,))
    return res.fetchall()

def insert_player_game(game_id, player_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()

    sql = "SELECT count(*) as num FROM games_players WHERE game_id = ? AND player_id = ?"
    res = cur.execute(sql, (game_id, player_id))
    num = res.fetchone()[0]

    if num > 0 :
        return None

    sql = "UPDATE games SET last_modified = CURRENT_TIMESTAMP WHERE game_id = ?"
    cur.execute(sql, (game_id,))
    con.commit()

    sql = "INSERT INTO games_players (game_id, player_id) VALUES(?, ?)"
    cur.execute(sql, (game_id, player_id))

    con.commit()
    return cur.lastrowid

def fetch_player_num(game_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT count(player_id) AS num FROM games_players WHERE game_id = ?"

    res = cur.execute(sql, (game_id,))
    return res.fetchone()[0]

def fetch_player_ids(game_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT player_id FROM games_players WHERE game_id = ?"

    res = cur.execute(sql, (game_id,))
    return [x[0] for x in res.fetchall()]

def fetch_player_name(player_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT COUNT(*) AS num, player_name FROM players WHERE player_id = ?"

    res = cur.execute(sql, (player_id,))
    row = res.fetchone()
    if row[0] <= 0:
        return None
    return row[1]

def is_game_running(game_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT is_running FROM games WHERE game_id = ?"

    res = cur.execute(sql, (game_id,))

    return res.fetchone()[0] == 1

def remove_game(game_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    #sql = "DELETE FROM games WHERE game_id = ?"
    sql = "UPDATE games SET is_active = 0 WHERE game_id = ?"
    cur.execute(sql, (game_id,))
    con.commit()

    return cur.rowcount > 0

def update_marker_db(game_id, player_id, grid_row, grid_col, num):
    con = sqlite3.connect("database.db")
    cur = con.cursor()

    sql = """SELECT markers.marker_id
    FROM markers
    JOIN players_markers
      ON markers.marker_id = players_markers.marker_id
    WHERE players_markers.player_id = ? AND markers.game_id = ? AND markers.grid_row = ? AND markers.grid_col = ?"""

    cur.execute(sql, (player_id, game_id, grid_row, grid_col))
    rows = cur.fetchall()
    updated = False
    for row in rows:
        marker_id = row[0]
        sql = "UPDATE markers SET num = ? WHERE marker_id = ?"
        cur.execute(sql, (num, marker_id))
        con.commit()
        updated = True
        break

    if not updated:
        sql = "INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(?, ?, ?, ?)"
        cur.execute(sql, (grid_row, grid_col, num, game_id))
        con.commit()

        marker_id = cur.lastrowid

        sql = "INSERT INTO players_markers(player_id, marker_id) VALUES(?, ?)"
        cur.execute(sql, (player_id, marker_id))
        con.commit()

    sql = "UPDATE games SET last_modified = CURRENT_TIMESTAMP WHERE game_id = ?"
    cur.execute(sql, (game_id,))
    con.commit()

def set_winner(game_id, player_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "UPDATE games SET winner = ?, is_active = 0, last_modified = CURRENT_TIMESTAMP WHERE game_id = ?"

    cur.execute(sql, (player_id, game_id))
    con.commit()

    return cur.rowcount > 0

def get_winner(game_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "SELECT winner FROM games WHERE game_id = ?"
    cur.execute(sql, (game_id,))

    winner = cur.fetchone()[0]
    return winner if winner is not None else None

def set_game_as_running(game_id):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "UPDATE games SET is_running = 1 WHERE game_id = ?"
    cur.execute(sql, (game_id,))
    con.commit()
    return cur.rowcount > 0

def update_game_name(game_id, name):
    con = sqlite3.connect("database.db")
    cur = con.cursor()
    sql = "UPDATE games SET game_name = ? WHERE game_id = ?"
    cur.execute(sql, (name, game_id))
    con.commit()
    return cur.rowcount > 0

def fetch_game_status(player_id, game_id):
    con = sqlite3.connect("database.db")
    con.row_factory = sqlite3.Row
    cur = con.cursor()
    sql = """SELECT
      markers.grid_row,
      markers.grid_col,
      markers.num,
      games.game_name,
      games.is_active,
      games.is_map,
      games.winner,
      players.player_name,
      CASE WHEN players_markers.player_id = (
        SELECT sub_players_markers.player_id
        FROM markers AS sub_markers
        JOIN players_markers AS sub_players_markers
          ON sub_markers.marker_id = sub_players_markers.marker_id
        WHERE sub_markers.game_id = markers.game_id
          AND sub_markers.grid_row = markers.grid_row
          AND sub_markers.grid_col = markers.grid_col
        ORDER BY sub_markers.num DESC
        LIMIT 1
      ) THEN 1 ELSE 0 END AS is_highest_number
    FROM markers
    JOIN players_markers
      ON markers.marker_id = players_markers.marker_id
    JOIN players
      ON players.player_id = players_markers.player_id
    JOIN games
      ON games.game_id = markers.game_id
    WHERE players.player_id = ? AND markers.game_id = ?
    GROUP BY markers.marker_id;"""

    res = cur.execute(sql, (player_id, game_id))
    return [dict(row) for row in res.fetchall()]

def fetch_game_status_all(player_ids, game_id):
    return [{player_id: fetch_game_status(player_id, game_id)} for player_id in player_ids]