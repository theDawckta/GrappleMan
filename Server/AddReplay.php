<?php
	$db = mysqli_connect("localhost","grappleApp","vSMZ3I3NmZ5lH2e0","grappler");

	// Check connection
	if (mysqli_connect_errno()){
		echo "Failed to connect to MySQL: " . mysqli_connect_error();
	}

	$hash = $_GET['hash'] ?? '';
	$userName = mysqli_real_escape_string ($db, $_GET['userName'] ?? '');
	$levelName = mysqli_real_escape_string ($db, $_GET['levelName'] ?? '');
	$replayTime = mysqli_real_escape_string ($db, $_GET['replayTime'] ?? '');
	$replayData = file_get_contents('php://input' ?? '');
	$politestring = sanitize($userName);
	$secretKey="SOMESECRETKEY";
	$expected_hash = md5($userName . $secretKey);

	if($expected_hash == $hash) {
		$getUserIdQuery = "SELECT Id FROM Users WHERE UserName = '$politestring';";
		$getUserIdResult = mysqli_query($db, $getUserIdQuery) or die(mysqli_error($db));

		$getLevelIdQuery = "SELECT Id FROM Levels WHERE LevelName = '$levelName';";
		$getLevelIdResult = mysqli_query($db, $getLevelIdQuery) or die(mysqli_error($db));

		if(mysqli_num_rows($getUserIdResult) == 1 && mysqli_num_rows($getLevelIdResult) == 1){
			$userId = mysqli_fetch_assoc($getUserIdResult)['Id'];
			$levelId = mysqli_fetch_assoc($getLevelIdResult)['Id'];

			$query = "INSERT INTO Replays (UserId, LevelId, ReplayTime, ReplayData)VALUES ($userId[0], $levelId[0], $replayTime, '$replayData');";
			echo($query);
			$result = mysqli_query($db, $query) or die(mysqli_error($db));
		}
		else{
			if(mysqli_num_rows($getUserIdResult) <= 0){
				echo($getUserIdResult.test);
			}
			else if(mysqli_num_rows($getLevelIdResult) <= 0){
				echo($getLevelIdResult.test);
			}
		}
	}
	else{
		echo('Hash Failed');
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
