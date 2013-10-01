function reorient(e) {
	scrollTo(0,1);
	var portrait = (window.orientation % 180 == 0);
	$("#main-container-p").css("display", (portrait?"block":"none"));
	$("#main-container-l").css("display", (portrait?"none":"block"));
}

function locationHandler(x, y) {
	var side = getSide(),
            posX = parseInt($("#main-container-l").css("margin-left"));
	x = Math.round(x * side / 1000);
	x = side - x;
	y = Math.round(y * side / 1000);
	$("#avatar").show();
	$("#avatar").offset({top: y, left: x + posX});
	console.log("x: " + x + " :: y: " + y);
}

function getSide() {
	return Math.min(
		$(window).height(),
		$(window).width()
	);
}

function resize() {
	var side = getSide();
	$("#main-container-l").width(side).height(side);
}

$(document).ready(function() {
	console.log("Starting...");
	resize();
	$(window).resize(resize);

	// window.websocket = new WebSocket("ws://192.168.1.142:8887/teamx");
	window.websocket = new WebSocket("ws://" + window.location.host + "/teamx");

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
		// Based on: <http://stackoverflow.com/questions/3234977/using-jquery-how-to-get-click-coordinates-on-the-target-element>
		var posX = parseInt($("#main-container-l").css("margin-left"));
		posX = e.pageX - posX;
		var side = getSide();
		console.log("x: " + posX + " :: y: " + e.pageY);
		x = Math.round(posX    * 1000 / side + 4);
		y = Math.round(e.pageY * 1000 / side + 4);
		window.websocket.send(window.hash + "/touch: " + x + "," + y);
	});
	
	$("#quit-button").click(function(){
		window.websocket.send(window.hash + "/quit");
		$.get("/clear");
	});
	
	$(".color-picker").click(function() {
		window.websocket.send(window.hash + "/color: " + $(this).attr("id"));
		$("#avatar").css("background-color", $(this).attr("id"));
	});

	window.websocket.onmessage = function(msg){
		msg = msg.data;
		console.log("Got message: " + msg);
		if (msg.slice(0, ("location: ").length) == "location: ") {
    			msg = msg.slice("location: ".length);
    			msg = $.map(msg.split(","), function(x) {return parseInt(x)});
    			locationHandler(msg[0], msg[1]);
    			return;
    		}
		if (msg.slice(0, ("crashed").length) == "crashed") {
			$.get("/clear");
			$("#avatar").fadeOut();
			return;
		}
	}

}
