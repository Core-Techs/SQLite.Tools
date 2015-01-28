To make changes to the database add a migration script to this folder.
The script's build action must be Embedded Resource.
The script's file name must start with a version number. Examples:

Name					Version
-----------------------------------------
1.Add Some Table.sql	1
1.1 Add index.slq		1.1
2.0.123.sql				2.0
3anything.sql			3
-----------------------------------------

Scripts are ran in order of their version number.
The script will only be ran when it's version is greater than the last
successfully ran migration script's version.

Before the migration process a backup will be created beside the original
database file. If migration succeeds, the backup is deleted. 

When a migration fails, the backup is not deleted and the error is recorded
in the Migrations table, which holds a history of all database migrations.

Sqlite has a limited alter table syntax. The process of changing a table's
schema is described in great detail here:
https://www.sqlite.org/lang_altertable.html

It's very important that you follow the guidance on that page when altering a
table.