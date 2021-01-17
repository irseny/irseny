function TrackerListController() {

};
TrackerListController.$inject = ["$element"];
var module = angular.module("trackerList");
module.directive("trackerList", function() {
	return {
		scope: true,
		require: {
			shared: "^trackerSection"
		},
		bindToController: true,
		controller: TrackerListController,
		controllerAs: "$ctrl",
		templateUrl: "main-page/tracker-section/tracker-list/tracker-list.template.html"
	};
});
