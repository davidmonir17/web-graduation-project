function initResponsive(scheduler) {
	var navbarHeight = 59,
		navbarClassicHeight = 23,

		navbarMobileHeight = 130,
		navbarClassicMobileHeight = 140,

		loadQuickInfo = true,

		scaleDate = "%F, %D %d",
		scaleDateMobile = "%D %d";

	var classic = { "glossy": true, "classic": true };

	function setSizes(navHeight, navHeightClassicSkin, scaleDate) {
		scheduler.xy.nav_height = navHeight;

		if (classic[scheduler.skin]) {
			scheduler.xy.nav_height = navHeightClassicSkin;
		}

		scheduler.templates.week_scale_date = function (date) {
			return scheduler.date.date_to_str(scaleDate)(date);
		};
	}

	function setNavbarHeight() {
		/* set sizes based on screen size */
		if (typeof scheduler !== "undefined") {

			if (window.innerWidth >= 768) {
				setSizes(navbarHeight, navbarClassicHeight, scaleDate);
			} else {
				setSizes(navbarMobileHeight, navbarClassicMobileHeight, scaleDateMobile);
			}
		}
		return true;
	}

	scheduler.attachEvent("onSchedulerResize", function () {
		scheduler.setCurrentView();
	});
	scheduler.attachEvent("onBeforeViewChange", setNavbarHeight);

	scheduler.attachEvent("onTemplatesReady", function () {
		if (classic[scheduler.skin]) {
			addCss("../Content/dhtmlxScheduler/dhtmlxscheduler-responsive-classic.css");
		}
	});

	function addCss(source) {
		var cssFileTag = document.createElement("link");
		cssFileTag.setAttribute("rel", "stylesheet");
		cssFileTag.setAttribute("type", "text/css");
		cssFileTag.setAttribute("href", source);

		document.getElementsByTagName("head")[0].appendChild(cssFileTag);
	}

	function addJS(url, callback) {
		var head = document.getElementsByTagName('head')[0];
		var script = document.createElement('script');
		script.type = 'text/javascript';
		script.src = url;
		script.onreadystatechange = callback;
		script.onload = callback;
		head.appendChild(script);
	}


	if (/Android|webOS|iPhone|iPad|iPod|IEMobile/i.test(navigator.userAgent) && loadQuickInfo) {
		addJS("../Scripts/dhtmlxScheduler/ext/dhtmlxscheduler_quick_info.js", function () {
			scheduler.config.touch = "force";
			scheduler.xy.menu_width = 0;
		});
	}
}
