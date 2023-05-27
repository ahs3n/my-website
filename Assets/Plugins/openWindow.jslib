mergeInto(
LibraryManager.library, {

	openWindow: function (link)
	{
		//var url = link.toString();
		var url = UTF8ToString(link);
		window.open(url, "_blank");
		console.log("trying to open " + url);
	}
}
);

