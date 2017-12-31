<?php
  $db = mysqli_connect("localhost","grappleApp","nnoRMwtSXoHInbKu","grappler");

  // Check connection
  if (mysqli_connect_errno())
  {
    echo "Failed to connect to MySQL: " . mysqli_connect_error();
  }
  $levelName = mysqli_real_escape_string ($db, $_GET['levelName'] ?? '');
  $hash = $_GET['hash'] ?? '';
  $numOfReplays = $_GET['numOfReplays'] ?? '';

  $politestring = sanitize($levelName);
  $secretKey="SOMESECRETKEY";
  $expected_hash = md5($levelName . $secretKey);

  if($expected_hash == $hash)
  {
    $query = "SELECT Users.UserName, ReplayTime, ReplayData FROM Replay
    INNER JOIN Users ON Users.Id = Replay.UserId
    ORDER BY ReplayTime ASC LIMIT $numOfReplays;";
    $result = mysqli_query($db, $query) or die(mysqli_error($db));
    $rows = array();
    while($resultRow = mysqli_fetch_array($result, MYSQLI_ASSOC)) {
      $rows[] = $resultRow;
    }
      echo(json_encode($rows)) ;
  }
  else {
    echo("HASH CHECK HAS FAILED");
  }

  /////////////////////////////////////////////////
  // string sanitize functionality to avoid
  // sql or html injection abuse and bad words
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
