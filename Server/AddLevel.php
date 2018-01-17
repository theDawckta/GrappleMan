<?php
	$db = mysqli_connect("localhost","root","grapplepass","grappler");

	if (mysqli_connect_errno())
	{
		echo "Failed to connect to MySQL: " . mysqli_connect_error();
	}

	$levelName = mysqli_real_escape_string ($db, $_GET['levelName'] ?? '');
	$hash = $_GET['hash'] ?? '';
	$secretKey="SOMESECRETKEY";
    $expected_hash = md5($levelName . $secretKey);

    if($expected_hash == $hash)
	{
		$checkLevelNameQuery = "SELECT * FROM Levels WHERE LevelName = '$levelName';";
		$checkLevelNameResult = mysqli_query($db, $checkLevelNameQuery) or die(mysqli_error($db));
		if(mysqli_num_rows($checkLevelNameResult) >= 1)
		{
			echo("");
		}
		else
		{
			$query = "INSERT INTO Levels (LevelName) VALUES ('$levelName');";
			$result = mysqli_query($db, $query) or die(mysqli_error($db));
			echo ($levelName);
		}
	}
	else
	{
		echo('Hash Failed');
	}
?>
