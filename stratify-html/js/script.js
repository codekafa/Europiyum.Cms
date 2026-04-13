/*-----------------------------------------------------------------

Template Name: Stratify - Business Consulting HTML Template
Author:  ThemeMascot
Author URI: https://themeforest.net/user/thememascot/portfolio
Version: 1.0.0
Description: Stratify - Business Consulting HTML Template

-------------------------------------------------------------------
CSS TABLE OF CONTENTS
-------------------------------------------------------------------

01. preloader
02. color switcher
03. header
04. gsap animation
05. js animation
06. fullScreen search
07. swiper slider
08. hover add class remove class
09. search screen
10. background image
11. magnificPopup
12. coundawn
13. counterup
14. back to top
15. wow animation

------------------------------------------------------------------*/

(function ($) {
	("use strict");

	// Color mood area start here ***
	function setThemeColor(color) {
		const root = document.documentElement;
		root.setAttribute("data-theme", color);
	}
	// Color mood area end here ***

	// Preloader area start here ***
	const loader = () => {
		$(window).on("load", function () {
			$("#preloader").addClass("loaded");
			$("#preloader").delay(500).fadeOut();
		});
	};
	loader();
	// Preloader area end here ***

	// Header area start here ***
	// Mobile menu
	$(".header-area nav").meanmenu();

	// Menu Fixed
	var fixed_top = $(".header-area");
	$(window).on("scroll", function () {
		if ($(this).scrollTop() > 50) {
			fixed_top.addClass("menu-fixed animated fadeInDown");
		} else {
			fixed_top.removeClass("menu-fixed fadeInDown");
		}
	});
	// Header area end here ***

	// Megamenu-helper area start here ***
	 $(".megamenu").each(function () {
			$(this).closest(".mean-nav").addClass("megamenuHelper");
		});
	// Megamenu-helper area end here ***

	// Gsap Animation area start here ***
	gsap.utils.toArray(".gsap__parallax").forEach(function (container) {
		let image = container.querySelector("img");

		let tl = gsap.timeline({
			scrollTrigger: {
				trigger: container,
				scrub: 0.5,
			},
		});
		tl.from(image, {
			yPercent: -30,
			ease: "none",
		}).to(image, {
			yPercent: 30,
			ease: "none",
		});
	});
	// Gsap Animation area end here ***

	// Mouse move paralax area end here ***
	if ($(window).width() > 780) {
		$(".paralax__animation").mousemove(function (e) {
			$("[data-depth]").each(function () {
				var depth = $(this).data("depth");
				var amountMovedX = (e.pageX * -depth) / 4;
				var amountMovedY = (e.pageY * -depth) / 4;

				$(this).css({
					transform:
						"translate3d(" +
						amountMovedX +
						"px," +
						amountMovedY +
						"px, 0)",
				});
			});
		});
	}
	// Mouse move paralax area end here ***

	// Image move with mouse area start here ***
	const serviceImgItems = document.querySelectorAll(".achivement__item");
	serviceImgItems.forEach((item) => {
		const hoverImage = item.querySelector(".hover-image");
		if (!hoverImage) return;
		item.addEventListener("mousemove", ({ clientX, clientY }) => {
			const { x, y } = item.getBoundingClientRect();
			const dx = clientX - x;
			const dy = clientY - y;
			hoverImage.style.transform = `translate(${dx}px, ${dy}px)`;
		});
	});
	// Image move with mouse area end here ***

	// Scroll Fade area start here ***
	$(window).scroll(function () {
		var scrolled = $(this).scrollTop();

		$(".parallaxScroll").css({
			transform:
				"translate3d(0, " +
				-(scrolled * 0.2) +
				"px, 0) rotateX(" +
				scrolled * 0.1 +
				"deg)",
			opacity: 1 - scrolled / 600,
		});

		$(".parallaxScaleScroll").css({
			transform: "scale(" + (1 + scrolled / 1500) + ")",
		});

		$(".parallaxRightScroll").css({
			transform: "translateX(" + scrolled / 2 + "px)", // Move the element to the right
		});

		$(".parallaxLeftScroll").css({
			transform: "translateX(" + -(scrolled / 2) + "px)", // Move the element to the left
		});

		$(".parallaxRoteteYScroll").css({
			transform: "rotateY(" + scrolled * 0.2 + "deg)", // 3D rotate along Y-axis
			opacity: 1 - scrolled / 500, // Fade out slowly
		});

		$(".parallaxRotete360Scroll").css({
			transform: "rotate(" + scrolled + "deg)", // Rotate 360 degrees based on scroll
		});
	});
	// Scroll Fade area end here ***

	// FullScreen search area end here ***
	var $searchWrap = $(".search-wrap");
	var $navSearch = $(".nav-search");
	var $searchClose = $("#search-close");
	$(".search-trigger").on("click", function (e) {
		e.preventDefault();
		$searchWrap.animate({ opacity: "toggle" }, 500);
		$navSearch.add($searchClose).addClass("open");
	});
	$(".search-close").on("click", function (e) {
		e.preventDefault();
		$searchWrap.animate({ opacity: "toggle" }, 500);
		$navSearch.add($searchClose).removeClass("open");
	});
	function closeSearch() {
		$searchWrap.fadeOut(200);
		$navSearch.add($searchClose).removeClass("open");
	}
	$(document.body).on("click", function (e) {
		closeSearch();
	});
	$(".search-trigger, .main-search-input").on("click", function (e) {
		e.stopPropagation();
	});
	// FullScreen search area end here ***

	// Banner slider area start here ***
	(function BannerSlider() {
		var sliderActive1 = ".banner__slider";
		var sliderInit1 = new Swiper(sliderActive1, {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 0,
			speed: 2000,
			parallax: true,
			autoplay: {
				delay: 8000,
				disableOnInteraction: false,
			},
			navigation: {
				nextEl: ".banner__arry-next",
				prevEl: ".banner__arry-prev",
			},
			on: {
				slideChange: function () {
					animateContent();
				},
			},
			pagination: {
				el: ".banner__dot",
				clickable: true,
				renderBullet: function (index, className) {
					const dotContent = document.querySelectorAll(
						".banner__dot .dot-content"
					);
					return `
                <span class="${className}">
                    ${dotContent[index]?.outerHTML || ""}
                </span>
            `;
				},
			},
		});
		function animateContent() {
			var animatedElements = $("[data-animation]");
			animatedElements.each(function () {
				var $this = $(this);
				var anim = $this.data("animation");
				var delay = $this.data("delay") || "0s";
				var duration = $this.data("duration") || "1s";

				$this
					.removeClass(anim + " animated")
					.css({
						webkitAnimationDelay: delay,
						animationDelay: delay,
						webkitAnimationDuration: duration,
						animationDuration: duration,
					})
					.addClass(anim + " animated")
					.one("animationend", function () {
						$this.removeClass(anim + " animated");
					});
			});
		}
		animateContent();
	})();
	// Banner slider area end here ***

	// Banner six slider area start here ***
	var sliderActive1 = ".banner-six__slider";
	var sliderInit1 = new Swiper(sliderActive1, {
		loop: true,
		slidesPerView: 1,
		effect: "fade",
		speed: 3000,
		autoplay: {
			delay: 7000,
			disableOnInteraction: false,
		},
		navigation: {
			nextEl: ".banner-two__arry-next",
			prevEl: ".banner-two__arry-prev",
		},
	});
	function animated_swiper(selector, init) {
		var animated = function animated() {
			$(selector + " [data-animation]").each(function () {
				var anim = $(this).data("animation");
				var delay = $(this).data("delay");
				var duration = $(this).data("duration");
				$(this)
					.removeClass("anim" + anim)
					.addClass(anim + " animated")
					.css({
						webkitAnimationDelay: delay,
						animationDelay: delay,
						webkitAnimationDuration: duration,
						animationDuration: duration,
					})
					.one("animationend", function () {
						$(this).removeClass(anim + " animated");
					});
			});
		};
		animated();
		init.on("slideChange", function () {
			$(sliderActive1 + " [data-animation]").removeClass("animated");
		});
		init.on("slideChange", animated);
	}
	animated_swiper(sliderActive1, sliderInit1);
	// Banner six slider area end here ***

	// Banner ten slider area start here ***
	const sliderSelector = ".banner-ten__slider";
	const sliderInstance = new Swiper(sliderSelector, {
		loop: true,
		slidesPerView: 1,
		effect: "fade",
		speed: 3000,
		autoplay: {
			delay: 7000,
			disableOnInteraction: false,
		},
		navigation: {
			nextEl: ".banner-ten__arrow-next",
			prevEl: ".banner-ten__arrow-prev",
		},
	});
	function initializeAnimations(slider, instance) {
		const animateContent = () => {
			$(`${slider} [data-animation]`).each(function () {
				const $element = $(this);
				const animation = $element.data("animation");
				const delay = $element.data("delay") || "0s";
				const duration = $element.data("duration") || "1s";

				$element
					.css({
						animationDelay: delay,
						animationDuration: duration,
					})
					.addClass(`${animation} animated`)
					.one("animationend", function () {
						$element.removeClass(`${animation} animated`);
					});
			});
		};

		instance.on("slideChange", function () {
			$(`${slider} [data-animation]`).removeClass("animated");
			animateContent();
		});

		animateContent();
	}
	initializeAnimations(sliderSelector, sliderInstance);
	// Banner ten slider area end here ***

	// About slider area start here ***
	var aboutSlider = new Swiper(".about-five__slider", {
		spaceBetween: 30,
		speed: 500,
		breakpoints: {
			575: {
				slidesPerView: 2,
			},
		},
		navigation: {
			nextEl: ".about__arry-next",
			prevEl: ".about__arry-prev",
		},
	});
	// About slider area end here ***

	// Blog slider area start here ***
	var blogSlider = new Swiper(".blog-two__slider", {
		loop: "true",
		spaceBetween: 50,
		speed: 1000,
		autoplay: {
			delay: 3000,
			disableOnInteraction: false,
		},
		breakpoints: {
			767: {
				slidesPerView: 2,
			},
			991: {
				slidesPerView: 2,
			},
			1199: {
				slidesPerView: 3,
			},
		},
	});

	var blogSixSlider = new Swiper(".blog-six__slider", {
		loop: "true",
		spaceBetween: 30,
		speed: 1000,
		centeredSlides: true,
		autoplay: {
			delay: 3000,
			disableOnInteraction: false,
		},
		breakpoints: {
			575: {
				slidesPerView: 2,
			},
			991: {
				slidesPerView: 3,
			},
			1199: {
				slidesPerView: 4,
			},
		},
	});
	// Blog slider area end here ***

	// Professional slider area start here ***
	var swiper = new Swiper(".professional-seven__slider", {
		loop: "true",
		speed: 2000,
		effect: "fade",
		autoplay: {
			delay: 5000,
			disableOnInteraction: false,
		},
		slidesPerView: 1,
		navigation: {
			nextEl: ".professional-seven__arry-next",
			prevEl: ".professional-seven__arry-prev",
		},
	});
	// Professional slider area start here ***

	// Testimonial slider area start here ***
	var swiper = new Swiper(".testimonial__slider", {
		loop: "true",
		spaceBetween: 50,
		speed: 1000,
		autoplay: {
			delay: 5000,
			disableOnInteraction: false,
		},
		navigation: {
			nextEl: ".testimonial__arry-next",
			prevEl: ".testimonial__arry-prev",
		},
		breakpoints: {
			767: {
				slidesPerView: 2,
			},
		},
	});

	var swiperThumb = new Swiper(".testimonial-three__slider-thumb", {
		spaceBetween: 20,
		speed: 1000,
		freeMode: true,
		breakpoints: {
			767: {
				slidesPerView: 3,
			},
		},
	});
	var swiper = new Swiper(".testimonial-three__slider", {
		spaceBetween: 30,
		speed: 1000,
		thumbs: {
			swiper: swiperThumb,
		},
		navigation: {
			nextEl: ".testimonial-three__arry-next",
			prevEl: ".testimonial-three__arry-prev",
		},
	});

	var swiper = new Swiper(".testimonial-four__slider", {
		spaceBetween: 30,
		speed: 1000,
		navigation: {
			nextEl: ".testimonial-four__arry-next",
			prevEl: ".testimonial-four__arry-prev",
		},
		autoplay: true,
		loop: true,
		breakpoints: {
			0: {
				slidesPerView: 1,
			},
			991: {
				slidesPerView: 2,
			},
			1199: {
				slidesPerView: 3,
			},
			1399: {
				slidesPerView: 4,
			},
		},
	});

	var swiper = new Swiper(".testimonial-five__slider", {
		spaceBetween: 50,
		speed: 1000,
		autoplay: {
			delay: 5000,
			disableOnInteraction: false,
		},
		loop: true,
		pagination: {
			el: ".testimonial__dot",
			clickable: true,
		},
	});

	var swiper = new Swiper(".testimonial-seven__slider", {
		spaceBetween: 30,
		speed: 1000,
		navigation: {
			nextEl: ".testimonial-seven__arry-next",
			prevEl: ".testimonial-seven__arry-prev",
		},
	});

	var swiper = new Swiper(".testimonial-ten__slider", {
		loop: true,
		speed: 1000,
		autoplay: {
			delay: 5000,
			disableOnInteraction: false,
		},
		slidesPerView: 1,
		pagination: {
			el: ".testimonial-ten__dot",
			clickable: true,
		},
	});

	var swiper = new Swiper(".testimonial-twelve__slider", {
		spaceBetween: 30,
		speed: 1000,
		autoplay: {
			delay: 3000,
			disableOnInteraction: false,
		},
		loop: true,
		breakpoints: {
			0: {
				slidesPerView: 1,
			},
			767: {
				slidesPerView: 2,
			},
			991: {
				slidesPerView: 2,
			},
			1200: {
				slidesPerView: 3,
			},
		},
	});
	// Testimonial slider area end here ***

	// Project slider area start here ***
	var swiper = new Swiper(".project__slider", {
		spaceBetween: 0,
		speed: 1000,
		pagination: false,
		navigation: {
			nextEl: ".project__arry-next",
			prevEl: ".project__arry-prev",
		},
		mousewheel: false,
		keyboard: true,
		autoplay: false,
		loop: false,
		breakpoints: {
			0: {
				slidesPerView: 1,
			},
			480: {
				slidesPerView: 2,
			},
			787: {
				slidesPerView: 3,
			},
			991: {
				slidesPerView: 3,
			},
			1200: {
				slidesPerView: 4,
			},
		},
	});

	var swiper = new Swiper(".project-three__slider", {
		loop: "true",
		spaceBetween: 30,
		speed: 1000,
		// autoplay: {
		// 	delay: 5000,
		// 	disableOnInteraction: false,
		// },
		breakpoints: {
			575: {
				slidesPerView: 1,
			},
			991: {
				slidesPerView: 2,
			},
			1199: {
				slidesPerView: 3,
			},
		},
	});
	// Project slider area end here ***

	// Swiper marqee area start here ***
	var marqueeSlider = new Swiper(".marqueeSwiper__slider", {
		loop: true,
		freemode: true,
		slidesPerView: 1,
		spaceBetween: 0,
		centeredSlides: true,
		allowTouchMove: false,
		speed: 2000,
		autoplay: {
			delay: 1,
			disableOnInteraction: true,
		},
		breakpoints: {
			320: {
				slidesPerView: 2,
			},
			425: {
				slidesPerView: 3,
			},
			800: {
				slidesPerView: 3,
			},
			1024: {
				slidesPerView: 4,
			},
			1300: {
				slidesPerView: 5,
			},
			1900: {
				slidesPerView: 6,
			},
		},
	});

	var marqueeSliderFive = new Swiper(".marqueeSwiper-five__slider", {
		loop: true,
		freemode: true,
		slidesPerView: 1,
		spaceBetween: 0,
		centeredSlides: true,
		allowTouchMove: false,
		speed: 2000,
		autoplay: {
			delay: 1,
			disableOnInteraction: true,
		},
		breakpoints: {
			320: {
				slidesPerView: 2,
			},
			360: {
				slidesPerView: 3,
			},
			800: {
				slidesPerView: 3,
			},
			1024: {
				slidesPerView: 4,
			},
			1300: {
				slidesPerView: 5,
			},
		},
	});
	// Swiper marqee area end here ***

	// Project change background image area end here ***
	$(".project__slider .swiper-slide").on("mouseenter click", function () {
		var tab_id = $(this).attr("data-tab");
		$(".project__slider .swiper-slide").removeClass("active");
		$(this).addClass("active");

		$(".project__image .tab-img ").removeClass("active");
		$("#" + tab_id).addClass("active");

		if ($(this).hasClass("active")) {
			return false;
		}
	});
	$(".project__arry-next").on("click", function () {
		// Perform the intended action here, e.g., trigger a slider move
		console.log("Next button clicked");
	});

	$(".project__arry-prev").on("click", function () {
		// Perform the intended action here, e.g., trigger a slider move
		console.log("Prev button clicked");
	});
	// Project change background image area end here ***

	// Hover add & remove js area start here ***
	$(".hover-item").on("hover", function () {
		$(".hover-item").removeClass("active");
		$(this).addClass("active");
	});
	$(".service-three__item").on("hover", function () {
		$(".service-three__item").removeClass("active");
		$(this).addClass("active");
	});
	$(".service-eleven__item").on("hover", function () {
		$(".service-eleven__item").removeClass("active");
		$(this).addClass("active");
	});
	$(".feature-five__item").on("hover", function () {
		$(".feature-five__item").removeClass("active");
		$(this).addClass("active");
	});
	// Hover add & remove js area end here ***

	// Horizontal accordion js area start here ***
	$(".hzAccordion__item").on("click", function () {
		$(this).addClass("active").siblings().removeClass("active");
	});
	// Horizontal accordion js area end here ***

	//Tabs Box
	if ($(".tabs-box").length) {
		$(".tabs-box .tab-buttons .tab-btn").on("click", function (e) {
			e.preventDefault();
			var target = $($(this).attr("data-tab"));

			if ($(target).is(":visible")) {
				return false;
			} else {
				target
					.parents(".tabs-box")
					.find(".tab-buttons")
					.find(".tab-btn")
					.removeClass("active-btn");
				$(this).addClass("active-btn");
				target
					.parents(".tabs-box")
					.find(".tabs-content")
					.find(".tab")
					.fadeOut(0);
				target
					.parents(".tabs-box")
					.find(".tabs-content")
					.find(".tab")
					.removeClass("active-tab animated fadeIn");
				$(target).fadeIn(300);
				$(target).addClass("active-tab animated fadeIn");
			}
		});
	}

	//Accordion Box
	if ($(".accordion-box").length) {
		$(".accordion-box").on("click", ".acc-btn", function () {
			var outerBox = $(this).closest(".accordion-box");
			var target = $(this).closest(".accordion");
			var content = $(this).next(".acc-content");

			if ($(this).hasClass("active")) {
				// If already active, close it
				$(this).removeClass("active");
				target.removeClass("active-block");
				content.slideUp(300);
			} else {
				// Remove active state from all buttons and accordions
				outerBox.find(".acc-btn").removeClass("active");
				outerBox.find(".accordion").removeClass("active-block");
				outerBox.find(".acc-content").slideUp(300);

				// Activate clicked button and accordion
				$(this).addClass("active");
				target.addClass("active-block");
				content.slideDown(300);
			}
		});
	}

	//product bxslider
	if ($(".product-details .bxslider").length) {
		$(".product-details .bxslider").bxSlider({
			nextSelector: ".product-details #slider-next",
			prevSelector: ".product-details #slider-prev",
			nextText: '<i class="fa fa-angle-right"></i>',
			prevText: '<i class="fa fa-angle-left"></i>',
			mode: "fade",
			auto: "true",
			speed: "700",
			pagerCustom: ".product-details .slider-pager .thumb-box",
		});
	}

	// count Bar
	if ($(".count-bar").length) {
		$(".count-bar").appear(
			function () {
				var el = $(this);
				var percent = el.data("percent");
				$(el).css("width", percent).addClass("counted");
			},
			{
				accY: -50,
			}
		);
	}

	// Background image area start here ***
	$("[data-background").each(function () {
		$(this).css(
			"background-image",
			"url( " + $(this).attr("data-background") + "  )"
		);
	});
	// Background image area end here ***

	// Background image hover change area start here ***
	$(".project__item").on("hover", function () {
		let newBackground = $(this).data("bg");
		$(".project__wrp")
			.attr("data-background", newBackground)
			.css("background-image", "url(" + newBackground + ")");
	});
	// Background image hover change area end here ***

	// Video popup area start here ***
	$(".video-popup").magnificPopup({
		type: "iframe",
		iframe: {
			markup:
				'<div class="mfp-iframe-scaler">' +
				'<div class="mfp-close"></div>' +
				'<iframe class="mfp-iframe" frameborder="0" allowfullscreen></iframe>' +
				"</div>",

			patterns: {
				youtube: {
					index: "youtube.com/",

					id: "v=",

					src: "https://www.youtube.com/embed/%id%?autoplay=1",
				},
				vimeo: {
					index: "vimeo.com/",
					id: "/",
					src: "//player.vimeo.com/video/%id%?autoplay=1",
				},
				gmaps: {
					index: "//maps.google.",
					src: "%id%&output=embed",
				},
			},

			srcAction: "iframe_src",
		},
	});
	// Video popup area end here ***

	// Coundawn area start here ***
	var targetDate = new Date("2025-07-12 00:00:00").getTime();
	var countdownInterval = setInterval(function () {
		var currentDate = new Date().getTime();
		var remainingTime = targetDate - currentDate;

		if (remainingTime <= 0) {
			clearInterval(countdownInterval);
			// Display a message or perform any action when the countdown timer reaches zero
			$("#countdown-container").text("Countdown has ended!");
		} else {
			var days = Math.floor(remainingTime / (1000 * 60 * 60 * 24));
			var hours = Math.floor(
				(remainingTime % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
			);
			var minutes = Math.floor(
				(remainingTime % (1000 * 60 * 60)) / (1000 * 60)
			);
			var seconds = Math.floor((remainingTime % (1000 * 60)) / 1000);

			// Pad single-digit values with leading zeros
			$("#day").text(days.toString().padStart(2, "0"));
			$("#hour").text(hours.toString().padStart(2, "0"));
			$("#min").text(minutes.toString().padStart(2, "0"));
			$("#sec").text(seconds.toString().padStart(2, "0"));
		}
	}, 1000);
	// Coundawn area end here ***

	//Quantity box
	$(".quantity-box .add").on("click", function () {
		if ($(this).prev().val() < 999) {
			$(this)
				.prev()
				.val(+$(this).prev().val() + 1);
		}
	});
	$(".quantity-box .sub").on("click", function () {
		if ($(this).next().val() > 1) {
			if ($(this).next().val() > 1)
				$(this)
					.next()
					.val(+$(this).next().val() - 1);
		}
	});

	//Price Range Slider
	if ($(".price-range-slider").length) {
		$(".price-range-slider").slider({
			range: true,
			min: 10,
			max: 99,
			values: [10, 60],
			slide: function (event, ui) {
				$("input.property-amount").val(ui.values[0] + " - " + ui.values[1]);
			},
		});

		$("input.property-amount").val(
			$(".price-range-slider").slider("values", 0) +
				" - $" +
				$(".price-range-slider").slider("values", 1)
		);
	}

	//Gallery Filters
	if ($(".filter-list").length) {
		$(".filter-list").mixItUp({});
	}

	//Fact Counter + Text Count
	if($('.count-box').length){
		$('.count-box').appear(function(){
	
			var $t = $(this),
				n = $t.find(".count-text").attr("data-stop"),
				r = parseInt($t.find(".count-text").attr("data-speed"), 10);
				
			if (!$t.hasClass("counted")) {
				$t.addClass("counted");
				$({
					countNum: $t.find(".count-text").text()
				}).animate({
					countNum: n
				}, {
					duration: r,
					easing: "linear",
					step: function() {
						$t.find(".count-text").text(Math.floor(this.countNum));
					},
					complete: function() {
						$t.find(".count-text").text(this.countNum);
					}
				});
			}
			
		},{accY: 0});
	}

	//Progress Bar
	if ($(".progress-line").length) {
		$(".progress-line").appear(
			function () {
				var el = $(this);
				var percent = el.data("width");
				$(el).css("width", percent + "%");
			},
			{ accY: 0 }
		);
	}

	// Counter up area start here ***
	$(".count").counterUp({
		delay: 50,
		time: 1500,
	});
	// Counter up area end here ***

	// Nice seclect area start here ***
	$(document).on("ready", function () {
		$("select").niceSelect();
	});
	// Nice seclect area end here ***

	// Type text area start here ***
	$(document).on("ready", function () {
		const typingElement = $("#typing-text");
		if (typingElement.length) {
			// Check if the element exists
			const words = typingElement.data("words").split(",");
			let index = 0;
			let letterIndex = 0;
			let direction = 1;
			let currentWord = words[0];
			let interval;

			function typeWriter() {
				const word = words[index];
				if (letterIndex < word.length) {
					typingElement.text(
						typingElement.text() + word.charAt(letterIndex)
					);
					letterIndex++;
				} else {
					clearInterval(interval);
					interval = setInterval(eraseText, 100);
				}
			}

			function eraseText() {
				if (letterIndex >= 0) {
					const text = currentWord.substring(0, letterIndex);
					typingElement.text(text);
					letterIndex--;
				} else {
					clearInterval(interval);
					index = (index + direction) % words.length;
					if (index < 0) index = words.length - 1;
					currentWord = words[index];
					interval = setInterval(typeWriter, 100);
				}
			}

			interval = setInterval(typeWriter, 100);
		}
	});
	// Type text area end here ***

	// Back to top area start here ***
	$(document).on("ready", function () {
		const scrollPath = document.querySelector(".scroll-up path");

		if (scrollPath) {
			// Check if the element exists
			const pathLength = scrollPath.getTotalLength();

			// Initialize the strokeDash properties
			scrollPath.style.transition = scrollPath.style.WebkitTransition =
				"none";
			scrollPath.style.strokeDasharray = `${pathLength} ${pathLength}`;
			scrollPath.style.strokeDashoffset = pathLength;
			scrollPath.getBoundingClientRect();
			scrollPath.style.transition = scrollPath.style.WebkitTransition =
				"stroke-dashoffset 10ms linear";

			// Update scroll progress
			const updateScroll = function () {
				const scroll = $(window).scrollTop();
				const height = $(document).height() - $(window).height();
				const offset = pathLength - (scroll * pathLength) / height;
				scrollPath.style.strokeDashoffset = offset;
			};

			updateScroll(); // Initialize progress on load
			$(window).scroll(updateScroll); // Update on scroll
		}

		const offset = 50; // Offset for showing the button
		const duration = 950; // Scroll duration

		// Show or hide the back-to-top button
		$(window).on("scroll", function () {
			if ($(this).scrollTop() > offset) {
				$(".scroll-up").addClass("active-scroll");
			} else {
				$(".scroll-up").removeClass("active-scroll");
			}
		});

		// Smooth scroll to top
		$(".scroll-up").on("click", function (event) {
			event.preventDefault();
			$("html, body").animate({ scrollTop: 0 }, duration);
			return false;
		});
	});
	// Back to top area end here ***

	// WOW Animatin area start here ***
	Splitting();
	wow = new WOW({
		animateClass: "animated",
		offset: 100,
		mobile: true,
	});
	wow.init();
	// WOW Animatin area start here ***
})(jQuery);
