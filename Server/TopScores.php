<?php
    $db = mysqli_connect("localhost","root","","grappler");

	// Check connection
	if (mysqli_connect_errno())
	  {
	  echo "Failed to connect to MySQL: " . mysqli_connect_error();
	  }
 
     //This query grabs the top 10 scores, sorting by score and timestamp.
    $query = "SELECT * FROM Scores ORDER by score DESC, ts ASC LIMIT 10";
    $result = mysqli_query($db, $query) or die(mysqli_error($db));
 
    //We find our number of rows
    $result_length = mysqli_num_rows($result); 
    
    //And now iterate through our results
    for($i = 0; $i < $result_length; $i++)
    {
         $row = mysqli_fetch_array($result);
         echo $row['name'] . "\t" . $row['score'] . "\n"; // And output them
    }
?>