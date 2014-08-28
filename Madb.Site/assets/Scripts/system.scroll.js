(function ($, document, window) {
	"use strict";
	$(function () {
		// smooth scrolling if the jquery scrollTo plugin exists.
		$("a[data-scroll]").each(function () {
			var self = $(this);
			if (self.data("scroll-setup")) { return; }
			self.data("scroll-setup", true);
			var trigger = self.data("scroll") || "click";
			self.on(trigger, function (evt) {
				if ($.scrollTo) {
					var evtx = evt || event;
					var $s = $(this);
					var topOffset = parseInt($s.data("scroll-offset"), 0);
					var target = $(this.hash);
					$.scrollTo(target, { duration: 1000, offset: topOffset });
					evtx.preventDefault();
				}
			});
		});
	});
})(window.jQuery, window.document, window);