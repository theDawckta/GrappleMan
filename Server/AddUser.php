<?php
	$db = mysqli_connect("localhost","grappleApp","vSMZ3I3NmZ5lH2e0","grappler");

	// Check connection
	if (mysqli_connect_errno())
	  {
	  echo "Failed to connect to MySQL: " . mysqli_connect_error();
	  }

        //These are our variables.
        //We use real escape string to stop people from injecting. We handle this in Unity too, but it's important we do it here as well in case people extract our url.
        $userName = mysqli_real_escape_string ($db, $_GET['userName'] ?? '');
        $hash = $_GET['hash'] ?? '';

        //This is the polite version of our name
        $politestring = sanitize($userName);

        //This is your key. You have to fill this in! Go and generate a strong one.
        $secretKey="SOMESECRETKEY";

        //We md5 hash our results.
        $expected_hash = md5($userName . $secretKey);

        //If what we expect is what we have:
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
