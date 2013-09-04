function reorient(e) {
	scrollTo(0,1);
	var portrait = (window.orientation % 180 == 0);
	$("#main-container-p").css("display", (portrait?"block":"none"));
	$("#main-container-l").css("display", (portrait?"none":"block"));
}

function getLocation() {
	$.getJSON('response.php?type=locator', function(data) {
		var x = data[0]["data"].split(",")[0]; // 480
		var y = data[0]["data"].split(",")[1]; // 290
		x = Math.round(x * 480 / 1000);
		x = 480 - x;
		y = Math.round(y * 290 / 1000);
		$("#avatar").show();
		$("#avatar").offset({top: y, left: x});
	});
}

$(document).ready(function() {
	setInterval(getLocation, 250);
	
	reorient();
	
	$(window).bind( 'orientationchange', reorient);

	setTimeout('scrollTo(0,1)',1000);
	window.setTimeout(reorient, 0);
	
	$("#main-container-l").click(function(e) {
		getLocation();
		me = $("#main-container-l");
		x = Math.round(e.pageX * 1000 / (parseInt(me.css("width"))  + 4));
		y = Math.round(e.pageY * 1000 / (parseInt(me.css("height")) + 4));
		$("#click-response").html("X: " + x + " -- Y: " + y);
		$('#click-response').load('callback.php?type=locator&pageX=' + x + '&pageY=' + y);
	});
	
	$("#quit-button").click(function(){
		window.location = './?action=drop_player';
	});
	
	$(".color-picker").each(function() {
		$(this).css("background-color", $(this).attr("id"));
	})
	
	$(".color-picker").click(function() {
		$("#click-response").load("callback.php?type=color&color=" + $(this).attr("id"));
		$("#avatar").css("background-color", $(this).attr("id"));
	})
});