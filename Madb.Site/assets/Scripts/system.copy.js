(function ($, document, window) {
	"use strict";
	$(function () {
		$("a[data-copy]").each(function () {
			var self = $(this);
			if (self.data("copy-setup")) { return; }
			self.data("copy-setup", true);
			var on = self.data("copy") || "click";
			self.on(on, function (event) {
				var $t = $(this);
				var target = $($t.data("copy-target"));
				__copy(target);
				event.preventDefault();
			});

		});

		var __copy = function (target) {
			var textarea = $("<textarea />").text(target.text()).css("display", "none");
			var texteareaElement = textarea.get(0);
			var t1 = "Text successfully copied to your clipboard";
			var t2 = "Press CTRL+C (CMD+C on Mac) to copy the text to your clipboard";
			var toastText = t2;
			// for IE
			if (texteareaElement.createTextRange) {
				var copied = texteareaElement.createTextRange();
				copied.executeCommand("Copy");
				toastText = t1;
			} else if (window.copy) {
				window.copy(textarea.text());
				toastText = t1;
			} 

			var range = document.createRange();
			range.selectNodeContents(target.get(0));
			var sel = window.getSelection();
			sel.removeAllRanges();
			sel.addRange(range);
			toastr.clear();
			toastr.options = {
				"closeButton": false,
				"debug": false,
				"positionClass": "toast-top-full-width",
				"onclick": null,
				"showDuration": 300,
				"hideDuration": 500,
				"timeOut": 5000,
				"extendedTimeOut": 2000,
				"showEasing": "linear",
				"hideEasing": "linear",
				"showMethod": "fadeIn",
				"hideMethod": "fadeOut"
			};

			toastr["info"](toastText);
		}
	})
})(window.jQuery, window.document, window);