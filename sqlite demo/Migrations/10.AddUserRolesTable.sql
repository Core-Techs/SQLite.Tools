--<UP>
CREATE TABLE UserRoles
(
	UserId INT  NOT NULL,
	"Role" TEXT  NOT NULL,
	PRIMARY KEY (UserId, "Role")
);
--</UP>

--<DOWN>
DROP TABLE UserRoles;
--</DOWN>