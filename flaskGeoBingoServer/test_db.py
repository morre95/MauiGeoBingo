import unittest
import sqlite3


class TestSQLQuery(unittest.TestCase):

    def setUp(self):
        # Skapa en in-memory SQLite-databas
        self.connection = sqlite3.connect(':memory:')
        self.cursor = self.connection.cursor()

        # Skapa tabeller
        self.cursor.execute('''
            CREATE TABLE players (
                player_id INTEGER PRIMARY KEY,
                player_name TEXT NOT NULL,
                last_played DATE DEFAULT CURRENT_TIMESTAMP
            )
        ''')

        self.cursor.execute('''
            CREATE TABLE games (
                game_id INTEGER PRIMARY KEY,
                game_name TEXT NOT NULL,
                game_owner INTEGER NOT NULL,
                latitude REAL NOT NULL,
                longitude REAL NOT NULL,
                is_active INTEGER DEFAULT 1 CHECK (is_active IN (0, 1)),
            
                is_running INTEGER DEFAULT 0 CHECK (is_active IN (0, 1)),
            
                winner INTEGER NULL,
            
                is_map INTEGER NOT NULL CHECK (is_map IN (0, 1)),
                last_modified DATE DEFAULT CURRENT_TIMESTAMP,
                created DATE DEFAULT CURRENT_TIMESTAMP
            )
        ''')

        self.cursor.execute('''
            CREATE TABLE markers (
               marker_id INTEGER PRIMARY KEY,
               grid_row INTEGER NOT NULL,
               grid_col INTEGER NOT NULL,
               num INTEGER NOT NULL DEFAULT 0,
               game_id INTEGER NOT NULL
            )
        ''')

        self.cursor.execute('''
            CREATE TABLE players_markers (
               player_id INTEGER,
               marker_id INTEGER,
               PRIMARY KEY (player_id, marker_id),
               FOREIGN KEY (player_id)
                  REFERENCES players (player_id)
                     ON DELETE CASCADE
                     ON UPDATE NO ACTION,
               FOREIGN KEY (marker_id)
                  REFERENCES markers (marker_id)
                     ON DELETE CASCADE
                     ON UPDATE NO ACTION
            )
        ''')

        # Lägg till data
        self.cursor.execute('INSERT INTO games (game_owner, latitude, longitude, game_name, is_active, is_map) VALUES (1, 58.317064, 15.102253, "Test Game", 1, 1)')
        self.cursor.execute('INSERT INTO players (player_name) VALUES ("Player 1")')
        self.cursor.execute('INSERT INTO players (player_name) VALUES ("Player 2")')
        self.cursor.execute('INSERT INTO markers (game_id, grid_row, grid_col, num) VALUES (1, 0, 1, 5)')
        self.cursor.execute('INSERT INTO markers (game_id, grid_row, grid_col, num) VALUES (1, 0, 1, 10)')
        self.cursor.execute('INSERT INTO players_markers (marker_id, player_id) VALUES (1, 1)')
        self.cursor.execute('INSERT INTO players_markers (marker_id, player_id) VALUES (2, 2)')

        self.connection.commit()

    def test_player_table(self):
        query = 'SELECT player_id, player_name FROM players'
        self.cursor.execute(query)
        result = self.cursor.fetchall()
        expected_result = [(1, 'Player 1'), (2, 'Player 2')]

        self.assertEqual(result, expected_result)

    def test_is_highest_number(self):
        # Den SQL-fråga du vill testa
        query = '''
        SELECT
          markers.grid_row,
          markers.grid_col,
          markers.num,
          games.game_name,
          games.is_active,
          games.is_map,
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
        WHERE players.player_id = 1 AND markers.game_id = 1
        GROUP BY markers.marker_id;
        '''

        # Kör frågan
        self.cursor.execute(query)
        result = self.cursor.fetchall()

        # Förväntat resultat: spelaren med player_id 1 ska INTE ha högsta num, så `is_highest_number` ska vara 0
        expected_result = [(0, 1, 5, 'Test Game', 1, 1, 'Player 1', 0)]

        # Kontrollera att resultatet är som förväntat
        self.assertEqual(result, expected_result)



    def tearDown(self):
        # Stäng anslutningen till databasen
        self.connection.close()


if __name__ == '__main__':
    unittest.main()