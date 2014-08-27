(function ($, toastr, document, window) {
	"use strict";

	$(function () {
		$("[data-toastr]").each(function (i, item) {
			var self = $(this);
			if (self.data("toastr-setup")) { return; }
			self.data("toastr-setup", true);

			var trigger = self.data("toastr") || "click";
			self.on(trigger, function (event,data) {
				var $t = $(this);
				var type = $t.data("toastr-type") || "success";
				var content = $t.data("toastr-content") || null;
				var text = $t.data("toastr-text") || $(content).html() || null;
				var close = $t.data("toastr-close") || false;
				var position = $t.data("toastr-position") || "top-full-width";
				var showDuration = parseInt($t.data("toastr-show-duration") || "300", 0);
				var hideDuration = parseInt($t.data("toastr-hide-duration") || "1000", 0);
				var timeout = parseInt($t.data("toastr-timeout") || "0", 0);
				var extendedTimeout = parseInt($t.data("toastr-extended-timeout") || "0", 0);
				var showEasing = $.data("toastr-show-easing") || "swing";
				var showMethod = $.data("toastr-show-method") || "slideDown";
				var hideEasing = $.data("toastr-hide-easing") || "linear";
				var hideMethod = $.data("toastr-hide-method") || "slideUp";
				toastr.options = {
					"closeButton": close,
					"debug": false,
					"positionClass": "toast-" + position,
					"onclick": null,
					"showDuration": showDuration,
					"hideDuration": hideDuration,
					"timeOut": timeout,
					"extendedTimeOut": extendedTimeout,
					"showEasing": showEasing,
					"hideEasing": hideEasing,
					"showMethod": showMethod,
					"hideMethod": hideMethod
				};

				toastr[type](text);

			});
		});
	});
	
})(jQuery, window.toastr, document, window);