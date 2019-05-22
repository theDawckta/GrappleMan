# GrappleMan

### Server Setup 
1. Create a new sql database
2. Run the sql command in Server/CreateGrapplerTables.sql to create the tables
3. Deploy the contents of Server/Endpoint to a suitable location
4. Copy Server/env.example.php and rename to env.php, modify the contents of the newly created env.php to match your database credentials, the DB_SECRET_KEY can be whatever you want
5. Deploy env.php to the same location as 3.
