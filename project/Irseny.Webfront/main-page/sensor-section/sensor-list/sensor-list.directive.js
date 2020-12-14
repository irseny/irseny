var module = angular.module("sensorList");
module.directive("sensorList", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^sensorSection"
		},
		bindToController: true,
		controller: function() {},
		controllerAs: "$ctrl",
		scope: true,
		templateUrl: "main-page/sensor-section/sensor-list/sensor-list.template.html"
	};
});
