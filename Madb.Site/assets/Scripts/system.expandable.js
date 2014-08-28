(function ($, document, window) {
	"use strict";

	$(function () {
		$("[data-expandable]").each(function (i, item) {
			var self = $(this);
			if (self.data("expandable-setup")) { return; }
			self.data("expandable-setup", true);

			var trigger = self.data("expandable") || "click";
			self.on(trigger, function (event) {
				var $t = $(this);
				var target = $($t.data("expandable-target"));
				var toggle = $t.data("expandable-toggle");
				if (target.hasClass(toggle)) {
					target.removeClass(toggle);
				} else {
					target.addClass(toggle);
				}
			});

		});
	});
})(window.jQuery, window.document,window);