--<UP>
CREATE TABLE Users
(
	"Id" INTEGER PRIMARY KEY NOT NULL,
	UserName TEXT NOT NULL,
	Email TEXT NULL,
	BirthDate TEXT NOT NULL
);

CREATE VIEW UsersView AS
	SELECT *,
		(strftime('%Y', 'now') - strftime('%Y', BirthDate)) - (strftime('%m-%d', 'now') < strftime('%m-%d', BirthDate)) AS "Age"
	FROM Users;

--</UP>

--<DOWN>
DROP VIEW UsersView;
DROP TABLE USERS;
--</DOWN>