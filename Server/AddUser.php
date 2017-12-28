<?php 
	$db = mysqli_connect("localhost","root","","grappler");

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
function no_naughty($string)
{
    $string = preg_replace('/shit/i', 'shoot', $string);
    $string = preg_replace('/fuck/i', 'fool', $string);
    $string = preg_replace('/asshole/i', 'animal', $string);
    $string = preg_replace('/bitches/i', 'dogs', $string);
    $string = preg_replace('/bitch/i', 'dog', $string);
    $string = preg_replace('/bastard/i', 'plastered', $string);
    $string = preg_replace('/nigger/i', 'newbie', $string);
    $string = preg_replace('/cunt/i', 'corn', $string);
    $string = preg_replace('/cock/i', 'rooster', $string);
    $string = preg_replace('/faggot/i', 'piglet', $string);
    $string = preg_replace('/suck/i', 'rock', $string);
    $string = preg_replace('/dick/i', 'deck', $string);
    $string = preg_replace('/crap/i', 'rap', $string);
    $string = preg_replace('/blows/i', 'shows', $string);
    // ie does not understand "&apos;" &#39; &rsquo;
    $string = preg_replace("/'/i", '&rsquo;', $string);
    $string = preg_replace('/%39/i', '&rsquo;', $string);
    $string = preg_replace('/&#039;/i', '&rsquo;', $string);
    $string = preg_replace('/&039;/i', '&rsquo;', $string);
    $string = preg_replace('/"/i', '&quot;', $string);
    $string = preg_replace('/%34/i', '&quot;', $string);
    $string = preg_replace('/&034;/i', '&quot;', $string);
    $string = preg_replace('/&#034;/i', '&quot;', $string);
    // these 3 letter words occur commonly in non-rude words...
    //$string = preg_replace('/fag', 'pig', $string);
    //$string = preg_replace('/ass', 'donkey', $string);
    //$string = preg_replace('/gay', 'happy', $string);
    return $string;
}
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
    // make sure it isn't waaaaaaaay too long
    $MAX_LENGTH = 250; // bytes per chat or text message - fixme?
    $string = substr($string, 0, $MAX_LENGTH);
    $string = no_naughty($string);
    // breaks apos and quot: // $string = htmlentities($string,ENT_QUOTES);
    // useless since the above gets rid of quotes...
    //$string = str_replace("'","&rsquo;",$string);
    //$string = str_replace("\"","&rdquo;",$string);
    //$string = str_replace('#','&pound;',$string); // special case
    $string = my_utf8($string);
    $string = safe_typing($string);
    return trim($string);
}
?>