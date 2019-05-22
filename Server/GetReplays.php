<?php
    include "autoload.php";

	$db = mysqli_connect(env('DB_HOST'),env('DB_USERNAME'),env('DB_PASSWORD'),env('DB_NAME'));

    if (mysqli_connect_errno())
    {
        echo "Failed to connect to MySQL: " . mysqli_connect_error();
    }
  
    $levelName = mysqli_real_escape_string ($db, $_GET['levelName'] ?? '');
    $hash = $_GET['hash'] ?? '';
    $numOfReplays = $_GET['numOfReplays'] ?? '';
    $politestring = sanitize($levelName);
    $secretKey="d41d8cd98f00b204e9800998ecf8427e";
    $expected_hash = md5($levelName . $secretKey);

    if($expected_hash == $hash)
    {
        $query = "SELECT Users.UserName, Levels.LevelName, ReplayTime, ReplayData FROM Replays
        INNER JOIN Users ON Users.Id = Replays.UserId
        INNER JOIN Levels ON Levels.Id = Replays.LevelId
        WHERE Levels.LevelName = '$levelName'
        ORDER BY ReplayTime ASC LIMIT $numOfReplays;";
        $result = mysqli_query($db, $query) or die(mysqli_error($db));
        $rows = array();
        while($resultRow = mysqli_fetch_array($result, MYSQLI_ASSOC)) {
            $rows[] = $resultRow;
        }
        echo(json_encode($rows, JSON_UNESCAPED_SLASHES)) ;
    }
    else 
    {
        echo("HASH CHECK HAS FAILED");
    }

    /////////////////////////////////////////////////
    // string sanitize functionality to avoid
    // sql or html injection abuse
    /////////////////////////////////////////////////
    function my_utf8($string)
    {
        return strtr($string,
        "/<>������������ ��Ց������������������������������ԕ���ٞ��������",
        "![]YuAAAAAAACEEEEIIIIDNOOOOOOUUUUYsaaaaaaaceeeeiiiionoooooouuuuyy");
    }
    function safe_typing($string)
    {
        return preg_replace("/[^a-zA-Z0-9 \!\@\%\^\&\*\.\*\?\+\[\]\(\)\{\}\^\$\:\;\,\-\_\=]/", "", $string);
    }
    function sanitize($string)
    {
        $MAX_LENGTH = 250;
        $string = substr($string, 0, $MAX_LENGTH);
        $string = my_utf8($string);
        $string = safe_typing($string);
        return trim($string);
    }
?>
