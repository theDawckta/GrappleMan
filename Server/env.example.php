<?php
  $variables = [
      'DB_HOST' => 'localhost',
      'DB_USERNAME' => 'root',
      'DB_PASSWORD' => 'root',
      'DB_NAME' => 'grappler',
	  'DB_SECRET_KEY' => '123456789',
  ];
  foreach ($variables as $key => $value) {
      putenv("$key=$value");
  }
?>