# GrappleMan
An endless runner that has the user competing against anyone that has the app installed.

## Platforms
PC, iOS, Android

## Unity Specifics
Unity ver. 2019.2.15f1

## Server Setup 
1. Create a new sql database
2. Run the sql command in Server/CreateGrapplerTables.sql to create the tables
3. Deploy the contents of Server/Endpoint to a suitable location
4. Copy Server/Endpoint/env.example.php and rename to env.php, modify the contents of the newly created env.php to match your database credentials, the DB_SECRET_KEY can be whatever you want
5. Deploy env.php to the same location as 3.

### Controls for pc
RMB will thow out a grappling hook
"E" will raise you up the rope when hooked
RMB while hooked will hop you in the direction of the mouse pointer and pull the hook back

### Controls for mobile
Swiping will thow out a grappling hook
Touching the screen while hooked will raise you up the rope
Swiping after while hooked will hop you in the direction of the mouse pointer and pull the hook back
