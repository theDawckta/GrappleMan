<?php
	$db = mysqli_connect("localhost","root","grapplepass","grappler");

	if (mysqli_connect_errno())
	{
		echo "Failed to connect to MySQL: " . mysqli_connect_error();
	}

    $userName = mysqli_real_escape_string ($db, $_GET['userName'] ?? '');
    $hash = $_GET['hash'] ?? '';
    $politestring = sanitize($userName);
    $secretKey="SOMESECRETKEY";
    $expected_hash = md5($userName . $secretKey);

    if($expected_hash == $hash)
	{
		$checkUserNameQuery = "SELECT * FROM USERS WHERE UserName = '$politestring';";
		$checkUserNameResult = mysqli_query($db, $checkUserNameQuery) or die(mysqli_error($db));
		if(mysqli_num_rows($checkUserNameResult)>=1)
		{
			echo("");
		}
		else
		{
			$query = "INSERT INTO Users (UserName) VALUES ('$politestring');";
			$result = mysqli_query($db, $query) or die(mysqli_error($db));
			echo ($userName);
		}
    }
    else
	{
		echo('Hash Failed');
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
