(function ($, mouseTrap) {
	"use strict";
	$(function () {
		// wire up the popovers
		$("[data-toggle='popover']").each(function () {
			var self = $(this);
			if (self.data("popover-setup")) {
				return;
			}
			self.data("popover-setup", true);
			self.popover();
		});

		// toggle the open/close
		$(".epm-nav-toggle").on("click", function () {
			var self = $(this);
			var target = $(self.data("epm-target"));
			var toggle = self.data("epm-toggle");
			var collapsed = "collapsed";
			if (target.hasClass(toggle)) {
				target.removeClass(toggle).addClass(collapsed);
				self.parent().removeClass(toggle).addClass(collapsed);
				self.attr("title", "Open");
			} else {
				target.removeClass(collapsed).addClass(toggle);
				self.parent().removeClass(collapsed).addClass(toggle);
				self.attr("title", "Close");
			}
			self.trigger("click.epm-mousetrap");
		});

		// filter the navigation with what is typed in the search field
		$(".epm-nav .nav-search input.search-field").on("keyup reset.epm-search", function (event) {
			var self = $(this);
			var $list = $(self).closest("ul.nav");
			var items = $("li", $list).not(".no-filter");
			var value = $(this).val();
			var $clear = $(".epm-nav .nav-search button.clear");
			$clear.trigger(value && value.length > 0 ? "show-clear.epm-search" : "hide-clear.epm-search");
			items.each(function () {
				var s = $(this);
				if (s.text().toLowerCase().search(value.toLowerCase()) > -1 || (!value || value.length == 0)) {
					s.removeClass("hidden");
				} else {
					s.addClass("hidden");
				}
			});
		});

		// this clears the search box and resets the navigation
		$(".epm-nav .nav-search button.clear").on("click", function (event) {
			$(".epm-nav .nav-search input.search-field").val("").trigger("reset.epm-search");
		}).on("show-clear.epm-search",function(event){
			$(this).removeClass("hidden");
		}).on("hide-clear.epm-search", function (event) {
			$(this).addClass("hidden");
		});

		// set up scrollspy on the body
		$("body").scrollspy({ target: ".epm-nav .nav", offset: 100 });

		// smooth scrolling if the jquery scrollTo plugin exists.
		$(".epm-nav .nav li a[data-scroll]").each(function () {
			var self = $(this);
			if (self.data("scroll-setup")) { return; }
			self.data("scroll-setup", true);
			var trigger = self.data("scroll") || "click";
			self.on(trigger, function (event) {
				if ($.scrollTo) {
					var $s = $(this);
					var topOffset = parseInt($s.data("scroll-offset"), 0);
					var target = $(this.hash);
					$.scrollTo(target, { duration: 1000, offset: topOffset });
					event.preventDefault();
				}
			});
		});



		// if you have mousetrap (http://craig.is/killing/mice) loaded, endpointMVC will bind some events
		if (mouseTrap) {
			// change the input to have the mouse bindings:
			var field = $(".epm-nav.expanded .nav-search input.search-field");
			var currentPH = field.attr("placeholder");
			field.attr("placeholder", currentPH + " (Ctrl+Q)");

			// change the title of open/close with ctrl+left/ctrl+right on the click trigger
			$(".epm-nav-toggle").on("click.epm-mousetrap", function () {
				$(".expanded.left .epm-nav-toggle").attr("title", "Close (Ctrl+Left)");
				$(".collapsed.right .epm-nav-toggle").attr("title", "Open (Ctrl+Left)");
				$(".collapsed.left .epm-nav-toggle").attr("title", "Open (Ctrl+Right)");
				$(".expanded.right .epm-nav-toggle").attr("title", "Close (Ctrl+Right)");
			}).trigger("click.epm-mousetrap");

			mouseTrap.bind("ctrl+q", function (e) {
				// bind ctrl+q to focus the search box
				$(".epm-nav.expanded .nav-search input.search-field").focus();
			}).bind("ctrl+left",function(e) {
				// bind ctrl+left and either open or close the navigation (depending on the location)
				$(".expanded.left .epm-nav-toggle").trigger("click").attr("title", "Open (Ctrl+Right)");
				$(".collapsed.right .epm-nav-toggle").trigger("click").attr("title", "Close (Ctrl+Right)");
			}).bind("ctrl+right", function (e) {
				// bind ctrl+right and either open or close the navigation (depending on the location)
				$(".expanded.right .epm-nav-toggle").trigger("click").attr("title", "Open (Ctrl+Left)");
				$(".collapsed.left .epm-nav-toggle").trigger("click").attr("title", "Close (Ctrl+Left)");
			});
		}
	});
})(window.jQuery, window.Mousetrap);