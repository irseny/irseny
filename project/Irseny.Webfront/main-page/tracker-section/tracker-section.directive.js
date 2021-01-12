function TrackerSectionController(LiveWireService, LiveExchangeService) {

};

TrackerSectionController.$inject = ["LiveWireService", "LiveExchangeService"];
var module = angular.module("trackerSection");
module.directive("trackerSection", function() {
	return {
		scope: true,
		controller: TrackerSectionController,
		controllerAs: "$sectionCtrl",
		templateUrl: "main-page/tracker-section/tracker-section.template.html"
	};
});
