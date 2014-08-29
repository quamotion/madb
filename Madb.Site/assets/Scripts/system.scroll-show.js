(function ($, document, window) {
	"use strict";
	$(function () {
		$(document).on("scroll", function () {
			var scrollPosition = $(this).scrollTop();
			$("[data-scroll-show-low]").each(function () {
				var self = $(this);
				var low = parseInt(self.data("scroll-show-low") || "0", 0);
				var high = parseInt(self.data("scroll-show-high") || "-1", 0);
				var toggle = self.data("scroll-toggle") || "hidden";
				if ( (scrollPosition >= low || low < 0 ) && ( scrollPosition <= high || high < 0 )) {
					self.removeClass(toggle);
				} else {
					self.addClass(toggle);
				}
			});
		});
	});
})(window.jQuery, window.document, window);