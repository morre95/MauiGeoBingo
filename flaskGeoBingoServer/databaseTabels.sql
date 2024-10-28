/* sqlite3 database.db */
/* .tables */

DROP TABLE players_markers;
DROP TABLE markers;
DROP TABLE games_players;
DROP TABLE games;
DROP TABLE players;


CREATE TABLE IF NOT EXISTS players (
	player_id INTEGER PRIMARY KEY,
	player_name TEXT NOT NULL,
    last_played DATE DEFAULT CURRENT_TIMESTAMP
);

/*ALTER TABLE players DROP COLUMN player_position;*/

INSERT INTO players (player_name) VALUES('player 1');
INSERT INTO players (player_name) VALUES('Kalle');
INSERT INTO players (player_name) VALUES('player 3');

SELECT * FROM players;


CREATE TABLE IF NOT EXISTS games (
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
);

INSERT INTO games (game_name, game_owner, latitude, longitude, is_map) VALUES('game 1', 1, 58.317064, 15.102253, 0);
INSERT INTO games (game_name, game_owner, latitude, longitude, is_map, last_modified) VALUES('game 4', 4, 58.317064, 15.102253, 0, CURRENT_TIMESTAMP);

UPDATE games SET game_name = "game 4" WHERE game_id = 3;
UPDATE games SET game_name = "jag har ändrat dig" WHERE game_id = 3;
INSERT INTO games_players (game_id, player_id) VALUES(3, 5);

SELECT * FROM games;

/* FÖR att häta ut ett värde som är nyare än det senaste hämtade värdet */
SELECT * FROM games WHERE created > '2024-09-28 07:28:42';


CREATE TABLE IF NOT EXISTS games_players (
   game_id INTEGER,
   player_id INTEGER,
   PRIMARY KEY (player_id, game_id),
   FOREIGN KEY (player_id)
      REFERENCES players (player_id)
         ON DELETE CASCADE
         ON UPDATE NO ACTION,
   FOREIGN KEY (game_id)
      REFERENCES games (game_id)
         ON DELETE CASCADE
         ON UPDATE NO ACTION
);

INSERT INTO games_players (game_id, player_id) VALUES(1, 1);
INSERT INTO games_players (game_id, player_id) VALUES(1, 2);
INSERT INTO games_players (game_id, player_id) VALUES(1, 3);

SELECT * FROM games_players;


CREATE TABLE IF NOT EXISTS markers (
   marker_id INTEGER PRIMARY KEY,
   grid_row INTEGER NOT NULL,
   grid_col INTEGER NOT NULL,
   num INTEGER NOT NULL DEFAULT 0,
   game_id INTEGER NOT NULL
);


/* player 1 markers for game 1 */
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 0, 0, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 1, 4, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 2, 8, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 3, 1, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 0, -2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 1, 5, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 2, 1, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 3, -2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 0, 0, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 1, 2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 2, -2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 3, 3, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 0, 1, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 1, -2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 2, 2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 3, 2, 1);


/* player 2 markers for game 1 */
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 0, 1, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 1, 3, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 2, 4, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(0, 3, 3, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 0, 1, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 1, 4, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 2, -2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(1, 3, -3, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 0, 0, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 1, 0, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 2, 0, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(2, 3, 5, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 0, -2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 1, 10, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 2, 2, 1);
INSERT INTO markers(grid_row, grid_col, num, game_id) VALUES(3, 3, 2, 1);


SELECT * FROM markers;
SELECT count(*) as num FROM markers;

CREATE TABLE IF NOT EXISTS players_markers (
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
);

/* player 1 markers */
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 1);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 2);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 3);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 4);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 5);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 6);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 7);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 8);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 9);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 10);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 11);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 12);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 13);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 14);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 15);
INSERT INTO players_markers(player_id, marker_id) VALUES(1, 16);


/* player 2 markers */
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 17);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 18);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 19);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 20);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 21);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 22);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 23);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 24);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 25);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 26);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 27);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 28);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 29);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 30);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 31);
INSERT INTO players_markers(player_id, marker_id) VALUES(2, 32);


SELECT * FROM players_markers;
SELECT count(*) as num FROM players_markers;


SELECT
  markers.grid_row,
  markers.grid_col,
  markers.num,
  games.game_name,
  games.is_active,
  games.is_map,
  players.player_name
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
JOIN players
  ON players.player_id = players_markers.player_id
JOIN games
  ON games.game_id = markers.game_id
WHERE players.player_id = 1 AND markers.game_id = 1
GROUP BY markers.marker_id;

