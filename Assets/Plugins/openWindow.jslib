mergeInto(
LibraryManager.library, {

	openWindow: function (link)
	{
		//var url = String(link);
		var url = link.toString();
		window.open(url, "_blank");
		console.log("trying to open " + url);

	}

}
);

