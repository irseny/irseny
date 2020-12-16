function MainPageController() {
	var availableSections = ["Sensors", "Trackers", "Devices", "Config"];
	var activeSection = availableSections[0];

	this.getAvailableSections = function() {
		return availableSections;
	};
	this.getActiveSection = function() {
		return activeSection;
	};
	this.setActiveSection = function(section) {
		activeSection = section;
	};
}

var module = angular.module("mainPage");
module.directive("mainPage", function() {
	return {
		restrict: 'E',
		scope: true,
		controller: MainPageController,
		controllerAs: "$ctrl",
		templateUrl: "main-page/main-page.template.html"
	};
});