/* ska ge 2|4 som svar */
SELECT markers.marker_id, markers.num
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
WHERE players_markers.player_id = 1 AND markers.game_id = 1 AND markers.grid_row = 0 AND markers.grid_col = 1;



/* Mitt försök */
SELECT players_markers.player_id, markers.game_id, (SELECT MAX(markers.num) FROM markers WHERE markers.game_id = 1 AND markers.grid_row = 0 AND markers.grid_col = 1)
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
WHERE players_markers.player_id = 1 AND markers.game_id = 1 AND markers.grid_row = 0 AND markers.grid_col = 1;


/* ChatGPT hjälpte mig att skapa denna fråga här https://chatgpt.com/share/670a0905-9c4c-8010-bc21-fcb6aa1a6efd */
SELECT players_markers.player_id, markers.game_id, (
    SELECT players_markers.player_id
    FROM markers
    JOIN players_markers
      ON markers.marker_id = players_markers.marker_id
    WHERE markers.game_id = 1
      AND markers.grid_row = 0
      AND markers.grid_col = 1
      AND markers.num = (
        SELECT MAX(markers.num)
        FROM markers
        WHERE markers.game_id = 1
          AND markers.grid_row = 0
          AND markers.grid_col = 1
    )
)
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
WHERE players_markers.player_id = 1 AND markers.game_id = 1 AND markers.grid_row = 0 AND markers.grid_col = 1;


/* Sen la jag bara till en case when sats för att få ut vilken spelare som har högsta poäng på respektive ruta */
SELECT players_markers.player_id, markers.game_id, CASE WHEN (
    SELECT players_markers.player_id
    FROM markers
    JOIN players_markers
      ON markers.marker_id = players_markers.marker_id
    WHERE markers.game_id = 1
      AND markers.grid_row = 0
      AND markers.grid_col = 1
      AND markers.num = (
        SELECT MAX(markers.num)
        FROM markers
        WHERE markers.game_id = 1
          AND markers.grid_row = 0
          AND markers.grid_col = 1
    )
) = 1 THEN 1 ELSE 0 END AS is_highest_number
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
WHERE players_markers.player_id = 1 AND markers.game_id = 1 AND markers.grid_row = 0 AND markers.grid_col = 1;



SELECT
  markers.grid_row,
  markers.grid_col,
  markers.num,
  games.game_name,
  games.is_active,
  games.is_map,
  players.player_name,
  CASE WHEN (
    SELECT players_markers.player_id
    FROM markers
    JOIN players_markers
      ON markers.marker_id = players_markers.marker_id
    WHERE markers.game_id = 1
      AND markers.grid_row = 0
      AND markers.grid_col = 1
      AND markers.num = (
        SELECT MAX(markers.num)
        FROM markers
        WHERE markers.game_id = 1
          AND markers.grid_row = 0
          AND markers.grid_col = 1
    )
  ) = 1 THEN 1 ELSE 0 END AS is_highest_number
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
JOIN players
  ON players.player_id = players_markers.player_id
JOIN games
  ON games.game_id = markers.game_id
WHERE players.player_id = 1 AND markers.game_id = 1
GROUP BY markers.marker_id;


/* Den här fungerar inte riktigt. Resultatet blir alltid 1 för is_highest_number */
SELECT
  markers.grid_row,
  markers.grid_col,
  markers.num,
  games.game_name,
  games.is_active,
  games.is_map,
  players.player_name,
  CASE WHEN (
    SELECT players_markers.player_id
    FROM markers AS sub_markers
    JOIN players_markers AS sub_players_markers
      ON sub_markers.marker_id = sub_players_markers.marker_id
    WHERE sub_markers.game_id = markers.game_id
      AND sub_markers.grid_row = markers.grid_row
      AND sub_markers.grid_col = markers.grid_col
      AND sub_markers.num = (
        SELECT MAX(sub_markers_inner.num)
        FROM markers AS sub_markers_inner
        WHERE sub_markers_inner.game_id = markers.game_id
          AND sub_markers_inner.grid_row = markers.grid_row
          AND sub_markers_inner.grid_col = markers.grid_col
      )
  ) = players_markers.player_id THEN 1 ELSE 0 END AS is_highest_number
FROM markers
JOIN players_markers
  ON markers.marker_id = players_markers.marker_id
JOIN players
  ON players.player_id = players_markers.player_id
JOIN games
  ON games.game_id = markers.game_id
WHERE players.player_id = 1 AND markers.game_id = 1
GROUP BY markers.marker_id;


/* Ett andra försöka från chatGPT */
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
WHERE players.player_id = 4 AND markers.game_id = 12
GROUP BY markers.marker_id;




