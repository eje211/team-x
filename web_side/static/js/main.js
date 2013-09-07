function reorient(e) {
	scrollTo(0,1);
	var portrait = (window.orientation % 180 == 0);
	$("#main-container-p").css("display", (portrait?"block":"none"));
	$("#main-container-l").css("display", (portrait?"none":"block"));
}

function locationHandler(x, y) {
	console.log("x: " + x + " :: y: " + y);
	x = Math.round(x * 480 / 1000);
	x = 480 - x;
	y = Math.round(y * 290 / 1000);
	$("#avatar").show();
	$("#avatar").offset({top: y, left: x});
}

$(document).ready(function() {
	console.log("Starting...");

	window.websocket = new WebSocket("ws://192.168.1.142:8887/teamx");

	console.log(window.websocket);

	window.websocket.onopen = function () {
		console.log("Websock open.");
		window.websocket.send("/test");
		$.get("/hash", function(data) {
			window.hash = data;
			registerGameEvents();
		});
	};

	reorient();
	
	$(window).bind('orientationchange', reorient);
	$(window).bind('focusin', reorient);

	setTimeout('scrollTo(0,1)',1000);
	setTimeout(reorient, 50);

	$(".color-picker").each(function() {
		$(this).css("background-color", $(this).attr("id"));
	});
});

function registerGameEvents() {
	console.log("hash: " + window.hash);
	window.websocket.send(window.hash + "/spawn");

	$("#main-container-l").click(function(e) {
		me = $("#main-container-l");
		x = Math.round(e.pageX * 1000 / (parseInt(me.css("width"))  + 4));
		y = Math.round(e.pageY * 1000 / (parseInt(me.css("height")) + 4));
		$("#click-response").html("X: " + x + " -- Y: " + y);
		window.websocket.send(window.hash + "/touch: " + x + "," + y);
	});
	
	$("#quit-button").click(function(){
		window.websocket.send(window.hash + "/quit");
	});
	
	$(".color-picker").click(function() {
		window.websocket.send(window.hash + "/color: " + $(this).attr("id"));
		$("#avatar").css("background-color", $(this).attr("id"));
	});

	window.websocket.onmessage = function(msg){
		msg = msg.data;
		if (msg.slice(0, ("location: ").length) == "location: ") {
    		msg = msg.slice("location: ".length);
    		msg = $.map(msg.split(","), function(x) {return parseInt(x)});
    		locationHandler(msg[0], msg[1]);
    		return
    	}
	}

}