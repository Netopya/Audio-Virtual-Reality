<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8">
    <title>V 4</title>
    <script>
    	var alpha = 0;
		var beta = 0;
		var gamma = 0;

		var delay = 100;

		//read and save the orientation information
		window.ondeviceorientation = function(event) {
			alpha = Math.round(event.alpha);
			beta = Math.round(event.beta);
			gamma = Math.round(event.gamma);
		}

		//AJAX request to send information to the server
		function processOritentation() {
			

			if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari
			  xmlhttp=new XMLHttpRequest();
			} else {// code for IE6, IE5
			  xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
			}
			xmlhttp.onreadystatechange=function()
			{
			  if (xmlhttp.readyState==4 && xmlhttp.status==200) 
			  {
			  	
			  	
			  	document.getElementById("errors").innerHTML = "Response:" + xmlhttp.responseText;
			  	//send the information again once a response was received
			  	setTimeout(processOritentation(),delay);
			  }
			}

			//send the alpha (heading) information to the server
			xmlhttp.open("POST","writeOrientation.php",true);
			xmlhttp.setRequestHeader("Content-type","application/x-www-form-urlencoded");
			xmlhttp.send("ori=" + alpha);
		} 

		//write the current heading value on the page
		setInterval(function () {
			document.getElementById("orientation").innerHTML = alpha;
		},delay);

		

    </script>
  </head>
  <body onload="processOritentation()">

  	<span id="orientation"></span>
  	<div id="errors"></div>
    <!-- page content -->
  </body>
</html>