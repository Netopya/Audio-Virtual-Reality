<?php
	//get the orientation information
	$ori = $_POST["ori"];

	//write the data to the text file
	file_put_contents("ori.txt", $ori);

	//send the data back
	echo $ori;
?>