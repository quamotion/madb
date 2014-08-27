(function ($, document, window) {
	"use strict";

	/*<div class="form-group">
				<label for="exampleInputEmail1">Disabled</label>
				<input type="text" class="form-control" id="exampleInputEmail1" placeholder="Disabled" disabled>
			</div>
	*/
	$(function () {
		$(".form-group .form-control").each(function (i, item) {
			var self = $(this);
			if (self.data("floating-label-setup")) { return; }
			self.data("floating-label-setup", true);

			//// init

			__initLabel(self);

			//__getLabel(self);

			//self.on("focus", function (event, data) {
			//	__getLabel($(this)).addClass("active");
			//}).on("blur", function (event, data) {
			//	__getLabel($(this)).removeClass("active");
			//});
		});
	});
	var __initLabel = function (input) {
		var form = input.closest("form");
		if(form.hasClass("form-horizontal")) {
			return;
		}
		var label = $(input).siblings("label").first();
		if (!label || label.length === 0) {
			label = $("<label />").append(input.attr("placeholder") || input.attr("name"));
			label.insertAfter(input);
		}
	};
	//var __getLabel = function (input) {
	//	var selfId = input.attr("id") || "ID-NOT-FOUND-FLOATING-LABEL";
	//	// what about this? <label>foo<input /></label> - should it be handled?

	//	var label = input.data("floating-label") || $("[for='" + selfId + "']", input.parent()).first() || null;
	//	if (!label || label.length === 0) {
	//		label = $("<label />").append(input.attr("placeholder") || input.attr("name"));
	//		input.insertBefore(input);
	//	}
	//	input.data("floating-label", label);

	//	return __setupLabel(label);
	//};
	//var __setupLabel = function (label) {
	//	if (!label.hasClass("floating-label")) {
	//		label.addClass("floating-label");
	//	}
	//	return label;
	//};
})(window.jQuery, window.document, window);